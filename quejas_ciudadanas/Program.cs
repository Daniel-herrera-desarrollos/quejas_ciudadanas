using Microsoft.EntityFrameworkCore;
using quejas_ciudadanas.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("QuejasCiudadanaDb");
builder.Services.AddDbContext<QuejasCiudadanasContext>(options =>
	options.UseSqlServer(connectionString));

// Add services to the container.
builder.Services.AddControllersWithViews();

// Habilita sesiones
builder.Services.AddSession(options =>
{
	options.IdleTimeout = TimeSpan.FromMinutes(30); // Tiempo de expiración de la sesión
	options.Cookie.HttpOnly = true;
	options.Cookie.IsEssential = true;
});

builder.Services.AddHttpContextAccessor(); // Permite acceder al contexto HTTP

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();

app.UseRouting();

// Habilitar la sesión aquí
app.UseSession();  // <-- Agrega esta línea

app.UseAuthorization();

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
