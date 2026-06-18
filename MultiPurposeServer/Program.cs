using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = ($"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml");
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath);
    }
});

// CORS - adjust origins for production
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// If the app is hosted under a virtual application path, set PathBase from configuration
var pathBase = builder.Configuration["PathBase"];
if (!string.IsNullOrEmpty(pathBase))
{
    if (!pathBase.StartsWith("/")) pathBase = "/" + pathBase;
    if (pathBase.EndsWith("/")) pathBase = pathBase.TrimEnd('/');
    app.UsePathBase(pathBase);
}

// Configure the HTTP request pipeline.
var enableSwagger = app.Environment.IsDevelopment() || builder.Configuration.GetValue<bool>("EnableSwagger");
var logger = app.Services.GetService(typeof(ILogger<Program>)) as ILogger<Program>;
logger?.LogInformation("EnableSwagger={EnableSwagger}, Environment={Env}, PathBase={PathBase}", enableSwagger, app.Environment.EnvironmentName, pathBase ?? "");

if (enableSwagger)
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        // Serve the UI at /swagger. If app is hosted under a PathBase, the endpoint will include it.
        var endpoint = string.IsNullOrEmpty(pathBase) ? "/swagger/v1/swagger.json" : $"{pathBase}/swagger/v1/swagger.json";
        options.SwaggerEndpoint(endpoint, "MPS API V1");
        options.RoutePrefix = "swagger"; // serve at /swagger
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
