// Source: https://github.com/NAXAM/toptabbedpage-xamarin-forms/blob/master/Naxam.TopTabbedPage.Forms/TopTabbedPage.cs

using System;

namespace TopTabbedPage
{
    public class CustomTopTabbedPage : TabbedPage
    {
        public static readonly BindableProperty BarIndicatorColorProperty = BindableProperty.Create(
           nameof(BarIndicatorColor),
           typeof(Color),
           typeof(CustomTopTabbedPage),
           Colors.White,
           BindingMode.OneWay);
        public Color BarIndicatorColor
        {
            get { return (Color)GetValue(BarIndicatorColorProperty); }
            set { SetValue(BarIndicatorColorProperty, value); }
        }


        public static readonly BindableProperty SwipeEnabledColorProperty = BindableProperty.Create(
            nameof(SwipeEnabled),
            typeof(bool),
            typeof(CustomTopTabbedPage),
            true,
            BindingMode.OneWay);
        public bool SwipeEnabled
        {
            get { return (bool)GetValue(SwipeEnabledColorProperty); }
            set { SetValue(SwipeEnabledColorProperty, value); }
        }
    }
}
