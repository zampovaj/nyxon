using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Nyxon.Client;
using Nyxon.Core.Extensions;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// logging
builder.Logging.SetMinimumLevel(LogLevel.Warning); 

// register everything
builder.Services.AddClientServices(builder.HostEnvironment);
builder.Services.AddCoreServices();

await builder.Build().RunAsync();