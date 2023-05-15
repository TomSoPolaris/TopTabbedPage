using CoreGraphics;
using Foundation;
using UIKit;

namespace TopTabbedPage
{
    public class MaterialDesignTabBar : UIView
    {
        IDisposable frameObserver;
        public MaterialDesignSegmentedControl SegmentedControl { get; set; }
        public UIScrollView ScrollView { get; set; }

        public ITabBarDelegate WeakDelegate { get; set; }

        public nint NumberOfItem { get => SegmentedControl.NumberOfSegments; }
        public NSMutableArray<UIView> Tabs
        {
            get
            {
                return (NSMutableArray<UIView>)SegmentedControl?.Tabs.Copy();
            }
        }

        public MaterialDesignTabBar(NSCoder nSCoder) : base(nSCoder)
        {
            InitContent();
        }

        public MaterialDesignTabBar(CGRect frame) : base(frame)
        {
            InitContent();
        }

        public MaterialDesignTabBar()
        {
            InitContent();
        }

        public override UIColor BackgroundColor
        {
            get => base.BackgroundColor;
            set
            {
                base.BackgroundColor = value;
                ScrollView.BackgroundColor = value;
            }
        }

        private UIColor textColor;
        public UIColor TextColor
        {
            get { return textColor; }
            set
            {
                textColor = value;
                UpdateItemAppearance();
            }
        }

        private UIColor normalTextColor;
        public UIColor NormalTextColor
        {
            get { return normalTextColor; }
            set
            {
                normalTextColor = value;
                UpdateItemAppearance();
            }
        }

        private UIColor indicatorColor;
        public UIColor IndicatorColor
        {
            get { return indicatorColor; }
            set
            {
                indicatorColor = value;
                SegmentedControl?.SetIndicatorColor(value);
            }
        }

        nfloat horizontalPaddingPerItem;
        public nfloat HorizontalPaddingPerItem
        {
            get => horizontalPaddingPerItem;
            set
            {
                horizontalPaddingPerItem = value;
                SegmentedControl.HorizontalPadding = horizontalPaddingPerItem;
            }
        }

        private UIFont textFont;
        public UIFont TextFont
        {
            get => textFont;
            set
            {
                textFont = value;
                UpdateItemAppearance();
            }
        }

        private UIFont normalTextFont;
        public UIFont NormalTextFont
        {
            get => normalTextFont;
            set
            {
                normalTextFont = value;
                UpdateItemAppearance();
            }
        }

        nuint selectedIndex;
        public nuint SelectedIndex
        {
            get => selectedIndex;
            set
            {
                if (value < (nuint)SegmentedControl?.NumberOfSegments)
                {
                    selectedIndex = value;
                    SegmentedControl.TintColor = UIColor.White;

                    if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                        SegmentedControl.SelectedSegmentTintColor = UIColor.Red;

                    if ((nuint)SegmentedControl?.SelectedSegment != selectedIndex)
                    {
                        SegmentedControl.SelectedSegment = (nint)selectedIndex;
                        ScrollToSelectedIndex();
                    }
                }
            }
        }

        float horizontalInset;
        public float HorizontalInset
        {
            get => horizontalInset;
            set
            {
                horizontalInset = value;
                SetNeedsLayout();
            }
        }        

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            ScrollView.Frame = new CGRect(0, 0, Bounds.Size.Width, Bounds.Size.Height);
            ScrollView.ContentInset = new UIEdgeInsets(0, HorizontalInset, 0, HorizontalInset);
            ScrollView.ContentSize = SegmentedControl.Bounds.Size;
        }

        public override void WillMoveToSuperview(UIView newsuper)
        {
            base.WillMoveToSuperview(newsuper);
            frameObserver = SegmentedControl.AddObserver("frame", NSKeyValueObservingOptions.Initial | NSKeyValueObservingOptions.New, OnFrameChanged);
        }        

        public override void RemoveFromSuperview()
        {
            if (frameObserver != null)
                frameObserver.Dispose();
            base.RemoveFromSuperview();
        }

        public void UpdateSelectedIndex(nint selectedIndex)
        {
            SelectedIndex = (nuint)selectedIndex;
            ScrollToSelectedIndex();
            if (WeakDelegate != null)
            {
                WeakDelegate.DidChangeSelectedIndex(this, (nuint)selectedIndex);
            }
        }

        public void SetItems(NSObject[] items)
        {
            SegmentedControl?.RemoveAllSegments();
            nint index = 0;
            foreach (var item in items)
            {
                InsertItemAtIndex(item, index, false);
                index++;
            }
            SelectedIndex = 0;
        }

        public void ReplaceItem(NSObject item, nint index)
        {
            if (item is NSString str)
            {
                SegmentedControl?.SetTitle((string)str, index);
            }
            else if (item is UIImage image)
            {
                SegmentedControl?.SetImage(image, index);
            }
        }

        /// <summary>
        /// Draw the supplied text and icon on the same image for each segment control
        /// </summary>
        public void SetIconsAndTitles(Dictionary<NSObject, NSObject> iconsAndTitles)
        {
            SegmentedControl?.RemoveAllSegments();
            nint index = 0;

            foreach (var item in iconsAndTitles)
            {
                var imageAndTitle = EmbedTextInImage(item);
                InsertItemAtIndex(imageAndTitle, index, true);
            }

            SelectedIndex = 0;
        }
       
        public void MoveIndicatorToFrame(CGRect frame, bool animated)
        {
            SegmentedControl?.MoveIndicatorToFrameWithAnimated(frame, animated);
        }

        void InitContent()
        {
            SegmentedControl = new MaterialDesignSegmentedControl(this);
            var image = new UIImage();
            SegmentedControl.SetBackgroundImage(image, UIControlState.Normal, UIBarMetrics.Default);
            SegmentedControl.SetDividerImage(image, UIControlState.Normal, UIControlState.Normal, UIBarMetrics.Default);
            if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
            {
                SegmentedControl.SelectedSegmentTintColor = UIColor.Clear;
            }
            SegmentedControl.BackgroundColor = UIColor.Clear;
            ScrollView = new UIScrollView();
            ScrollView.ShowsHorizontalScrollIndicator = false;
            ScrollView.ShowsVerticalScrollIndicator = false;
            ScrollView.Bounces = false;
            ScrollView.AddSubview(SegmentedControl);
            AddSubview(ScrollView);
            HorizontalPaddingPerItem = Device.Idiom == TargetIdiom.Tablet ? 24 : 0;
            SegmentedControl.HorizontalPadding = HorizontalPaddingPerItem;
        }

        private UIImage EmbedTextInImage(KeyValuePair<NSObject, NSObject> itemDict)
        {
            var image = UIImage.FromBundle(itemDict.Key.ToString());
            var title = (NSString)itemDict.Value;

            var font = UIFont.SystemFontOfSize((nfloat)12); // 12

            const int iconToTitleSpacing = 15;
            var expectedTextSize = title.GetSizeUsingAttributes(new UIStringAttributes { Font = font });
            var width = expectedTextSize.Width;
            var height = expectedTextSize.Height + image.Size.Height + iconToTitleSpacing; // make image tall enough for icon + title text
            var size = new CGSize(width, height);

            UIGraphics.BeginImageContextWithOptions(size, false, 0);
            var context = UIGraphics.GetCurrentContext();
            context.SetFillColor(UIColor.White.CGColor);

            var fontTopPos = height / 2;
            var textOrigin = 0;
            var textPoint = new CGPoint(textOrigin, fontTopPos);

            title.DrawString(textPoint, font);
            var flipVertical = new CGAffineTransform(1, 0, 0, -1, 0, size.Height);
            context.ConcatCTM(flipVertical);

            var alignment = (width - image.Size.Width) / 2.0;
            var yPosition = ((height - image.Size.Height) / 2.0) + iconToTitleSpacing;
            var bounds = new CGRect(alignment, yPosition, image.Size.Width, image.Size.Height);
            context.DrawImage(bounds, image.CGImage);

            var imageWithText = UIGraphics.GetImageFromCurrentImageContext();
            UIGraphics.EndImageContext();

            return imageWithText;
        }

        private void OnFrameChanged(NSObservedChange obj)
        {
            ScrollView.ContentSize = SegmentedControl.Bounds.Size;
        }

        void UpdateItemAppearance()
        {
            if (TextColor != null || TextFont != null)
            {
                SegmentedControl?.SetTextFont(TextFont, TextColor);
            }
        }

        void ScrollToSelectedIndex()
        {
            var frame = SegmentedControl.GetSelectedSegmentFrame();
            nfloat horizontalInset = HorizontalInset;
            var contentOffset = frame.Location.X + horizontalInset - ((Frame.Size.Width - frame.Size.Width) / 2);
            if (contentOffset > ScrollView.ContentSize.Width + horizontalInset - Frame.Size.Width)
            {
                contentOffset = ScrollView.ContentSize.Width + horizontalInset - Frame.Size.Width;
            }
            else if (contentOffset < -horizontalInset)
            {
                contentOffset = -horizontalInset;
            }
            ScrollView.SetContentOffset(new CGPoint(contentOffset, 0), true);
        }        

        void InsertItemAtIndex(NSObject item, nint index, bool animated)
        {
            if (item is NSString str)
            {
                SegmentedControl?.InsertSegment((string)str, index, animated);
            }
            else if (item is UIImage image)
            {
                SegmentedControl?.InsertSegment(image, index, animated);
            }
        }

        void RemoveItemAtIndex(nint index, bool animated)
        {
            SegmentedControl?.RemoveSegmentAtIndex(index, animated);
        }        
    }
}
