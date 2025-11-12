using System.Threading.Tasks;
using Xunit;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Microsoft.AspNetCore.Mvc.Testing;  // required for WebApplicationFactory

namespace HRStaffManagement.Tests
{
    public class StaffEditTests : StaffTestBase
    {
        public StaffEditTests(WebApplicationFactory<Program> factory) : base(factory) { }

        public static IEnumerable<object[]> GetExcelData() => ReadExcelData("EditStaff");

        [Theory]
        [MemberData(nameof(GetExcelData))]
        public async Task EditStaff_Test(
            string testName, string existingId, string newName, string newEmail,
            string newPhone, string newDate, string photoFile, string expected)
        {
            using var driver = StartBrowser();
            driver.Navigate().GoToUrl($"{BaseUrl}/Staff/Edit/{existingId}");
            await Task.Delay(DelayMs);

            driver.FindElement(By.Id("StaffName")).Clear();
            driver.FindElement(By.Id("StaffName")).SendKeys(newName);
            driver.FindElement(By.Id("Email")).Clear();
            driver.FindElement(By.Id("Email")).SendKeys(newEmail);
            driver.FindElement(By.Id("PhoneNumber")).Clear();
            driver.FindElement(By.Id("PhoneNumber")).SendKeys(newPhone);
            driver.FindElement(By.Id("StartingDate")).Clear();
            driver.FindElement(By.Id("StartingDate")).SendKeys(newDate);

            if (!string.IsNullOrWhiteSpace(photoFile))
            {
                string projectRoot = System.IO.Path.Combine(System.IO.Directory.GetParent(System.IO.Directory.GetCurrentDirectory()).Parent.Parent.FullName);
                string testImagePath = System.IO.Path.Combine(projectRoot, "TestData", photoFile);
                driver.FindElement(By.Id("Photo")).SendKeys(testImagePath);
            }

            driver.FindElement(By.CssSelector("button[type='submit']")).Click();
            await Task.Delay(DelayMs * 2);

            if (expected == "Success")
                Assert.Contains(newName, driver.PageSource);

            driver.Quit();
        }
    }
}
