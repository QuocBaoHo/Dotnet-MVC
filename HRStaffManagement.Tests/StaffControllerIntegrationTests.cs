using System.Net;
using System.Text;
using System.Text.Json;
using HRStaffManagement.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HRStaffManagement.Tests
{
    public class StaffControllerIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
    {
        private readonly WebApplicationFactory<Program> _factory;
        private readonly HttpClient _client;

        public StaffControllerIntegrationTests(WebApplicationFactory<Program> factory)
        {
            _factory = factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove the existing DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                    {
                        services.Remove(descriptor);
                    }

                    // Add in-memory database for testing
                    services.AddDbContext<ApplicationDbContext>(options =>
                    {
                        options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid().ToString());
                    });
                });
            });
            _client = _factory.CreateClient();
        }

        [Fact]
        public async Task Index_Get_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/Staff");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Create_Get_ReturnsSuccessStatusCode()
        {
            // Act
            var response = await _client.GetAsync("/Staff/Create");

            // Assert
            response.EnsureSuccessStatusCode();
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        public async Task Create_Post_WithValidData_RedirectsToIndex()
        {
            // Arrange
            var formData = new Dictionary<string, string>
            {
                { "StaffId", "TEST001" },
                { "StaffName", "Test User" },
                { "Email", "test@example.com" },
                { "PhoneNumber", "(123) 456-7890" },
                { "StartingDate", DateTime.Now.ToString("yyyy-MM-dd") }
            };

            var content = new FormUrlEncodedContent(formData);

            // Act
            var response = await _client.PostAsync("/Staff/Create", content);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
            Assert.Contains("/Staff", response.Headers.Location?.ToString());
        }

        [Fact]
        public async Task Create_Post_WithInvalidEmail_ReturnsViewWithError()
        {
            // Arrange
            var formData = new Dictionary<string, string>
            {
                { "StaffId", "TEST002" },
                { "StaffName", "Test User" },
                { "Email", "invalid-email" },
                { "PhoneNumber", "(123) 456-7890" },
                { "StartingDate", DateTime.Now.ToString("yyyy-MM-dd") }
            };

            var content = new FormUrlEncodedContent(formData);

            // Act
            var response = await _client.PostAsync("/Staff/Create", content);

            // Assert
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("email", responseContent, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Create_Post_WithInvalidPhone_ReturnsViewWithError()
        {
            // Arrange
            var formData = new Dictionary<string, string>
            {
                { "StaffId", "TEST003" },
                { "StaffName", "Test User" },
                { "Email", "test@example.com" },
                { "PhoneNumber", "invalid-phone" },
                { "StartingDate", DateTime.Now.ToString("yyyy-MM-dd") }
            };

            var content = new FormUrlEncodedContent(formData);

            // Act
            var response = await _client.PostAsync("/Staff/Create", content);

            // Assert
            var responseContent = await response.Content.ReadAsStringAsync();
            Assert.Contains("phone", responseContent, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Details_Get_WithValidId_ReturnsSuccessStatusCode()
        {
            // Arrange - Create a staff member first
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var staff = new Staff
                {
                    StaffId = "DETAIL001",
                    StaffName = "Detail Test",
                    Email = "detail@example.com",
                    PhoneNumber = "(123) 456-7890",
                    StartingDate = DateTime.Now
                };
                context.Staff.Add(staff);
                await context.SaveChangesAsync();

                // Act
                var response = await _client.GetAsync($"/Staff/Details/{staff.Id}");

                // Assert
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                Assert.Contains("DETAIL001", content);
            }
        }

        [Fact]
        public async Task Details_Get_WithInvalidId_ReturnsNotFound()
        {
            // Act
            var response = await _client.GetAsync("/Staff/Details/99999");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        [Fact]
        public async Task Edit_Get_WithValidId_ReturnsSuccessStatusCode()
        {
            // Arrange - Create a staff member first
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var staff = new Staff
                {
                    StaffId = "EDIT001",
                    StaffName = "Edit Test",
                    Email = "edit@example.com",
                    PhoneNumber = "(123) 456-7890",
                    StartingDate = DateTime.Now
                };
                context.Staff.Add(staff);
                await context.SaveChangesAsync();

                // Act
                var response = await _client.GetAsync($"/Staff/Edit/{staff.Id}");

                // Assert
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                Assert.Contains("EDIT001", content);
            }
        }

        [Fact]
        public async Task Edit_Post_WithValidData_RedirectsToIndex()
        {
            // Arrange - Create a staff member first
            int staffId;
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var staff = new Staff
                {
                    StaffId = "EDIT002",
                    StaffName = "Edit Test 2",
                    Email = "edit2@example.com",
                    PhoneNumber = "(123) 456-7890",
                    StartingDate = DateTime.Now
                };
                context.Staff.Add(staff);
                await context.SaveChangesAsync();
                staffId = staff.Id;
            }

            var formData = new Dictionary<string, string>
            {
                { "Id", staffId.ToString() },
                { "StaffId", "EDIT002" },
                { "StaffName", "Updated Name" },
                { "Email", "updated@example.com" },
                { "PhoneNumber", "(987) 654-3210" },
                { "StartingDate", DateTime.Now.ToString("yyyy-MM-dd") }
            };

            var content = new FormUrlEncodedContent(formData);

            // Act
            var response = await _client.PostAsync($"/Staff/Edit", content);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }

        [Fact]
        public async Task Delete_Get_WithValidId_ReturnsSuccessStatusCode()
        {
            // Arrange - Create a staff member first
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var staff = new Staff
                {
                    StaffId = "DELETE001",
                    StaffName = "Delete Test",
                    Email = "delete@example.com",
                    PhoneNumber = "(123) 456-7890",
                    StartingDate = DateTime.Now
                };
                context.Staff.Add(staff);
                await context.SaveChangesAsync();

                // Act
                var response = await _client.GetAsync($"/Staff/Delete/{staff.Id}");

                // Assert
                response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();
                Assert.Contains("DELETE001", content);
            }
        }

        [Fact]
        public async Task Delete_Post_WithValidId_RedirectsToIndex()
        {
            // Arrange - Create a staff member first
            int staffId;
            using (var scope = _factory.Services.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var staff = new Staff
                {
                    StaffId = "DELETE002",
                    StaffName = "Delete Test 2",
                    Email = "delete2@example.com",
                    PhoneNumber = "(123) 456-7890",
                    StartingDate = DateTime.Now
                };
                context.Staff.Add(staff);
                await context.SaveChangesAsync();
                staffId = staff.Id;
            }

            // Act
            var response = await _client.PostAsync($"/Staff/Delete/{staffId}", null);

            // Assert
            Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        }
    }
}

