using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Novibet.Api.Services;
using Novibet.Data;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);


builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

builder.Host.ConfigureContainer<ContainerBuilder>(container =>
{
    container.RegisterType<CacheService>().As<ICacheService>().InstancePerLifetimeScope();  

    container.RegisterType<WalletService>().As<IWalletService>().InstancePerLifetimeScope();
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

string? cacheConfig = builder.Configuration.GetConnectionString("Redis");

if (string.IsNullOrEmpty(cacheConfig)) throw new InvalidOperationException("NO REDIS");


var multiplexer = ConnectionMultiplexer.Connect(cacheConfig);

builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("general", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
    });

    options.AddFixedWindowLimiter("heavy", opt =>
    {
        opt.PermitLimit = 10;
        opt.Window = TimeSpan.FromMinutes(5);
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

string? cs = builder.Configuration.GetConnectionString("Postgres");

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(cs));

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();

app.Run();
