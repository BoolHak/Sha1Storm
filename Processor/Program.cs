using Commun.Entities;
using Microsoft.EntityFrameworkCore;
using Processor.BackgroundServices;

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<HashDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddHostedService<ConsumerBackgroundService>();


var app = builder.Build();

app.Run();
