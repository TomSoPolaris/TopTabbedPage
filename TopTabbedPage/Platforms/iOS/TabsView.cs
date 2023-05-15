// Source: https://github.com/NAXAM/toptabbedpage-xamarin-forms/blob/master/Naxam.TopTabbedPage.Platform.iOS/TabsView.cs

using System;
using System.Collections.Generic;
using System.Linq;
using Foundation;
using UIKit;

namespace TopTabbedPage
{
    public class TabsView : UIView, ITabBarDelegate
    {
        public event EventHandler<TabsSelectionChangedEventArgs> TabsSelectionChanged;

        readonly MaterialDesignTabBar tabBar;

        public override UIColor BackgroundColor
        {
            get => base.BackgroundColor;
            set
            {
                base.BackgroundColor = value;

                if (tabBar != null)
                {
                    tabBar.BackgroundColor = value;
                }
            }
        }

        public UIColor IndicatorColor
        {
            get => tabBar.IndicatorColor;
            set
            {
                tabBar.IndicatorColor = value;
            }
        }

        public UIColor TextColor
        {
            get { return tabBar.TextColor; }
            set
            {
                tabBar.TextColor = value;
            }
        }

        public int SelectedIndex
        {
            get { return (int)tabBar.SelectedIndex; }
            set
            {
                tabBar.SelectedIndex = (nuint)value;
            }
        }

        public TabsView()
        {
            tabBar = new MaterialDesignTabBar
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            tabBar.WeakDelegate = this;

            AddSubview(tabBar);

            var layoutAttributes = new[] { NSLayoutAttribute.Bottom, NSLayoutAttribute.Trailing, NSLayoutAttribute.Leading };
            for (int i = 0; i < 3; i++)
            {
                AddConstraint(NSLayoutConstraint.Create(tabBar, layoutAttributes[i], NSLayoutRelation.Equal, this, layoutAttributes[i], 1, 0));
            }

            AddConstraint(NSLayoutConstraint.Create(tabBar, NSLayoutAttribute.Top, NSLayoutRelation.GreaterThanOrEqual, this, NSLayoutAttribute.Top, 1, 0));

            tabBar.AddConstraint(NSLayoutConstraint.Create(tabBar, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1, 80));
        }

        public void DidChangeSelectedIndex(MaterialDesignTabBar tabBar, nuint selectedIndex)
        {
            TabsSelectionChanged?.Invoke(this, new TabsSelectionChangedEventArgs(selectedIndex));
        }

        internal void SetItems(IEnumerable<string> titles)
        {
            tabBar.SetItems(titles.Select(x => new NSString(x)).Cast<NSObject>().ToArray());
        }

        internal void ReplaceItem(string title, int index)
        {
            tabBar.ReplaceItem(new NSString(title), index);
        }

        internal void SetIconItems(IEnumerable<string> icons, IEnumerable<string> titles)
        {
            var iconsAndTitles = new Dictionary<NSObject, NSObject>();
            var i = 0;

            // Build dictionary of icons and their corresponding title text
            foreach (var icon in icons)
            {
                iconsAndTitles.Add(new NSString(icon), new NSString(titles.ElementAtOrDefault(i)));
                i++;
            }

            tabBar.SetIconsAndTitles(iconsAndTitles);
        }        
    }
}
