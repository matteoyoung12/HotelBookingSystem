using System.Windows;
using System.Windows.Media;

namespace HotelsManager;

public partial class StyledDialog : Window
{
    private StyledDialog(string title, string message, string icon, Color color, bool confirmation)
    {
        InitializeComponent();
        TitleText.Text = title;
        MessageText.Text = message;
        IconText.Text = icon;
        IconText.Foreground = new SolidColorBrush(color);
        IconCircle.Background = new SolidColorBrush(Color.FromArgb(28, color.R, color.G, color.B));
        CancelButton.Visibility = confirmation ? Visibility.Visible : Visibility.Collapsed;
        ConfirmButton.Content = confirmation ? "Подтвердить" : "Понятно";
    }

    public static void ShowError(Window owner, string message) => Show(owner, "Проверьте данные", message, "!", Color.FromRgb(220, 38, 38), false);
    public static void ShowSuccess(Window owner, string message) => Show(owner, "Готово", message, "✓", Color.FromRgb(22, 163, 74), false);
    public static bool Confirm(Window owner, string title, string message)
    {
        var dialog = Create(owner, title, message, "?", Color.FromRgb(234, 88, 12), true);
        return dialog.ShowDialog() == true;
    }

    private static void Show(Window owner, string title, string message, string icon, Color color, bool confirmation)
    {
        Create(owner, title, message, icon, color, confirmation).ShowDialog();
    }

    private static StyledDialog Create(Window owner, string title, string message, string icon, Color color, bool confirmation)
    {
        return new StyledDialog(title, message, icon, color, confirmation) { Owner = owner };
    }

    private void Confirm_Click(object sender, RoutedEventArgs e) => DialogResult = true;
    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}
