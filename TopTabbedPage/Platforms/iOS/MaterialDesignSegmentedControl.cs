using CoreGraphics;
using Foundation;
using UIKit;

namespace TopTabbedPage
{
    public class MaterialDesignSegmentedControl : UISegmentedControl
    {
        public UIFont Font;
        const int kMDTabBarHeight = 60;
        const int kMDIndicatorHeight = 2;
        IDisposable frameObserver;
        UIView indicatorView;
        UIView beingTouchedView;
        MaterialDesignTabBar tabBar;

        public nfloat HorizontalPadding { get; set; }
        public UIColor RippleColor { get; set; }
        public UIColor IndicatorColor { get; set; }
        public NSMutableArray<UIView> Tabs { get; set; }

        public MaterialDesignSegmentedControl(MaterialDesignTabBar bar)
        {
            Tabs = new NSMutableArray<UIView>();
            indicatorView = new UIView(new CGRect(0, kMDTabBarHeight - kMDIndicatorHeight, 0, kMDIndicatorHeight));
            indicatorView.Tag = int.MaxValue;
            AddSubview(indicatorView);
            AddTarget(SelectionChanged, UIControlEvent.ValueChanged);
            tabBar = bar;
        }

        public override void WillMoveToSuperview(UIView newsuper)
        {
            base.WillMoveToSuperview(newsuper);
            if (newsuper != null)
                frameObserver = newsuper.AddObserver("frame", NSKeyValueObservingOptions.Initial | NSKeyValueObservingOptions.New, OnFrameChanged);
        }        

        public override void RemoveFromSuperview()
        {
            if (frameObserver != null)
                frameObserver.Dispose();
            base.RemoveFromSuperview();
        }

        public override nint SelectedSegment
        {
            get => base.SelectedSegment;
            set
            {
                base.SelectedSegment = value;
                MoveIndicatorToSelectedIndexWithAnimated(true);
            }
        }

        public override void Select(NSObject sender)
        {
            base.Select(sender);
            tabBar.UpdateSelectedIndex(SelectedSegment);
        }        

        public override void InsertSegment(UIImage image, nint pos, bool animated)
        {
            base.InsertSegment(image, pos, animated);
            ResizeItems();
            UpdateSegmentsList();
            PerformSelector(new ObjCRuntime.Selector("MoveIndicatorToSelectedIndexWithAnimated:"), NSNumber.FromBoolean(animated), 0.001f);
        }

        public override void InsertSegment(string title, nint pos, bool animated)
        {
            base.InsertSegment(title, pos, animated);
            ResizeItems();
            UpdateSegmentsList();
            PerformSelector(new ObjCRuntime.Selector("MoveIndicatorToSelectedIndexWithAnimated:"), NSNumber.FromBoolean(animated), 0.001f);
        }

        public override void SetImage(UIImage image, nint segment)
        {
            base.SetImage(image, segment);
            ResizeItems();
            PerformSelector(new ObjCRuntime.Selector("MoveIndicatorToSelectedIndexWithAnimated:"), NSNumber.FromBoolean(true), 0.001f);
        }

        public override void RemoveSegmentAtIndex(nint segment, bool animated)
        {
            base.RemoveSegmentAtIndex(segment, animated);
            UpdateSegmentsList();
            ResizeItems();
            PerformSelector(new ObjCRuntime.Selector("MoveIndicatorToSelectedIndexWithAnimated:"), NSNumber.FromBoolean(animated), 0.001f);
        }

        public void SetIndicatorColor(UIColor color)
        {
            IndicatorColor = color;
            indicatorView.BackgroundColor = color;
        }

        public void SetTextFont(UIFont textFont, UIColor textColor)
        {
            Font = textFont;
            nfloat disabledTextAlpha = 0.6f;

            var normalTextColor = tabBar.NormalTextColor;
            if (normalTextColor == null)
            {
                normalTextColor = textColor.ColorWithAlpha(disabledTextAlpha);
            }

            var normalTextFont = tabBar.NormalTextFont;
            if (normalTextFont == null)
            {
                normalTextFont = textFont;
            }

            var attributes = new UIStringAttributes()
            {
                Font = normalTextFont,
                ForegroundColor = normalTextColor
            };
            SetTitleTextAttributes(attributes, UIControlState.Normal);

            var selectedAttributes = new UIStringAttributes()
            {
                Font = textFont,
                ForegroundColor = textColor
            };
            SetTitleTextAttributes(selectedAttributes, UIControlState.Selected);
        }

        public void MoveIndicatorToFrameWithAnimated(CGRect frame, bool animated)
        {
            if (animated)
            {
                Animate(.2f, () => { indicatorView.Frame = new CGRect(frame.Location.X, Bounds.Size.Height - kMDIndicatorHeight, frame.Size.Width, kMDIndicatorHeight); }, null);
            }
            else
            {
                indicatorView.Frame = new CGRect(frame.Location.X, Bounds.Size.Height - kMDIndicatorHeight, frame.Size.Width, kMDIndicatorHeight);
            }
        }

        [Export("MoveIndicatorToSelectedIndexWithAnimated:")]
        public void MoveIndicatorToSelectedIndexWithAnimated(bool animated)
        {
            if (SelectedSegment < 0 && NumberOfSegments > 0)
            {
                SelectedSegment = 0;
            }
            var index = SelectedSegment;
            var frame = CGRect.Empty;
            if ((Tabs != null) && (index >= 0))
            {
                if ((index >= NumberOfSegments) || (index >= (nint)Tabs.Count))
                {
                    return;
                }
                frame = Tabs[(nuint)SelectedSegment].Frame;
            }
            MoveIndicatorToFrameWithAnimated(frame, animated);
        }

        public CGRect GetSelectedSegmentFrame()
        {
            if (SelectedSegment >= 0 && Tabs.Count > 0)
            {
                var frame = Tabs[(nuint)SelectedSegment].Frame;
                return frame;
            }
            return CGRect.Empty;
        }

        public override void TouchesBegan(NSSet touches, UIEvent evt)
        {
            base.TouchesBegan(touches, evt);
            if (beingTouchedView != null)
                return;
            UITouch touch = touches.AnyObject as UITouch;
            var point = touch.LocationInView(this);
            foreach (UIView view in Subviews)
            {
                if (view.Tag != int.MaxValue && view.Frame.Contains(point))
                {
                    beingTouchedView = view;
                }
            }
        }

        public override void TouchesCancelled(NSSet touches, UIEvent evt)
        {
            base.TouchesCancelled(touches, evt);
            beingTouchedView = null;
        }

        public override void TouchesEnded(NSSet touches, UIEvent evt)
        {
            base.TouchesEnded(touches, evt);
            beingTouchedView = null;
        }

        private void SelectionChanged(object sender, EventArgs e)
        {
            MoveIndicatorToSelectedIndexWithAnimated(true);
            tabBar.UpdateSelectedIndex(SelectedSegment);
        }

        private void OnFrameChanged(NSObservedChange obj)
        {
            ResizeItems();
            UpdateSegmentsList();
            MoveIndicatorToSelectedIndexWithAnimated(false);
        }

        void ResizeItems()
        {
            if (NumberOfSegments <= 0)
                return;
            nfloat maxItemSize = 0;
            nfloat segmentedControlWidth = 0;
            var attributes = new UIStringAttributes()
            {
                Font = Font
            };
            for (int i = 0; i < NumberOfSegments; i++)
            {
                var title = TitleAt(i);
                var itemSize = CGSize.Empty;
                if (!string.IsNullOrEmpty(title))
                {
                    itemSize = new NSString(title).GetSizeUsingAttributes(attributes);
                }
                else
                {
                    var image = ImageAt(i);
                    var height = Bounds.Size.Height;
                    var width = height / image.Size.Height * image.Size.Width;
                    itemSize = new CGSize(width, height);
                }
                itemSize.Width += HorizontalPadding * 2;
                SetWidth(itemSize.Width, i);
                segmentedControlWidth += itemSize.Width;
                maxItemSize = (nfloat)Math.Max(maxItemSize, itemSize.Width);
            }
            var holderWidth = Superview.Bounds.Size.Width - (tabBar.HorizontalInset * 2);
            if (segmentedControlWidth < holderWidth)
            {
                if (NumberOfSegments * maxItemSize < holderWidth)
                {
                    maxItemSize = holderWidth / NumberOfSegments;
                }
                segmentedControlWidth = 0;
                for (int i = 0; i < NumberOfSegments; i++)
                {
                    SetWidth(maxItemSize, i);
                    segmentedControlWidth += maxItemSize;
                }
            }
            Frame = new CGRect(0, 0, segmentedControlWidth, kMDTabBarHeight);
        }       

        void UpdateSegmentsList()
        {
            var segments = GetSegmentList().MutableCopy();
            if (segments is NSArray tabs)
            {
                Tabs = new NSMutableArray<UIView>(NSArray.FromArray<UIView>(tabs));
            }
        }

        NSArray GetSegmentList()
        {
            LayoutIfNeeded();
            var segments = new NSMutableArray((nuint)NumberOfSegments);
            foreach (var view in from UIView view in Subviews
                                 where view.Class.Name == "UISegment"
                                 select view)
            {
                segments.Add(view);
            }

            var sortedSegments = segments.Sort((a, b) =>
            {
                if (!(!(a is UIView viewA) || !(b is UIView viewB)))
                {
                    if (viewA.Frame.Location.X < viewB.Frame.Location.X)
                    {
                        return NSComparisonResult.Ascending;
                    }
                    else if (viewA.Frame.Location.X > viewB.Frame.Location.X)
                    {
                        return NSComparisonResult.Descending;
                    }
                    return NSComparisonResult.Same;
                }
                return NSComparisonResult.Same;
            });
            return sortedSegments;
        }        
    }
}
