# SMS.Frameworks Coding Standards

## Overview
This document defines the coding standards for the Frameworks layer (Utilities, Helpers, Tag Helpers).

## Project Structure
```
SMS.Frameworks/
├── SMSTagHelper/
│   └── MyCustomTagHelper.cs
├── Helpers/
│   └── FileHelper.cs
├── Extensions/
│   └── StringExtensions.cs
└── (Other utilities)
```

## Purpose
This layer should contain:
- Reusable UI components (Tag Helpers)
- Helper classes
- Extension methods
- Utility functions
- Common functionality used across layers

## Tag Helper Naming
- Suffix with `TagHelper`
- Use proper attribute naming with hyphen case

```csharp
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SMS.Frameworks.SMSTagHelper;

[HtmlTargetElement("student-badge")]
public class StudentBadgeTagHelper : TagHelper
{
    public string Status { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.TagName = "span";
        output.Content.SetContent(Status);
        output.Attributes.Add("class", "badge badge-primary");
    }
}
```

## Extension Methods
- Place in `Extensions` folder
- Suffix class name with `Extensions`
- Make methods static

```csharp
namespace SMS.Frameworks.Extensions;

public static class StringExtensions
{
    public static string ToTitleCase(this string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
    }
}
```

## Helper Classes
- Create static helper classes for utility functions
- Use meaningful class and method names

```csharp
namespace SMS.Frameworks.Helpers;

public static class FileHelper
{
    public static async Task<string> SaveFileAsync(IFormFile file, string folder)
    {
        // File saving logic
    }
}
```

## Guidelines
1. Keep framework code general and reusable
2. Avoid dependencies on specific entities
3. Use interfaces where possible
4. Document complex utility methods
5. Keep methods focused and single-purpose

## Notes
- This layer should have minimal dependencies
- Avoid circular references
- Frameworks should be referenced by App layer, not Entities
- Use for truly reusable components only