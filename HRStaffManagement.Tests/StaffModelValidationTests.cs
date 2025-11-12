using System.ComponentModel.DataAnnotations;
using HRStaffManagement.Models;
using Microsoft.AspNetCore.Http;

namespace HRStaffManagement.Tests
{
    public class StaffModelValidationTests
    {
        [Fact]
        public void Staff_ValidEmail_ShouldPassValidation()
        {
            // Arrange
            var staff = new Staff
            {
                StaffId = "STF001",
                StaffName = "John Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "(123) 456-7890",
                StartingDate = DateTime.Now
            };

            // Act
            var validationResults = ValidateModel(staff);

            // Assert
            var emailErrors = validationResults
                .Where(r => r.MemberNames.Contains(nameof(Staff.Email)))
                .Select(r => r.ErrorMessage);
            
            Assert.Empty(emailErrors);
        }

        [Theory]
        [InlineData("invalid-email")]
        [InlineData("invalid@")]
        [InlineData("@example.com")]
        [InlineData("invalid..email@example.com")]
        [InlineData("invalid@example")]
        [InlineData("")]
        public void Staff_InvalidEmail_ShouldFailValidation(string invalidEmail)
        {
            // Arrange
            var staff = new Staff
            {
                StaffId = "STF001",
                StaffName = "John Doe",
                Email = invalidEmail,
                PhoneNumber = "(123) 456-7890",
                StartingDate = DateTime.Now
            };

            // Act
            var validationResults = ValidateModel(staff);

            // Assert
            var emailErrors = validationResults
                .Where(r => r.MemberNames.Contains(nameof(Staff.Email)))
                .Select(r => r.ErrorMessage);
            
            Assert.NotEmpty(emailErrors);
        }

        [Theory]
        [InlineData("(123) 456-7890")]
        [InlineData("123-456-7890")]
        [InlineData("123.456.7890")]
        [InlineData("1234567890")]
        [InlineData("+1-123-456-7890")]
        [InlineData("+1 (123) 456-7890")]
        public void Staff_ValidPhoneNumber_ShouldPassValidation(string validPhone)
        {
            // Arrange
            var staff = new Staff
            {
                StaffId = "STF001",
                StaffName = "John Doe",
                Email = "john.doe@example.com",
                PhoneNumber = validPhone,
                StartingDate = DateTime.Now
            };

            // Act
            var validationResults = ValidateModel(staff);

            // Assert
            var phoneErrors = validationResults
                .Where(r => r.MemberNames.Contains(nameof(Staff.PhoneNumber)))
                .Select(r => r.ErrorMessage);
            
            Assert.Empty(phoneErrors);
        }

        [Theory]
        [InlineData("abc-def-ghij")]
        [InlineData("123")]
        [InlineData("12345")]
        [InlineData("")]
        [InlineData("123-456")]
        [InlineData("(123) 456")]
        public void Staff_InvalidPhoneNumber_ShouldFailValidation(string invalidPhone)
        {
            // Arrange
            var staff = new Staff
            {
                StaffId = "STF001",
                StaffName = "John Doe",
                Email = "john.doe@example.com",
                PhoneNumber = invalidPhone,
                StartingDate = DateTime.Now
            };

            // Act
            var validationResults = ValidateModel(staff);

            // Assert
            var phoneErrors = validationResults
                .Where(r => r.MemberNames.Contains(nameof(Staff.PhoneNumber)))
                .Select(r => r.ErrorMessage);
            
            Assert.NotEmpty(phoneErrors);
        }

        [Fact]
        public void Staff_ValidModel_ShouldPassAllValidations()
        {
            // Arrange
            var staff = new Staff
            {
                StaffId = "STF001",
                StaffName = "John Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "(123) 456-7890",
                StartingDate = DateTime.Now
            };

            // Act
            var validationResults = ValidateModel(staff);

            // Assert
            Assert.Empty(validationResults);
        }

        [Fact]
        public void Staff_MissingRequiredFields_ShouldFailValidation()
        {
            // Arrange
            var staff = new Staff();

            // Act
            var validationResults = ValidateModel(staff);

            // Assert
            Assert.NotEmpty(validationResults);
            Assert.Contains(validationResults, r => r.MemberNames.Contains(nameof(Staff.StaffId)));
            Assert.Contains(validationResults, r => r.MemberNames.Contains(nameof(Staff.StaffName)));
            Assert.Contains(validationResults, r => r.MemberNames.Contains(nameof(Staff.Email)));
            Assert.Contains(validationResults, r => r.MemberNames.Contains(nameof(Staff.PhoneNumber)));
            Assert.Contains(validationResults, r => r.MemberNames.Contains(nameof(Staff.StartingDate)));
        }

        [Theory]
        [InlineData("AB")] // Too short
        [InlineData("")] // Empty
        public void Staff_InvalidStaffIdLength_ShouldFailValidation(string staffId)
        {
            // Arrange
            var staff = new Staff
            {
                StaffId = staffId,
                StaffName = "John Doe",
                Email = "john.doe@example.com",
                PhoneNumber = "(123) 456-7890",
                StartingDate = DateTime.Now
            };

            // Act
            var validationResults = ValidateModel(staff);

            // Assert
            var staffIdErrors = validationResults
                .Where(r => r.MemberNames.Contains(nameof(Staff.StaffId)))
                .Select(r => r.ErrorMessage);
            
            Assert.NotEmpty(staffIdErrors);
        }

        [Theory]
        [InlineData("A")] // Too short
        [InlineData("")] // Empty
        public void Staff_InvalidStaffNameLength_ShouldFailValidation(string staffName)
        {
            // Arrange
            var staff = new Staff
            {
                StaffId = "STF001",
                StaffName = staffName,
                Email = "john.doe@example.com",
                PhoneNumber = "(123) 456-7890",
                StartingDate = DateTime.Now
            };

            // Act
            var validationResults = ValidateModel(staff);

            // Assert
            var staffNameErrors = validationResults
                .Where(r => r.MemberNames.Contains(nameof(Staff.StaffName)))
                .Select(r => r.ErrorMessage);
            
            Assert.NotEmpty(staffNameErrors);
        }

        // Helper method to validate model
        private IList<ValidationResult> ValidateModel(object model)
        {
            var validationResults = new List<ValidationResult>();
            var ctx = new ValidationContext(model, null, null);
            Validator.TryValidateObject(model, ctx, validationResults, true);
            return validationResults;
        }
    }
}

