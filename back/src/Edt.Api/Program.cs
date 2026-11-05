using System.Text.Json;
using System.Text.Json.Serialization;
using Asp.Versioning;
using Edt.Api.Extensions;
using Edt.Api.Handlers;
using Edt.Application;
using Edt.Application.Timetables;
using Edt.Infrastructure;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options => options.AddServerHeader = false);
builder.Logging.AddFilter("System.Net.Http.HttpClient", LogLevel.Warning);

builder.Services
    .AddApplication(builder.Configuration.GetSection("Timetables").Get<TimetableOptions>() ?? new())
    .AddInfrastructure(builder.Configuration, builder.Environment.ContentRootPath);

builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)));

builder.Services
    .AddApiVersioning(options =>
    {
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
        options.ReportApiVersions = true;
    })
    .AddMvc()
    .AddApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'V";
        options.SubstituteApiVersionInUrl = true;
    });

builder.Services.AddSwaggerGen(options =>
    options.SwaggerDoc("v1", new() { Title = "EDT BUT", Version = "v1" }));

builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddSecurity(builder.Configuration);

var app = builder.Build();

var front = new PhysicalFileProvider(
    Path.GetFullPath(app.Configuration["Front:Path"] ?? "../../../front", app.Environment.ContentRootPath));

app.UseExceptionHandler();
app.UseSecurity();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles(new DefaultFilesOptions { FileProvider = front });
app.UseStaticFiles(new StaticFileOptions { FileProvider = front });

app.MapControllers();

app.Run();
