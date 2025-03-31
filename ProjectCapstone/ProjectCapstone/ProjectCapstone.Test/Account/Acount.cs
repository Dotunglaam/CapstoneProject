using Xunit;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using BusinessObject.Models;
using Respository.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Respository.Interfaces;
using static BusinessObject.DTOS.Request;
using System.Net.Mail;

namespace ProjectCapstone.Test.Accounts;

public class AccountRepositoryTests
{
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly DbContextOptions<kmsContext> _dbOptions;

    public AccountRepositoryTests()
    {
        // Mock IConfiguration để cung cấp giá trị cho Jwt (nếu cần)
        _mockConfiguration = new Mock<IConfiguration>();
        _mockConfiguration.Setup(config => config["Jwt:Key"]).Returns("aqwertipoyportmkfcnvdsjnjsanxhjasoiadidsjfn");
        _mockConfiguration.Setup(config => config["Jwt:Issuer"]).Returns("localhost");
        _mockConfiguration.Setup(config => config["Jwt:Audience"]).Returns("localhost");

        // Tạo DbContextOptions cho kmsContext sử dụng InMemoryDatabase
        var dbName = $"TestDatabase_{Guid.NewGuid()}";
        _dbOptions = new DbContextOptionsBuilder<kmsContext>()
            .UseInMemoryDatabase(dbName)
            .EnableSensitiveDataLogging()  // Bật logging để kiểm tra chi tiết
            .Options;
    }

    [Theory]
    [InlineData("lamdthe163085@fpt.edu.vn", 2, true)]  // Kiểm tra với email đúng
    [InlineData("nonexistent@example.com", 0, false)] // Kiểm tra email không tồn tại
    [InlineData("lamdthe163085", 1, false)]  
    [InlineData("", 0, false)] // Kiểm tra email không tồn tại
    [InlineData("", 2, false)]
    [InlineData("nonexistent@example.com", 1, false)] 

    public void GetUserByEmail_ShouldReturnCorrectUserOrNull(
    string testEmail, int expectedUserId, bool shouldExist)
    {
        using (var context = new kmsContext(_dbOptions))
        {
            // Xóa và tạo lại cơ sở dữ liệu giả lập trước mỗi kiểm thử để tránh nhiễu dữ liệu
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();


            // Arrange: thêm dữ liệu người dùng vào database giả lập
            context.Users.AddRange(
                new User
                {
                    UserId = 1,
                    Mail = "user1@example.com",
                    RoleId = 2,
                    Password = "password1",
                    SaltKey = "salt1",
                    Status = 1,
                    Firstname = "User",
                    LastName = "One"
                },
                new User
                {
                    UserId = 2,
                    Mail = "lamdthe163085@fpt.edu.vn", // Email bạn muốn kiểm tra
                    RoleId = 3,
                    Password = "password2",
                    SaltKey = "salt2",
                    Status = 1,
                    Firstname = "User",
                    LastName = "Two"
                },
                new User
                {
                    UserId = 3,
                    Mail = "user3@example.com",
                    RoleId = 1,
                    Password = "password3",
                    SaltKey = "salt3",
                    Status = 1,
                    Firstname = "User",
                    LastName = "Three"
                }
            );
            context.SaveChanges();  // Lưu dữ liệu vào cơ sở dữ liệu giả lập

            // Khởi tạo AccountRepository với các dependencies đã mock
            var accountRepository = new AccountRepository(context, _mockConfiguration.Object);

            // Act: gọi phương thức GetUserByEmail với email test
            var result = accountRepository.GetUserByEmail(testEmail);

            // Assert: kiểm tra kết quả trả về
            if (shouldExist)
            {
                // Nếu người dùng tồn tại, kết quả không được null và UserId phải khớp
                Assert.NotNull(result);
                Assert.Equal(expectedUserId, result.UserId); // Kiểm tra UserId trả về
            }
            else
            {
                // Nếu người dùng không tồn tại, kết quả phải là null
                Assert.Null(result);
            }
        }
    }

    [Theory]
    [InlineData(1, true)] // Kiểm tra người dùng có ID = 1 tồn tại
    [InlineData(2, true)] // Kiểm tra người dùng có ID = 2 tồn tại
    [InlineData(99, false)] // Kiểm tra người dùng có ID = 99 không tồn tại
    [InlineData(null,false)]
    [InlineData(0, false)]
    [InlineData(3, false)]
    [InlineData(30, false)]

    public void GetUserById_ShouldReturnCorrectUserOrNull(int userId, bool shouldExist)
    {
        using (var context = new kmsContext(_dbOptions))
        {
            // Xóa và tạo lại cơ sở dữ liệu giả lập trước mỗi kiểm thử
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();


            // Arrange: Thêm dữ liệu người dùng vào cơ sở dữ liệu giả lập
            context.Users.AddRange(
                new User
                {
                    UserId = 1,
                    Mail = "user1@example.com",
                    RoleId = 2,
                    Password = "password1",
                    SaltKey = "salt1",
                    Status = 1,
                    Firstname = "User",
                    LastName = "One"
                },
                new User
                {
                    UserId = 2,
                    Mail = "user2@example.com",
                    RoleId = 3,
                    Password = "password2",
                    SaltKey = "salt2",
                    Status = 1,
                    Firstname = "User",
                    LastName = "Two"
                }
            );
            context.SaveChanges(); // Lưu vào cơ sở dữ liệu giả lập

            // Khởi tạo AccountRepository với các dependencies đã mock
            var accountRepository = new AccountRepository(context, _mockConfiguration.Object);

            // Act: gọi phương thức GetUserByEmail với email test
            var result = accountRepository.GetUserById(userId);

            // Assert: kiểm tra kết quả trả về
            if (shouldExist)
            {
                // Nếu người dùng tồn tại, kết quả không được null và UserId phải khớp
                Assert.NotNull(result);
                Assert.Equal(userId, result.UserId);
            }
            else
            {
                // Nếu người dùng không tồn tại, kết quả phải là null
                Assert.Null(result);
            }
        }
    }
    
    [Theory]
    [InlineData("testuser@example.com", 1, 2, "test-secret-key", "test-issuer", "test-audience", true, false)]  // Thành công
    [InlineData("testuser@example.com", 1, 2, null, "test-issuer", "test-audience", true, false)]           
    [InlineData("testuser@example.com", 1, 2, "test-secret-key", null, "test-audience", true, false)]        
    [InlineData("testuser@example.com", 1, 2, "test-secret-key", "test-issuer", null, true, false)]          
    [InlineData(null, 0, 0, "test-secret-key", "test-issuer", "test-audience", false, true)]                  
    [InlineData(null, 1, 0, "test-secret-key", "test-issuer", "test-audience", false, true)]                   
    [InlineData(null, 1, 3, "test-secret-key", "test-issuer", "test-audience", false, true)]                
    [InlineData(null, 1, 3, "null", "test-issuer", "test-audience", false, true)]
    [InlineData("tes@example.com", 1, 3, "test-secret-key", "test-issuer", null, true, false)]
    [InlineData("tes@example.com", 2, 3, null, "test-issuer", null, true, false)]
    [InlineData("tes@example.com", 2, 3, "test-secret-key", "test-issuer", null, true, false)]

    public void GenerateToken_ShouldHandleVariousScenarios(
     string email,
     int userId,
     int roleId,
     string secretKey,
     string issuer,
     string audience,
     bool shouldSucceed,
     bool expectArgumentNullException)
    {
        var testUser = email != null
            ? new User
            {
                UserId = userId,
                Mail = email,
                RoleId = roleId
            }
            : null;

        var accountRepository = new AccountRepository(null, _mockConfiguration.Object);

        if (expectArgumentNullException)
        {
            // Act & Assert: Nếu mong đợi ngoại lệ ArgumentNullException
            var ex = Assert.Throws<ArgumentNullException>(() => accountRepository.GenerateToken(testUser));
            Assert.Equal("user", ex.ParamName); // Kiểm tra tham số nào gây lỗi
        }
        else if (!shouldSucceed)
        {
            // Act & Assert: Nếu mong đợi InvalidOperationException do cấu hình sai
            Assert.Throws<InvalidOperationException>(() => accountRepository.GenerateToken(testUser));
        }
        else
        {
            // Act: Gọi phương thức GenerateToken với dữ liệu hợp lệ
            var result = accountRepository.GenerateToken(testUser);

            // Assert: Kiểm tra kết quả trả về
            Assert.NotNull(result.AccessToken);
            Assert.NotNull(result.RefreshToken);
            Assert.True(result.ExpiryDate > DateTime.UtcNow); // Đảm bảo thời gian hết hạn trong tương lai

            // Kiểm tra thêm nội dung token
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(result.AccessToken);
            Assert.Equal(userId.ToString(), token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Sub).Value);
            Assert.Equal(email, token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value);
            Assert.Equal(roleId.ToString(), token.Claims.First(c => c.Type == ClaimTypes.Role).Value);
        }
    }

    [Theory]
    [InlineData("password123", "randomsalt", true)] 
    [InlineData("", "randomsalt", false)]          
    [InlineData("password123", "", false)]          
    [InlineData("password123", null, false)]        
    [InlineData(null, "randomsalt", false)]
    [InlineData(null, "rando", false)]
    [InlineData(null, null, false)]
    [InlineData(null, null, false)]
    [InlineData("password123", null, false)]
    [InlineData(null, "randomsalt", false)]
    [InlineData(null, "randomsalt", false)]
    [InlineData("password123", "randoms", true)]
    public void EncryptPassword_ShouldHandleVariousScenarios(
            string originPassword, string salt, bool shouldSucceed)
    {
        if (!shouldSucceed)
        {
            // Act & Assert: Expect an ArgumentNullException for invalid inputs
            var exception = Assert.Throws<ArgumentNullException>(() => AccountRepository.EncryptPassword(originPassword, salt));
            Assert.NotNull(exception);
        }
        else
        {
            // Act: Call EncryptPassword with valid inputs
            var result = AccountRepository.EncryptPassword(originPassword, salt);

            // Assert: Ensure result is not null or empty
            Assert.NotNull(result);
            Assert.NotEmpty(result);

            // Additional: Verify the result is consistent
            var repeatedResult = AccountRepository.EncryptPassword(originPassword, salt);
            Assert.Equal(result, repeatedResult); // Ensure consistent hashing for the same inputs
        }
    }

    [Theory]
    [InlineData("123", "53e9f60615bd0bd9e9e75f67cc61dce4e22a22a0daa327f9ae25814e6a92d857", "7WxwP9rF1MaQEDai81B5aIpnVFFuW3uRj6V7iMvmRew=", true)] // Valid password
    [InlineData("wrongpassword", "e99a18c428cb38d5f260853678922e03", "randomsalt", false)] // Incorrect password
    [InlineData("password123", "e99a18c428cb38d5f260853678922e03", "wrongSalt", false)] // Incorrect salt
    [InlineData("password123", "e99a18c428cb38d5f260853678922e03", null, false)] // Null salt
    [InlineData(null, "e99a18c428cb38d5f260853678922e03", "randomsalt", false)] // Null password
    [InlineData("password123", null, "randomsalt", false)] // Null storedPassword
    [InlineData(null, null, null, false)] // All inputs null
    public void VerifyPassword_ShouldHandleVariousScenarios(
    string originPassword, string storedPassword, string salt, bool expectedResult)
    {
        if (originPassword == null || storedPassword == null || salt == null)
        {
            // Arrange & Act: Call VerifyPassword with invalid inputs
            var result = AccountRepository.VerifyPassword(originPassword, storedPassword, salt);

            // Assert: Ensure the method returns false
            Assert.False(result);
        }
        else
        {
            // Arrange: Compute the encrypted password if valid
            string encryptedPassword = AccountRepository.EncryptPassword(originPassword, salt);

            // Act: Verify the password
            var result = AccountRepository.VerifyPassword(originPassword, storedPassword, salt);

            // Assert: Validate the result matches expected outcome
            Assert.Equal(expectedResult, result);
        }
    }

    [Theory]
    [InlineData("admin@gmail.com", "123", true, true, 1, null)] // Đăng nhập thành công
    [InlineData("nonexistent@example.com", "any_password", false, false, 0, "Your account not exsit.")] // Tài khoản không tồn tại
    [InlineData("valid@example.com", "wrong_password", true, false, 1, "Incorrect password, please try again.")] // Mật khẩu sai
    [InlineData("admin@gmail.com", "123", true, true, 0, "Your account has been locked.")] // Tài khoản bị khóa
    public void Login_ShouldHandleVariousCases(
        string email,
        string password,
        bool userExists,
        bool passwordCorrect,
        int accountStatus,
        string expectedMessage)
    {
        using (var context = new kmsContext(_dbOptions))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();



            // Arrange: Thêm dữ liệu người dùng vào database giả lập
            if (userExists)
            {
                var salt = "test-salt";
                context.Users.Add(new User
                {
                    UserId = 1,
                    Mail = email,
                    RoleId = 2,
                    Password = passwordCorrect ?  AccountRepository.EncryptPassword(password, salt) : AccountRepository.EncryptPassword("correct_password", salt),
                    SaltKey = salt,
                    Status = (sbyte)accountStatus,
                    Firstname = "Test",
                    LastName = "User"
                });
                context.SaveChanges();
            }

            var accountRepository = new AccountRepository(context, _mockConfiguration.Object);

            // Act
            var response = accountRepository.Login(email, password);

            // Assert
            if (expectedMessage != null)
            {
                // Nếu có thông báo lỗi, kiểm tra thông báo lỗi
                Assert.Null(response.User); // Không trả về user
                Assert.Null(response.AccessToken); // Không có access token
                Assert.Equal(expectedMessage, response.Message); // Thông báo phải khớp
            }
            else
            {
                // Nếu đăng nhập thành công, kiểm tra thông tin
                Assert.NotNull(response.User);
                Assert.NotNull(response.AccessToken);
                Assert.NotNull(response.RefreshToken);
                Assert.True(response.ExpiryDate > DateTime.UtcNow); // Thời gian hết hạn phải hợp lệ
                Assert.Equal(email, response.User.Mail);
            }
        }
    }

    [Fact]
    public void SaveToken_ShouldSaveTokenToDatabase()
    {
        // Arrange: Set up the test context and data
        var user = new User
        {
            UserId = 1,
            Mail = "testuser@example.com",
            RoleId = 2,
            Firstname = "Test",
            LastName = "User",
            // Mock or set required properties
            Password = "hashedPassword", // Mocked or actual password
            SaltKey = "saltKey" // Mocked or actual salt
        };

        // Use an in-memory database for testing
        using (var context = new kmsContext(_dbOptions))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            //context.Users.RemoveRange(context.Users);
            //context.SaveChanges();
        }

        // Create the AccountRepository instance with mocked configuration
        using (var context = new kmsContext(_dbOptions))
        {
            var accountRepository = new AccountRepository(context, _mockConfiguration.Object);

            // Act: Call SaveToken to save the token
            string accessToken = "sampleAccessToken";
            string refreshToken = "sampleRefreshToken";
            DateTime expiryDate = DateTime.UtcNow.AddDays(30);
            var savedToken = accountRepository.SaveToken(user, accessToken, refreshToken, expiryDate);

            // Assert: Verify the token is saved correctly
            Assert.NotNull(savedToken);
            Assert.Equal(accessToken, savedToken.AccessToken);
            Assert.Equal(refreshToken, savedToken.RefreshToken);
            Assert.Equal(user.UserId, savedToken.UserId);
            Assert.Equal(expiryDate, savedToken.ExpiryDate);

            // Verify the token is persisted in the database
            var tokenInDb = context.Tokens.FirstOrDefault(t => t.UserId == user.UserId);
            Assert.NotNull(tokenInDb);
            Assert.Equal(savedToken.AccessToken, tokenInDb.AccessToken);
            Assert.Equal(savedToken.RefreshToken, tokenInDb.RefreshToken);
            Assert.Equal(savedToken.ExpiryDate, tokenInDb.ExpiryDate);
        }
    }

    [Theory]
    [InlineData("test1@example.com", 2, "password123", true)] // Đăng ký Parent
    [InlineData("test2@example.com", 5, "password456", true)] // Đăng ký Teacher
    [InlineData("duplicate@example.com", 2, "password789", false)] // Email trùng
    [InlineData("invalidrole@example.com", 99, "password000", false)] // Vai trò không hợp lệ
    [InlineData("invali@example.com", null, "password000", false)] // Vai trò không hợp lệ
    [InlineData("", null, "password000", false)] // Vai trò không hợp lệ
    [InlineData("duplicate@example.com", 5, "password789", false)] // Email trùng
    [InlineData("duplicate@example.com", 5, "", false)]
    [InlineData("duplica", 7, "", false)] 
    [InlineData("duplica", null, "", false)] 
    [InlineData("dup@example.com", 22, "password789", false)]
    [InlineData("dup@example.com", 22, null, false)]
    [InlineData("dup@example.com", 22, "", false)]

    public void Register_ShouldHandleVariousScenarios(
        string email, int roleId, string password, bool shouldSucceed)
    {
        using (var context = new kmsContext(_dbOptions))
        {
            // Xóa và tạo lại cơ sở dữ liệu giả lập trước mỗi kiểm thử
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            // Seed dữ liệu ban đầu
            context.Roles.AddRange(
                new Role { RoleId = 2, RoleName = "Parent" },
                new Role { RoleId = 5, RoleName = "Teacher" }
            );
            context.Users.Add(new User
            {
                Mail = "duplicate@example.com",
                Password = "hashedpassword",
                RoleId = 2,
                SaltKey = "somesalt"
            });
            context.SaveChanges();

            var accountRepository = new AccountRepository(context, null);

            if (!shouldSucceed)
            {
                Assert.Throws<Exception>(() =>
                {
                    accountRepository.Register(new RegisterViewModel
                    {
                        Mail = email,
                        RoleId = roleId,
                        Password = password
                    });
                });
            }
            else
            {
                var user = accountRepository.Register(new RegisterViewModel
                {
                    Mail = email,
                    RoleId = roleId,
                    Password = password
                });

                Assert.NotNull(user);
                Assert.Equal(email, user.Mail);
                Assert.Equal(roleId, user.RoleId);

                // Kiểm tra Parent hoặc Teacher được tạo
                if (roleId == 2)
                {
                    var parent = context.Parents.FirstOrDefault(p => p.ParentNavigation.UserId == user.UserId);
                    Assert.NotNull(parent);
                }
                else if (roleId == 5)
                {
                    var teacher = context.Teachers.FirstOrDefault(t => t.TeacherNavigation.UserId == user.UserId);
                    Assert.NotNull(teacher);
                }

                // Kiểm tra mật khẩu được mã hóa đúng
                var isPasswordValid = AccountRepository.VerifyPassword(password, user.Password, user.SaltKey);
                Assert.True(isPasswordValid);
            }
        }
    }

    [Fact]
    public void UpdateUser_ShouldUpdateUserDetails()
    {
        // Arrange: Set up the test context and initial user data
        using (var context = new kmsContext(_dbOptions))
        {
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            var originalUser = new User
            {
                UserId = 1,
                Firstname = "OriginalFirstName",
                LastName = "OriginalLastName",
                Address = "123 Main St",
                PhoneNumber = "123456789",
                Mail = "test@example.com",
                RoleId = 2,
                Password = "hashedPassword",
                SaltKey = "saltKey",
                Status = 1,
                Gender = 1,
                Dob = new DateTime(1990, 1, 1),
                Code = "USR001",
                Avatar = "original-avatar-url"
            };

            context.Users.Add(originalUser);
            context.SaveChanges();
        }

        using (var context = new kmsContext(_dbOptions))
        {
            var repository = new AccountRepository(context, null); // Replace with your actual repository class

            // Act: Update the user details
            var userToUpdate = context.Users.First();
            userToUpdate.Firstname = "UpdatedFirstName";
            userToUpdate.LastName = "UpdatedLastName";
            userToUpdate.Address = "456 Updated St";
            userToUpdate.PhoneNumber = "987654321";
            userToUpdate.Avatar = "updated-avatar-url";

            var updatedUser = repository.Update(userToUpdate);

            // Assert: Verify the user details were updated
            Assert.NotNull(updatedUser);
            Assert.Equal("UpdatedFirstName", updatedUser.Firstname);
            Assert.Equal("UpdatedLastName", updatedUser.LastName);
            Assert.Equal("456 Updated St", updatedUser.Address);
            Assert.Equal("987654321", updatedUser.PhoneNumber);
            Assert.Equal("updated-avatar-url", updatedUser.Avatar);

            // Verify the changes are persisted in the database
            var userInDb = context.Users.First();
            Assert.Equal("UpdatedFirstName", userInDb.Firstname);
            Assert.Equal("UpdatedLastName", userInDb.LastName);
            Assert.Equal("456 Updated St", userInDb.Address);
            Assert.Equal("987654321", userInDb.PhoneNumber);
            Assert.Equal("updated-avatar-url", userInDb.Avatar);
        }
    }
    [Fact]
    public void GeneratePassword_ShouldReturnValidPassword()
    {
        // Arrange
        var expectedLength = 6;
        const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        var context = new kmsContext(_dbOptions); // Instantiate kmsContext with in-memory options
        var passwordGenerator = new AccountRepository(context, _mockConfiguration.Object);

        // Act
        var password1 = passwordGenerator.GeneratePassword();
        var password2 = passwordGenerator.GeneratePassword();

        // Assert
        // Check the length of the password
        Assert.Equal(expectedLength, password1.Length);
        Assert.Equal(expectedLength, password2.Length);

        // Check if all characters are within the valid character set
        Assert.All(password1, c => Assert.Contains(c, validChars));
        Assert.All(password2, c => Assert.Contains(c, validChars));

        // Check passwords are not null or empty
        Assert.False(string.IsNullOrEmpty(password1));
        Assert.False(string.IsNullOrEmpty(password2));

        // Verify uniqueness (probabilistic)
        Assert.NotEqual(password1, password2);
    }

    [Fact]
    public void GenerateSaltKey_ShouldReturnValidSaltKey()
    {
        // Arrange
        var context = new kmsContext(_dbOptions); // Instantiate kmsContext with in-memory options
        var accountRepository = new AccountRepository(context, _mockConfiguration.Object);

        // Act
        var saltKey1 = accountRepository.GenerateSaltKey();
        var saltKey2 = accountRepository.GenerateSaltKey();

        // Assert
        // Check the salt key is not null or empty
        Assert.False(string.IsNullOrEmpty(saltKey1));
        Assert.False(string.IsNullOrEmpty(saltKey2));

        // Check the length of the salt key
        Assert.True(saltKey1.Length > 0); // Ensure it's not empty
        Assert.True(saltKey2.Length > 0);

        // Ensure each salt key is unique (probabilistic test; should rarely fail)
        Assert.NotEqual(saltKey1, saltKey2);

        // Check the salt key can be successfully decoded from Base64
        var saltBytes1 = Convert.FromBase64String(saltKey1);
        var saltBytes2 = Convert.FromBase64String(saltKey2);

        // Ensure the decoded bytes have the expected length
        Assert.Equal(32, saltBytes1.Length);
        Assert.Equal(32, saltBytes2.Length);
    }
    [Fact]
    public void SaveResetToken_ShouldHandleNewAndExistingTokensProperly()
    {
        // Arrange
        var initialToken = new Resetpasswordtoken
        {
            UserId = 1,
            Token = "initial-token",
            ExpiryTime = DateTime.UtcNow.AddMinutes(10)
        };

        var updatedToken = new Resetpasswordtoken
        {
            UserId = 1,
            Token = "updated-token",
            ExpiryTime = DateTime.UtcNow.AddMinutes(30)
        };

        var tokenForDuplicateTest = new Resetpasswordtoken
        {
            UserId = 2,
            Token = "token-1",
            ExpiryTime = DateTime.UtcNow.AddMinutes(10)
        };

        var tokenForDuplicateTest2 = new Resetpasswordtoken
        {
            UserId = 2,
            Token = "token-2",
            ExpiryTime = DateTime.UtcNow.AddMinutes(20)
        };

        using (var context = new kmsContext(_dbOptions))
        {
            var repository = new ResetPasswordTokenRepository(context);

            // Act: Save the first token for user 1
            repository.SaveResetToken(initialToken);

            // Assert: Check if the token for user 1 is saved correctly
            var savedToken = context.Resetpasswordtokens.FirstOrDefault(t => t.UserId == initialToken.UserId);
            Assert.NotNull(savedToken);
            Assert.Equal(initialToken.Token, savedToken.Token);
            Assert.Equal(initialToken.ExpiryTime, savedToken.ExpiryTime);

            // Act: Update token for user 1
            repository.SaveResetToken(updatedToken);

            // Assert: Check if the token for user 1 is updated correctly
            var updatedSavedToken = context.Resetpasswordtokens.FirstOrDefault(t => t.UserId == updatedToken.UserId);
            Assert.NotNull(updatedSavedToken);
            Assert.Equal(updatedToken.Token, updatedSavedToken.Token);
            Assert.Equal(updatedToken.ExpiryTime, updatedSavedToken.ExpiryTime);

            // Act: Save token for user 2
            repository.SaveResetToken(tokenForDuplicateTest);

            // Assert: Check if token for user 2 is saved correctly
            var savedTokenForDuplicate = context.Resetpasswordtokens.FirstOrDefault(t => t.UserId == tokenForDuplicateTest.UserId);
            Assert.NotNull(savedTokenForDuplicate);
            Assert.Equal(tokenForDuplicateTest.Token, savedTokenForDuplicate.Token);

            // Act: Save a new token for user 2, which should replace the old one
            repository.SaveResetToken(tokenForDuplicateTest2);

            // Assert: Check if token for user 2 was updated (no duplicates)
            var updatedTokenForDuplicate = context.Resetpasswordtokens.FirstOrDefault(t => t.UserId == tokenForDuplicateTest2.UserId);
            Assert.NotNull(updatedTokenForDuplicate);
            Assert.Equal(tokenForDuplicateTest2.Token, updatedTokenForDuplicate.Token);
            Assert.Equal(tokenForDuplicateTest2.ExpiryTime, updatedTokenForDuplicate.ExpiryTime);
        }
    }
    [Theory]
    [InlineData(1, true)]  // Test for userId 1 (token exists)
    [InlineData(2, false)] // Test for userId 2 (token does not exist)
    [InlineData(3, true)]  // Test for userId 3 (token exists)
    [InlineData(4, false)] // Test for userId 4 (token does not exist)
    [InlineData(5, false)] // Test for userId 4 (token does not exist)
    [InlineData(6, false)] // Test for userId 4 (token does not exist)
    [InlineData(7, false)] // Test for userId 4 (token does not exist)
    [InlineData(8, false)] // Test for userId 4 (token does not exist)
    [InlineData(9, false)] // Test for userId 4 (token does not exist)
    [InlineData(10, false)] // Test for userId 4 (token does not exist)
    [InlineData(11, false)] // Test for userId 4 (token does not exist)
    [InlineData(12, false)] // Test for userId 4 (token does not exist)

    public void GetResetTokenByUserId_ShouldReturnCorrectResult(int userId, bool tokenExists)
    {
        // Arrange
        var token = "sample-token";
        var tokenExpiryTime = DateTime.UtcNow.AddDays(1); // Set token expiry time
        var resetToken = new Resetpasswordtoken
        {
            UserId = userId,
            Token = token,
            ExpiryTime = tokenExpiryTime
        };

        using (var context = new kmsContext(_dbOptions))
        {
            var repository = new ResetPasswordTokenRepository(context);

            if (tokenExists)
            {
                // Save token to the in-memory database
                context.Resetpasswordtokens.Add(resetToken);
                context.SaveChanges();
            }

            // Act: Get the token by UserId
            var result = repository.GetResetTokenByUserId(userId);

            // Assert
            if (tokenExists)
            {
                // If token should exist, verify it's returned correctly
                Assert.NotNull(result);
                Assert.Equal(userId, result.UserId);
                Assert.Equal(token, result.Token);
                Assert.Equal(tokenExpiryTime, result.ExpiryTime);
            }
            else
            {
                // If token should not exist, verify null is returned
                Assert.Null(result);
            }
        }
    }
    [Theory]
    [InlineData("existingToken", true)]  // Test for an existing token
    [InlineData("nonExistingToken", false)] // Test for a non-existing token
    [InlineData("expiredToken", false)] // Test for an expired token
    [InlineData("expiredToken1", false)] // Test for an expired token
    [InlineData(null, false)] // Test for an expired token

    public void GetResetTokenByToken_ShouldReturnCorrectResult(string token, bool tokenExists)
    {
        // Arrange
        var sampleToken = "existingToken";
        var tokenExpiryTime = DateTime.UtcNow.AddDays(1);  // Set token expiry time
        var resetToken = new Resetpasswordtoken
        {
            UserId = 1,
            Token = sampleToken,
            ExpiryTime = tokenExpiryTime
        };

        using (var context = new kmsContext(_dbOptions))
        {
            var repository = new ResetPasswordTokenRepository(context);

            if (tokenExists)
            {
                // Save the token to the in-memory database
                context.Resetpasswordtokens.Add(resetToken);
                context.SaveChanges();
            }

            // Act: Get the token by token value
            var result = repository.GetResetTokenByToken(token);

            // Assert
            if (tokenExists)
            {
                // If token should exist, verify it's returned correctly
                Assert.NotNull(result);
                Assert.Equal(sampleToken, result.Token);
                Assert.Equal(1, result.UserId);
                Assert.Equal(tokenExpiryTime, result.ExpiryTime);
            }
            else
            {
                // If token should not exist, verify null is returned
                Assert.Null(result);
            }
        }
    }
    [Theory]
    [InlineData(1, true)]  // Existing UserId should successfully remove the token
    [InlineData(999, false)] // Non-existing UserId should not remove any token
    [InlineData(null, false)]
    [InlineData(2, false)]
    [InlineData(20, false)]

    public void RemoveResetToken_ShouldRemoveOrNotRemoveToken_BasedOnUserIdExistence(int userId, bool tokenShouldExistAfterRemoval)
    {
        // Arrange
        var existingUserId = 1;
        var resetToken = new Resetpasswordtoken
        {
            UserId = existingUserId,
            Token = "sample-token",
            ExpiryTime = DateTime.UtcNow.AddDays(1)
        };

        using (var context = new kmsContext(_dbOptions))
        {
            // Add a token for an existing user to the in-memory database
            context.Resetpasswordtokens.Add(resetToken);
            context.SaveChanges();
        }

        // Act: Try to remove token based on the passed userId
        using (var context = new kmsContext(_dbOptions))
        {
            var repository = new ResetPasswordTokenRepository(context);
            repository.RemoveResetToken(userId);

            // Assert: Check if the token removal was successful or not based on the `InlineData`
            var tokenInDb = context.Resetpasswordtokens.FirstOrDefault(t => t.UserId == existingUserId);
            if (tokenShouldExistAfterRemoval)
            {
                Assert.Null(tokenInDb); // The token should be removed for the existing user
            }
            else
            {
                Assert.NotNull(tokenInDb); // The token for the existing user should still exist
            }
        }
    }
   
}
