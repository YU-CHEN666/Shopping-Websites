using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using MimeDetective;
using WebApplication1.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute());
});
builder.Services.AddSingleton<IContentInspector>(new ContentInspectorBuilder() {
    Definitions = MimeDetective.Definitions.DefaultDefinitions.FileTypes.Images.All()
}.Build());
builder.Services.AddScoped<Database>(sp =>
{
    string connection = builder.Configuration["DBConnect"];
    var httpContentService = sp.GetRequiredService<IHttpContextAccessor>();
    var logger = sp.GetRequiredService<ILogger<Database>>();

	return new Database(connection, httpContentService, logger);
});
builder.Services.AddScoped<FileProcess>();
builder.Services.AddSingleton<ShoppingCarManage>();
builder.Services.Configure<PeriodicTimerSettings>(builder.Configuration.GetSection("PeriodicTimerSettings"));
builder.Services.AddHostedService<CleanAnonymousShoppingCar>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
{
    options.LoginPath = "/Access/Login";
    options.AccessDeniedPath = "/Access/Prohibit";
    options.Cookie.SameSite = SameSiteMode.Strict;
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("OnlyRoot", policy =>
    {
        policy.RequireRole("root");
    });
});
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=HomePage}/{id?}")
    .WithStaticAssets();
app.MapControllers();

app.Run();
