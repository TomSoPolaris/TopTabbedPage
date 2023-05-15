using System;
using UIKit;
//using Xamarin.Forms;
using Rectangle = Microsoft.Maui.Graphics.Rect;

namespace TopTabbedPage
{
    public partial class TopTabbedRenderer
	{
		public override void DidMoveToParentViewController(UIViewController parent)
		{
			base.DidMoveToParentViewController(parent);

			tabBarHeight.Constant = 60;
        }

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();

			if (Element == null)
				return;
			if (!Element.Bounds.IsEmpty)
			{
				View.Frame = new System.Drawing.RectangleF((float)Element.X, (float)Element.Y, (float)Element.Width, (float)Element.Height);
			}

			var tabsFrame = tabBar.Frame;
			var frame = ParentViewController != null ? ParentViewController.View.Frame : View.Frame;
			var height = frame.Height - 20; // Fix gray area at the bottom
			PageController.ContainerArea = new Rectangle(0, 0, frame.Width, height);

			if (!queuedSize.IsZero)
			{
				Element.Layout(new Rectangle(Element.X, Element.Y, queuedSize.Width, queuedSize.Height));
				queuedSize = Size.Zero;
			}

			pageViewController.SetViewControllers(pageViewController.ViewControllers, UIPageViewControllerNavigationDirection.Forward, false, null);

			loaded = true;
		}
	}
}
