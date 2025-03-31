using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace BusinessObject.Models
{
    public partial class kmsContext : DbContext
    {
        public kmsContext()
        {
        }

        public kmsContext(DbContextOptions<kmsContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Activity> Activities { get; set; } = null!;
        public virtual DbSet<Album> Albums { get; set; } = null!;
        public virtual DbSet<Attendance> Attendances { get; set; } = null!;
        public virtual DbSet<AttendanceDetail> AttendanceDetails { get; set; } = null!;
        public virtual DbSet<Cagetoryservice> Cagetoryservices { get; set; } = null!;
        public virtual DbSet<Checkservice> Checkservices { get; set; } = null!;
        public virtual DbSet<Child> Children { get; set; } = null!;
        public virtual DbSet<ChildrenHasService> ChildrenHasServices { get; set; } = null!;
        public virtual DbSet<Class> Classes { get; set; } = null!;
        public virtual DbSet<ClassHasChild> ClassHasChildren { get; set; } = null!;
        public virtual DbSet<Discount> Discounts { get; set; } = null!;
        public virtual DbSet<Grade> Grades { get; set; } = null!;
        public virtual DbSet<Image> Images { get; set; } = null!;
        public virtual DbSet<Location> Locations { get; set; } = null!;
        public virtual DbSet<Menu> Menus { get; set; } = null!;
        public virtual DbSet<MenuHasGrade> MenuHasGrades { get; set; } = null!;
        public virtual DbSet<Menudetail> Menudetails { get; set; } = null!;
        public virtual DbSet<Notification> Notifications { get; set; } = null!;
        public virtual DbSet<Parent> Parents { get; set; } = null!;
        public virtual DbSet<Payment> Payments { get; set; } = null!;
        public virtual DbSet<PickupPerson> PickupPeople { get; set; } = null!;
        public virtual DbSet<Request> Requests { get; set; } = null!;
        public virtual DbSet<Resetpasswordtoken> Resetpasswordtokens { get; set; } = null!;
        public virtual DbSet<Role> Roles { get; set; } = null!;
        public virtual DbSet<Schedule> Schedules { get; set; } = null!;
        public virtual DbSet<Scheduledetail> Scheduledetails { get; set; } = null!;
        public virtual DbSet<School> Schools { get; set; } = null!;
        public virtual DbSet<Semester> Semesters { get; set; } = null!;
        public virtual DbSet<Semesterreal> Semesterreals { get; set; } = null!;
        public virtual DbSet<Service> Services { get; set; } = null!;
        public virtual DbSet<Teacher> Teachers { get; set; } = null!;
        public virtual DbSet<TimeSlot> TimeSlots { get; set; } = null!;
        public virtual DbSet<Token> Tokens { get; set; } = null!;
        public virtual DbSet<Tuition> Tuitions { get; set; } = null!;
        public virtual DbSet<User> Users { get; set; } = null!;
        public virtual DbSet<Usernotification> Usernotifications { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
                optionsBuilder.UseMySql("server=localhost;database=kms;user=root;password=123456", Microsoft.EntityFrameworkCore.ServerVersion.Parse("8.0.39-mysql"));
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.UseCollation("utf8mb4_0900_ai_ci")
                .HasCharSet("utf8mb4");

            modelBuilder.Entity<Activity>(entity =>
            {
                entity.ToTable("activity");

                entity.Property(e => e.ActivityName).HasMaxLength(200);
            });

            modelBuilder.Entity<Album>(entity =>
            {
                entity.ToTable("album");

                entity.HasIndex(e => e.ClassId, "FK_class_idx");

                entity.HasIndex(e => e.ModifiBy, "ModifiBy_userID_idx");

                entity.HasIndex(e => e.CreateBy, "createBy_userid_idx");

                entity.Property(e => e.AlbumId).HasColumnName("AlbumID");

                entity.Property(e => e.AlbumName).HasMaxLength(45);

                entity.Property(e => e.Description).HasMaxLength(45);

                entity.Property(e => e.IsActive).HasColumnName("isActive");

                entity.Property(e => e.Reason).HasMaxLength(100);

                entity.Property(e => e.Status).HasColumnName("status");

                entity.Property(e => e.TimePost).HasColumnType("datetime");

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.Albums)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_class");

                entity.HasOne(d => d.CreateByNavigation)
                    .WithMany(p => p.AlbumCreateByNavigations)
                    .HasForeignKey(d => d.CreateBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("createBy_userid");

                entity.HasOne(d => d.ModifiByNavigation)
                    .WithMany(p => p.AlbumModifiByNavigations)
                    .HasForeignKey(d => d.ModifiBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("ModifiBy_userID");

                entity.HasMany(d => d.ClassClasses)
                    .WithMany(p => p.AlbumAlbums)
                    .UsingEntity<Dictionary<string, object>>(
                        "AlbumHasClass",
                        l => l.HasOne<Class>().WithMany().HasForeignKey("ClassClassId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_album_has_class_class1"),
                        r => r.HasOne<Album>().WithMany().HasForeignKey("AlbumAlbumId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_album_has_class_album1"),
                        j =>
                        {
                            j.HasKey("AlbumAlbumId", "ClassClassId").HasName("PRIMARY").HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                            j.ToTable("album_has_class");

                            j.HasIndex(new[] { "AlbumAlbumId" }, "fk_album_has_class_album1_idx");

                            j.HasIndex(new[] { "ClassClassId" }, "fk_album_has_class_class1_idx");

                            j.IndexerProperty<int>("AlbumAlbumId").HasColumnName("album_AlbumID");

                            j.IndexerProperty<int>("ClassClassId").HasColumnName("class_ClassID");
                        });
            });

            modelBuilder.Entity<Attendance>(entity =>
            {
                entity.ToTable("attendance");

                entity.HasIndex(e => e.ClassId, "fk_attendance_class_idx");

                entity.Property(e => e.AttendanceId).HasColumnName("AttendanceID");

                entity.Property(e => e.ClassId).HasColumnName("ClassID");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.Type).HasMaxLength(10);

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.Attendances)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_attendance_class");
            });

            modelBuilder.Entity<AttendanceDetail>(entity =>
            {
                entity.ToTable("attendance_detail");

                entity.HasIndex(e => e.AttendanceId, "fk_attendance_detail_attendance_idx");

                entity.HasIndex(e => e.StudentId, "fk_attendance_detail_student_idx");

                entity.Property(e => e.AttendanceDetailId).HasColumnName("AttendanceDetailID");

                entity.Property(e => e.AttendanceId).HasColumnName("AttendanceID");

                entity.Property(e => e.CreatedAt).HasColumnType("datetime");

                entity.Property(e => e.ImageUrl).HasMaxLength(255);

                entity.Property(e => e.Status).HasMaxLength(20);

                entity.Property(e => e.StudentId).HasColumnName("StudentID");

                entity.HasOne(d => d.Attendance)
                    .WithMany(p => p.AttendanceDetails)
                    .HasForeignKey(d => d.AttendanceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_attendance_detail_attendance");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.AttendanceDetails)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_attendance_detail_student");
            });

            modelBuilder.Entity<Cagetoryservice>(entity =>
            {
                entity.HasKey(e => e.CategoryServiceId)
                    .HasName("PRIMARY");

                entity.ToTable("cagetoryservices");

                entity.Property(e => e.CategoryServiceId).HasColumnName("CategoryServiceID");

                entity.Property(e => e.CategoryName).HasMaxLength(45);
            });

            modelBuilder.Entity<Checkservice>(entity =>
            {
                entity.ToTable("checkservice");

                entity.HasIndex(e => e.StudentId, "fk_checkservice_children1_idx");

                entity.HasIndex(e => e.ServiceId, "fk_checkservice_services1_idx");

                entity.Property(e => e.CheckServiceId).HasColumnName("CheckServiceID");

                entity.Property(e => e.ServiceId).HasColumnName("ServiceID");

                entity.Property(e => e.StudentId).HasColumnName("StudentID");

                entity.HasOne(d => d.Service)
                    .WithMany(p => p.Checkservices)
                    .HasForeignKey(d => d.ServiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_checkservice_services1");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.Checkservices)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_checkservice_children1");
            });

            modelBuilder.Entity<Child>(entity =>
            {
                entity.HasKey(e => e.StudentId)
                    .HasName("PRIMARY");

                entity.ToTable("children");

                entity.HasIndex(e => e.ParentId, "fk_Children_Parent1_idx");

                entity.HasIndex(e => e.GradeId, "fk_class_Grade1_idx");

                entity.Property(e => e.StudentId).HasColumnName("StudentID");

                entity.Property(e => e.Avatar).HasMaxLength(500);

                entity.Property(e => e.Code).HasMaxLength(45);

                entity.Property(e => e.Dob)
                    .HasColumnType("datetime")
                    .HasColumnName("DOB");

                entity.Property(e => e.EthnicGroups).HasMaxLength(45);

                entity.Property(e => e.FullName).HasMaxLength(45);

                entity.Property(e => e.Nationality).HasMaxLength(45);

                entity.Property(e => e.NickName).HasMaxLength(45);

                entity.Property(e => e.ParentId).HasColumnName("ParentID");

                entity.Property(e => e.Religion).HasMaxLength(45);

                entity.HasOne(d => d.Grade)
                    .WithMany(p => p.Children)
                    .HasForeignKey(d => d.GradeId)
                    .HasConstraintName("fk_child_Grade1");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.Children)
                    .HasForeignKey(d => d.ParentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Children_Parent1");
            });

            modelBuilder.Entity<ChildrenHasService>(entity =>
            {
                entity.HasKey(e => e.ChidrenServicesId)
                    .HasName("PRIMARY");

                entity.ToTable("children_has_services");

                entity.HasIndex(e => e.StudentId, "fk_children_has_services_children1_idx");

                entity.HasIndex(e => e.ServiceId, "fk_children_has_services_services1_idx");

                entity.Property(e => e.ChidrenServicesId)
                    .ValueGeneratedNever()
                    .HasColumnName("ChidrenServicesID");

                entity.Property(e => e.ServiceId).HasColumnName("ServiceID");

                entity.Property(e => e.StudentId).HasColumnName("StudentID");

                entity.HasOne(d => d.Service)
                    .WithMany(p => p.ChildrenHasServices)
                    .HasForeignKey(d => d.ServiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_children_has_services_services1");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.ChildrenHasServices)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_children_has_services_children1");
            });

            modelBuilder.Entity<Class>(entity =>
            {
                entity.ToTable("class");

                entity.HasIndex(e => e.GradeId, "fk_class_Grade1_idx");

                entity.HasIndex(e => e.SemesterId, "fk_class_Semester1_idx");

                entity.HasIndex(e => e.SchoolId, "fk_class_school1_idx");

                entity.Property(e => e.ClassId).HasColumnName("ClassID");

                entity.Property(e => e.ClassName).HasMaxLength(45);

                entity.Property(e => e.GradeId).HasColumnName("GradeID");

                entity.Property(e => e.SchoolId).HasColumnName("SchoolID");

                entity.Property(e => e.SemesterId).HasColumnName("SemesterID");

                entity.HasOne(d => d.Grade)
                    .WithMany(p => p.Classes)
                    .HasForeignKey(d => d.GradeId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_class_Grade2");

                entity.HasOne(d => d.School)
                    .WithMany(p => p.Classes)
                    .HasForeignKey(d => d.SchoolId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_class_school1");

                entity.HasOne(d => d.Semester)
                    .WithMany(p => p.Classes)
                    .HasForeignKey(d => d.SemesterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_class_Semester1");
            });

            modelBuilder.Entity<ClassHasChild>(entity =>
            {
                entity.HasKey(e => new { e.ClassId, e.StudentId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.ToTable("class_has_children");

                entity.HasIndex(e => e.StudentId, "fk_class_has_children_children1_idx");

                entity.HasIndex(e => e.ClassId, "fk_class_has_children_class1_idx");

                entity.Property(e => e.ClassId).HasColumnName("ClassID");

                entity.Property(e => e.StudentId).HasColumnName("StudentID");

                entity.Property(e => e.Date).HasColumnType("datetime");

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.ClassHasChildren)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_class_has_children_class1");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.ClassHasChildren)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_class_has_children_children1");
            });

            modelBuilder.Entity<Discount>(entity =>
            {
                entity.ToTable("discount");

                entity.Property(e => e.Discount1).HasColumnName("Discount");
            });

            modelBuilder.Entity<Grade>(entity =>
            {
                entity.ToTable("grade");

                entity.Property(e => e.GradeId).HasColumnName("GradeID");

                entity.Property(e => e.BaseTuitionFee).HasPrecision(12);

                entity.Property(e => e.Description)
                    .HasMaxLength(45)
                    .HasColumnName("description");

                entity.Property(e => e.Name).HasMaxLength(45);
            });

            modelBuilder.Entity<Image>(entity =>
            {
                entity.ToTable("image");

                entity.HasIndex(e => e.AlbumId, "fk_image_album1_idx");

                entity.Property(e => e.ImageId).HasColumnName("ImageID");

                entity.Property(e => e.AlbumId).HasColumnName("AlbumID");

                entity.Property(e => e.Caption).HasMaxLength(100);

                entity.Property(e => e.ImgUrl)
                    .HasMaxLength(200)
                    .HasColumnName("imgURL");

                entity.Property(e => e.PostedAt).HasColumnType("datetime");

                entity.HasOne(d => d.Album)
                    .WithMany(p => p.Images)
                    .HasForeignKey(d => d.AlbumId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_image_album1");
            });

            modelBuilder.Entity<Location>(entity =>
            {
                entity.ToTable("location");

                entity.Property(e => e.LocationId).HasColumnName("LocationID");

                entity.Property(e => e.LocationName).HasMaxLength(45);
            });

            modelBuilder.Entity<Menu>(entity =>
            {
                entity.ToTable("menu");

                entity.HasIndex(e => e.SchoolId, "fk_menu_school1_idx");

                entity.Property(e => e.MenuId).HasColumnName("MenuID");

                entity.Property(e => e.SchoolId).HasColumnName("SchoolID");

                entity.HasOne(d => d.School)
                    .WithMany(p => p.Menus)
                    .HasForeignKey(d => d.SchoolId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_menu_school1");
            });

            modelBuilder.Entity<MenuHasGrade>(entity =>
            {
                entity.HasKey(e => new { e.MenuId, e.GradeId })
                    .HasName("PRIMARY")
                    .HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                entity.ToTable("menu_has_grade");

                entity.HasIndex(e => e.GradeId, "fk_grade_idx");

                entity.HasIndex(e => e.MenuId, "fk_menu_idx");

                entity.Property(e => e.MenuId).HasColumnName("MenuID");

                entity.Property(e => e.GradeId).HasColumnName("GradeID");

                entity.Property(e => e.CreatedDate)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(d => d.Grade)
                    .WithMany(p => p.MenuHasGrades)
                    .HasForeignKey(d => d.GradeId)
                    .HasConstraintName("fk_grade");

                entity.HasOne(d => d.Menu)
                    .WithMany(p => p.MenuHasGrades)
                    .HasForeignKey(d => d.MenuId)
                    .HasConstraintName("fk_menu");
            });

            modelBuilder.Entity<Menudetail>(entity =>
            {
                entity.ToTable("menudetail");

                entity.HasIndex(e => e.MenuId, "MenuID");

                entity.Property(e => e.MenuDetailId).HasColumnName("MenuDetailID");

                entity.Property(e => e.DayOfWeek).HasMaxLength(10);

                entity.Property(e => e.FoodName).HasMaxLength(255);

                entity.Property(e => e.MealCode).HasMaxLength(10);

                entity.Property(e => e.MenuId).HasColumnName("MenuID");

                entity.HasOne(d => d.Menu)
                    .WithMany(p => p.Menudetails)
                    .HasForeignKey(d => d.MenuId)
                    .HasConstraintName("menudetail_ibfk_1");
            });

            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("notifications");

                entity.HasIndex(e => e.RoleId, "RoleID");

                entity.Property(e => e.NotificationId).HasColumnName("NotificationID");

                entity.Property(e => e.CreatedAt)
                    .HasColumnType("datetime")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.Property(e => e.Message).HasColumnType("text");

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.Title).HasMaxLength(255);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Notifications)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("notifications_ibfk_1");
            });

            modelBuilder.Entity<Parent>(entity =>
            {
                entity.ToTable("parent");

                entity.HasIndex(e => e.ParentId, "fk_Parent_user1_idx");

                entity.Property(e => e.ParentId)
                    .ValueGeneratedNever()
                    .HasColumnName("ParentID");

                entity.Property(e => e.Name).HasMaxLength(45);

                entity.HasOne(d => d.ParentNavigation)
                    .WithOne(p => p.Parent)
                    .HasForeignKey<Parent>(d => d.ParentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("q");
            });

            modelBuilder.Entity<Payment>(entity =>
            {
                entity.ToTable("payment");

                entity.HasIndex(e => e.ServiceId, "Service_idx");

                entity.HasIndex(e => e.StudentId, "studen_idx");

                entity.HasIndex(e => e.TuitionId, "tution_idx");

                entity.Property(e => e.PaymentId).HasColumnName("PaymentID");

                entity.Property(e => e.PaymentName).HasMaxLength(100);

                entity.Property(e => e.ServiceId).HasColumnName("ServiceID");

                entity.Property(e => e.StudentId).HasColumnName("StudentID");

                entity.Property(e => e.TotalAmount).HasPrecision(10);

                entity.Property(e => e.TuitionId).HasColumnName("TuitionID");

                entity.HasOne(d => d.Service)
                    .WithMany(p => p.Payments)
                    .HasForeignKey(d => d.ServiceId)
                    .HasConstraintName("Service");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.Payments)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("studen");

                entity.HasOne(d => d.Tuition)
                    .WithMany(p => p.Payments)
                    .HasForeignKey(d => d.TuitionId)
                    .HasConstraintName("tution");
            });

            modelBuilder.Entity<PickupPerson>(entity =>
            {
                entity.ToTable("pickup_person");

                entity.HasIndex(e => e.ParentId, "fk_PickupPerson_ParentID_idx");

                entity.Property(e => e.PickupPersonId).HasColumnName("PickupPersonID");

                entity.Property(e => e.ImageUrl).HasMaxLength(500);

                entity.Property(e => e.Name).HasMaxLength(45);

                entity.Property(e => e.ParentId).HasColumnName("ParentID");

                entity.Property(e => e.PhoneNumber).HasMaxLength(15);

                entity.Property(e => e.Uuid)
                    .HasMaxLength(255)
                    .HasColumnName("UUID");

                entity.HasOne(d => d.Parent)
                    .WithMany(p => p.PickupPeople)
                    .HasForeignKey(d => d.ParentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_PickupPerson_ParentID");

                entity.HasMany(d => d.Students)
                    .WithMany(p => p.PickupPeople)
                    .UsingEntity<Dictionary<string, object>>(
                        "PickupPersonHasChild",
                        l => l.HasOne<Child>().WithMany().HasForeignKey("StudentId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_PickupPersonChildren_StudentID"),
                        r => r.HasOne<PickupPerson>().WithMany().HasForeignKey("PickupPersonId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_PickupPersonChildren_PickupPersonID"),
                        j =>
                        {
                            j.HasKey("PickupPersonId", "StudentId").HasName("PRIMARY").HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                            j.ToTable("pickup_person_has_children");

                            j.HasIndex(new[] { "StudentId" }, "fk_PickupPersonChildren_StudentID");

                            j.IndexerProperty<int>("PickupPersonId").HasColumnName("PickupPersonID");

                            j.IndexerProperty<int>("StudentId").HasColumnName("StudentID");
                        });
            });

            modelBuilder.Entity<Request>(entity =>
            {
                entity.ToTable("request");

                entity.HasIndex(e => e.CreateBy, "fk_Request_user1_idx");

                entity.Property(e => e.RequestId).HasColumnName("RequestID");

                entity.Property(e => e.CreateAt).HasColumnType("datetime");

                entity.Property(e => e.Description).HasMaxLength(255);

                entity.Property(e => e.ProcessNote).HasMaxLength(200);

                entity.Property(e => e.StudentId).HasColumnName("StudentID");

                entity.Property(e => e.Title).HasMaxLength(255);

                entity.HasOne(d => d.CreateByNavigation)
                    .WithMany(p => p.Requests)
                    .HasForeignKey(d => d.CreateBy)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Request_user1");
            });

            modelBuilder.Entity<Resetpasswordtoken>(entity =>
            {
                entity.HasKey(e => e.UserId)
                    .HasName("PRIMARY");

                entity.ToTable("resetpasswordtokens");

                entity.Property(e => e.UserId).ValueGeneratedNever();

                entity.Property(e => e.ExpiryTime).HasColumnType("datetime");

                entity.Property(e => e.Token).HasMaxLength(300);
            });

            modelBuilder.Entity<Role>(entity =>
            {
                entity.ToTable("role");

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.RoleName).HasMaxLength(45);
            });

            modelBuilder.Entity<Schedule>(entity =>
            {
                entity.ToTable("schedule");

                entity.HasIndex(e => e.ClassId, "fk_Schedule_class1_idx");

                entity.Property(e => e.ScheduleId).HasColumnName("ScheduleID");

                entity.Property(e => e.ClassId).HasColumnName("ClassID");

                entity.Property(e => e.TeacherName).HasMaxLength(45);

                entity.HasOne(d => d.Class)
                    .WithMany(p => p.Schedules)
                    .HasForeignKey(d => d.ClassId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Schedule_class1");
            });

            modelBuilder.Entity<Scheduledetail>(entity =>
            {
                entity.ToTable("scheduledetail");

                entity.HasIndex(e => e.ActivityId, "fk_schedule_time_activity_idx");

                entity.HasIndex(e => e.LocationId, "fk_schedule_time_location_idx");

                entity.HasIndex(e => e.TimeSlotId, "fk_schedule_time_slot1_idx");

                entity.HasIndex(e => e.ScheduleId, "fk_scheduledetail_time_slot1_idx");

                entity.Property(e => e.ScheduleDetailId).HasColumnName("ScheduleDetailID");

                entity.Property(e => e.Day)
                    .HasMaxLength(45)
                    .HasColumnName("day");

                entity.Property(e => e.LocationId).HasColumnName("LocationID");

                entity.Property(e => e.Note).HasMaxLength(45);

                entity.Property(e => e.ScheduleId).HasColumnName("ScheduleID");

                entity.Property(e => e.TimeSlotId).HasColumnName("TimeSlotID");

                entity.Property(e => e.Weekdate)
                    .HasMaxLength(45)
                    .HasColumnName("weekdate");

                entity.HasOne(d => d.Activity)
                    .WithMany(p => p.Scheduledetails)
                    .HasForeignKey(d => d.ActivityId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_scheduledetail_time_activity");

                entity.HasOne(d => d.Location)
                    .WithMany(p => p.Scheduledetails)
                    .HasForeignKey(d => d.LocationId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_scheduledetail_time_location");

                entity.HasOne(d => d.Schedule)
                    .WithMany(p => p.Scheduledetails)
                    .HasForeignKey(d => d.ScheduleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_scheduledetail_schedule");

                entity.HasOne(d => d.TimeSlot)
                    .WithMany(p => p.Scheduledetails)
                    .HasForeignKey(d => d.TimeSlotId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_scheduledetail_time_slot1");
            });

            modelBuilder.Entity<School>(entity =>
            {
                entity.ToTable("school");

                entity.Property(e => e.SchoolId).HasColumnName("SchoolID");

                entity.Property(e => e.Address).HasMaxLength(45);

                entity.Property(e => e.Email).HasMaxLength(45);

                entity.Property(e => e.Phone).HasMaxLength(45);

                entity.Property(e => e.SchoolDes).HasMaxLength(45);
            });

            modelBuilder.Entity<Semester>(entity =>
            {
                entity.ToTable("semester");

                entity.Property(e => e.SemesterId).HasColumnName("SemesterID");

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(45);

                entity.Property(e => e.StartDate).HasColumnType("datetime");
            });

            modelBuilder.Entity<Semesterreal>(entity =>
            {
                entity.ToTable("semesterreal");

                entity.HasIndex(e => e.SemesterId, "SemesterID_idx");

                entity.Property(e => e.SemesterrealId).HasColumnName("semesterrealID");

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.Name).HasMaxLength(45);

                entity.Property(e => e.SemesterId).HasColumnName("SemesterID");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.HasOne(d => d.Semester)
                    .WithMany(p => p.Semesterreals)
                    .HasForeignKey(d => d.SemesterId)
                    .HasConstraintName("SemesterID");
            });

            modelBuilder.Entity<Service>(entity =>
            {
                entity.ToTable("services");

                entity.HasIndex(e => e.CategoryServiceId, "fk_services_cagetoryservices1_idx");

                entity.HasIndex(e => e.SchoolId, "fk_services_school1_idx");

                entity.Property(e => e.ServiceId).HasColumnName("ServiceID");

                entity.Property(e => e.CategoryServiceId).HasColumnName("CategoryServiceID");

                entity.Property(e => e.SchoolId).HasColumnName("SchoolID");

                entity.Property(e => e.ServiceDes).HasMaxLength(45);

                entity.Property(e => e.ServiceName).HasMaxLength(45);

                entity.Property(e => e.ServicePrice).HasPrecision(10);

                entity.HasOne(d => d.CategoryService)
                    .WithMany(p => p.Services)
                    .HasForeignKey(d => d.CategoryServiceId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_services_cagetoryservices1");

                entity.HasOne(d => d.School)
                    .WithMany(p => p.Services)
                    .HasForeignKey(d => d.SchoolId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_services_school1");
            });

            modelBuilder.Entity<Teacher>(entity =>
            {
                entity.ToTable("teacher");

                entity.HasIndex(e => e.TeacherId, "fk_Teacher_user1_idx");

                entity.Property(e => e.TeacherId)
                    .ValueGeneratedNever()
                    .HasColumnName("TeacherID");

                entity.Property(e => e.Education).HasMaxLength(255);

                entity.Property(e => e.Experience).HasMaxLength(255);

                entity.Property(e => e.HomeroomTeacher).HasColumnName("Homeroom teacher");

                entity.Property(e => e.Name).HasMaxLength(45);

                entity.HasOne(d => d.TeacherNavigation)
                    .WithOne(p => p.Teacher)
                    .HasForeignKey<Teacher>(d => d.TeacherId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_Teacher_user1");

                entity.HasMany(d => d.Classes)
                    .WithMany(p => p.Teachers)
                    .UsingEntity<Dictionary<string, object>>(
                        "TeacherHasClass",
                        l => l.HasOne<Class>().WithMany().HasForeignKey("ClassId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_Teacher_has_class_class1"),
                        r => r.HasOne<Teacher>().WithMany().HasForeignKey("TeacherId").OnDelete(DeleteBehavior.ClientSetNull).HasConstraintName("fk_Teacher_has_class_Teacher1"),
                        j =>
                        {
                            j.HasKey("TeacherId", "ClassId").HasName("PRIMARY").HasAnnotation("MySql:IndexPrefixLength", new[] { 0, 0 });

                            j.ToTable("teacher_has_class");

                            j.HasIndex(new[] { "TeacherId" }, "fk_Teacher_has_class_Teacher1_idx");

                            j.HasIndex(new[] { "ClassId" }, "fk_Teacher_has_class_class1_idx");

                            j.IndexerProperty<int>("TeacherId").HasColumnName("TeacherID");

                            j.IndexerProperty<int>("ClassId").HasColumnName("ClassID");
                        });
            });

            modelBuilder.Entity<TimeSlot>(entity =>
            {
                entity.ToTable("time_slot");

                entity.Property(e => e.TimeSlotId).HasColumnName("TimeSlotID");

                entity.Property(e => e.TimeName).HasMaxLength(45);
            });

            modelBuilder.Entity<Token>(entity =>
            {
                entity.ToTable("token");

                entity.HasIndex(e => e.UserId, "fk_token_user1_idx");

                entity.Property(e => e.TokenId).HasColumnName("TokenID");

                entity.Property(e => e.AccessToken).HasMaxLength(512);

                entity.Property(e => e.ExpiryDate).HasColumnType("datetime");

                entity.Property(e => e.RefreshToken).HasMaxLength(512);

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Tokens)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_token_user1");
            });

            modelBuilder.Entity<Tuition>(entity =>
            {
                entity.ToTable("tuition");

                entity.HasIndex(e => e.StudentId, "tuition_ibfk_1");

                entity.HasIndex(e => e.SemesterId, "tuition_ibfk_2_idx");

                entity.HasIndex(e => e.DiscountId, "tuition_ibfk_3_idx");

                entity.Property(e => e.EndDate).HasColumnType("datetime");

                entity.Property(e => e.LastEmailSentDate).HasColumnType("datetime");

                entity.Property(e => e.SemesterId).HasColumnName("SemesterID");

                entity.Property(e => e.StartDate).HasColumnType("datetime");

                entity.Property(e => e.StudentId).HasColumnName("StudentID");

                entity.Property(e => e.TotalFee).HasPrecision(10);

                entity.Property(e => e.TuitionFee).HasPrecision(10);

                entity.HasOne(d => d.Discount)
                    .WithMany(p => p.Tuitions)
                    .HasForeignKey(d => d.DiscountId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("tuition_ibfk_3");

                entity.HasOne(d => d.Semester)
                    .WithMany(p => p.Tuitions)
                    .HasForeignKey(d => d.SemesterId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("tuition_ibfk_2");

                entity.HasOne(d => d.Student)
                    .WithMany(p => p.Tuitions)
                    .HasForeignKey(d => d.StudentId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("tuition_ibfk_1");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("user");

                entity.HasIndex(e => e.RoleId, "fk_user_role1_idx");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.Property(e => e.Address).HasMaxLength(45);

                entity.Property(e => e.Avatar).HasMaxLength(500);

                entity.Property(e => e.Code).HasMaxLength(45);

                entity.Property(e => e.CreateAt).HasColumnType("datetime");

                entity.Property(e => e.Dob).HasColumnType("datetime");

                entity.Property(e => e.Firstname).HasMaxLength(45);

                entity.Property(e => e.LastName).HasMaxLength(45);

                entity.Property(e => e.Mail).HasMaxLength(45);

                entity.Property(e => e.Password).HasMaxLength(64);

                entity.Property(e => e.PhoneNumber).HasMaxLength(45);

                entity.Property(e => e.RoleId).HasColumnName("RoleID");

                entity.Property(e => e.SaltKey).HasMaxLength(45);

                entity.HasOne(d => d.Role)
                    .WithMany(p => p.Users)
                    .HasForeignKey(d => d.RoleId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("fk_user_role1");
            });

            modelBuilder.Entity<Usernotification>(entity =>
            {
                entity.ToTable("usernotifications");

                entity.HasIndex(e => e.NotificationId, "NotificationID");

                entity.HasIndex(e => e.UserId, "UserID");

                entity.Property(e => e.UserNotificationId).HasColumnName("UserNotificationID");

                entity.Property(e => e.NotificationId).HasColumnName("NotificationID");

                entity.Property(e => e.ReadAt).HasColumnType("datetime");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .HasDefaultValueSql("'unread'");

                entity.Property(e => e.UserId).HasColumnName("UserID");

                entity.HasOne(d => d.Notification)
                    .WithMany(p => p.Usernotifications)
                    .HasForeignKey(d => d.NotificationId)
                    .HasConstraintName("usernotifications_ibfk_2");

                entity.HasOne(d => d.User)
                    .WithMany(p => p.Usernotifications)
                    .HasForeignKey(d => d.UserId)
                    .HasConstraintName("usernotifications_ibfk_1");
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
