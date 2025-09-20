using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.ComponentModel.DataAnnotations;

var builder = WebApplication.CreateBuilder(args);

// Configuración de MongoDB
builder.Services.Configure<MongoDbSettings>(
    builder.Configuration.GetSection("MongoDbSettings"));

builder.Services.AddSingleton<IMongoClient>(serviceProvider =>
{
    var settings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
    return new MongoClient(settings.ConnectionString);
});

builder.Services.AddScoped<PropertyService>();

// Configuración de Swagger
// Saber más sobre Swagger/OpenAPI => https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Pipeline de desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ===== ENDPOINTS =====

// GET: Obtener todas las propiedades
app.MapGet("/api/properties", async (PropertyService propertyService) =>
{
    var properties = await propertyService.GetAllAsync();
    return Results.Ok(properties);
})
.WithName("GetAllProperties")
.WithOpenApi();

// GET: Buscar propiedades por nombre
app.MapGet("/api/properties/name", async (string name, PropertyService propertyService) =>
{
    var properties = await propertyService.GetByNameAsync(name);
    return Results.Ok(properties);
})
.WithName("GetPropertiesByName")
.WithOpenApi();

// GET: Buscar propiedades por direccion
app.MapGet("/api/properties/address", async (string address, PropertyService propertyService) =>
{
    var properties = await propertyService.GetByAddressAsync(address);
    return Results.Ok(properties);
})
.WithName("GetPropertiesByAddress")
.WithOpenApi();

// GET: Buscar propiedades por rango de precio
app.MapGet("/api/properties/price-range", async (decimal minPrice, decimal maxPrice, PropertyService propertyService) =>
{
    var properties = await propertyService.GetByPriceRangeAsync(minPrice, maxPrice);
    return Results.Ok(properties);
})
.WithName("GetPropertiesByPriceRange")
.WithOpenApi();

// GET: Buscar propiedades por nombre, direccion y rango de precio
app.MapGet("/api/properties/filter", async (string? name, string? address, decimal minPrice, decimal maxPrice, PropertyService propertyService) =>
{
    var properties = await propertyService.FilterAsync(name, address, minPrice, maxPrice);
    return Results.Ok(properties);
})
.WithName("GetPropertiesFilter")
.WithOpenApi();

// POST: Crear nuevo property
app.MapPost("/api/properties", async (CreatePropertyRequest request, PropertyService propertyService) =>
{
    var property = new Property
    {
        Name = request.Name,
        Address = request.Address,
        Price = request.Price,
        CodeInternal = request.CodeInternal,
        Year = request.Year,
        Owner = request.Owner,
        Images = request.Images,
        Traces = request.Traces,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };

    await propertyService.CreateAsync(property);
    return Results.Created($"/api/properties/{property.Id}", property);
})
.WithName("CreateProperty")
.WithOpenApi();

app.Run();

// ===== MODELOS =====

// Modelo propiedad
public class Property
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("address")]
    public string? Address { get; set; }

    [BsonElement("price")]
    [Range(0, double.MaxValue, ErrorMessage = "El precio debe ser mayor a 0")]
    public decimal Price { get; set; }

    [BsonElement("code_internal")]
    public string CodeInternal { get; set; } = string.Empty;

    [BsonElement("year")]
    [Range(1900, 2100, ErrorMessage = "Año inválido")]
    public int Year { get; set; }

    [BsonElement("owner")]
    public Owner Owner { get; set; } = new Owner();

    [BsonElement("images")]
    public List<PropertyImage> Images { get; set; } = [];

    [BsonElement("traces")]
    public List<PropertyTrace> Traces { get; set; } = [];

    [BsonElement("created_at")]
    public DateTime CreatedAt { get; set; }

    [BsonElement("updated_at")]
    public DateTime UpdatedAt { get; set; }
}

// Modelo propietario
public class Owner
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("address")]
    public string Address { get; set; } = string.Empty;

    [BsonElement("photo")]
    public string Photo { get; set; } = string.Empty;

    [BsonElement("birthday")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime Birthday { get; set; }
}

// Modelo imágenes de la propiedad
public class PropertyImage
{
    [BsonElement("file")]
    public string File { get; set; } = string.Empty;

    [BsonElement("enabled")]
    public bool Enabled { get; set; } = true;
}

// Modelo historial de transacciones
public class PropertyTrace
{
    [BsonElement("dateSale")]
    [BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
    public DateTime DateSale { get; set; }

    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("value")]
    [Range(0, double.MaxValue, ErrorMessage = "El valor debe ser mayor a 0")]
    public decimal Value { get; set; }

    [BsonElement("tax")]
    [Range(0, double.MaxValue, ErrorMessage = "El impuesto debe ser mayor a 0")]
    public decimal Tax { get; set; }
}

// ===== DTOs =====

public class PropertyListDto
{
    public string Id { get; set; } = string.Empty;
    public string IdOwner { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string AddressProperty { get; set; } = string.Empty;
    public decimal PriceProperty { get; set; }
    public string? Image { get; set; } // Solo una imagen
}

public record CreatePropertyRequest(
    string Name,
    string Address,
    decimal Price,
    string CodeInternal,
    int Year,
    Owner Owner,
    List<PropertyImage> Images,
    List<PropertyTrace> Traces
);


// ===== CONFIGURACIÓN =====

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = string.Empty;
    public string CollectionName { get; set; } = string.Empty;
}

// ===== SERVICIO =====

public class PropertyService
{
    private readonly IMongoCollection<Property> _properties;

    public PropertyService(IMongoClient mongoClient, IConfiguration configuration)
    {
        var settings = configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
        var database = mongoClient.GetDatabase(settings.DatabaseName);
        _properties = database.GetCollection<Property>(settings.CollectionName);
    }

    public async Task<List<Property>> GetAllAsync() =>
        await _properties.Find(_ => true).ToListAsync();

    public async Task<Property?> GetByIdAsync(string id) =>
        await _properties.Find(x => x.Id == id).FirstOrDefaultAsync();

    public async Task CreateAsync(Property property) =>
        await _properties.InsertOneAsync(property);

    public async Task UpdateAsync(string id, Property property) =>
        await _properties.ReplaceOneAsync(x => x.Id == id, property);

    public async Task DeleteAsync(string id) =>
        await _properties.DeleteOneAsync(x => x.Id == id);

    public async Task<List<Property>> GetByNameAsync(string name) =>
        await _properties.Find(x => x.Name.ToLower() == name.ToLower()).ToListAsync();

    public async Task<List<Property>> GetByAddressAsync(string address) =>
        await _properties.Find(x => x.Address.ToLower() == address.ToLower()).ToListAsync();

    public async Task<List<Property>> GetByPriceRangeAsync(decimal minPrice, decimal maxPrice) =>
        await _properties.Find(x => x.Price >= minPrice && x.Price <= maxPrice).ToListAsync();

    public async Task<List<Property>> FilterAsync(string? name, string? address, decimal? minPrice, decimal? maxPrice)
    {
        var filterBuilder = Builders<Property>.Filter;
        var filters = new List<FilterDefinition<Property>>();

        // Filtro por nombre (si se envía)
        if (!string.IsNullOrWhiteSpace(name))
            filters.Add(filterBuilder.Regex(x => x.Name, new BsonRegularExpression(name, "i")));

        // Filtro por dirección (si se envía)
        if (!string.IsNullOrWhiteSpace(address))
            filters.Add(filterBuilder.Regex(x => x.Address, new BsonRegularExpression(address, "i")));

        // Filtro por precio mínimo (si se envía)
        if (minPrice.HasValue)
            filters.Add(filterBuilder.Gte(x => x.Price, minPrice.Value));

        // Filtro por precio máximo (si se envía)
        if (maxPrice.HasValue)
            filters.Add(filterBuilder.Lte(x => x.Price, maxPrice.Value));

        // Si no hay filtros, devuelve todo
        var finalFilter = filters.Count > 0
            ? filterBuilder.And(filters)
            : FilterDefinition<Property>.Empty;

        return await _properties.Find(finalFilter).ToListAsync();
    }
}