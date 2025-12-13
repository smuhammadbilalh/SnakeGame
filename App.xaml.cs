using Microsoft.Extensions.DependencyInjection;
using SnakeGame.Services;
using SnakeGame.Views;

namespace SnakeGame
{
    public partial class App : Application
    {
        public App(IServiceProvider serviceProvider)
        {
            InitializeComponent();

            // Get HomePage from DI container with AudioService injected
            var homePage = serviceProvider.GetRequiredService<HomePage>();
            MainPage = new NavigationPage(homePage);
        }
    }
}