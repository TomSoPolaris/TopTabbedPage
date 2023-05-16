//using Xamarin.Forms;
using System.Collections.Specialized;
using System.ComponentModel;
using Foundation;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Platform;
//using Xamarin.Forms.Internals;
//using Xamarin.Forms.Platform.iOS;
using TopTabbedPage;
using UIKit;
using Platform = Microsoft.Maui.Controls.Compatibility.Platform.iOS.Platform;

//[assembly: ExportRenderer(typeof(CustomTopTabbedPage), typeof(TopTabbedRenderer))]
namespace TopTabbedPage
{
    public partial class TopTabbedRenderer : UIViewController
    {
        public static new void Init()
        {
        }
        protected UIViewController selectedViewController;
        protected IList<UIViewController> viewControllers;

        UIColor defaultBarColor;
        bool defaultBarColorSet;
        bool loaded;
        Size queuedSize;
        int lastSelectedIndex;

        Page Page => Element as Page;

        UIPageViewController pageViewController;

        

        protected IPageController PageController
        {
            get { return Page; }
        }

        protected CustomTopTabbedPage Tabbed
        {
            get { return (CustomTopTabbedPage)Element; }
        }

        protected TabsView tabBar;
        private NSLayoutConstraint tabBarHeight;

        public TopTabbedRenderer()
        {
            viewControllers = new UIViewController[0];

            pageViewController = new UIPageViewController(UIPageViewControllerTransitionStyle.Scroll, UIPageViewControllerNavigationOrientation.Horizontal, UIPageViewControllerSpineLocation.None);

            tabBar = new TabsView
            {
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            tabBar.TabsSelectionChanged += HandleTabsSelectionChanged;
        }

        public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
        {
            base.DidRotate(fromInterfaceOrientation);

            View.SetNeedsLayout();
        }

        public override void ViewDidAppear(bool animated)
        {
            PageController.SendAppearing();
            base.ViewDidAppear(animated);
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);
            PageController.SendDisappearing();
        }

        public override void ViewWillAppear(bool animated)
        {
            if (!loaded && pageViewController.ViewControllers.Length == 0 && lastSelectedIndex >= 0 && lastSelectedIndex < viewControllers.Count)
            {
                pageViewController.SetViewControllers(new[] { viewControllers[lastSelectedIndex] }, UIPageViewControllerNavigationDirection.Forward, true, null);
                loaded = true;
            }

            base.ViewWillAppear(animated);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            View.AddSubview(tabBar); // Root view

            AddChildViewController(pageViewController);
            View.AddSubview(pageViewController.View);
            pageViewController.View.TranslatesAutoresizingMaskIntoConstraints = false;
            pageViewController.DidMoveToParentViewController(this);

            var views = NSDictionary.FromObjectsAndKeys(new NSObject[] { tabBar, pageViewController.View }, new NSObject[] { (NSString)"tabbar", (NSString)"content" });

            View.AddConstraints(NSLayoutConstraint.FromVisualFormat("V:|-0-[tabbar]-0-[content]-0-|", 0, null, views));
            View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-0-[tabbar]-0-|", 0, null, views));
            View.AddConstraints(NSLayoutConstraint.FromVisualFormat("H:|-0-[content]-0-|", 0, null, views));

            tabBarHeight = NSLayoutConstraint.Create(tabBar, NSLayoutAttribute.Height, NSLayoutRelation.Equal, 1, 68);

            tabBar.AddConstraint(tabBarHeight);

            UpdateSwipe(Tabbed.SwipeEnabled);
            pageViewController.DidFinishAnimating += HandlePageViewControllerDidFinishAnimating;
        }

        public override UIViewController ChildViewControllerForStatusBarHidden()
        {
            var current = Tabbed.CurrentPage;
            if (current == null)
                return null;

            return GetViewController(current);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                PageController?.SendDisappearing();

                if (Tabbed != null)
                {
                    Tabbed.PropertyChanged -= OnPropertyChanged;
                    Tabbed.PagesChanged -= OnPagesChanged;
                    tabBar.TabsSelectionChanged -= HandleTabsSelectionChanged;
                }

                if (pageViewController != null)
                {
                    pageViewController.WeakDataSource = null;
                    pageViewController.DidFinishAnimating -= HandlePageViewControllerDidFinishAnimating;
                    pageViewController?.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        protected virtual void OnElementChanged(VisualElementChangedEventArgs e)
        {
            ElementChanged?.Invoke(this, e);
        }        

        UIViewController GetViewController(Page page)
        {
            var renderer = Platform.GetRenderer(page);
            return renderer?.ViewController;
        }

        void OnPagePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != Page.TitleProperty.PropertyName)
                return;

            if (!(sender is Page page) || page.Title is null)
                return;

            tabBar.ReplaceItem(page.Title, Tabbed.Children.IndexOf(page));
        }

        void OnPagesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            e.Apply((o, i, c) => SetupPage((Page)o, i), (o, i) => TeardownPage((Page)o, i), Reset);

            SetControllers();

            UIViewController controller = null;
            if (Tabbed.CurrentPage != null)
            {
                controller = GetViewController(Tabbed.CurrentPage);
            }

            if (controller != null && controller != selectedViewController)
            {
                selectedViewController = controller;
                var index = viewControllers.IndexOf(selectedViewController);
                MoveToByIndex(index);
                tabBar.SelectedIndex = index;
            }
        }

        void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TabbedPage.CurrentPage))
            {
                var current = Tabbed.CurrentPage;
                if (current == null)
                    return;

                var controller = GetViewController(current);
                if (controller == null)
                    return;

                selectedViewController = controller;
                var index = viewControllers.IndexOf(selectedViewController);
                MoveToByIndex(index);
                tabBar.SelectedIndex = index;
            }
            else if (e.PropertyName == TabbedPage.BarBackgroundColorProperty.PropertyName)
            {
                UpdateBarBackgroundColor();
            }
            else if (e.PropertyName == TabbedPage.BarTextColorProperty.PropertyName)
            {
                UpdateBarTextColor();
            }
            else if (e.PropertyName == CustomTopTabbedPage.BarIndicatorColorProperty.PropertyName)
            {
                UpdateBarIndicatorColor();
            }
            else if (e.PropertyName == CustomTopTabbedPage.SwipeEnabledColorProperty.PropertyName)
            {
                UpdateSwipe(Tabbed.SwipeEnabled);
            }
        }        

        void UpdateSwipe(bool swipeEnabled)
        {
            pageViewController.WeakDataSource = swipeEnabled ? this : null;
        }

        void Reset()
        {
            var i = 0;
            foreach (var page in Tabbed.Children)
            {
                SetupPage(page, i++);
            }
        }

        void SetControllers()
        {
            var list = new List<UIViewController>();
            var titles = new List<string>();
            var icons = new List<string>();
            for (var i = 0; i < Tabbed.Children.Count; i++)
            {
                var child = Tabbed.Children[i];
                var v = child as VisualElement;
                if (v == null)
                    continue;

                var renderer = Platform.GetRenderer(v);

                if (renderer == null)
                    continue;

                list.Add(renderer.ViewController);
                titles.Add(Tabbed.Children[i].Title);

                if (Tabbed.Children[i].IconImageSource != null)
                    icons.Add(Tabbed.Children[i].IconImageSource.ToString().Remove(0, 6));
            }

            viewControllers = list.ToArray();
            //We reverse the order to match the order from the segmented control
            icons.Reverse();
            titles.Reverse();
            tabBar.SetIconItems(icons, titles);
        }

        void SetupPage(Page page, int index)
        {
            IVisualElementRenderer renderer = Platform.GetRenderer(page);
            if (renderer == null)
            {
                renderer = Platform.CreateRenderer(page);
                Platform.SetRenderer(page, renderer);
            }

            page.PropertyChanged -= OnPagePropertyChanged;
            page.PropertyChanged += OnPagePropertyChanged;
        }

        void TeardownPage(Page page, int index)
        {
            page.PropertyChanged -= OnPagePropertyChanged;

            Platform.SetRenderer(page, null);
        }

        void UpdateBarBackgroundColor()
        {
            if (Tabbed == null || tabBar == null)
                return;

            var barBackgroundColor = Tabbed.BarBackgroundColor;

            if (!defaultBarColorSet)
            {
                defaultBarColor = tabBar.BackgroundColor;

                defaultBarColorSet = true;
            }

            if (barBackgroundColor == null)
                tabBar.BackgroundColor = Colors.Lime.ToPlatform();
            else
                tabBar.BackgroundColor = barBackgroundColor.ToPlatform();
        }

        void UpdateBarTextColor()
        {
            if (Tabbed.BarTextColor == null)
                tabBar.TextColor = Colors.Black.ToPlatform();
            else
                tabBar.TextColor = Tabbed.BarTextColor.ToPlatform();
        }

        void UpdateBarIndicatorColor()
        {
            tabBar.IndicatorColor = Tabbed.BarIndicatorColor.ToPlatform();
        }
    }
}
