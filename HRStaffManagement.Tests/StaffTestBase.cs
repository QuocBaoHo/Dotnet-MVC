using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using HRStaffManagement.Models;
using Microsoft.AspNetCore.Hosting;

namespace HRStaffManagement.Tests
{
    public abstract class StaffTestBase : IClassFixture<WebApplicationFactory<Program>>
    {
        protected readonly string BaseUrl = "http://localhost:5000";
        protected readonly int DelayMs = 1000;
        protected readonly WebApplicationFactory<Program> Factory;

        protected StaffTestBase(WebApplicationFactory<Program> factory)
        {
            Factory = factory.WithWebHostBuilder(builder =>
            {
                // Set environment to Testing
                builder.UseEnvironment("Testing");

                builder.ConfigureServices(services =>
                {
                    // Remove existing DbContext
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Add in-memory database
                    services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseInMemoryDatabase("ExcelSeleniumDb_" + Guid.NewGuid()));

                    // Disable Anti-Forgery for tests
                    services.AddControllersWithViews(options =>
                    {
                        options.Filters.Add(new Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryTokenAttribute());
                    });
                });
            });
        }

        protected IWebDriver StartBrowser()
        {
            var options = new ChromeOptions();
            options.AddArgument("--start-maximized");
            // options.AddArgument("--headless"); // optional
            return new ChromeDriver(options);
        }

        protected static IEnumerable<object[]> ReadExcelData(string sheetName)
        {
            string projectRoot = Path.Combine(Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName);
            string excelPath = Path.Combine(projectRoot, "TestData", "StaffTests.xlsx");

            using var workbook = new XLWorkbook(excelPath);
            var sheet = workbook.Worksheet(sheetName);

            foreach (var row in sheet.RowsUsed().Skip(1))
            {
                string rawDate = row.Cell(6).GetString();
                if (rawDate.Contains(" "))
                    rawDate = rawDate.Split(' ')[0];

                yield return new object[]
                {
                    row.Cell(1).GetString(),
                    row.Cell(2).GetString(),
                    row.Cell(3).GetString(),
                    row.Cell(4).GetString(),
                    row.Cell(5).GetString(),
                    rawDate,
                    row.Cell(7).GetString(),
                    row.Cell(8).GetString()
                };
            }
        }
    }
}
