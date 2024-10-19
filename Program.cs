using dotenv.net;
using ReversedTetrisApi;

DotEnv.Load();

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
            if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
            {
                builder.WithOrigins("http://10.187.137.241:5173") // Your frontend URL
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials(); 
            }
            else
            {
                builder.WithOrigins("https://reversed-tetris.netlify.app")
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .AllowCredentials();
            }
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

app.MapGet("/", () =>
{
    return "wqdqwd";
});

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