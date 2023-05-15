using System;
using UIKit;

namespace TopTabbedPage
{
    public partial class TopTabbedRenderer : IUIPageViewControllerDataSource
    {
        public UIViewController GetPreviousViewController(UIPageViewController pageViewController, UIViewController referenceViewController)
        {
            var index = viewControllers.IndexOf(referenceViewController) - 1;
            if (index < 0)
                return null;

            return viewControllers[index];
        }

        public UIViewController GetNextViewController(UIPageViewController pageViewController, UIViewController referenceViewController)
        {
            var index = viewControllers.IndexOf(referenceViewController) + 1;
            if (index == viewControllers.Count)
                return null;

            return viewControllers[index];
        }
    }
}
