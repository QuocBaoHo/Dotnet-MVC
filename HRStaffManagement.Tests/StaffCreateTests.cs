using System.Threading.Tasks;
using Xunit;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Microsoft.AspNetCore.Mvc.Testing;
  // required for WebApplicationFactory
namespace HRStaffManagement.Tests
{
    public class StaffCreateTests : StaffTestBase
    {
        public StaffCreateTests(WebApplicationFactory<Program> factory) : base(factory) { }

        public static IEnumerable<object[]> GetExcelData() => ReadExcelData("CreateStaff");

        [Theory]
        [MemberData(nameof(GetExcelData))]
        public async Task CreateStaff_Test(
            string testName, string staffId, string staffName, string email,
            string phone, string date, string photoFile, string expected)
        {
            using var driver = StartBrowser();
            driver.Navigate().GoToUrl($"{BaseUrl}/Staff/Create");
            await Task.Delay(DelayMs);

            if (!string.IsNullOrWhiteSpace(staffId))
                driver.FindElement(By.Id("StaffId")).SendKeys(staffId);
            if (!string.IsNullOrWhiteSpace(staffName))
                driver.FindElement(By.Id("StaffName")).SendKeys(staffName);
            if (!string.IsNullOrWhiteSpace(email))
                driver.FindElement(By.Id("Email")).SendKeys(email);
            if (!string.IsNullOrWhiteSpace(phone))
                driver.FindElement(By.Id("PhoneNumber")).SendKeys(phone);
            if (!string.IsNullOrWhiteSpace(date))
                driver.FindElement(By.Id("StartingDate")).SendKeys(date);

            if (!string.IsNullOrWhiteSpace(photoFile))
            {
                string projectRoot = System.IO.Path.Combine(System.IO.Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName);
                string testImagePath = System.IO.Path.Combine(projectRoot, "TestData", photoFile);
                driver.FindElement(By.Id("Photo")).SendKeys(testImagePath);
            }

            driver.FindElement(By.CssSelector("button[type='submit']")).Click();
            await Task.Delay(DelayMs * 2);

            var wait = new WebDriverWait(driver, System.TimeSpan.FromSeconds(10));

            switch (expected)
            {
                case "Success":
                    wait.Until(d => d.Url.Contains("/") && !d.Url.Contains("/Create"));
                    Assert.Contains(staffName, driver.PageSource);
                    break;
                case "EmailError":
                    Assert.True(driver.PageSource.Contains("valid email"));
                    break;
                case "PhoneError":
                    Assert.True(driver.PageSource.Contains("valid phone"));
                    break;
                case "StaffIdError":
                    Assert.True(driver.PageSource.Contains("Staff ID is required"));
                    break;
            }

            driver.Quit();
        }
    }
}
