using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.Logging;
using Server.Services;
using Server.SignalR;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddSignalR();


// Add logging filter
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddControllersWithViews(options =>
    {
        options.Filters.Add<LoggingFilter>();
    });
}

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
    );
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHostedService<CheckToken>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetService<ApplicationDbContext>();
    db?.Database.EnsureCreated();
    var checkToken = scope.ServiceProvider.GetService<CheckToken>();
    checkToken?.AddContext(db);
}
app.UseAuthorization();

app.MapHub<NewMessageHub>("new-message");

app.MapControllers();


app.Run();
