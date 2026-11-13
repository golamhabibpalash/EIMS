# SMS.Entities Coding Standards

## Overview
This document defines the coding standards for the Entities layer (Domain Models).

## Class Naming Conventions
- Use PascalCase for class names
- Entity classes should be named after the table they represent (e.g., `Student`, `Employee`, `AcademicClass`)
- ViewModels/DTOs should be suffixed with appropriate suffix:
  - `VM` - ViewModel
  - `DTO` - Data Transfer Object
  - `Enum` - Enumerations

## File Organization
```
SMS.Entities/
├── Student.cs
├── Employee.cs
├── Enums/
│   ├── StudentPaymentStatus.cs
│   └── ExamCategory.cs
├── AdditionalModels/
│   ├── StudentListVM.cs
│   └── StudentPaymentSummery.cs
└── RptModels/
    ├── AttendanceVM/
    └── Results/
```

## Entity Class Structure
```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SMS.Entities;

public class Student : CommonProps
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; }

    [ForeignKey(nameof(AcademicClass))]
    public int AcademicClassId { get; set; }

    public virtual AcademicClass AcademicClass { get; set; }
}
```

## Common Props Usage
All entities should inherit from `CommonProps` for audit fields:
```csharp
public class Student : CommonProps
{
    // ... properties
}
```

## Enumeration Structure
```csharp
namespace SMS.Entities.Enums;

public enum StudentPaymentStatus
{
    Unpaid = 0,
    Partial = 1,
    Paid = 2
}
```

## Property Naming
- Use PascalCase for all properties
- ForeignKey properties should end with `Id` (e.g., `AcademicClassId`)
- Navigation properties should be named after the related entity (e.g., `AcademicClass`)
- Boolean properties should use `Is` prefix (e.g., `IsActive`, `IsResidential`)

## Data Annotations
- Use `[Required]` for mandatory fields
- Use `[StringLength(max)]` for string limitations
- Use `[EmailAddress]` for email validation
- Use `[Phone]` for phone number validation
- Use `[ForeignKey]` to explicitly define relationships
- Use `[NotMapped]` for properties not stored in database

## ViewModel/DTO Guidelines
- Keep ViewModels flat (no navigation properties unless necessary)
- Use separate ViewModels for different views
- Include only necessary properties for the specific use case

## Notes
- All entities must have `Id` as primary key
- Use `int` for ID unless specifically required otherwise
- Avoid using `string` for foreign keys - use integers
- All DateTime properties should use `DateTime` type
- For monetary values, use `decimal` instead of `double`