using ProjectPano.Model;
using ProjectPano.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // to preserve PascalCase
    })
;

//i added this 6/23/25 to connect to sharepoint
builder.Services.AddSingleton<GraphSharePointService>();
// added 7/26/25
builder.Services.AddScoped<DAL>();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
