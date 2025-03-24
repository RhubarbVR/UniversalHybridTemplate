using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.FluentUI.AspNetCore.Components;

namespace UniversalHybridTemplate_Web;

public class Program
{
	public static async Task Main(string[] args) {
		UniversalSystemCalls.SystemCaller.SetUpDefaultSystemCaller();
		var builder = WebAssemblyHostBuilder.CreateDefault(args);
		builder.RootComponents.Add<App>("#app");
		builder.RootComponents.Add<HeadOutlet>("head::after");


		var unused1 = builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
		var unused = builder.Services.AddFluentUIComponents();

		await builder.Build().RunAsync();
	}
}
