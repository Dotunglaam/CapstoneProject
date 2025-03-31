using AutoMapper;
using BusinessObject.DTOS;
using BusinessObject.Models;
using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAO
{
    public class ChildrenDAO
    {
        private readonly kmsContext _context;
        private readonly IMapper _mapper;
        private readonly Cloudinary _cloudinary;
        public ChildrenDAO(kmsContext dbContext, IMapper mapper)
        {
            _context = dbContext;
            _mapper = mapper;
        }

        public ChildrenDAO(kmsContext dbContext, IMapper mapper, Cloudinary cloudinary)
        {
            _context = dbContext;
            _mapper = mapper;
            _cloudinary = cloudinary;
        }
        public async Task<List<ChildrenClassMapper>> GetAllChildren()
        {
            try
            {
                var getAllChildren = await _context.Children
                    .Include(c => c.ClassHasChildren)
                        .ThenInclude(cc => cc.Class)
                    .ToListAsync();

                var result = getAllChildren.Select(child => new ChildrenClassMapper
                {
                    StudentId = child.StudentId,
                    Code = child.Code,
                    FullName = child.FullName,
                    NickName = child.NickName,
                    GradeId = child.GradeId,
                    Dob = child.Dob,
                    ParentId = child.ParentId,
                    Gender = child.Gender,
                    Status = child.Status,
                    EthnicGroups = child.EthnicGroups,
                    Nationality = child.Nationality,
                    Religion = child.Religion,
                    Avatar = child.Avatar,
                    Classes = child.ClassHasChildren
                        .Select(cc => new ClassMapper
                        {
                            ClassId = cc.Class.ClassId,
                            ClassName = cc.Class.ClassName,
                            Number = cc.Class.Number,
                            IsActive = cc.Class.IsActive,
                            SchoolId = cc.Class.SchoolId,
                            SemesterId = cc.Class.SemesterId,
                            GradeId = cc.Class.GradeId,
                            Status = cc.Class.Status
                        }).ToList()
                }).ToList();

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Error retrieving children: " + ex.Message);
            }
        }

        public async Task<ChildrenClassMapper> GetChildrenByChildrenId(int childrenID)
        {
            try
            {
                var child = await _context.Children
                    .Include(c => c.ClassHasChildren)
                        .ThenInclude(cc => cc.Class)
                    .FirstOrDefaultAsync(p => p.StudentId == childrenID);

                if (child == null)
                {
                    throw new Exception("Children not found");
                }

                return new ChildrenClassMapper
                {
                    StudentId = child.StudentId,
                    Code = child.Code,
                    FullName = child.FullName,
                    NickName = child.NickName,
                    GradeId = child.GradeId,
                    Dob = child.Dob,
                    ParentId = child.ParentId,
                    Gender = child.Gender,
                    Status = child.Status,
                    EthnicGroups = child.EthnicGroups,
                    Nationality = child.Nationality,
                    Religion = child.Religion,
                    Avatar = child.Avatar,
                    Classes = child.ClassHasChildren
                        .Select(cc => new ClassMapper
                        {
                            ClassId = cc.Class.ClassId,
                            ClassName = cc.Class.ClassName,
                            Number = cc.Class.Number,
                            IsActive = cc.Class.IsActive,
                            SchoolId = cc.Class.SchoolId,
                            SemesterId = cc.Class.SemesterId,
                            GradeId = cc.Class.GradeId,
                            Status = cc.Class.Status
                        }).ToList()
                };
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred: {ex.Message}");
            }
        }

        public async Task ImportChildrenExcel(IFormFile file)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension?.Rows ?? 0;  

                        for (int row = 4; row <= rowCount; row++)  
                        {
                            string fullName = worksheet.Cells[row, 1].Text.Trim(); 
                            if (string.IsNullOrEmpty(fullName))
                            {
                                continue;  
                            }

                            string parentIdText = worksheet.Cells[row, 9].Text.Trim();  
                            int parentId = 0;
                            if (!string.IsNullOrEmpty(parentIdText) && int.TryParse(parentIdText, out int parentIdValue))
                            {
                                parentId = parentIdValue;
                            }

                            var child = new Child
                            {
                                Code = "ST000000",
                                FullName = fullName,
                                NickName = worksheet.Cells[row, 2].Text.Trim(),
                                GradeId = int.TryParse(worksheet.Cells[row, 3].Text, out int gradeValue) ? gradeValue : (int?)null,
                                Dob = DateTime.TryParse(worksheet.Cells[row, 4].Text, out DateTime dobValue) ? dobValue : (DateTime?)null,
                                Gender = int.TryParse(worksheet.Cells[row, 5].Text, out int genderValue) ? (sbyte?)genderValue : null,
                                Status = 1,
                                EthnicGroups = worksheet.Cells[row, 6].Text.Trim(),
                                Nationality = worksheet.Cells[row, 7].Text.Trim(),
                                Religion = worksheet.Cells[row, 8].Text.Trim(),
                                ParentId = parentId,
                                Avatar = "none"
                            };

                            _context.Children.Add(child);
                            await _context.SaveChangesAsync();

                            child.Code = GenerateCode(child.StudentId); 
                            _context.Children.Update(child);
                            await _context.SaveChangesAsync();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                throw new Exception($"Error importing children data: {ex.Message}. Inner Exception: {innerExceptionMessage}");
            }
        }

        public async Task<int> AddChildren(ChildrenMapper childrenMapper)
        {
            if (string.IsNullOrWhiteSpace(childrenMapper.FullName))
            {
                throw new Exception("FullName is required.");
            }

            if (!childrenMapper.Dob.HasValue)
            {
                throw new Exception("Date of Birth is required.");
            }
            var parentExists = await _context.Parents.AnyAsync(p => p.ParentId == childrenMapper.ParentId);
            if (!parentExists)
            {
                throw new Exception($"ParentId {childrenMapper.ParentId} is invalid.");
            }

            var gradeExists = await _context.Grades.AnyAsync(g => g.GradeId == childrenMapper.GradeId);
            if (!gradeExists)
            {
                throw new Exception($"GradeId {childrenMapper.GradeId} is invalid.");
            } 

            try
            {
                var childrenEntity = _mapper.Map<Child>(childrenMapper);

                childrenEntity.Code = "ST000000";
                childrenEntity.Status = 1;

                await _context.Children.AddAsync(childrenEntity);
                await _context.SaveChangesAsync();

                childrenEntity.Code = GenerateCode(childrenEntity.StudentId);

                _context.Children.Update(childrenEntity);
                await _context.SaveChangesAsync();

                return childrenEntity.StudentId;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UploadImageAndSaveToDatabaseAsync(int studentId, IFormFile image)
        {
            var student = await _context.Children
                .FirstOrDefaultAsync(ad => ad.StudentId == studentId);

            if (student == null)
            {
                throw new Exception("Student not found.");
            }

            string imageUrl = await UploadImageToCloudinaryAsync(image, studentId);

            if (!string.IsNullOrEmpty(imageUrl))
            {
                await UpdateChildrenImageAsync(studentId, imageUrl);
            }
            else
            {
                throw new Exception("Image upload failed");
            }
        }

        private async Task<string> UploadImageToCloudinaryAsync(IFormFile image, int studentId)
        {
            var classId = await GetClassIdByStudentId(studentId);

            var uploadParams = new ImageUploadParams()
            {
                File = new FileDescription(image.FileName, image.OpenReadStream()),
                Folder = $"ChildrenProfileImages/{classId}",
                PublicId = $"{Guid.NewGuid()}",
                Overwrite = true
            };

            var uploadResult = await _cloudinary.UploadAsync(uploadParams);

            if (uploadResult != null && !string.IsNullOrEmpty(uploadResult.SecureUri.ToString()))
            {
                return uploadResult.SecureUri.ToString();
            }

            return null;
        }

        private async Task UpdateChildrenImageAsync(int studentId, string imageUrl)
        {
            var children = await _context.Children
                .FirstOrDefaultAsync(ad => ad.StudentId == studentId);

            if (children != null)
            {
                children.Avatar = imageUrl;

                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteChildren(int childrenId)
        {
            try
            {
                var childrenEntity = await _context.Children.FirstOrDefaultAsync(p => p.StudentId == childrenId);
                if (childrenEntity != null)
                {
                    _context.Children.Remove(childrenEntity);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Product not found");
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task UpdateChildren(ChildrenMapper childrenMapper)
        {
            try
            {
                if (childrenMapper == null)
                {
                    throw new ArgumentNullException(nameof(childrenMapper), "Input data cannot be null");
                }

                if (string.IsNullOrWhiteSpace(childrenMapper.FullName))
                {
                    throw new Exception("FullName is required.");
                }

                if (childrenMapper.Dob == default)
                {
                    throw new Exception("Date of birth is required.");
                }

                if (childrenMapper.GradeId <= 0)
                {
                    throw new Exception("GradeId must be a valid value.");
                }

                if (childrenMapper.Gender < 0 || childrenMapper.Gender > 1) 
                {
                    throw new Exception("Gender must be either 0 or 1.");
                }

                var Product = await _context.Children.FirstOrDefaultAsync(p => p.StudentId == childrenMapper.StudentId);
                if (Product != null)
                {
                    Product.FullName = childrenMapper.FullName;
                    Product.NickName = childrenMapper.NickName;
                    Product.GradeId = childrenMapper.GradeId;
                    Product.Dob = childrenMapper.Dob;
                    Product.Gender = childrenMapper.Gender;
                    Product.EthnicGroups = childrenMapper.EthnicGroups;
                    Product.Nationality = childrenMapper.Nationality;
                    Product.Religion = childrenMapper.Religion;
                    Product.ParentId = childrenMapper.ParentId;

                    _context.Children.Update(Product);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    throw new Exception("Children not found.");
                }
            }
            catch (ArgumentException ex)
            {
                throw new Exception($"Validation failed: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"An error occurred while updating child: {ex.Message}", ex);
            }
        }

        public async Task AddChildToClass(int classId, int studentId)
        {
            try
            {
                var currentClass = await _context.Classes
                    .Include(c => c.ClassHasChildren)  
                    .FirstOrDefaultAsync(c => c.ClassId == classId);

                if (currentClass == null)
                {
                    throw new Exception("Class not found");
                }

                var currentStudent = await _context.Children
                    .FirstOrDefaultAsync(c => c.StudentId == studentId);

                if (currentStudent == null)
                {
                    throw new Exception("Children not found");
                }

                var classHasChildren = await _context.ClassHasChildren
                    .Where(ch => ch.ClassId == classId && ch.StudentId == studentId)
                    .FirstOrDefaultAsync();

                if (classHasChildren != null)
                {
                    throw new Exception("The student has been assigned to this class");
                }

                var existingClasses = await _context.ClassHasChildren
                    .Where(ch => ch.StudentId == studentId && ch.ClassId != classId)
                    .ToListAsync();

                if (existingClasses.Any())
                {
                    throw new Exception("Student already has another class. Cannot be added to this class.");
                }

                var classHasChild = new ClassHasChild
                {
                    ClassId = classId,
                    StudentId = studentId,
                    Date = DateTime.Now 
                };

                _context.ClassHasChildren.Add(classHasChild);

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new Exception("Error adding students to class: " + ex.Message);
            }
        }

        public async Task<List<ChildrenMapper>> GetChildrensWithoutClass(int classId)
        {
            try
            {
                var classExists = await _context.Classes.AnyAsync(c => c.ClassId == classId);
                if (!classExists)
                {
                    throw new Exception("ClassId does not exist.");
                }

                var gradeId = await GetGradeIdByClassId(classId);

                if (gradeId == null)
                {
                    throw new Exception("GradeId not found for the given classId.");
                }

                var studentsWithoutClass = await _context.Children
                    .Where(student => !_context.ClassHasChildren
                        .Any(ch => ch.StudentId == student.StudentId) &&
                        student.GradeId == gradeId &&
                        student.Status == 1) 
                    .Select(student => new ChildrenMapper
                    {
                        StudentId = student.StudentId,
                        Code = student.Code,
                        FullName = student.FullName,
                        NickName = student.NickName,
                        GradeId = student.GradeId,
                        Dob = student.Dob,
                        Gender = student.Gender,
                        Status = student.Status,
                        EthnicGroups = student.EthnicGroups,
                        Nationality = student.Nationality,
                        Religion = student.Religion,
                        ParentId = student.ParentId,
                        Avatar = student.Avatar
                    })
                    .ToListAsync();

                return studentsWithoutClass ?? new List<ChildrenMapper>();
            }
            catch (Exception ex)
            {
                throw new Exception("Error fetching students without class: " + ex.Message);
            }
        }
        public async Task<string> ExportChildrenWithoutClassToExcel(int classId)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            var filePath = Path.Combine(Path.GetTempPath(), $"ChildrenWithoutClass_{Guid.NewGuid()}.xlsx");

            try
            {
                var className = await GetClassNameByClassId(classId);
                var childrenWithoutClass = await GetChildrensWithoutClass(classId);

                using (var package = new ExcelPackage(new FileInfo(filePath)))
                {
                    var worksheet = package.Workbook.Worksheets.Add("Children Without Class");

                    worksheet.Cells[1, 1].Value = "Student ID";
                    worksheet.Cells[1, 2].Value = "Student Code";
                    worksheet.Cells[1, 3].Value = "Full Name";
                    worksheet.Cells[1, 4].Value = "Nick Name";
                    worksheet.Cells[1, 5].Value = "Grade ID";
                    worksheet.Cells[1, 6].Value = "Date of Birth";
                    worksheet.Cells[1, 7].Value = "Gender";
                    worksheet.Cells[1, 8].Value = "Ethnic Groups";
                    worksheet.Cells[1, 9].Value = "Nationality";
                    worksheet.Cells[1, 10].Value = "Religion";
                    worksheet.Cells[1, 11].Value = "Avatar";
                    worksheet.Cells[1, 12].Value = "Class ID";
                    worksheet.Cells[1, 13].Value = "Class Name";

                    worksheet.Cells["A1:L1"].Style.Font.Bold = true;
                    worksheet.Cells.AutoFitColumns();

                    int row = 2;

                    worksheet.Cells[row, 12].Value = classId;
                    worksheet.Cells[row, 13].Value = className;

                    foreach (var child in childrenWithoutClass)
                    {
                        worksheet.Cells[row, 1].Value = child.StudentId;
                        worksheet.Cells[row, 2].Value = child.Code;
                        worksheet.Cells[row, 3].Value = child.FullName;
                        worksheet.Cells[row, 4].Value = child.NickName;
                        worksheet.Cells[row, 5].Value = child.GradeId;
                        worksheet.Cells[row, 6].Value = child.Dob?.ToString("yyyy-MM-dd");
                        worksheet.Cells[row, 7].Value = child.Gender == 0 ? "Nữ" : "Nam"; 
                        worksheet.Cells[row, 8].Value = child.EthnicGroups;
                        worksheet.Cells[row, 9].Value = child.Nationality;
                        worksheet.Cells[row, 10].Value = child.Religion;
                        worksheet.Cells[row, 11].Value = child.Avatar;
                        

                        row++;
                    }

                    

                    await package.SaveAsync();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error exporting data to Excel: " + ex.Message);
            }

            return filePath;
        }
        public async Task AddChildrenToClassesFromExcel(IFormFile file)
        {
            var errors = new List<string>(); // Lưu trữ các lỗi

            try
            {
                var studentClassPairs = await ImportStudentIdsAndClassIds(file);

                foreach (var (studentId, classId) in studentClassPairs)
                {
                    if (studentId.HasValue)
                    {
                        try
                        {
                            await AddChildToClass(classId, studentId.Value); 
                        }
                        catch (Exception ex)
                        {
                            errors.Add($"Error adding student with ID {studentId.Value} to class {classId}: {ex.Message}");
                        }
                    }
                    else
                    {
                        errors.Add($"Student ID not found for student code in class {classId}");
                    }
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error during import process: {ex.Message}");
            }

            if (errors.Any())
            {
                throw new Exception($"One or more errors occurred during processing:\n{string.Join("\n", errors)}");
            }
        }

        public async Task<List<(int? StudentId, int ClassId)>> ImportStudentIdsAndClassIds(IFormFile file)
        {
            try
            {
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

                var result = new List<(int? StudentId, int ClassId)>();

                using (var stream = new MemoryStream())
                {
                    await file.CopyToAsync(stream);

                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var rowCount = worksheet.Dimension?.Rows ?? 0;

                        for (int row = 5; row <= rowCount; row++)
                        {
                            string studentCode = worksheet.Cells[row, 1].Text.Trim();
                            if (string.IsNullOrEmpty(studentCode))
                            {
                                throw new Exception($"Student code is missing at row {row}.");
                            }

                            string classIdText = worksheet.Cells[row, 3].Text.Trim();
                            if (int.TryParse(classIdText, out int classId))
                            {
                                var studentId = await GetstudentIdByStudentCode(studentCode);

                                if (studentId.HasValue)
                                {
                                    result.Add((studentId, classId));
                                }
                                else
                                {
                                    throw new Exception($"Student code {studentCode} not found in the system at row {row}.");
                                }
                            }
                            else
                            {
                                throw new Exception($"Invalid class ID at row {row}.");
                            }
                        }
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                var innerExceptionMessage = ex.InnerException != null ? ex.InnerException.Message : "No inner exception";
                throw new Exception($"Error importing student IDs and class IDs: {ex.Message}. Inner Exception: {innerExceptionMessage}");
            }
        }

        public async Task<int?> GetClassIdByStudentId(int studentId)
        {
            var classId = await _context.Classes
                .Where(c => c.IsActive == 1 && c.ClassHasChildren.Any(cc => cc.StudentId == studentId))
                .Select(c => c.ClassId)
                .FirstOrDefaultAsync();

            return classId;
        }
        public async Task<string?> GetClassNameByClassId(int ClassId)
        {
            var ClassName = await _context.Classes
                .Where(c => c.IsActive == 1 && c.ClassId == ClassId)
                .Select(c => c.ClassName)
                .FirstOrDefaultAsync();

            return ClassName;
        }

        public async Task<int?> GetstudentIdByStudentCode(string studentCode)
        {
            var studentId = await _context.Children
                .Where(c => c.Status == 1 && c.Code == studentCode)
                .Select(c => c.StudentId)
                .FirstOrDefaultAsync();

            return studentId;
        }

        public async Task<int?> GetGradeIdByClassId(int classId)
        {
            var gradeId = await _context.Classes
                .Where(c => c.IsActive == 1 && c.ClassId == classId)
                .Select(c => c.GradeId)
                .FirstOrDefaultAsync();

            return gradeId;
        }

        private string GenerateCode(int studentId)
        {
            int currentYear = DateTime.Now.Year;

            int yearSuffix = currentYear - 2023;

            return $"ST{yearSuffix:D2}{studentId:D4}";
        }

    }
}
