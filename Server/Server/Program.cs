using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
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
if (!builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpsRedirection(options =>
    {
        options.RedirectStatusCode = 308;
        options.HttpsPort = 443;
    });
}
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"))
    );
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Test01", Version = "v1" });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."

    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            []
        }
    });
});
builder.Services.AddHostedService<CheckToken>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.Use(next => context =>
    {
        context.Request.EnableBuffering();
        return next(context);
    });
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
