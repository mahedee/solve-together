using Asp.Versioning;
using Asp.Versioning.ApiExplorer;
using AspNetCore.ApiVersioningTest.Constants;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddMvc();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

var env = builder.Environment;

// Use null-coalescing operator to handle missing configuration values safely

builder.Configuration
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true);

builder.Services.AddApiVersioning(options =>
{
    //options.DefaultApiVersion = new ApiVersion(Convert.ToInt32(ApiVersionConfiguration.Major), Convert.ToInt32(ApiVersionConfiguration.Minor));
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;

    // Read version from URL segments, headers, or query strings
    options.ApiVersionReader = ApiVersionReader.Combine(
        new UrlSegmentApiVersionReader(),
        new HeaderApiVersionReader("x-api-version"),
        new MediaTypeApiVersionReader("x-api-version"));
}).AddApiExplorer(options =>
{
    //options.GroupNameFormat = ApiVersionConfiguration.GroupNameFormat;
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(
        ApiVersionConfiguration.Version,
        new OpenApiInfo
        {
            Title = ApiVersionConfiguration.VersionTitle,
            Version = ApiVersionConfiguration.Version,
        });

    c.SwaggerDoc(
        ApiVersionConfiguration.MinorVersion,
        new OpenApiInfo
        {
            Title = ApiVersionConfiguration.MinorVersionTitle,
            Version = ApiVersionConfiguration.MinorVersion,
        });

    // Include XML comments for all assemblies dynamically
    var baseDirectory = AppContext.BaseDirectory;
    var xmlFiles = Directory.GetFiles(baseDirectory, "*.xml", SearchOption.TopDirectoryOnly);

    foreach (var xmlFile in xmlFiles)
    {
        c.IncludeXmlComments(Path.Combine(baseDirectory, xmlFile));
    }
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {

        c.EnableFilter();
        c.DisplayRequestDuration();

        app.UseSwaggerUI(options =>
        {
            var apiVersionDescriptionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();
            foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
            {
                var formattedGroupName = $"v{description.ApiVersion.ToString()}"; // Enforce full version
                Console.WriteLine($"GroupName: {description.GroupName}, ApiVersion: {description.ApiVersion}");
                options.SwaggerEndpoint($"/swagger/{formattedGroupName}/swagger.json",
                    formattedGroupName.ToUpperInvariant());
                //options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                //    description.GroupName.ToUpperInvariant());
            }

            //options.SwaggerEndpoint("/swagger/v1.0/swagger.json", "API v1.0");
            //options.SwaggerEndpoint("/swagger/v1.1/swagger.json", "API v1.1");
        });

    });

}

app.UseHttpsRedirection();
app.Run();


