using Foundation;

namespace TopTabbedPage;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
	protected override MauiApp CreateMauiApp()
	{
        // https://github.com/NAXAM/toptabbedpage-xamarin-forms
        // Not sure this is needed. The init method is empty.
        //TopTabbedRenderer.Init();

        return MauiProgram.CreateMauiApp();
	}
}

