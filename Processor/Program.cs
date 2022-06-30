using Commun.Entities;
using Microsoft.EntityFrameworkCore;
using Processor.BackgroundServices;
using Processor.Channels;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<HashDbContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddSingleton(_ => Channel.CreateUnbounded<InsertQueryMessage>());
builder.Services.AddHostedService<ConsumerBackgroundService>();
builder.Services.AddHostedService<InsertHasehsBackgroundService>();


var app = builder.Build();

app.Run();
