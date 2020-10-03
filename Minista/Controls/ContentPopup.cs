// Copyright (c) Unigram

using System.Linq;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Shapes;
#pragma warning disable IDE0019 // Use pattern matching

namespace Minista
{
    public class ContentPopup : ContentDialog
    {
        public bool IsLightDismissEnabled { get; set; } = true;
        public ContentPopup() => DefaultStyleKey = typeof(ContentPopup);
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            var canvas = this.GetAncestorsOfType<DependencyObject>().FirstOrDefault() as Canvas;
            if (canvas == null)
                return;
            var rectangle = canvas.Children[0] as Rectangle;
            if (rectangle == null)
                return;
            rectangle.PointerReleased += Rectangle_PointerReleased;
        }
        private void Rectangle_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            var pointer = e.GetCurrentPoint(this);
            if (pointer.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased && IsLightDismissEnabled) Hide();
        }
    }
}
