# SMS_App (MVC) Coding Standards

## Overview
This document defines the coding standards for the Presentation Layer (ASP.NET Core MVC).

## Project Structure
```
SMS_App/
├── Controllers/
│   ├── Api/                    # API Controllers
│   ├── StudentsController.cs
│   └── HomeController.cs
├── Views/
│   ├── Shared/
│   ├── Students/
│   └── _ViewImports.cshtml
├── ViewModels/
│   ├── Students/
│   └── Common/
├── Models/
├── wwwroot/
│   ├── css/
│   ├── js/
│   └── lib/
├── Configurations/
│   ├── ServiceExtensions.cs
│   └── DependencyInjectionConfiguration.cs
└── Program.cs
```

## Controller Naming
- Use plural form: `StudentsController`, not `StudentController`
- Suffix with `Controller`
- API controllers should use `Controller` suffix or have `Api` in namespace

```csharp
namespace SMS_App.Controllers;

public class StudentsController : Controller
{
    private readonly IStudentManager _studentManager;

    public StudentsController(IStudentManager studentManager)
    {
        _studentManager = studentManager;
    }

    public async Task<IActionResult> Index()
    {
        var students = await _studentManager.GetAllAsync();
        return View(students);
    }
}
```

## Action Method Naming
- GET actions: `Index`, `Details`, `Create`, `Edit`, `Delete`
- POST actions: `Create`, `Edit`, `Delete` (with appropriate model binding)
- Use `[HttpGet]` and `[HttpPost]` attributes explicitly

```csharp
[HttpGet]
public async Task<IActionResult> Create()
{
    return View();
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(Student model)
{
    if (!ModelState.IsValid) return View(model);
    await _studentManager.AddAsync(model);
    return RedirectToAction(nameof(Index));
}
```

## ViewModels Guidelines
- Create separate ViewModels for each view
- Use meaningful names: `StudentCreateVM`, `StudentEditVM`
- Include only properties needed for the specific view

```csharp
namespace SMS_App.ViewModels.Students;

public class StudentCreateVM
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [Required]
    public int AcademicClassId { get; set; }

    public IEnumerable<SelectListItem> AcademicClasses { get; set; }
}
```

## View Folder Structure
- Match controller name: `StudentsController` → `Views/Students/`
- Use `_` prefix for partial views: `_StudentForm.cshtml`
- Use `_` prefix for shared views in `Views/Shared/`

## Routing Conventions
- Use attribute routing for API controllers
- Use conventional routing for MVC controllers

```csharp
[Route("api/[controller]")]
[ApiController]
public class StudentsApiController : ControllerBase
{
    // API endpoints
}
```

## Dependency Injection
- Inject services through constructor
- Use interfaces: `IStudentManager`, not `StudentManager`
- Register services in `Program.cs` or `ServiceExtensions.cs`

## Best Practices
1. Use ViewModels instead of Entity models in views
2. Validate models on both client and server side
3. Use `[ValidateAntiForgeryToken]` for POST requests
4. Return appropriate HTTP status codes in API
5. Use ` IActionResult` for flexible responses
6. Keep controllers thin, delegate to managers
7. Use `TempData` for flash messages

## View Import
```csharp
@using SMS_App
@using SMS_App.ViewModels
@using SMS.Entities
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

## Notes
- Avoid business logic in controllers
- Use AutoMapper for entity-to-viewmodel mapping
- Return `NotFound()` for non-existent entities
- Use `BadRequest()` for invalid model state