using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using HRStaffManagement.Models;
using System.Text.RegularExpressions;

namespace HRStaffManagement.Tests
{
    public class StaffSeleniumE2ETests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
    {
        private readonly WebApplicationFactory<Program> _factory;
        private IWebDriver _driver;
        private string _baseUrl;
        private readonly int _delayMs = 2000; // 2 seconds delay between actions for visibility

        public StaffSeleniumE2ETests(WebApplicationFactory<Program> factory)
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
                        options.UseInMemoryDatabase("SeleniumTestDb_" + Guid.NewGuid().ToString());
                    });
                });
            });

            // Get the server URL from the factory
            using var client = _factory.CreateClient();
            _baseUrl = client.BaseAddress!.ToString().TrimEnd('/');

            // Setup Chrome driver
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            // Disable headless mode so user can see the browser
            // options.AddArgument("--headless"); // Commented out so browser is visible
            _driver = new ChromeDriver(options);
        }

        [Fact]
        public async Task E2E_CreateStaff_ValidateFormat_DeleteStaff()
        {
            try
            {
                // Step 1: Navigate to Staff Index page
                Console.WriteLine("Step 1: Navigating to Staff Index page...");
                _driver.Navigate().GoToUrl($"{_baseUrl}/Staff");
                await Task.Delay(_delayMs);

                // Step 2: Click "Add New Staff" button
                Console.WriteLine("Step 2: Clicking 'Add New Staff' button...");
                var addButton = _driver.FindElement(By.LinkText("Add New Staff"));
                addButton.Click();
                await Task.Delay(_delayMs);

                // Verify we're on the Create page
                Assert.Contains("/Staff/Create", _driver.Url);

                // Step 3: Fill in Staff ID
                Console.WriteLine("Step 3: Filling in Staff ID...");
                var staffIdInput = _driver.FindElement(By.Id("StaffId"));
                staffIdInput.Clear();
                staffIdInput.SendKeys("SEL001");
                await Task.Delay(_delayMs);

                // Step 4: Fill in Staff Name
                Console.WriteLine("Step 4: Filling in Staff Name...");
                var staffNameInput = _driver.FindElement(By.Id("StaffName"));
                staffNameInput.Clear();
                staffNameInput.SendKeys("Selenium Test User");
                await Task.Delay(_delayMs);

                // Step 5: Fill in Email (valid format)
                Console.WriteLine("Step 5: Filling in Email (valid format: test@example.com)...");
                var emailInput = _driver.FindElement(By.Id("Email"));
                emailInput.Clear();
                emailInput.SendKeys("selenium.test@example.com");
                await Task.Delay(_delayMs);

                // Step 6: Fill in Phone Number (valid format)
                Console.WriteLine("Step 6: Filling in Phone Number (valid format: (123) 456-7890)...");
                var phoneInput = _driver.FindElement(By.Id("PhoneNumber"));
                phoneInput.Clear();
                phoneInput.SendKeys("(123) 456-7890");
                await Task.Delay(_delayMs);

                // Step 7: Fill in Starting Date
                Console.WriteLine("Step 7: Filling in Starting Date...");
                var dateInput = _driver.FindElement(By.Id("StartingDate"));
                dateInput.Clear();
                dateInput.SendKeys(DateTime.Now.ToString("yyyy-MM-dd"));
                await Task.Delay(_delayMs);

                // Step 8: Test invalid email format
                Console.WriteLine("Step 8: Testing invalid email format...");
                emailInput.Clear();
                emailInput.SendKeys("invalid-email");
                emailInput.SendKeys(Keys.Tab); // Trigger validation
                await Task.Delay(_delayMs);

                // Check for validation error
                var emailError = _driver.FindElements(By.CssSelector("span[data-valmsg-for='Email']"));
                if (emailError.Count > 0 && emailError[0].Displayed)
                {
                    Console.WriteLine($"✓ Email validation error detected: {emailError[0].Text}");
                    Assert.True(emailError[0].Displayed, "Email validation error should be visible");
                }

                // Step 9: Fix email to valid format
                Console.WriteLine("Step 9: Fixing email to valid format...");
                emailInput.Clear();
                emailInput.SendKeys("selenium.test@example.com");
                await Task.Delay(_delayMs);

                // Step 10: Test invalid phone format
                Console.WriteLine("Step 10: Testing invalid phone format...");
                phoneInput.Clear();
                phoneInput.SendKeys("invalid-phone");
                phoneInput.SendKeys(Keys.Tab); // Trigger validation
                await Task.Delay(_delayMs);

                // Check for validation error
                var phoneError = _driver.FindElements(By.CssSelector("span[data-valmsg-for='PhoneNumber']"));
                if (phoneError.Count > 0 && phoneError[0].Displayed)
                {
                    Console.WriteLine($"✓ Phone validation error detected: {phoneError[0].Text}");
                    Assert.True(phoneError[0].Displayed, "Phone validation error should be visible");
                }

                // Step 11: Fix phone to valid format
                Console.WriteLine("Step 11: Fixing phone to valid format...");
                phoneInput.Clear();
                phoneInput.SendKeys("(123) 456-7890");
                await Task.Delay(_delayMs);

                // Step 12: Submit the form
                Console.WriteLine("Step 12: Submitting the form...");
                var submitButton = _driver.FindElement(By.CssSelector("button[type='submit']"));
                submitButton.Click();
                await Task.Delay(_delayMs * 2);

                // Step 13: Verify redirect to Index page
                Console.WriteLine("Step 13: Verifying redirect to Index page...");
                WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(10));
                wait.Until(d => d.Url.Contains("/Staff") && !d.Url.Contains("/Create"));
                Assert.Contains("/Staff", _driver.Url);
                Assert.DoesNotContain("/Create", _driver.Url);
                await Task.Delay(_delayMs);

                // Step 14: Verify staff appears in the table
                Console.WriteLine("Step 14: Verifying staff appears in the table...");
                var tableBody = _driver.FindElement(By.CssSelector("table tbody"));
                Assert.Contains("SEL001", tableBody.Text);
                Assert.Contains("Selenium Test User", tableBody.Text);
                Assert.Contains("selenium.test@example.com", tableBody.Text);
                await Task.Delay(_delayMs);

                // Step 15: Find and click Delete button for the created staff
                Console.WriteLine("Step 15: Finding delete button for created staff...");
                var rows = _driver.FindElements(By.CssSelector("table tbody tr"));
                IWebElement? deleteButton = null;
                int staffId = 0;

                foreach (var row in rows)
                {
                    if (row.Text.Contains("SEL001"))
                    {
                        // Find the delete link in this row
                        var deleteLink = row.FindElement(By.CssSelector("a[href*='/Staff/Delete/']"));
                        var href = deleteLink.GetAttribute("href");
                        var match = Regex.Match(href, @"/Staff/Delete/(\d+)");
                        if (match.Success)
                        {
                            staffId = int.Parse(match.Groups[1].Value);
                            deleteButton = deleteLink;
                            break;
                        }
                    }
                }

                Assert.NotNull(deleteButton);
                await Task.Delay(_delayMs);

                // Step 16: Click Delete button
                Console.WriteLine("Step 16: Clicking Delete button...");
                deleteButton!.Click();
                await Task.Delay(_delayMs);

                // Step 17: Confirm deletion
                Console.WriteLine("Step 17: Confirming deletion...");
                wait.Until(d => d.Url.Contains("/Staff/Delete/"));
                var confirmDeleteButton = _driver.FindElement(By.CssSelector("form button[type='submit']"));
                confirmDeleteButton.Click();
                await Task.Delay(_delayMs * 2);

                // Step 18: Verify redirect back to Index
                Console.WriteLine("Step 18: Verifying redirect back to Index...");
                wait.Until(d => d.Url.Contains("/Staff") && !d.Url.Contains("/Delete"));
                Assert.Contains("/Staff", _driver.Url);
                Assert.DoesNotContain("/Delete", _driver.Url);
                await Task.Delay(_delayMs);

                // Step 19: Verify staff is removed from table
                Console.WriteLine("Step 19: Verifying staff is removed from table...");
                var updatedTableBody = _driver.FindElement(By.CssSelector("table tbody"));
                Assert.DoesNotContain("SEL001", updatedTableBody.Text);
                Console.WriteLine("✓ Staff successfully deleted!");
                await Task.Delay(_delayMs);

                Console.WriteLine("✓ All E2E tests completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during E2E test: {ex.Message}");
                throw;
            }
        }

        [Fact]
        public async Task E2E_ValidateEmailRegex_ClientSide()
        {
            try
            {
                // Navigate to Create page
                Console.WriteLine("Navigating to Create page...");
                _driver.Navigate().GoToUrl($"{_baseUrl}/Staff/Create");
                await Task.Delay(_delayMs);

                var emailInput = _driver.FindElement(By.Id("Email"));

                // Test various email formats
                var testCases = new[]
                {
                    ("valid.email@example.com", true),
                    ("test.user@domain.co.uk", true),
                    ("invalid-email", false),
                    ("invalid@", false),
                    ("@example.com", false),
                    ("invalid..email@example.com", false)
                };

                foreach (var (email, shouldBeValid) in testCases)
                {
                    Console.WriteLine($"Testing email: {email} (expected valid: {shouldBeValid})");
                    emailInput.Clear();
                    emailInput.SendKeys(email);
                    emailInput.SendKeys(Keys.Tab);
                    await Task.Delay(_delayMs);

                    var emailError = _driver.FindElements(By.CssSelector("span[data-valmsg-for='Email']"));
                    bool hasError = emailError.Count > 0 && emailError[0].Displayed && !string.IsNullOrEmpty(emailError[0].Text);

                    if (shouldBeValid)
                    {
                        Assert.False(hasError, $"Email '{email}' should be valid but has error");
                        Console.WriteLine($"  ✓ Valid email accepted: {email}");
                    }
                    else
                    {
                        Assert.True(hasError, $"Email '{email}' should be invalid but no error shown");
                        Console.WriteLine($"  ✓ Invalid email rejected: {email}");
                    }
                    await Task.Delay(_delayMs);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during email validation test: {ex.Message}");
                throw;
            }
        }

        [Fact]
        public async Task E2E_ValidatePhoneRegex_ClientSide()
        {
            try
            {
                // Navigate to Create page
                Console.WriteLine("Navigating to Create page...");
                _driver.Navigate().GoToUrl($"{_baseUrl}/Staff/Create");
                await Task.Delay(_delayMs);

                var phoneInput = _driver.FindElement(By.Id("PhoneNumber"));

                // Test various phone formats
                var testCases = new[]
                {
                    ("(123) 456-7890", true),
                    ("123-456-7890", true),
                    ("123.456.7890", true),
                    ("1234567890", true),
                    ("+1-123-456-7890", true),
                    ("invalid-phone", false),
                    ("123", false),
                    ("12345", false),
                    ("123-456", false)
                };

                foreach (var (phone, shouldBeValid) in testCases)
                {
                    Console.WriteLine($"Testing phone: {phone} (expected valid: {shouldBeValid})");
                    phoneInput.Clear();
                    phoneInput.SendKeys(phone);
                    phoneInput.SendKeys(Keys.Tab);
                    await Task.Delay(_delayMs);

                    var phoneError = _driver.FindElements(By.CssSelector("span[data-valmsg-for='PhoneNumber']"));
                    bool hasError = phoneError.Count > 0 && phoneError[0].Displayed && !string.IsNullOrEmpty(phoneError[0].Text);

                    if (shouldBeValid)
                    {
                        Assert.False(hasError, $"Phone '{phone}' should be valid but has error");
                        Console.WriteLine($"  ✓ Valid phone accepted: {phone}");
                    }
                    else
                    {
                        Assert.True(hasError, $"Phone '{phone}' should be invalid but no error shown");
                        Console.WriteLine($"  ✓ Invalid phone rejected: {phone}");
                    }
                    await Task.Delay(_delayMs);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during phone validation test: {ex.Message}");
                throw;
            }
        }

        public void Dispose()
        {
            _driver?.Quit();
            _driver?.Dispose();
        }
    }
}

