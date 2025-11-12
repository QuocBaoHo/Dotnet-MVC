using System.Threading.Tasks;
using Xunit;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Microsoft.AspNetCore.Mvc.Testing;  // required for WebApplicationFactory

namespace HRStaffManagement.Tests
{
    public class StaffDeleteTests : StaffTestBase
    {
        public StaffDeleteTests(WebApplicationFactory<Program> factory) : base(factory) { }

        public static IEnumerable<object[]> GetExcelData() => ReadExcelData("DeleteStaff");

        [Theory]
        [MemberData(nameof(GetExcelData))]
        public async Task DeleteStaff_Test(
            string testName, string staffId, string ignore1, string ignore2,
            string ignore3, string ignore4, string ignore5, string expected)
        {
            using var driver = StartBrowser();
            driver.Navigate().GoToUrl($"{BaseUrl}/Staff/Delete/{staffId}");
            await Task.Delay(DelayMs);

            if (expected == "Success")
                driver.FindElement(By.Id("DeleteButton")).Click();

            await Task.Delay(DelayMs);
            driver.Quit();
        }


    }
}
