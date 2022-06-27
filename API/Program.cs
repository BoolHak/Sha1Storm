using API.BackgroundServices;
using API.Channels;
using System.Threading.Channels;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddSingleton(_ => Channel.CreateUnbounded<GenerateMessage>());
builder.Services.AddHostedService<HashSenderBackgroundService>();

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

app.Run();
