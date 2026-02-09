using BookInventory.Data;
using BookInventory.Filters;
using BookInventory.Repositories;
using BookInventory.Services;
using QuestPDF.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

QuestPDF.Settings.License = LicenseType.Community;

// Add services to the container.
builder.Services.AddControllersWithViews();

// Session
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(24);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;

    // ✅ FIX FOR HTTP IIS
    options.Cookie.SameSite = SameSiteMode.Lax;

    // for https
    //options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    // for http
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});

// MongoDB
builder.Services.AddSingleton<MongoContext>();

builder.Services.AddScoped<ClearEnquiryCacheFilter>();

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
builder.Services.AddScoped<MongoLogRepository>();
builder.Services.AddHttpClient<WhatsAppService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

//Commented this for HTTP/IIS
//app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseCookiePolicy(new CookiePolicyOptions
{
    MinimumSameSitePolicy = SameSiteMode.Lax
});
app.UseSession();

app.UseAuthorization();

// Default route → Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();
