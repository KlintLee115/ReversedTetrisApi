var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllersWithViews();
builder.Services.AddSignalR();
builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsSettings",
        builder =>
        {
            builder.WithOrigins("http://10.187.162.67:5173", "https://reversed-tetris.netlify.app")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
        });
});

// Configure Kestrel to listen on all network interfaces
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5159); // HTTP port
    serverOptions.ListenAnyIP(7092, listenOptions =>
    {
        listenOptions.UseHttps(); // HTTPS port
    });
});

var app = builder.Build();

app.UseCors("CorsSettings");

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