using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace HotelsManager;

public partial class BookingWindow : Window
{
    public BookingWindow(IEnumerable<RoomRow> rooms, RoomRow? selectedRoom = null)
    {
        InitializeComponent();
        RoomCombo.ItemsSource = new ObservableCollection<RoomRow>(rooms);
        StrategyCombo.ItemsSource = new[]
        {
            new PriceStrategyOption("Обычный — скидка от 7 ночей", () => new RegularPriceStrategy()),
            new PriceStrategyOption("Выходные — надбавка за субботу и воскресенье", () => new WeekendPriceStrategy()),
            new PriceStrategyOption("Сезонный — летний и новогодний коэффициент", () => new SeasonalPriceStrategy())
        };
        CheckInPicker.SelectedDate = DateTime.Today.AddDays(1);
        CheckOutPicker.SelectedDate = DateTime.Today.AddDays(2);
        StrategyCombo.SelectedIndex = 0;
        RoomCombo.SelectedItem = selectedRoom;
        UpdateTotal();
    }

    public BookingRequest? Request { get; private set; }

    private void BookingValueChanged(object? sender, SelectionChangedEventArgs e) => UpdateTotal();
    private void UpdateTotal()
    {
        if (RoomCombo.SelectedItem is not RoomRow room || StrategyCombo.SelectedItem is not PriceStrategyOption strategy || CheckInPicker.SelectedDate is null || CheckOutPicker.SelectedDate is null)
        {
            TotalText.Text = "—";
            HintText.Text = "Выберите номер, даты и тариф";
            return;
        }

        var nights = (CheckOutPicker.SelectedDate.Value.Date - CheckInPicker.SelectedDate.Value.Date).Days;
        if (nights <= 0)
        {
            TotalText.Text = "—";
            HintText.Text = "Дата выезда должна быть позже даты заезда";
            return;
        }

        var total = strategy.Create().CalculatePrice(room.Room.BasePrice, nights, CheckInPicker.SelectedDate.Value);
        TotalText.Text = $"{total:N0} руб.";
        HintText.Text = $"{nights} ночей · {room.Room.BasePrice:N0} руб. за ночь";
    }

    private void Create_Click(object sender, RoutedEventArgs e)
    {
        if (RoomCombo.SelectedItem is not RoomRow room || StrategyCombo.SelectedItem is not PriceStrategyOption strategy || CheckInPicker.SelectedDate is null || CheckOutPicker.SelectedDate is null)
        {
            StyledDialog.ShowError(this, "Выберите номер, даты проживания и тариф.");
            return;
        }

        if (CheckInPicker.SelectedDate.Value.Date < DateTime.Today || CheckOutPicker.SelectedDate.Value.Date <= CheckInPicker.SelectedDate.Value.Date)
        {
            StyledDialog.ShowError(this, "Проверьте даты проживания: заезд должен быть сегодня или позже, а выезд — позже заезда.");
            return;
        }

        if (string.IsNullOrWhiteSpace(GuestNameBox.Text) || string.IsNullOrWhiteSpace(GuestEmailBox.Text))
        {
            StyledDialog.ShowError(this, "Укажите имя гостя и адрес для уведомлений.");
            return;
        }

        Request = new BookingRequest(room.RoomNumber, GuestNameBox.Text.Trim(), GuestEmailBox.Text.Trim(), CheckInPicker.SelectedDate.Value, CheckOutPicker.SelectedDate.Value, strategy.Create());
        DialogResult = true;
    }

    private void Cancel_Click(object sender, RoutedEventArgs e) => DialogResult = false;
}

public sealed record BookingRequest(string RoomNumber, string GuestName, string GuestEmail, DateTime CheckIn, DateTime CheckOut, IPriceStrategy PriceStrategy);
