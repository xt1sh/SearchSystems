using Azure;
using BookSearch.Configs;
using BookSearch.Services;
using Microsoft.Extensions.Azure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

var azureSearchConfig = builder.Configuration.Get<AzureSearchConfig>();

builder.Services.AddAzureClients(b => b.AddSearchClient(azureSearchConfig.ServiceUrl, azureSearchConfig.IndexName, new AzureKeyCredential(azureSearchConfig.ApiKey)));

builder.Services.AddTransient<IAzureSearchService, AzureSearchService>();
builder.Services.AddTransient<IVectorizerApi, VectorizerApi>();

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
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Books}/{action=Search}/{id?}")
    .WithStaticAssets();


app.Run();
