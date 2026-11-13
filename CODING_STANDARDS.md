# EIMS (School Management System) - Project Coding Standards

## Architecture Overview

This project follows a **Layered Architecture** pattern:

```
┌─────────────────────────────────────┐
│         SMS_App (MVC)               │  Presentation Layer
├─────────────────────────────────────┤
│             SMS.BLL                  │  Business Logic Layer
├─────────────────────────────────────┤
│    SMS.DAL (Repository Pattern)     │  Data Access Layer
├─────────────────────────────────────┤
│             SMS.DB                  │  Database/EF Core
├─────────────────────────────────────┤
│           SMS.Entities              │  Domain Entities
├─────────────────────────────────────┤
│          SMS.Frameworks             │  Utilities/Helpers
└─────────────────────────────────────┘
```

## Layer Responsibilities

| Layer | Responsibility |
|-------|----------------|
| **SMS.Entities** | Domain models, Enums, ViewModels |
| **SMS.DB** | DbContext, Migrations, Database configuration |
| **SMS.BLL** | Business logic, Manager classes, Interfaces |
| **SMS.Frameworks** | Reusable helpers, TagHelpers, Extensions |
| **SMS_App** | Controllers, Views, ViewModels, Configuration |

## Project Dependencies

```
SMS_App → SMS.BLL, SMS.Entities, SMS.Frameworks
SMS.BLL → SMS.DAL, SMS.Entities
SMS.DB → SMS.Entities
SMS.Frameworks → (minimal dependencies)
```

## General Coding Conventions

### Naming
- **Classes/Interfaces**: PascalCase (e.g., `StudentManager`, `IStudentManager`)
- **Methods**: PascalCase (e.g., `GetStudentByIdAsync`)
- **Properties**: PascalCase (e.g., `StudentName`, `AcademicClassId`)
- **Private Fields**: _camelCase (e.g., `_studentRepository`)
- **Constants**: PascalCase (e.g., `DefaultPageSize`)

### File Organization
- One class per file
- File name matches class name
- Use folders/namespaces to organize by feature

### Async/Await
- Use `async`/`await` for all I/O operations
- Suffix async methods with `Async`
- Return `Task<T>` for async methods

### Error Handling
- Use try-catch for expected exceptions
- Throw meaningful custom exceptions
- Log errors appropriately

## Layer-Specific Standards

- [SMS.Entities Coding Standards](SMS.Entities/CODING_STANDARDS.md)
- [SMS.DB Coding Standards](SMS.DB/CODING_STANDARDS.md)
- [SMS.BLL Coding Standards](SMS.BLL/CODING_STANDARDS.md)
- [SMS.Frameworks Coding Standards](SMS.Frameworks/CODING_STANDARDS.md)
- [SMS_App Coding Standards](SMS_App/CODING_STANDARDS.md)

## Git Branch Strategy

- `main` - Production code
- `develop` - Development code
- `feature/feature-name` - Feature branches

## Code Review Checklist

- [ ] Follows naming conventions
- [ ] Proper error handling
- [ ] Async/await used correctly
- [ ] No hardcoded values
- [ ] Dependencies injected properly
- [ ] Code is DRY (Don't Repeat Yourself)
- [ ] Proper logging
- [ ] Security best practices followed

## Database Conventions

- Use migrations for all schema changes
- Add meaningful migration descriptions
- Use foreign key constraints
- Use appropriate data types (decimal for money, int for IDs)
- Add indexes for frequently queried columns

## API Design

- Use RESTful conventions
- Return appropriate HTTP status codes
- Use `[ApiController]` attribute
- Document endpoints with comments

## Notes
- Always run lint/typecheck before committing
- Write meaningful commit messages
- Keep methods small and focused
- Follow SOLID principles