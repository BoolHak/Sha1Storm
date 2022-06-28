using API.BackgroundServices;
using API.Channels;
using Commun.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton(_ => Channel.CreateUnbounded<GenerateMessage>());
builder.Services.AddHostedService<HashSenderBackgroundService>();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<HashDbContext>(options =>  
    options.UseSqlServer(connectionString ,
    b => b.MigrationsAssembly("API"))
);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

await using var scope = app.Services.CreateAsyncScope();
using var db = scope.ServiceProvider.GetService<HashDbContext>();
await db.Database.MigrateAsync();

app.Run();
