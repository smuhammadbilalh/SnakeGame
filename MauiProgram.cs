using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Mopups.Hosting;
using Plugin.Maui.Audio;
using SnakeGame.Services;
using SnakeGame.Views;  // ⭐ ADD THIS

namespace SnakeGame
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureMopups()
                .AddAudio()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialIconsRegular.ttf", "MaterialIcons");
                });

            // Register services
           // builder.Services.AddTransient<HighScoreService>();
            builder.Services.AddSingleton<AudioService>();
            builder.Services.AddTransient<HomePage>();  // ⭐ ADD THIS

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
