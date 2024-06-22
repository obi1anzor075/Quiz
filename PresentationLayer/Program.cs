using DataAccessLayer.DataContext;
using DataAccessLayer.Repositories;
using DataAccessLayer.Repositories.Contracts;
using BusinessLogicLayer.Services;
using BusinessLogicLayer.Services.Contracts;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using PresentationLayer.Hubs;

var builder = WebApplication.CreateBuilder(args);

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

// Настройка кэша
builder.Services.AddDistributedMemoryCache();

// Настройка сессий
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Добавление сервисов в контейнер
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<DataStoreDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("Sql"));
});

builder.Services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));
builder.Services.AddScoped<IQuestionsService, QuestionsService>();

builder.Services.AddSignalR();

builder.Services.AddHttpContextAccessor();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration["Redis:ConnectionString"];
});

// Добавление защиты данных
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(new DirectoryInfo(@"C:\temp\keys\"))
    .SetApplicationName("Quiz")
    .SetDefaultKeyLifetime(TimeSpan.FromDays(14)); // Задаем срок действия ключей


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

app.UseCookiePolicy();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapHub<GameHub>("/gamehub");

app.Run();
