using System;
using System.Collections.Generic;
using System.Linq;
using UIKit;
//using Xamarin.Forms;
//using Xamarin.Forms.Platform.iOS;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;

namespace TopTabbedPage
{
    public partial class TopTabbedRenderer
    {
        void HandlePageViewControllerDidFinishAnimating(object sender, UIPageViewFinishedAnimationEventArgs e)
        {
            if (pageViewController.ViewControllers.Length == 0)
                return;

            selectedViewController = pageViewController.ViewControllers[0];
            var index = viewControllers.IndexOf(selectedViewController);

            tabBar.SelectedIndex = index;
            lastSelectedIndex = index;
            UpdateToolbarItems(index);
        }

        void HandleTabsSelectionChanged(object sender, TabsSelectionChangedEventArgs e)
        {
            MoveToByIndex((int)e.SelectedIndex);
        }

        void MoveToByIndex(int selectedIndex, bool forced = false)
        {
            if (selectedIndex == lastSelectedIndex && !forced)
                return;

            var nextPage = Tabbed.Children.ElementAt(selectedIndex);
            if (Tabbed.CurrentPage != nextPage)
            {
                Tabbed.CurrentPage = nextPage;
                return;
            }

            var direction = lastSelectedIndex < selectedIndex
                             ? UIPageViewControllerNavigationDirection.Forward
                             : UIPageViewControllerNavigationDirection.Reverse;

            lastSelectedIndex = selectedIndex;

            selectedViewController = viewControllers[lastSelectedIndex];

            UpdateToolbarItems(selectedIndex);

            pageViewController.SetViewControllers(
                new[] { selectedViewController }, direction, false, (finished) =>
                {
                    var failed = pageViewController.ViewControllers.Length == 0 || pageViewController.ViewControllers[0] != selectedViewController;
                    if (failed)
                    {   //Sometimes setViewControllers doesn't work as expected
                        pageViewController.SetViewControllers(new[] { selectedViewController }, direction, false, null);
                    }
                });
        }

        void UpdateToolbarItems(int selectedIndex)
        {
            if (NavigationController == null)
                return;

            var toolbarItems = new List<ToolbarItem>(Tabbed.ToolbarItems);

            var newChild = Tabbed.Children[selectedIndex];
            var navigationItem = this.NavigationController.TopViewController.NavigationItem;
            toolbarItems.AddRange(newChild.ToolbarItems);

            navigationItem.SetRightBarButtonItems(toolbarItems.Select(x => x.ToUIBarButtonItem()).ToArray(), false);
        }
    }
}
