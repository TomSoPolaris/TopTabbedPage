using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Compatibility.Hosting;

namespace TopTabbedPage;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
			})
            .UseMauiCompatibility()
            .ConfigureMauiHandlers(handlers =>
			{
				#if IOS

				// The tabs only appear if I don't register this. But they only appear on the bottom of the screen
                handlers.AddCompatibilityRenderer(typeof(CustomTopTabbedPage), typeof(TopTabbedRenderer));

				#endif
			});

#if IOS
        // https://github.com/NAXAM/toptabbedpage-xamarin-forms
        // Not sure this is needed. The Init method is empty.
        TopTabbedRenderer.Init();
#endif

#if DEBUG
        builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}

