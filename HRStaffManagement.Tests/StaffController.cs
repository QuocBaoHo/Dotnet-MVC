using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HRStaffManagement.Controllers;
using HRStaffManagement.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace HRStaffManagement.Tests
{
    public class StaffControllerTests
    {
        private DbContextOptions<ApplicationDbContext> _dbOptions;
        private ApplicationDbContext _context;
        private Mock<IWebHostEnvironment> _envMock;

        public StaffControllerTests()
        {
            // Use in-memory database for testing
            _dbOptions = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(_dbOptions);

            _envMock = new Mock<IWebHostEnvironment>();
            _envMock.Setup(e => e.WebRootPath).Returns("wwwroot");
        }

        private StaffController GetController()
        {
            return new StaffController(_context, _envMock.Object);
        }

        [Fact]
        public async Task Index_ReturnsAllStaffs()
        {
            // Arrange
            _context.Staff.Add(new Staff { StaffId = "STF001", StaffName = "John Doe", Email = "john@example.com", PhoneNumber = "123-456-7890", StartingDate = DateTime.Today });
            _context.Staff.Add(new Staff { StaffId = "STF002", StaffName = "Jane Smith", Email = "jane@example.com", PhoneNumber = "321-654-0987", StartingDate = DateTime.Today });
            await _context.SaveChangesAsync();

            var controller = GetController();

            // Act
            var result = await controller.Index() as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsAssignableFrom<List<Staff>>(result.Model);
            Assert.Equal(2, model.Count);
        }

        [Fact]
        public async Task Details_WithValidId_ReturnsStaff()
        {
            // Arrange
            var staff = new Staff { StaffId = "STF003", StaffName = "Alice", Email = "alice@example.com", PhoneNumber = "111-222-3333", StartingDate = DateTime.Today };
            _context.Staff.Add(staff);
            await _context.SaveChangesAsync();

            var controller = GetController();

            // Act
            var result = await controller.Details("STF003") as ViewResult;

            // Assert
            Assert.NotNull(result);
            var model = Assert.IsType<Staff>(result.Model);
            Assert.Equal("Alice", model.StaffName);
        }

        [Fact]
        public async Task Details_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var controller = GetController();

            // Act
            var result = await controller.Details("NON_EXISTENT");

            // Assert
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteConfirmed_RemovesStaff()
        {
            // Arrange
            var staff = new Staff { StaffId = "STF004", StaffName = "Bob", Email = "bob@example.com", PhoneNumber = "444-555-6666", StartingDate = DateTime.Today };
            _context.Staff.Add(staff);
            await _context.SaveChangesAsync();

            var controller = GetController();

            // Act
            var result = await controller.DeleteConfirmed(staff.Id);

            // Assert
            Assert.IsType<RedirectToActionResult>(result);
            Assert.Empty(_context.Staff.ToList());
        }
    }
}
