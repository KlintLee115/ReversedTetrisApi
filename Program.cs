using dotenv.net;
using ReversedTetrisApi;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSignalR();

builder.Services.AddCors(options =>
{
    options.AddPolicy("CorsSettings",
        corsBuilder =>
        {
            corsBuilder.WithOrigins(builder.Environment.IsDevelopment()
                ? "http://localhost:5173"
                : "https://reversed-tetris.netlify.app");
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
app.MapHub<MessageHub>("/MessageHub");

app.MapGet("/", () => "hey");

app.MapGet("/roomId", () =>
{
    var roomId = RoomManager.GenerateUniqueRoomId();
    return roomId;
});

app.Run();