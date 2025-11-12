using System.Threading.Tasks;
using Xunit;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Microsoft.AspNetCore.Mvc.Testing;  // required for WebApplicationFactory

namespace HRStaffManagement.Tests
{
    public class StaffDetailsTests : StaffTestBase
    {
        public StaffDetailsTests(WebApplicationFactory<Program> factory) : base(factory) { }

        public static IEnumerable<object[]> GetExcelData() => ReadExcelData("DetailsStaff");

        [Theory]
        [MemberData(nameof(GetExcelData))]
        public async Task DetailsStaff_Test(
            string testName, string staffId, string ignore1, string ignore2,
            string ignore3, string ignore4, string ignore5, string expected)
        {
            using var driver = StartBrowser();
            driver.Navigate().GoToUrl($"{BaseUrl}/Staff/Details/{staffId}");
            await Task.Delay(DelayMs);

            if (expected == "Success")
                Assert.Contains(staffId, driver.PageSource);
            else if (expected == "NotFound")
                Assert.Contains("Not Found", driver.PageSource, System.StringComparison.OrdinalIgnoreCase);

            driver.Quit();
        }
    }
}
