using BookInventory.Data;
using BookInventory.Filters;
using BookInventory.Repositories;
using BookInventory.Services;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

// Add services to the container.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<ClearEnquiryCacheFilter>();
});

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24); // ✅ 24 hours
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// MongoDB
builder.Services.AddSingleton<MongoContext>();

builder.Services.AddScoped<CounterRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddScoped<BookRepository>();
builder.Services.AddScoped<BookService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<InvoiceService>();
builder.Services.AddScoped<InvoiceRepository>();
builder.Services.AddScoped<InvoicePdfService>();
builder.Services.AddScoped<PublishingServiceRepository>();
builder.Services.AddScoped<PublishingServiceService>();
builder.Services.AddScoped<PublishingInvoiceRepository>();
builder.Services.AddScoped<PublishingInvoicePdfService>();
builder.Services.AddScoped<EnquiryRepository>();

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
app.UseSession();

app.UseAuthorization();

// Default route → Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
