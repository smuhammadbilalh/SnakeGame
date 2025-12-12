#if WINDOWS
using Microsoft.UI.Xaml.Input;
using Windows.System;

namespace SnakeGame.Platforms.Windows
{
    public static class KeyboardHelper
    {
        public static event EventHandler<VirtualKey> KeyPressed;
        private static bool _isInitialized = false;

        public static void Initialize()
        {
            if (_isInitialized) return;

            // Delay initialization until window is ready
            Microsoft.Maui.Controls.Application.Current.Dispatcher.Dispatch(() =>
            {
                try
                {
                    if (Microsoft.Maui.Controls.Application.Current?.Windows.Count > 0)
                    {
                        var mauiWindow = Microsoft.Maui.Controls.Application.Current.Windows[0];
                        var nativeWindow = mauiWindow?.Handler?.PlatformView as Microsoft.UI.Xaml.Window;

                        if (nativeWindow?.Content is Microsoft.UI.Xaml.FrameworkElement rootElement)
                        {
                            rootElement.KeyDown += (s, e) =>
                            {
                                KeyPressed?.Invoke(null, e.Key);
                            };
                            _isInitialized = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"KeyboardHelper Init Error: {ex.Message}");
                }
            });
        }
    }
}
#endif
