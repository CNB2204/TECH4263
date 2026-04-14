using EquipmentAPI.Models;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using EquipmentAPI.Data;
using Microsoft.AspNetCore.Authentication;
using EquipmentAPI.Auth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("basic", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "basic",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your username and password"
    });
}); builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddAuthentication("BasicAuth")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthHandler>("BasicAuth", null);

builder.Services.AddAuthorization();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

//var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

app.MapPost("/equipments", async (CreateEquipmentDto dto, AppDbContext context) => {
    var equipment = new Equipment(dto.Name, dto.Category, dto.Status, dto.Location);

    context.Equipments.Add(equipment);
    await context.SaveChangesAsync();

    return Results.Created($"/equipments/{equipment.Id}", new EquipmentResponseDto
    {
        Id = equipment.Id,
        Name = dto.Name,
        Category = dto.Category,
        Status = dto.Status,
        Location = dto.Location
    });

})
   .WithName("CreateEquipment")
   .WithOpenApi();


app.MapGet("/equipments", async (AppDbContext context) => {
    var equipments = await context.Equipments.ToListAsync();

    return Results.Ok(equipments.Select(s => new EquipmentResponseDto
    {
        Id = s.Id,
        Name = s.Name,
        Category = s.Category,
        Status = s.Status,
        Location = s.Location
    }));
    
})
   .WithName("GetEquipments")
   .WithOpenApi();

app.MapGet("/equipments/{id:int:min(1)}", async (int id, AppDbContext context) =>
{
    var equipment = await context.Equipments.FindAsync(id);

    if (equipment is null)
        return Results.NotFound();

    return Results.Ok(new EquipmentResponseDto
    {
        Id = equipment.Id,
        Name = equipment.Name,
        Category = equipment.Category,
        Status = equipment.Status,
        Location = equipment.Location
    });
}).WithName("GetEquipmentById").WithOpenApi();


app.Run();



