using AppBroker.Entity;
using BusinessCore.Entity;
using BusinessCore.Interfaces;
using BusinessCore.Services;
using Infrastructure.Data;
using Infrastructure.Interfaces;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
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

var appSettingSection = builder.Configuration.GetSection("JwtSetting");
builder.Services.AddDbContextPool<AppDbContext>(opt =>
{
    opt.UseSqlServer(conDefault);
});

builder.Services.Configure<JwtSetting>(appSettingSection);
var appSettings = appSettingSection.Get<JwtSetting>();
if (appSettings == null) throw new Exception("Asspsetting Object null");
builder.Services.AddAuthentication(opt =>
{
    opt.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(opt =>
{
    opt.SaveToken = true;
    opt.RequireHttpsMetadata = false;
    opt.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
    {
        ValidIssuer = appSettings.IsUser,
        ValidAudience = appSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.SecretKey)),
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero
    };
});
builder.Services.AddAuthorization();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddAutoMapper(typeof(MapperProfile));
builder.Services.AddScoped<IHelper, Helper>();
builder.Services.AddScoped<IUserService, UserService>();

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
    pattern: "{controller=User}/{action=SignIn}/{id?}");

app.Run();
