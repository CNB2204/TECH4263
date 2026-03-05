using EquipmentAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var equipmentList = new List<Equipment>();

app.UseHttpsRedirection();

app.MapPost("/equipments", (CreateEquipmentDto dto) => {
    var equipment = new Equipment(dto.Name, dto.Category, dto.Status, dto.Location);

    equipmentList.Add(equipment);

    return Results.Created($"/equipments/{equipment.Id}", new EquipmentResponseDto
    {
        Id = equipment.Id,
        Name = equipment.Name,
        Category = equipment.Category,
        Status = equipment.Status,
        Location = equipment.Location
    });
})
   .WithName("CreateEquipment")
   .WithOpenApi();


app.MapGet("/equipments", () => {
    var result = equipmentList.Select(e => new EquipmentResponseDto
    {
        Id = e.Id,
        Name = e.Name,
        Category = e.Category,
        Status = e.Status,
        Location = e.Location
    }).ToList();
    return Results.Ok(equipmentList);
})
   .WithName("GetEquipments")
   .WithOpenApi();

app.MapGet("/equipments/{id:int:min(1)}", (int id) => {
    var equipment = equipmentList.FirstOrDefault(e => e.Id == id);
    if (equipment == null) 
    {
        return Results.NotFound();
    }
    return Results.Ok(new EquipmentResponseDto
        {
        Id = equipment.Id,
        Name = equipment.Name,
        Category = equipment.Category,
        Status = equipment.Status,
        Location = equipment.Location
    });
})
   .WithName("GetEquipmentById")
   .WithOpenApi();


app.Run();


