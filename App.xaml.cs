using Microsoft.Extensions.DependencyInjection;

namespace SnakeGame
{
   using SnakeGame.Views;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        MainPage = new NavigationPage(new HomePage());
    }
}

}