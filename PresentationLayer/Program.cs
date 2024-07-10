using DataAccessLayer.DataContext;
using DataAccessLayer.Repositories;
using DataAccessLayer.Repositories.Contracts;
using BusinessLogicLayer.Services;
using BusinessLogicLayer.Services.Contracts;
using DataAccessLayer.Models;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using PresentationLayer.Hubs;
using PresentationLayer.ErrorDescriber;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;
using Microsoft.AspNetCore.Mvc.Razor;

var builder = WebApplication.CreateBuilder(args);

// Add Google Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = GoogleDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme)
.AddGoogle(GoogleDefaults.AuthenticationScheme, options =>
{
    options.ClientId = builder.Configuration.GetSection("GoogleKeys:ClientId").Value;
    options.ClientSecret = builder.Configuration.GetSection("GoogleKeys:ClientSecret").Value;
    options.SaveTokens = true;
});


// Добавление CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost/")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

//Добавление локализации
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var supportedCultures = new List<CultureInfo>
    {
        new CultureInfo("en"),
        new CultureInfo("ru")
    };

    options.DefaultRequestCulture = new RequestCulture("ru");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
});

builder.Services.AddControllersWithViews()
    .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
    .AddDataAnnotationsLocalization();

// Настройка кэша
builder.Services.AddDistributedMemoryCache();

// Настройка сессий
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// Configure cookie authentication
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromDays(30); // Длительность сеанса при RememberMe
    options.SlidingExpiration = true;
    options.LoginPath = "/Home/Login";
    options.LogoutPath = "/Home/Logout";
    options.AccessDeniedPath = "/Home/AccessDenied";
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
});

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
});


// Добавление сервисов в контейнер
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<DataStoreDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Sql"));
});

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Sql"));
});

builder.Services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IQuestionsService, QuestionsService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddSignalR();

builder.Services.AddHttpContextAccessor();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
});

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.Password.RequiredUniqueChars = 0;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 8;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
})
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddErrorDescriber<LocalizedIdentityErrorDescriber>()
    .AddDefaultTokenProviders();

// Регистрация LocalizedIdentityErrorDescriber
builder.Services.AddScoped<LocalizedIdentityErrorDescriber>();

// Добавление защиты данных
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\temp\keys\"))
    .SetApplicationName("Quiz")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(14));


var app = builder.Build();

app.UseCors();

// Настройка конвейера обработки HTTP-запросов
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseSession();
app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax,
});
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Login}/{id?}");

app.MapHub<GameHub>("/gamehub");

app.Run();
