# HR Staff Management - Testing Tutorial

This project contains a comprehensive test suite for the HR Staff Management web application. The test suite includes unit tests, integration tests, and end-to-end (E2E) UI tests using Selenium.

## 📋 Table of Contents

- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Test Types](#test-types)
- [Running Tests](#running-tests)
- [Test Details](#test-details)
- [Troubleshooting](#troubleshooting)

## 🏗️ Project Structure

```
Dotnet-MVC/
├── HRStaffManagement.csproj          # Main web application
├── HRStaffManagement.Tests/           # Test project
│   ├── StaffModelValidationTests.cs  # Unit tests for model validation
│   ├── StaffControllerIntegrationTests.cs  # Integration tests
│   └── StaffSeleniumE2ETests.cs      # Selenium E2E tests
└── FinalLab.sln                      # Solution file
```

## 📦 Prerequisites

Before running the tests, ensure you have:

1. **.NET 8.0 SDK** or later
   - Download from: https://dotnet.microsoft.com/download

2. **Chrome Browser** (for Selenium tests)
   - ChromeDriver is automatically managed via NuGet package

3. **Visual Studio 2022** or **VS Code** (optional, for IDE support)

## 🧪 Test Types

### 1. Unit Tests (`StaffModelValidationTests.cs`)

**Purpose**: Tests the Staff model validation logic, specifically email and phone number regex patterns.

**What it tests**:
- ✅ Valid email formats (e.g., `john.doe@example.com`)
- ❌ Invalid email formats (e.g., `invalid-email`, `@example.com`)
- ✅ Valid phone formats (e.g., `(123) 456-7890`, `123-456-7890`)
- ❌ Invalid phone formats (e.g., `invalid-phone`, `123`)
- Required field validation
- String length validation

**Location**: `HRStaffManagement.Tests/StaffModelValidationTests.cs`

### 2. Integration Tests (`StaffControllerIntegrationTests.cs`)

**Purpose**: Tests the controller layer and API endpoints using `WebApplicationFactory`.

**What it tests**:
- GET endpoints (Index, Create, Details, Edit, Delete)
- POST endpoints with valid/invalid data
- Form validation errors
- Redirects and status codes
- Database operations (using in-memory database)

**Location**: `HRStaffManagement.Tests/StaffControllerIntegrationTests.cs`

### 3. End-to-End Tests (`StaffSeleniumE2ETests.cs`)

**Purpose**: Tests the complete user workflow through the web interface using Selenium WebDriver.

**What it tests**:
- Complete staff creation workflow
- Client-side validation (email/phone regex)
- Form submission and data persistence
- Staff deletion workflow
- UI element interactions

**Features**:
- ⏱️ **Slow execution** (2-second delays) for visibility
- 🖥️ **Visible browser** (not headless) so you can watch the automation
- 🧹 **Auto cleanup** - automatically deletes test data after completion

**Location**: `HRStaffManagement.Tests/StaffSeleniumE2ETests.cs`

## 🚀 Running Tests

### Run All Tests

```bash
dotnet test FinalLab.sln
```

### Run Specific Test Classes

**Unit Tests Only:**
```bash
dotnet test --filter "FullyQualifiedName~StaffModelValidationTests"
```

**Integration Tests Only:**
```bash
dotnet test --filter "FullyQualifiedName~StaffControllerIntegrationTests"
```

**Selenium E2E Tests Only:**
```bash
dotnet test --filter "FullyQualifiedName~StaffSeleniumE2ETests"
```

### Run Specific Test Methods

```bash
# Run only email validation tests
dotnet test --filter "FullyQualifiedName~Staff_ValidEmail"

# Run only the main E2E workflow test
dotnet test --filter "FullyQualifiedName~E2E_CreateStaff_ValidateFormat_DeleteStaff"
```

### Run Tests with Verbose Output

```bash
dotnet test FinalLab.sln --verbosity normal
```

### Run Tests in Visual Studio

1. Open `FinalLab.sln` in Visual Studio
2. Open Test Explorer (Test → Test Explorer)
3. Click "Run All" or select specific tests to run

## 📖 Test Details

### Unit Tests - Email Validation

The email validation uses the following regex pattern:
```regex
^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$
```

**Valid Examples:**
- `john.doe@example.com`
- `test.user@domain.co.uk`
- `user+tag@example.com`

**Invalid Examples:**
- `invalid-email` (no @ symbol)
- `invalid@` (incomplete domain)
- `@example.com` (missing local part)

### Unit Tests - Phone Validation

The phone validation uses the following regex pattern:
```regex
^(\+?[0-9]{1,3}[-.\s]?)?\(?[0-9]{3}\)?[-.\s]?[0-9]{3}[-.\s]?[0-9]{4}$
```

**Valid Examples:**
- `(123) 456-7890`
- `123-456-7890`
- `123.456.7890`
- `1234567890`
- `+1-123-456-7890`

**Invalid Examples:**
- `invalid-phone` (contains letters)
- `123` (too short)
- `123-456` (incomplete)

### Integration Tests - Test Database

Integration tests use an **in-memory database** that is:
- Created fresh for each test
- Isolated from other tests
- Automatically cleaned up after each test

This ensures tests don't interfere with each other or your development database.

### Selenium E2E Tests - Workflow

The main E2E test (`E2E_CreateStaff_ValidateFormat_DeleteStaff`) performs the following steps:

1. **Navigate** to Staff Index page
2. **Click** "Add New Staff" button
3. **Fill in** all form fields:
   - Staff ID: `SEL001`
   - Staff Name: `Selenium Test User`
   - Email: `selenium.test@example.com`
   - Phone: `(123) 456-7890`
   - Starting Date: Today's date
4. **Test invalid email** format and verify error message
5. **Fix email** to valid format
6. **Test invalid phone** format and verify error message
7. **Fix phone** to valid format
8. **Submit** the form
9. **Verify** staff appears in the table
10. **Find and click** Delete button for the created staff
11. **Confirm** deletion
12. **Verify** staff is removed from table

**Note**: Each step has a 2-second delay so you can watch the automation in action!

## 🔧 Configuration

### Adjusting Selenium Test Speed

To change the delay between actions in Selenium tests, edit `StaffSeleniumE2ETests.cs`:

```csharp
private readonly int _delayMs = 2000; // Change this value (in milliseconds)
```

- `1000` = 1 second delay
- `2000` = 2 seconds delay (default)
- `500` = 0.5 seconds delay

### Running Selenium Tests Headless

To run Selenium tests in headless mode (browser not visible), edit `StaffSeleniumE2ETests.cs`:

```csharp
var options = new ChromeOptions();
options.AddArgument("--start-maximized");
options.AddArgument("--headless"); // Uncomment this line
```

## 🐛 Troubleshooting

### Tests Fail to Build

**Error**: `The type or namespace name 'Xunit' could not be found`

**Solution**: Restore NuGet packages
```bash
dotnet restore FinalLab.sln
```

### Selenium Tests Fail - ChromeDriver Not Found

**Error**: `ChromeDriver executable not found`

**Solution**: The ChromeDriver NuGet package should handle this automatically. If issues persist:
1. Ensure Chrome browser is installed
2. Try updating the `Selenium.WebDriver.ChromeDriver` package

### Integration Tests Fail - Database Issues

**Error**: Database connection errors

**Solution**: Integration tests use in-memory databases, so this shouldn't happen. If it does:
1. Ensure `Microsoft.EntityFrameworkCore.InMemory` package is installed
2. Check that the test setup correctly replaces the database configuration

### Selenium Tests Timeout

**Error**: Test timeout waiting for elements

**Solution**: 
1. Ensure the web application is running (if testing against a live server)
2. Check that the base URL in `StaffSeleniumE2ETests.cs` is correct
3. Increase wait times if needed

### Tests Run Too Fast/Slow

**Solution**: Adjust the `_delayMs` value in `StaffSeleniumE2ETests.cs` as described in the [Configuration](#configuration) section.

## 📚 Additional Resources

- [xUnit Documentation](https://xunit.net/)
- [ASP.NET Core Integration Testing](https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests)
- [Selenium WebDriver Documentation](https://www.selenium.dev/documentation/webdriver/)
- [Entity Framework Core In-Memory Database](https://docs.microsoft.com/en-us/ef/core/providers/in-memory/)

## 📝 Test Coverage

The test suite covers:

- ✅ **Model Layer**: Validation rules (email, phone, required fields, length)
- ✅ **Controller Layer**: All CRUD operations, error handling, redirects
- ✅ **UI Layer**: Form interactions, client-side validation, user workflows

## 🎯 Best Practices Demonstrated

1. **Isolation**: Each test is independent and doesn't rely on other tests
2. **Cleanup**: Tests clean up after themselves (especially E2E tests)
3. **Readability**: Tests are well-named and organized
4. **Maintainability**: Tests use clear patterns and helper methods
5. **Realistic Data**: Tests use realistic test data

## 🤝 Contributing

When adding new tests:

1. Follow the existing naming conventions
2. Ensure tests are isolated and can run independently
3. Add appropriate delays to Selenium tests for visibility
4. Clean up any test data created during tests
5. Document any new test patterns or requirements

## 📄 License

This project is part of the FinalLab solution for educational purposes.

---

**Happy Testing! 🧪✨**

