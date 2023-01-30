using Business.Location.Abstract;
using Business.Location.Concrete;
using Cache.Abstract;
using Cache.Concrete;
using Core.Abstract;
using Core.Concrete;
using DataAccess.Context;
using Microsoft.EntityFrameworkCore;
using Model;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//Redis Configuration
IConfiguration configuration = builder.Configuration;
string redisConnection = configuration.GetValue<string>("Redis");
string connectionString = configuration.GetValue<string>("ConnectionString");
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connectionString));
ConnectionMultiplexer multiplexer = ConnectionMultiplexer.Connect(redisConnection);
builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);
builder.Services.AddScoped<ILocationBusiness, LocationBusiness>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
builder.Services.AddScoped<ICityRepository, CityRepository>();
builder.Services.AddScoped<IDistrictRepository, DistrictRepository>();
builder.Services.AddScoped<INeighborhoodRepository, NeighborhoodRepository>();
builder.Services.AddScoped<ILocationRepository, LocationRepository>();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
