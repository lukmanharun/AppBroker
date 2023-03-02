using AppBroker.Entity;
using Serilog;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
#region Serilog

string environment = string.Empty;
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == null) throw new Exception("Environtment object null");
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Debug()
    .WriteTo.Console()
    .WriteTo.File("bin/AppLog/Log.text",
        rollingInterval: RollingInterval.Day,
        rollOnFileSizeLimit: true,
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Error)
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

var appSettingSection = builder.Configuration.GetSection("JwtSetting");
var conDefault = builder.Configuration.GetConnectionString("Default");
builder.Services.Configure<JwtSetting>(appSettingSection);
var appSettings = appSettingSection.Get<JwtSetting>();
if (appSettings == null) throw new Exception("Asspsetting Object null");
var key = Encoding.UTF8.GetBytes(appSettings.SecretKey);

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=User}/{action=Index}/{id?}");

app.Run();
