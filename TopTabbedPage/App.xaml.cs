namespace TopTabbedPage;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();

        // https://github.com/NAXAM/toptabbedpage-xamarin-forms
        MainPage = new NaxamTabbedPage();
    }
}

