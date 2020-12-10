//
// Created by Ramtin Jokar [ Ramtinak@live.com ]
// 2018 - Paeiz 1397
//

using System;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;

namespace Minista.Helpers
{
    public class GestureHelper: IDisposable
    {
        #region Properties
        const double MAX = 15;
        const double MAX_SWIPE_DISTANCE = 50;
        private bool _isSwiped;
        private Point _manipulationStarted;
        public UIElement Element { get; private set; }
        public GestureMode GestureMode { get; private set; }
        #endregion Properties

        #region Events
        public event EventHandler<EventArgs> LeftSwipe;
        public event EventHandler<EventArgs> RightSwipe;
        public event EventHandler<EventArgs> UpSwipe;
        public event EventHandler<EventArgs> DownSwipe;
        #endregion Events

        public GestureHelper(UIElement uiElement, GestureMode gestureMode = GestureMode.UpDown)
        {
            Element = uiElement;
            GestureMode = gestureMode;
            switch (gestureMode)
            {
                case GestureMode.All:
                    Element.ManipulationMode = ManipulationModes.TranslateY |
                        ManipulationModes.TranslateInertia |
                        ManipulationModes.TranslateX |
                        ManipulationModes.System;
                    break;
                case GestureMode.LeftRight:
                    Element.ManipulationMode = ManipulationModes.TranslateInertia |
                        ManipulationModes.TranslateX |
                        ManipulationModes.System;
                    break;
                default:
                case GestureMode.UpDown:
                    Element.ManipulationMode = ManipulationModes.TranslateY |
                        ManipulationModes.TranslateInertia |
                        ManipulationModes.System;
                    break;
            }

            Element.ManipulationStarted += UiManipulationStarted;
            Element.ManipulationDelta += UiManipulationDelta;
            Element.ManipulationCompleted += UiManipulationCompleted;
        }
        ~GestureHelper() => Dispose();

        private void UiManipulationStarted(object sender, ManipulationStartedRoutedEventArgs e)
        {
            _manipulationStarted = e.Cumulative.Translation;
        }
        private void UiManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            var current = e.Cumulative.Translation;

            if (GestureMode == GestureMode.All)
            {
                if (_manipulationStarted.X + MAX <= current.X && Math.Abs(current.Y) > 0 ||
                    _manipulationStarted.X - MAX <= current.X && Math.Abs(current.Y) > 0)
                {
                    CalculateUpDown(current, e.IsInertial);

                    return;
                }
                if (_manipulationStarted.Y + MAX <= current.Y && Math.Abs(current.X) > 0 ||
                    _manipulationStarted.Y - MAX <= current.Y && Math.Abs(current.X) > 0)
                {
                    CalculateLeftRight(current, e.IsInertial);
                }
            }
            else if (GestureMode == GestureMode.UpDown)
                CalculateUpDown(current, e.IsInertial);
            else
                CalculateLeftRight(current, e.IsInertial);
        }
        private void UiManipulationCompleted(object sender, ManipulationCompletedRoutedEventArgs e)
        {
            _isSwiped = false;
        }

        void CalculateLeftRight(Point current, bool isInertial)
        {
            if (isInertial && !_isSwiped)
            {
                var swipedDistance = current.X;
                if (Math.Abs(swipedDistance) <= MAX_SWIPE_DISTANCE) return;

                if (swipedDistance > 0)
                    RightSwipe?.Invoke(this, EventArgs.Empty);
                else
                    LeftSwipe?.Invoke(this, EventArgs.Empty);
                _isSwiped = true;
            }
        }
        void CalculateUpDown(Point current, bool isInertial)
        {
            if (isInertial && !_isSwiped)
            {
                var swipedDistance = current.Y;
                if (Math.Abs(swipedDistance) <= MAX_SWIPE_DISTANCE) return;

                if (swipedDistance > 0)
                    DownSwipe?.Invoke(this, EventArgs.Empty);
                else
                    UpSwipe?.Invoke(this, EventArgs.Empty);
                _isSwiped = true;
            }
        }

        public void Dispose()
        {
            Element = null;
        }
    }
    public enum GestureMode
    {
        UpDown,
        LeftRight,
        All
    }
}
