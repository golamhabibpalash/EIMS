# SMS.BLL Coding Standards

## Overview
This document defines the coding standards for the Business Logic Layer (BLL).

## Project Structure
```
SMS.BLL/
├── Contracts/              # Interfaces
│   ├── Base/
│   │   └── IManager.cs
│   └── IStudentManager.cs
├── Managers/               # Implementations
│   ├── Base/
│   │   └── Manager.cs
│   └── StudentManager.cs
└── Services/              # Additional services (if needed)
```

## Interface Naming
- Use `I` prefix for all interfaces
- Suffix with `Manager` or `Service` depending on purpose
- Example: `IStudentManager`, `IAcademicSessionManager`

```csharp
namespace SMS.BLL.Contracts;

public interface IStudentManager : IManager<Student>
{
    Task<Student> GetStudentByClassRollAsync(int classRoll);
    Task<List<Student>> GetStudentsByClassIdAsync(int classId);
}
```

## Manager Class Structure
- Inherit from `Manager<T>` base class
- Implement corresponding interface
- Inject dependencies through constructor
- Use `_` prefix for private fields

```csharp
namespace SMS.BLL.Managers;

public class StudentManager : Manager<Student>, IStudentManager
{
    private readonly IStudentRepository _studentRepository;

    public StudentManager(IStudentRepository studentRepository) : base(studentRepository)
    {
        _studentRepository = studentRepository;
    }

    public async Task<Student> GetStudentByClassRollAsync(int classRoll)
    {
        return await _studentRepository.GetStudentByClassRollAsync(classRoll);
    }
}
```

## Method Naming Conventions
- Use PascalCase for method names
- Async methods should have `Async` suffix
- Get methods: `GetByIdAsync`, `GetAllAsync`, `GetByConditionAsync`
- Add methods: `AddAsync`, `AddRangeAsync`
- Update methods: `UpdateAsync`
- Delete methods: `RemoveAsync`, `DeleteAsync`

## Return Types
- Always use `Task<T>` for async operations
- Use `IReadOnlyCollection<T>` for collections (read-only)
- Use `List<T>` when modification is needed
- Use `bool` for operation success/failure

## Dependency Injection
- Inject through constructor
- Follow single responsibility principle
- Use interface injection over concrete classes

## Error Handling
- Throw custom exceptions with meaningful messages
- Log errors appropriately
- Return null or empty collections instead of throwing for not-found scenarios

## Best Practices
1. Keep managers focused on single entity type
2. Use repository for data access, not direct DB context
3. Validate input parameters at manager level
4. Use async/await for all I/O operations
5. Return domain entities, not DTOs (use mapper in presentation layer)

## Common Base Interface
```csharp
public interface IManager<T> where T : class
{
    Task<bool> AddAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> RemoveAsync(T entity);
    Task<T> GetByIdAsync(int id);
    Task<IReadOnlyCollection<T>> GetAllAsync();
    Task<bool> IsExistAsync(T entity);
}
```

## Notes
- All managers must implement `IManager<T>` interface
- Use `base(repository)` in constructor
- Avoid business logic in constructors
- Keep methods small and focused