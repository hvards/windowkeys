using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using WindowKeys.Interfaces;
using WindowKeys.Settings;

namespace WindowKeys;

public static class Program
{
	[STAThread]
	private static void Main()
	{
		var serviceCollection = new ServiceCollection();
		var configuration = new ConfigurationBuilder()
			.SetBasePath(AppContext.BaseDirectory)
			.AddJsonFile("appsettings.json")
			.Build();
		Log.Logger = new LoggerConfiguration()
			.ReadFrom.Configuration(configuration)
			.CreateLogger();

		ConfigureServices(serviceCollection, configuration);

		var serviceProvider = serviceCollection.BuildServiceProvider();
		var keyboardEventHandler = serviceProvider.GetRequiredService<IKeyboardEventHandler>();
		keyboardEventHandler.Start();

		Application.Run();
	}

	private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
	{
		services.AddLogging(config => { config.AddSerilog(); });

		services.AddOptions()
			.Configure<ActivationSettings>(
				options => configuration.GetSection(nameof(ActivationSettings)).Bind(options))
			.Configure<OverlaySettings>(
				options => configuration.GetSection(nameof(OverlaySettings)).Bind(options));

		services.AddSingleton<IKeyboardEventHandler, KeyboardEventHandler>();
		services.AddSingleton<INativeHelper, NativeHelper>();
		services.AddSingleton<IWindowHandler, WindowHandler>();
		services.AddSingleton<ICombinationGenerator, CombinationGenerator>();
		services.AddSingleton<IGeometry, Geometry>();
	}
}
