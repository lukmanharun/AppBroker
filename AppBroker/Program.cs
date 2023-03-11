using AppBroker.Entity;
using AppBroker.Interfaces;
using AppBroker.Services;
using BusinessCore.Entity;
using BusinessCore.Interfaces;
using BusinessCore.Services;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Sinks.MSSqlServer;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var conDefault = builder.Configuration.GetConnectionString("Default");
#region Serilog

string environment = string.Empty;
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == null) throw new Exception("Environtment object null");
ColumnOptions columnOptions = new ColumnOptions();
columnOptions.Store.Remove(StandardColumn.Properties);
columnOptions.Store.Add(StandardColumn.LogEvent);
columnOptions.LogEvent.DataLength = 2048;
columnOptions.PrimaryKey = columnOptions.TimeStamp;
columnOptions.TimeStamp.NonClusteredIndex = true;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Debug()
    .WriteTo.Console()
    .WriteTo.File("bin/AppLog/Log.text",
        rollingInterval: RollingInterval.Day,
        rollOnFileSizeLimit: true,
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
    .WriteTo.MSSqlServer(
        connectionString: conDefault,
        sinkOptions: new Serilog.Sinks.MSSqlServer.MSSqlServerSinkOptions
        {
            TableName = "Logs"
        },
        columnOptions:columnOptions
    )
    //.WriteTo.Elasticsearch(ConfigureElasticSink(configuration, environment))
    .Enrich.WithProperty("Environment", environment)
    .ReadFrom.Configuration(new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile(
        $"appsettings.{environment}.json",
        optional: true)
    .Build())
    .CreateLogger();

builder.Host.UseSerilog();
#endregion

builder.Services.AddDbContextPool<AppDbContext>(opt =>
{
    opt.UseSqlServer(conDefault);
});

builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAutoMapper(typeof(MapperProfile));
builder.Services.AddScoped<IHelper, Helper>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IHelperService, HelperService>();

builder.Services.AddDirectoryBrowser();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

var folder = builder.Configuration.GetSection("FileManagement").GetSection("Folder").Value??"Resources";

if (!Directory.Exists(Path.Combine(builder.Environment.ContentRootPath, folder)))
{
    Directory.CreateDirectory(Path.Combine(builder.Environment.ContentRootPath, folder));
}
var fileProvider = new PhysicalFileProvider(Path.Combine(builder.Environment.ContentRootPath, folder));
var folderRoot = $"/{folder}";
app.UseStaticFiles( new StaticFileOptions
{
    FileProvider = fileProvider,
    RequestPath = folderRoot,
});
app.UseDirectoryBrowser(new DirectoryBrowserOptions
{
    FileProvider = fileProvider,
    RequestPath = folderRoot,
});
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=SignIn}/{id?}");

app.Run();
