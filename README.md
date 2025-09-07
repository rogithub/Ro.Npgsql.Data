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

## Cómo Usarlo

La librería está diseñada para ser usada con inyección de dependencias.

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

var app = builder.Build();

// ...
```

### 2. Uso de Mappers y DataReaders

La librería incluye una serie de métodos de extensión para `IDataReader` que simplifican la lectura de datos y la hacen más segura, manejando conversiones y valores `DBNull.Value` automáticamente.

#### Métodos de Extensión Comunes

Puedes acceder a los valores de las columnas directamente por su nombre.

```csharp
private Producto MapProducto(IDataReader dr)
{
    return new Producto()
    {
        Id = dr.GetGuid("Id"), // Lanza excepción si es DBNull
        Nombre = dr.GetString("Nombre"), // Lanza excepción si es DBNull
        Precio = dr.GetDecimal("Precio"), // Lanza excepción si es DBNull
        // ... otros campos
    };
}
```

#### Manejo de Nulos con `FromDb<T>`

Para columnas que pueden ser nulas (`Nullable`), el método genérico `FromDb<T>` es la mejor opción.

```csharp
private Ajuste MapAjuste(IDataReader dr)
{
    return new Ajuste()
    {
        Id = dr.GetGuid("Id"),
        // ClienteId puede ser nulo en la base de datos
        ClienteId = dr.FromDb<Guid?>("ClienteId"), 
        Pago = dr.FromDb<decimal>("Pago"),
        // Proporcionar un valor por defecto si UserUpdatedId es nulo
        UserUpdatedId = dr.FromDb("UserUpdatedId", Guid.Empty),
    };
}
```

### 3. Ejemplos de la Interfaz `IDbAsync`

A continuación se muestran ejemplos para cada uno de los métodos disponibles en la interfaz `IDbAsync`.

#### `ExecuteNonQuery`

Úsalo para sentencias `INSERT`, `UPDATE` o `DELETE` que no retornan un resultado. Devuelve el número de filas afectadas.

```csharp
public Task<int> UpdateNombreProducto(Guid id, string nuevoNombre)
{
    var sql = "UPDATE Productos SET Nombre = @nombre WHERE Id = @id;";
    var cmd = sql.ToCmd(
        nuevoNombre.ToParam("@nombre"),
        id.ToParam("@id")
    );
    return _db.ExecuteNonQuery(cmd);
}
```

#### `ExecuteScalar`

Úsalo cuando esperas un único valor como resultado (por ejemplo, un `COUNT`, `SUM` o un ID).

```csharp
public async Task<int> ContarProductosEnStock()
{
    var sql = "SELECT COUNT(*) FROM v_inventario WHERE Stock > 0;";
    var cmd = sql.ToCmd();
    object result = await _db.ExecuteScalar(cmd);
    return Convert.ToInt32(result);
}
```

#### `GetOneRow`

Recupera un único registro del resultado de una consulta. Si no se encuentran filas, devuelve `null`.

```csharp
public Task<Producto> GetOne(Guid id)
{
    var sql = "SELECT * FROM Productos WHERE Id = @id";
    var cmd = sql.ToCmd(id.ToParam("@id"));
    return _db.GetOneRow(cmd, MapProducto); // MapProducto es tu función de mapeo
}
```

#### `GetRows`

Recupera una colección de registros (`IEnumerable<T>`) del resultado de una consulta.

```csharp
public Task<IEnumerable<Producto>> GetAll()
{
    var sql = "SELECT * FROM Productos ORDER BY Nombre";
    var cmd = sql.ToCmd();
    return _db.GetRows(cmd, MapProducto);
}
```

#### `ExecuteReader`

Procesa un resultado fila por fila. Es útil para manejar grandes volúmenes de datos sin cargarlos todos en memoria a la vez.

```csharp
public async Task ProcesarProductos()
{
    var sql = "SELECT Id, Nombre FROM Productos";
    var cmd = sql.ToCmd();

    await _db.ExecuteReader(cmd, dr =>
    {
        // Esta acción se ejecuta por cada fila leída
        var id = dr.GetGuid("Id");
        var nombre = dr.GetString("Nombre");
        Console.WriteLine($"Procesando: {id} - {nombre}");
    });
}
```

#### `GetOneRowAsync` y `GetRowsAsync`

Estas variantes son útiles cuando la propia lógica de mapeo es asíncrona (por ejemplo, si necesitas hacer otra llamada a la base de datos o a una API para construir tu objeto).

```csharp
// Ejemplo de un mapeador asíncrono
private async Task<ProductoConDetalles> MapProductoConDetallesAsync(IDataReader dr)
{
    var producto = new ProductoConDetalles
    {
        Id = dr.GetGuid("Id"),
        Nombre = dr.GetString("Nombre")
    };
    
    // Llama a otro servicio/repo de forma asíncrona durante el mapeo
    producto.DetallesExtras = await _otroServicio.GetDetalles(producto.Id); 
    
    return producto;
}

public Task<ProductoConDetalles> GetOneConDetalles(Guid id)
{
    var sql = "SELECT Id, Nombre FROM Productos WHERE Id = @id";
    var cmd = sql.ToCmd(id.ToParam("@id"));
    return _db.GetOneRowAsync(cmd, MapProductoConDetallesAsync);
}

public Task<IEnumerable<ProductoConDetalles>> GetAllConDetalles()
{
    var sql = "SELECT Id, Nombre FROM Productos";
    var cmd = sql.ToCmd();
    return _db.GetRowsAsync(cmd, MapProductoConDetallesAsync);
}
```

### 4. Registrar y Usar el Repositorio

Finalmente, registra tu repositorio en el contenedor de DI y úsalo en tus servicios o controladores.

```csharp
// Program.cs
...
builder.Services.AddScoped<IProductosRepo, ProductosRepo>();
...
```

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
