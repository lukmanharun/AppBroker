using AppBroker.Interfaces;
using AppBroker.Models;
using AppBroker.Services;
using BusinessCore.Entity;
using BusinessCore.Interfaces;
using BusinessCore.Services;
using Infrastructure.Data;
using Infrastructure.DTO;
using Infrastructure.Interfaces;
using Infrastructure.Services;
using Infrastructure.Services.Kafka;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.FileProviders;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using Serilog.Sinks.MSSqlServer;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
var conDefault = builder.Configuration.GetConnectionString("Default");
var conLogs = builder.Configuration.GetConnectionString("DbLog");
#region Serilog

string environment = string.Empty;
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == null) throw new Exception("Environtment object null");
var sinkOpts = new MSSqlServerSinkOptions()
{
    TableName = "Logs",
    AutoCreateSqlTable = true,
    BatchPeriod = TimeSpan.FromSeconds(1),
    LevelSwitch = new Serilog.Core.LoggingLevelSwitch
    {
        MinimumLevel = Serilog.Events.LogEventLevel.Error
    }
};

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Debug()
    .WriteTo.Console(Serilog.Events.LogEventLevel.Information)
    .WriteTo.File("bin/AppLog/Log.text",
        rollingInterval: RollingInterval.Day,
        rollOnFileSizeLimit: true,
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
    .WriteTo.File("bin/AppLog/Log_Info.text",
        rollingInterval: RollingInterval.Day,
        rollOnFileSizeLimit: true,
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
    .AuditTo.MSSqlServer(conLogs,sinkOpts,restrictedToMinimumLevel:LogEventLevel.Error)
    .Enrich.WithProperty("Environment", environment)
    .ReadFrom.Configuration(new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile(
        $"appsettings.{environment}.json",optional: true)
.Build())
.CreateLogger();
builder.Host.UseSerilog();
builder.Services.AddSingleton(Log.Logger);
#endregion
builder.Services.AddDbContextPool<AppDbContext>(opt =>
{
    opt.UseSqlServer(conDefault);
});

builder.Services.AddAutoMapper(typeof(MapperProfile));
builder.Services.AddScoped<IHelper, Helper>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IHelperService, HelperService>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(
        options =>
        {
            options.ExpireTimeSpan = TimeSpan.FromHours(10);
            options.SlidingExpiration = true;
            options.AccessDeniedPath = new PathString("/User/Forbidden");
            options.LoginPath = new PathString("/User/SignIn");
            options.LogoutPath = new PathString("/User/Logout");
        });
builder.Services.AddHttpContextAccessor();
//Message broker DI
builder.Services.Configure<RabbitMQConfiguration>(opt => builder.Configuration.GetSection(nameof(RabbitMQConfiguration)).Bind(opt));
builder.Services.AddSingleton<IRabbitMQService, RabbitMQService>();
builder.Services.AddSingleton<IMessageBrokerService, MessageBrokerService>();
builder.Services.AddHostedService<ConsumerHostedService>();
//builder.Services.Configure<ConsumerConfig>(opt => builder.Configuration.GetSection(nameof(ConsumerConfig)).Bind(opt));
builder.Services.AddHttpClient<HttpServices>().AddPolicyHandler(HttpClientHelper.GetRetryPolicy());
//builder.Services.AddSingleton<KafkaProducer>();
//builder.Services.AddHostedService<KafkaConsumer>();
//builder.Services.AddSingleton<IHostedService,KafkaConsumer>();
// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    //app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseExceptionHandler("/GlobalHandling/index");
app.UseHttpsRedirection();
app.UseStaticFiles();

var folder = builder.Configuration.GetSection("FileManagement").GetSection("Folder").Value ?? "Resources";

if (!Directory.Exists(Path.Combine(builder.Environment.ContentRootPath, folder)))
{
    Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, folder));
}
var fileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, folder));
var folderRoot = $"/{folder}";
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = fileProvider,
    RequestPath = folderRoot,
});
app.UseDirectoryBrowser(new DirectoryBrowserOptions
{
    FileProvider = fileProvider,
    RequestPath = folderRoot,
});

app.Use(async (context, next) =>
{
    if (context.Response.StatusCode == 401)
        context.Request.Path = "/User/SignIn";
    else if (context.Response.StatusCode == 500)
    {
        var pathOri = context.Request.Path;
        context.Request.Path = "/GlobalHandling/index";
        var handler = context.Features.Get<IExceptionHandlerPathFeature>();
        var log = context.RequestServices.GetService<Serilog.ILogger>();
        log?.Error(handler?.Error.Message??$"Internl Server Error:{pathOri}");
    }
    await next();
});

app.UseRouting();
app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Strict,
});
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=SignIn}/{id?}");

app.Run();
