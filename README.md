# Ro.Npgsql.Data

`Ro.Npgsql.Data` es una librería ligera y asíncrona para acceso a datos con PostgreSQL en aplicaciones .NET. Su objetivo es simplificar la ejecución de comandos SQL, la gestión de parámetros y la lectura de datos, proveyendo un conjunto de métodos de extensión y helpers.

## Características

-   Operaciones de base de datos totalmente asíncronas.
-   Métodos de extensión para crear `DbCommand` y `IDbDataParameter` de forma fluida.
-   Helpers para leer y convertir tipos de datos de forma segura desde un `IDataReader`.
-   Diseñado para ser inyectado a través de un contenedor de Inyección de Dependencias (DI) usando la interfaz `IDbAsync`.

## Instalación

Para agregar esta librería a tu proyecto, puedes instalarla desde NuGet.

```bash
dotnet add package Ro.Npgsql.Data
```

O puedes agregar la referencia directamente en tu archivo `.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">  
  ...
  <ItemGroup>
    <PackageReference Include="Ro.Npgsql.Data" Version="0.0.7" />
  </ItemGroup>
  ...
</Project>
```

## Guía de Uso: Creando un Repositorio de Productos

Esta guía te mostrará cómo usar `Ro.Npgsql.Data` para construir un repositorio de datos completo para una entidad `Producto`.

### 1. Configuración en `Program.cs`

Primero, registra la implementación de `IDbAsync` en tu contenedor de servicios. La implementación `Database` recibe el connection string de tu base de datos PostgreSQL.

```csharp
// Program.cs
using Ro.Npgsql.Data;

var builder = WebApplication.CreateBuilder(args);

// ... otros servicios

// Configuración de la cadena de conexión
var pg_host = builder.Configuration.GetSection("PostgresDb:Host").Value;
var pg_port = builder.Configuration.GetSection("PostgresDb:Port").Value;
var pg_db = builder.Configuration.GetSection("PostgresDb:Database").Value;
var pg_user = builder.Configuration.GetSection("PostgresDb:Username").Value;
var pg_pass = builder.Configuration.GetSection("PostgresDb:Password").Value;

string connString = $"Host={pg_host};Port={pg_port};Database={pg_db};Username={pg_user};Password={pg_pass};";

// Registrar IDbAsync para inyección de dependencias
builder.Services.AddTransient<IDbAsync>((svc) =>
{
    return new Database(connString);
});

// ... resto de la configuración
```

### 2. La Entidad `Producto` y la Interfaz del Repositorio

Definimos nuestro modelo y la interfaz que implementaremos. La propiedad `Descripcion` es nullable para demostrar el manejo de nulos.

```csharp
public class Producto
{
    public Guid Id { get; set; }
    public string Nombre { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }
    public string? Descripcion { get; set; } // Campo Nulable
}

public interface IProductosRepo
{
    Task<Producto?> GetById(Guid id);
    Task<IEnumerable<Producto>> GetAll();
    Task<int> Create(Producto producto);
    Task<int> Update(Producto producto);
    Task<int> Delete(Guid id);
    Task<int> GetTotalStock();
}
```

### 3. Implementación del Repositorio

Ahora, creamos la clase `ProductosRepo` que implementa la interfaz.

#### 3.1. Inyección y Mapeo de Datos

Inyectamos `IDbAsync` en el constructor. El método `MapProducto` se encargará de convertir los datos de `IDataReader` a nuestro objeto `Producto`.

-   **`dr.GetString("Nombre")`**: Métodos de extensión para leer tipos de datos de forma segura.
-   **`dr.FromDb<string?>("Descripcion")`**: Método genérico para manejar columnas que pueden ser `DBNull.Value`, especialmente útil para tipos `Nullable`.

```csharp
using Ro.Npgsql.Data;
using System.Data;

public class ProductosRepo : IProductosRepo
{
    private readonly IDbAsync _db;

    public ProductosRepo(IDbAsync db)
    {
        _db = db;
    }

    private Producto MapProducto(IDataReader dr)
    {
        return new Producto()
        {
            Id = dr.GetGuid("Id"),
            Nombre = dr.GetString("Nombre"),
            Precio = dr.GetDecimal("Precio"),
            Stock = dr.GetInt("Stock"),
            Descripcion = dr.FromDb<string?>("Descripcion") // Manejo de nulos
        };
    }
    
    // ... implementación de métodos CRUD
}
```

#### 3.2. Creación de Comandos y Parámetros (`ToCmd` y `ToParam`)

Para crear comandos, usamos los métodos de extensión `ToCmd()` y `ToParam()`. Esto hace que el código sea más limpio y legible.

-   **`sql.ToCmd(...)`**: Convierte un string SQL en un `DbCommand`.
-   **`valor.ToParam("@nombre")`**: Convierte un valor (string, int, Guid, etc.) en un `IDbDataParameter`.

#### 3.3. Implementación de Métodos CRUD

**Read (Leer uno y leer todos)**

-   **`GetOneRow`**: Recupera un único registro. Devuelve `null` si no lo encuentra.
-   **`GetRows`**: Recupera una colección de registros.

```csharp
public Task<Producto?> GetById(Guid id)
{
    var sql = "SELECT * FROM Productos WHERE Id = @id;";
    var cmd = sql.ToCmd(id.ToParam("@id"));
    return _db.GetOneRow(cmd, MapProducto);
}

public Task<IEnumerable<Producto>> GetAll()
{
    var sql = "SELECT * FROM Productos ORDER BY Nombre;";
    var cmd = sql.ToCmd();
    return _db.GetRows(cmd, MapProducto);
}
```

**Create, Update, Delete (Crear, Actualizar, Borrar)**

-   **`ExecuteNonQuery`**: Se usa para operaciones que no devuelven un conjunto de resultados. Devuelve el número de filas afectadas.

```csharp
public Task<int> Create(Producto producto)
{
    var sql = @"INSERT INTO Productos (Id, Nombre, Precio, Stock, Descripcion) 
                VALUES (@id, @nombre, @precio, @stock, @descripcion);";
    var cmd = sql.ToCmd(
        producto.Id.ToParam("@id"),
        producto.Nombre.ToParam("@nombre"),
        producto.Precio.ToParam("@precio"),
        producto.Stock.ToParam("@stock"),
        producto.Descripcion.ToParam("@descripcion")
    );
    return _db.ExecuteNonQuery(cmd);
}

public Task<int> Update(Producto producto)
{
    var sql = @"UPDATE Productos SET 
                    Nombre = @nombre, Precio = @precio, 
                    Stock = @stock, Descripcion = @descripcion
                WHERE Id = @id;";
    var cmd = sql.ToCmd(
        producto.Nombre.ToParam("@nombre"),
        producto.Precio.ToParam("@precio"),
        producto.Stock.ToParam("@stock"),
        producto.Descripcion.ToParam("@descripcion"),
        producto.Id.ToParam("@id")
    );
    return _db.ExecuteNonQuery(cmd);
}

public Task<int> Delete(Guid id)
{
    var sql = "DELETE FROM Productos WHERE Id = @id;";
    var cmd = sql.ToCmd(id.ToParam("@id"));
    return _db.ExecuteNonQuery(cmd);
}
```

#### 3.4. Casos de Uso Adicionales

**`ExecuteScalar` para obtener un valor único**

Útil para agregaciones como `COUNT`, `SUM`, `AVG`, etc.

```csharp
public async Task<int> GetTotalStock()
{
    var sql = "SELECT SUM(Stock) FROM Productos;";
    var cmd = sql.ToCmd();
    object? result = await _db.ExecuteScalar(cmd);
    return result != null ? Convert.ToInt32(result) : 0;
}
```

**`ExecuteReader` para procesar grandes resultados**

Ideal para procesar datos fila por fila sin cargarlos todos en memoria.

```csharp
public async Task LogStockDeProductos()
{
    var sql = "SELECT Nombre, Stock FROM Productos;";
    var cmd = sql.ToCmd();

    await _db.ExecuteReader(cmd, dr =>
    {
        var nombre = dr.GetString("Nombre");
        var stock = dr.GetInt("Stock");
        Console.WriteLine($"Producto: {nombre}, Stock: {stock}");
    });
}
```

### 4. Registrar y Usar el Repositorio

Finalmente, registra tu repositorio en `Program.cs` y luego inyéctalo en tus controladores o servicios.

```csharp
// Program.cs
// ...
builder.Services.AddScoped<IProductosRepo, ProductosRepo>();
// ...
```

```csharp
// ProductosController.cs
[ApiController]
[Route("[controller]")]
public class ProductosController : ControllerBase
{
    private readonly IProductosRepo _productosRepo;

    public ProductosController(IProductosRepo productosRepo)
    {
        _productosRepo = productosRepo;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var producto = await _productosRepo.GetById(id);
        return producto != null ? Ok(producto) : NotFound();
    }
}
```

## Publicar una Nueva Versión

## Publicar una Nueva Versión

Para crear una nueva release del paquete NuGet, sigue estos pasos:

1.  **Actualizar el Número de Versión**
    
    En el archivo `Ro.Npgsql.Data.csproj`, incrementa el número en la etiqueta `<Version>`:
    
    ```xml
    <PropertyGroup>
        <Version>0.0.8</Version> <!-- Incrementar el número de versión -->
    </PropertyGroup>
    ```
    
2.  **Hacer Commit y Tag en Git**
    
    Guarda los cambios y crea un tag en Git. Esto disparará el workflow de GitHub Actions para construir y publicar el paquete automáticamente.
    
    ```bash
    # Hacer commit de los cambios
    git add .
    git commit -m "Release version 0.0.8"
    
    # Crear un tag de Git para la nueva versión
    git tag v0.0.8
    
    # Subir el commit y el tag a GitHub
    git push origin master
    git push origin v0.0.8
    ```
    
3.  **Publicación Manual (Alternativa)**
    
    Si necesitas publicar manualmente, puedes usar los siguientes comandos:
    
    ```bash
    dotnet restore
    dotnet build Ro.Npgsql.Data
    dotnet pack
    dotnet nuget push Ro.Npgsql.Data/bin/Release/Ro.Npgsql.Data.0.0.8.nupkg --api-key <TU_TOKEN> --source "github"
    ```

## Licencia

Este proyecto está bajo la licencia [GNU GENERAL PUBLIC LICENSE Version 3](LICENSE).
