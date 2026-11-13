# SMS.DB Coding Standards

## Overview
This document defines the coding standards for the Data Access Layer (DB).

## Project Structure
```
SMS.DB/
├── ApplicationDbContext.cs
├── Migrations/
│   ├── 20240101000000_InitialMigration.cs
│   └── ...
└── (Repositories)
```

## DbContext Configuration

### Single DbContext
- Use single `ApplicationDbContext` for entire application
- Register all DbSets in one place

```csharp
namespace SMS.DB;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Student> Students { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<AcademicClass> AcademicClasses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // Fluent API configurations
    }
}
```

## Migration Naming Convention
- Format: `YYYYMMDDHHMMSS_MigrationName.cs`
- Example: `20240101000000_InitialMigration.cs`
- Always add descriptive migration name

```csharp
public partial class InitialMigration : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Students",
            columns: table => new
            {
                Id = table.Column<int(nullable: false)
                    .Annotation("SqlServer:Identity", "1, 1"),
                Name = table.Column<string(maxLength: 100, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Students", x => x.Id);
            });
    }
}
```

## Fluent API Configuration Order
1. Primary keys
2. Required/optional columns
3. String length constraints
4. Default values
5. Foreign keys
6. Indexes
7. Unique constraints

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<Student>(entity =>
    {
        entity.HasKey(e => e.Id);
        entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
        entity.HasOne(e => e.AcademicClass)
              .WithMany()
              .HasForeignKey(e => e.AcademicClassId);
    });
}
```

## Relationship Conventions
- Use `HasOne` / `WithMany` for one-to-many
- Use `HasMany` / `WithOne` for many-to-one
- Use `HasMany` / `WithMany` for many-to-many with join table

## Index Configuration
```csharp
entity.HasIndex(e => e.UniqueId).IsUnique();
entity.HasIndex(e => new { e.AcademicClassId, e.ClassRoll });
```

## Query Filter (Soft Delete/Global Filter)
```csharp
modelBuilder.Entity<Student>().HasQueryFilter(s => s.IsActive);
```

## Notes
- Never hardcode connection strings
- Use dependency injection for DbContext
- Use migrations for schema changes
- Keep migrations small and focused
- Add comments for complex migrations
- Use `nullable: false` for required fields