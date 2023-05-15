using System.Collections.Specialized;
//using Xamarin.Forms;
//using Xamarin.Forms.Platform.iOS;
using Microsoft.Maui.Controls.Compatibility.Platform.iOS;
//using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform;
using UIKit;
using Rectangle = Microsoft.Maui.Graphics.Rect;

namespace TopTabbedPage
{
    public partial class TopTabbedRenderer : IVisualElementRenderer, IEffectControlProvider
    {
        public event EventHandler<VisualElementChangedEventArgs> ElementChanged;

        public VisualElement Element { get; private set; }

        public UIView NativeView => View;

        public UIViewController ViewController => this;

        public SizeRequest GetDesiredSize(double widthConstraint, double heightConstraint)
        {
            return NativeView.GetSizeRequest(widthConstraint, heightConstraint);
        }

        void IEffectControlProvider.RegisterEffect(Effect effect)
        {
            VisualElementRenderer<VisualElement>.RegisterEffect(effect, View);
        }

        public void SetElement(VisualElement element)
        {
            var oldElement = Element;
            Element = element;

            Tabbed.PropertyChanged += OnPropertyChanged;
            Tabbed.PagesChanged += OnPagesChanged;

            OnElementChanged(new VisualElementChangedEventArgs(oldElement, element));

            OnPagesChanged(null, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

            if (element != null)
            {
                element.SendViewInitialized(NativeView);
            }

            UpdateBarBackgroundColor();
            UpdateBarTextColor();
            UpdateBarIndicatorColor();

            EffectUtilities.RegisterEffectControlProvider(this, oldElement, element);
        }

        public void SetElementSize(Size size)
        {
            if (loaded)
                Element.Layout(new Rectangle(Element.X, Element.Y, size.Width, size.Height));
            else
                queuedSize = size;
        }
    }
}
