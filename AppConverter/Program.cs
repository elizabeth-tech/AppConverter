using AppConverter.API.Extensions;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi.Models;
using NLog.Web;
using System.IO.Compression;

var logger = NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();

try
{
    logger.Info("Init application Converter");
    var builder = WebApplication.CreateBuilder(args);

    const string CorsPolicy = "CorsPolicy";

    // Configure and add services to the container.

    builder.Services.AddCors(options =>
    {
        options.AddPolicy(
            name: CorsPolicy,
            policy =>
            {
                policy.WithOrigins("http://localhost:3000");
            });
    });

    builder.Services.AddControllers().AddNewtonsoftJson();
    builder.Services.AddResponseCompression(options => options.EnableForHttps = true);
    builder.Services.Configure<GzipCompressionProviderOptions>(options =>
    {
        options.Level = CompressionLevel.Optimal;
    });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("1.0", new OpenApiInfo { Title = "AppConverter API", Version = "1.0" });
        List<string> xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly).ToList();
        xmlFiles.ForEach(xmlFile => options.IncludeXmlComments(xmlFile));
    });

    builder.Services.AddServices();

    // NLog: Setup NLog for Dependency injection
    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(LogLevel.Trace);
    builder.Host.UseNLog();

    var app = builder.Build();

    app.UseResponseCompression();

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
        app.UseHsts();

    app.UseHttpsRedirection();

    app.UseSwagger();
    app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/1.0/swagger.json", "API 1.0"); });

    app.UseCors(CorsPolicy);

    app.UseRouting();
    app.UseCors(CorsPolicy);
    app.MapControllers();

    app.Run();
}
catch (Exception exception)
{
    //NLog: catch setup errors
    logger.Error(exception, "Stopped program because of exception");
    throw;
}
finally
{
    // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
    NLog.LogManager.Shutdown();
}