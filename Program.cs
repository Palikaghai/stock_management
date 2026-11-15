using SmartStock.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

// Product & Category CRUD Services
builder.Services.AddSingleton<SmartStockDataStore>();
builder.Services.AddSingleton<ICategoryService, CategoryService>();
builder.Services.AddSingleton<IProductService, ProductService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();