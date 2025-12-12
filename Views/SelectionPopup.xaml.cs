using Mopups.Pages;
using Mopups.Services;

namespace SnakeGame.Views;

public partial class SelectionPopup : PopupPage
{
    public class OptionItem
    {
        public string IconGlyph { get; set; }
        public string Text { get; set; }
        public object Value { get; set; }
    }

    public object SelectedValue { get; private set; }

    public SelectionPopup(string title, List<OptionItem> options)
    {
        InitializeComponent();
        TitleLabel.Text = title;
        OptionsCollection.ItemsSource = options;
    }

    private async void OnOptionSelected(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is OptionItem selected)
        {
            SelectedValue = selected.Value;
            await MopupService.Instance.PopAsync();
        }
    }

    private async void OnCloseClicked(object sender, EventArgs e)
    {
        await MopupService.Instance.PopAsync();
    }
}
