var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();

var app = builder.Build();

app.UseCors(builder => builder
    .WithOrigins("http://localhost:5173")
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()
);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();
app.MapHub<MessageHub>("/MessageHub");

app.MapGet("/roomId", () =>
{
    string roomId;
    do
    {
        roomId = RoomManager.GenerateRoomId();
    } while (RoomManager.usedRoomIds.Contains(roomId));

    RoomManager.usedRoomIds.Add(roomId);
    return roomId;
});

app.Run();