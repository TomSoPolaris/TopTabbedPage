using System;
using System.Linq;
using UIKit;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;

namespace TopTabbedPage
{
    static class ElementExtensions
    {
        public static void SendViewInitialized(this VisualElement element, UIView nativeView)
        {
            // Old XF code
            //var method = typeof(Forms).GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            //                                        .FirstOrDefault(x => x.Name == nameof(SendViewInitialized));


            // This returns null. Can't find the method
            //var method = typeof(MauiContext).GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
            //                                        .FirstOrDefault(x => x.Name == nameof(SendViewInitialized));


            // Method now lives here: https://github.com/dotnet/maui/blob/1ab0c639f83e4ddc7c3cda979ced74e05a155c21/src/Compatibility/Core/src/iOS/Forms.cs#L184
            var method = typeof(Microsoft.Maui.Controls.Compatibility.Forms).GetMethods(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .FirstOrDefault(x => x.Name == nameof(SendViewInitialized));

            method.Invoke(null, new object[] { element, nativeView });
        }
    }
}
