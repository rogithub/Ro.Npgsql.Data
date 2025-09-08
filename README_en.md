# Ro.Npgsql.Data

`Ro.Npgsql.Data` is a lightweight, asynchronous library for data access with PostgreSQL in .NET applications. Its goal is to simplify the execution of SQL commands, parameter management, and data reading by providing a set of extension methods and helpers.

## Features

-   Fully asynchronous database operations.
-   Extension methods to fluently create `DbCommand` and `IDbDataParameter`.
-   Helpers to safely read and convert data types from an `IDataReader`.
-   Designed to be injected via a Dependency Injection (DI) container using the `IDbAsync` interface.

## Installation

To add this library to your project, you can install it from NuGet.

```bash
dotnet add package Ro.Npgsql.Data
```

Or you can add the reference directly in your `.csproj` file:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">  
  ...
  <ItemGroup>
    <PackageReference Include="Ro.Npgsql.Data" Version="0.0.7" />
  </ItemGroup>
  ...
</Project>
```

## Usage Guide: Creating a Product Repository

This guide will show you how to use `Ro.Npgsql.Data` to build a complete data repository for a `Product` entity.

### 1. Configuration in `Program.cs`

First, register the `IDbAsync` implementation in your service container. The `Database` implementation receives the connection string for your PostgreSQL database.

```csharp
// Program.cs
using Ro.Npgsql.Data;

var builder = WebApplication.CreateBuilder(args);

// ... other services

// Connection string configuration
var pg_host = builder.Configuration.GetSection("PostgresDb:Host").Value;
var pg_port = builder.Configuration.GetSection("PostgresDb:Port").Value;
var pg_db = builder.Configuration.GetSection("PostgresDb:Database").Value;
var pg_user = builder.Configuration.GetSection("PostgresDb:Username").Value;
var pg_pass = builder.Configuration.GetSection("PostgresDb:Password").Value;

string connString = $"Host={pg_host};Port={pg_port};Database={pg_db};Username={pg_user};Password={pg_pass};";

// Register IDbAsync for dependency injection
builder.Services.AddTransient<IDbAsync>((svc) =>
{
    return new Database(connString);
});

// ... rest of the configuration
```

### 2. The `Product` Entity and Repository Interface

Define your model and the interface you will implement. The `Description` property is nullable to demonstrate null handling.

```csharp
public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? Description { get; set; } // Nullable field
}

public interface IProductsRepo
{
    Task<Product?> GetById(Guid id);
    Task<IEnumerable<Product>> GetAll();
    Task<int> Create(Product product);
    Task<int> Update(Product product);
    Task<int> Delete(Guid id);
    Task<int> GetTotalStock();
}
```

### 3. Repository Implementation

Now, create the `ProductsRepo` class that implements the interface.

#### 3.1. Injection and Data Mapping

Inject `IDbAsync` in the constructor. The `MapProduct` method will handle converting data from `IDataReader` to our `Product` object.

-   **`dr.GetString("Name")`**: Extension methods for safely reading data types.
-   **`dr.FromDb<string?>("Description")`**: A generic method to handle columns that can be `DBNull.Value`, especially useful for `Nullable` types.

```csharp
using Ro.Npgsql.Data;
using System.Data;

public class ProductsRepo : IProductsRepo
{
    private readonly IDbAsync _db;

    public ProductsRepo(IDbAsync db)
    {
        _db = db;
    }

    private Product MapProduct(IDataReader dr)
    {
        return new Product()
        {
            Id = dr.GetGuid("Id"),
            Name = dr.GetString("Name"),
            Price = dr.GetDecimal("Price"),
            Stock = dr.GetInt("Stock"),
            Description = dr.FromDb<string?>("Description") // Null handling
        };
    }
    
    // ... CRUD method implementations
}
```

#### 3.2. Creating Commands and Parameters (`ToCmd` and `ToParam`)

To create commands, use the `ToCmd()` and `ToParam()` extension methods. This makes the code cleaner and more readable.

-   **`sql.ToCmd(...)`**: Converts an SQL string into a `DbCommand`.
-   **`value.ToParam("@name")`**: Converts a value (string, int, Guid, etc.) into an `IDbDataParameter`.

#### 3.3. CRUD Method Implementation

**Read (Get one and get all)**

-   **`GetOneRow`**: Retrieves a single record. Returns `null` if not found.
-   **`GetRows`**: Retrieves a collection of records.

```csharp
public Task<Product?> GetById(Guid id)
{
    var sql = "SELECT * FROM Products WHERE Id = @id;";
    var cmd = sql.ToCmd(id.ToParam("@id"));
    return _db.GetOneRow(cmd, MapProduct);
}

public Task<IEnumerable<Product>> GetAll()
{
    var sql = "SELECT * FROM Products ORDER BY Name;";
    var cmd = sql.ToCmd();
    return _db.GetRows(cmd, MapProduct);
}
```

**Create, Update, Delete**

-   **`ExecuteNonQuery`**: Used for operations that do not return a result set. Returns the number of rows affected.

```csharp
public Task<int> Create(Product product)
{
    var sql = @"INSERT INTO Products (Id, Name, Price, Stock, Description) 
                VALUES (@id, @name, @price, @stock, @description);";
    var cmd = sql.ToCmd(
        product.Id.ToParam("@id"),
        product.Name.ToParam("@name"),
        product.Price.ToParam("@price"),
        product.Stock.ToParam("@stock"),
        product.Description.ToParam("@description")
    );
    return _db.ExecuteNonQuery(cmd);
}

public Task<int> Update(Product product)
{
    var sql = @"UPDATE Products SET 
                    Name = @name, Price = @price, 
                    Stock = @stock, Description = @description
                WHERE Id = @id;";
    var cmd = sql.ToCmd(
        product.Name.ToParam("@name"),
        product.Price.ToParam("@price"),
        product.Stock.ToParam("@stock"),
        product.Description.ToParam("@description"),
        product.Id.ToParam("@id")
    );
    return _db.ExecuteNonQuery(cmd);
}

public Task<int> Delete(Guid id)
{
    var sql = "DELETE FROM Products WHERE Id = @id;";
    var cmd = sql.ToCmd(id.ToParam("@id"));
    return _db.ExecuteNonQuery(cmd);
}
```

#### 3.4. Additional Use Cases

**`ExecuteScalar` to get a single value**

Useful for aggregations like `COUNT`, `SUM`, `AVG`, etc.

```csharp
public async Task<int> GetTotalStock()
{
    var sql = "SELECT SUM(Stock) FROM Products;";
    var cmd = sql.ToCmd();
    object? result = await _db.ExecuteScalar(cmd);
    return result != null ? Convert.ToInt32(result) : 0;
}
```

**`ExecuteReader` to process large results**

Ideal for processing data row by row without loading it all into memory.

```csharp
public async Task LogProductStock()
{
    var sql = "SELECT Name, Stock FROM Products;";
    var cmd = sql.ToCmd();

    await _db.ExecuteReader(cmd, dr =>
    {
        var name = dr.GetString("Name");
        var stock = dr.GetInt("Stock");
        Console.WriteLine($"Product: {name}, Stock: {stock}");
    });
}
```

### 4. Register and Use the Repository

Finally, register your repository in `Program.cs` and then inject it into your controllers or services.

```csharp
// Program.cs
// ...
builder.Services.AddScoped<IProductsRepo, ProductsRepo>();
// ...
```

```csharp
// ProductsController.cs
[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductsRepo _productsRepo;

    public ProductsController(IProductsRepo productsRepo)
    {
        _productsRepo = productsRepo;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var product = await _productsRepo.GetById(id);
        return product != null ? Ok(product) : NotFound();
    }
}
```

## Publishing a New Version

To create a new release of the NuGet package, follow these steps:

1.  **Update the Version Number**
    
    In the `Ro.Npgsql.Data.csproj` file, increment the number in the `<Version>` tag:
    
    ```xml
    <PropertyGroup>
        <Version>0.0.8</Version> <!-- Increment the version number -->
    </PropertyGroup>
    ```
    
2.  **Commit and Tag in Git**
    
    Save the changes and create a Git tag. This will trigger the GitHub Actions workflow to build and publish the package automatically.
    
    ```bash
    # Commit the changes
    git add .
    git commit -m "Release version 0.0.8"
    
    # Create a Git tag for the new version
    git tag v0.0.8
    
    # Push the commit and tag to GitHub
    git push origin master
    git push origin v0.0.8
    ```
    
3.  **Manual Publishing (Alternative)**
    
    If you need to publish manually, you can use the following commands:
    
    ```bash
    dotnet restore
    dotnet build Ro.Npgsql.Data
    dotnet pack
    dotnet nuget push Ro.Npgsql.Data/bin/Release/Ro.Npgsql.Data.0.0.8.nupkg --api-key <YOUR_TOKEN> --source "github"
    ```

## License

This project is licensed under the [GNU GENERAL PUBLIC LICENSE Version 3](LICENSE).
