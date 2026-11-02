using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace HotelsManager;

public partial class MainWindow : Window, INotifyPropertyChanged
{
    private readonly NotificationManager _notificationManager = new();
    private readonly HotelManager _hotelManager;
    private readonly RoomFactory _roomFactory = new();
    private RoomRow? _selectedRoom;
    private BookingRow? _selectedBooking;
    private RoomFilterOption? _selectedRoomFilter;
    private PriceStrategyOption? _selectedPriceStrategy;
    private DateTime? _checkInDate;
    private DateTime? _checkOutDate;
    private string _minimumCapacity = string.Empty;
    private string _maximumPrice = string.Empty;
    private string _guestName = string.Empty;
    private string _guestEmail = string.Empty;
    private string _newRoomNumber = string.Empty;
    private RoomType _newRoomType;
    private string _estimatedTotal = "—";
    private string _priceHint = "Укажите номер, даты и тариф";

    public MainWindow()
    {
        InitializeComponent();

        var uiNotification = new UiNotification((type, recipient, message) =>
        {
            Notifications.Insert(0, new NotificationEntry($"{type} · {DateTime.Now:HH:mm}", $"{recipient}: {message}"));
        });
        _notificationManager.AddNotification(uiNotification);
        _notificationManager.AddNotification(new EmailNotification());
        _notificationManager.AddNotification(new SMSNotification());
        _hotelManager = new HotelManager(_notificationManager);

        RoomFilterOptions = new ObservableCollection<RoomFilterOption>(
            new[] { new RoomFilterOption("Все типы", null) }
                .Concat(Enum.GetValues<RoomType>().Select(type => new RoomFilterOption(GetRoomTypeName(type), type))));
        RoomTypes = new ObservableCollection<RoomType>(Enum.GetValues<RoomType>());
        PriceStrategies = new ObservableCollection<PriceStrategyOption>
        {
            new("Обычный — скидка за длительное проживание", () => new RegularPriceStrategy()),
            new("Выходные — надбавка в субботу и воскресенье", () => new WeekendPriceStrategy()),
            new("Сезонный — летний и новогодний коэффициент", () => new SeasonalPriceStrategy())
        };

        SelectedRoomFilter = RoomFilterOptions[0];
        SelectedPriceStrategy = PriceStrategies[0];
        NewRoomType = RoomType.Standard;
        CheckInDate = DateTime.Today.AddDays(1);
        CheckOutDate = DateTime.Today.AddDays(2);

        RoomsView = CollectionViewSource.GetDefaultView(Rooms);
        RoomsView.Filter = IsRoomMatch;
        AddInitialRooms();
        RefreshDashboard();
        DataContext = this;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<RoomRow> Rooms { get; } = new();
    public ObservableCollection<BookingRow> Bookings { get; } = new();
    public ObservableCollection<NotificationEntry> Notifications { get; } = new();
    public ObservableCollection<RoomFilterOption> RoomFilterOptions { get; private set; } = null!;
    public ObservableCollection<RoomType> RoomTypes { get; private set; } = null!;
    public ObservableCollection<PriceStrategyOption> PriceStrategies { get; private set; } = null!;
    public ICollectionView RoomsView { get; }

    public int TotalRooms => Rooms.Count;
    public int AvailableRooms => Rooms.Count(room => room.Room.IsAvailable);
    public int ActiveBookings => Bookings.Count(booking => !booking.Booking.IsCancelled);

    public RoomRow? SelectedRoom
    {
        get => _selectedRoom;
        set
        {
            _selectedRoom = value;
            OnPropertyChanged(nameof(SelectedRoom));
            OnPropertyChanged(nameof(SelectedRoomTitle));
            UpdateEstimate();
        }
    }

    public BookingRow? SelectedBooking
    {
        get => _selectedBooking;
        set => SetField(ref _selectedBooking, value, nameof(SelectedBooking));
    }

    public RoomFilterOption? SelectedRoomFilter
    {
        get => _selectedRoomFilter;
        set => SetField(ref _selectedRoomFilter, value, nameof(SelectedRoomFilter));
    }

    public PriceStrategyOption? SelectedPriceStrategy
    {
        get => _selectedPriceStrategy;
        set
        {
            _selectedPriceStrategy = value;
            OnPropertyChanged(nameof(SelectedPriceStrategy));
            UpdateEstimate();
        }
    }

    public DateTime? CheckInDate
    {
        get => _checkInDate;
        set
        {
            _checkInDate = value;
            OnPropertyChanged(nameof(CheckInDate));
            UpdateEstimate();
        }
    }

    public DateTime? CheckOutDate
    {
        get => _checkOutDate;
        set
        {
            _checkOutDate = value;
            OnPropertyChanged(nameof(CheckOutDate));
            UpdateEstimate();
        }
    }

    public string MinimumCapacity { get => _minimumCapacity; set => SetField(ref _minimumCapacity, value, nameof(MinimumCapacity)); }
    public string MaximumPrice { get => _maximumPrice; set => SetField(ref _maximumPrice, value, nameof(MaximumPrice)); }
    public string GuestName { get => _guestName; set => SetField(ref _guestName, value, nameof(GuestName)); }
    public string GuestEmail { get => _guestEmail; set => SetField(ref _guestEmail, value, nameof(GuestEmail)); }
    public string NewRoomNumber { get => _newRoomNumber; set => SetField(ref _newRoomNumber, value, nameof(NewRoomNumber)); }
    public RoomType NewRoomType { get => _newRoomType; set => SetField(ref _newRoomType, value, nameof(NewRoomType)); }
    public string SelectedRoomTitle => SelectedRoom is null ? "Номер не выбран" : $"Выбран номер {SelectedRoom.RoomNumber} · {SelectedRoom.TypeName}";
    public string EstimatedTotal { get => _estimatedTotal; private set => SetField(ref _estimatedTotal, value, nameof(EstimatedTotal)); }
    public string PriceHint { get => _priceHint; private set => SetField(ref _priceHint, value, nameof(PriceHint)); }

    private void AddInitialRooms()
    {
        foreach (var roomNumber in Enumerable.Range(101, 20))
        {
            AddRoom(_roomFactory.CreateStandardRoom(roomNumber.ToString()));
        }

        foreach (var roomNumber in Enumerable.Range(201, 15))
        {
            AddRoom(_roomFactory.CreateDeluxeRoom(roomNumber.ToString()));
        }

        foreach (var roomNumber in Enumerable.Range(301, 10))
        {
            AddRoom(_roomFactory.CreateSuite(roomNumber.ToString()));
        }

        foreach (var roomNumber in Enumerable.Range(401, 8))
        {
            AddRoom(_roomFactory.CreateFamilyRoom(roomNumber.ToString()));
        }
    }

    private void AddRoom(IRoom room)
    {
        _hotelManager.AddRoom(room);
        Rooms.Add(new RoomRow(room));
    }

    private bool IsRoomMatch(object item)
    {
        if (item is not RoomRow room || !room.Room.IsAvailable)
        {
            return false;
        }

        if (SelectedRoomFilter?.RoomType is { } type && room.Room.Type != type)
        {
            return false;
        }

        if (int.TryParse(MinimumCapacity, out var minimumCapacity) && minimumCapacity > 0 && room.Room.Capacity < minimumCapacity)
        {
            return false;
        }

        return !decimal.TryParse(MaximumPrice, NumberStyles.Number, CultureInfo.CurrentCulture, out var maximumPrice)
               || maximumPrice <= 0
               || room.Room.BasePrice <= maximumPrice;
    }

    private void Search_Click(object sender, RoutedEventArgs e)
    {
        if (!IsOptionalPositiveNumber(MinimumCapacity, isInteger: true) || !IsOptionalPositiveNumber(MaximumPrice, isInteger: false))
        {
            ShowError("Введите положительное число в полях «Гостей от» и «Цена до». ");
            return;
        }

        RoomsView.Refresh();
        SelectedRoom = null;
    }

    private void ResetFilters_Click(object sender, RoutedEventArgs e)
    {
        SelectedRoomFilter = RoomFilterOptions[0];
        MinimumCapacity = string.Empty;
        MaximumPrice = string.Empty;
        RoomsView.Refresh();
        SelectedRoom = null;
    }

    private void Room_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateEstimate();
    }

    private void Strategy_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateEstimate();
    }

    private void DateChanged(object? sender, SelectionChangedEventArgs e)
    {
        UpdateEstimate();
    }

    private void CreateBooking_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedRoom is null)
        {
            ShowError("Сначала выберите свободный номер в таблице.");
            return;
        }

        if (CheckInDate is null || CheckOutDate is null || SelectedPriceStrategy is null)
        {
            ShowError("Укажите даты проживания и тариф.");
            return;
        }

        if (CheckInDate.Value.Date < DateTime.Today)
        {
            ShowError("Дата заезда не может быть в прошлом.");
            return;
        }

        try
        {
            var booking = _hotelManager.CreateBooking(
                SelectedRoom.RoomNumber,
                GuestName,
                GuestEmail,
                CheckInDate.Value,
                CheckOutDate.Value,
                SelectedPriceStrategy.Create());

            booking.Confirm();
            Bookings.Insert(0, new BookingRow(booking));
            GuestName = string.Empty;
            GuestEmail = string.Empty;
            SelectedRoom.Refresh();
            SelectedRoom = null;
            RefreshDashboard();
            RoomsView.Refresh();
        }
        catch (Exception exception)
        {
            ShowError(exception.Message);
        }
    }

    private void OpenBookingWindow_Click(object sender, RoutedEventArgs e)
    {
        var availableRooms = Rooms.Where(room => room.Room.IsAvailable).ToList();
        if (availableRooms.Count == 0)
        {
            ShowError("Нет свободных номеров для бронирования.");
            return;
        }

        var bookingWindow = new BookingWindow(availableRooms, SelectedRoom) { Owner = this };
        if (bookingWindow.ShowDialog() != true || bookingWindow.Request is null)
        {
            return;
        }

        try
        {
            var request = bookingWindow.Request;
            var booking = _hotelManager.CreateBooking(request.RoomNumber, request.GuestName, request.GuestEmail, request.CheckIn, request.CheckOut, request.PriceStrategy);
            booking.Confirm();
            Bookings.Insert(0, new BookingRow(booking));
            Rooms.First(room => ReferenceEquals(room.Room, booking.Room)).Refresh();
            RefreshDashboard();
            RoomsView.Refresh();
            StyledDialog.ShowSuccess(this, $"Бронирование №{booking.BookingId} подтверждено. Гость получит уведомление на {booking.GuestEmail}.");
        }
        catch (Exception exception)
        {
            ShowError(exception.Message);
        }
    }

    private void CancelBooking_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedBooking is null)
        {
            ShowError("Выберите бронирование, которое нужно отменить.");
            return;
        }

        if (SelectedBooking.Booking.IsCancelled)
        {
            ShowError("Это бронирование уже отменено.");
            return;
        }

        if (!StyledDialog.Confirm(this, "Отменить бронирование?", $"Бронирование №{SelectedBooking.BookingId} будет отменено, а номер снова станет доступен."))
        {
            return;
        }

        SelectedBooking.Booking.Cancel();
        SelectedBooking.Refresh();
        Rooms.First(room => ReferenceEquals(room.Room, SelectedBooking.Booking.Room)).Refresh();
        RefreshDashboard();
        RoomsView.Refresh();
    }

    private void DeleteSelectedRoom_Click(object sender, RoutedEventArgs e)
    {
        if (SelectedRoom is null)
        {
            ShowError("Выберите свободный номер в таблице, который нужно удалить.");
            return;
        }

        if (!StyledDialog.Confirm(this, "Удалить номер?", $"Номер {SelectedRoom.RoomNumber} будет удалён из отеля. Это действие нельзя отменить."))
        {
            return;
        }

        try
        {
            _hotelManager.RemoveRoom(SelectedRoom.RoomNumber);
            Rooms.Remove(SelectedRoom);
            SelectedRoom = null;
            RefreshDashboard();
            RoomsView.Refresh();
            StyledDialog.ShowSuccess(this, "Номер удалён из списка отеля.");
        }
        catch (Exception exception)
        {
            ShowError(exception.Message);
        }
    }

    private void AddRoom_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var room = _roomFactory.CreateRoom(NewRoomType, NewRoomNumber);
            AddRoom(room);
            NewRoomNumber = string.Empty;
            RefreshDashboard();
            RoomsView.Refresh();
        }
        catch (Exception exception)
        {
            ShowError(exception.Message);
        }
    }

    private void UpdateEstimate()
    {
        if (SelectedRoom is null || CheckInDate is null || CheckOutDate is null || SelectedPriceStrategy is null)
        {
            EstimatedTotal = "—";
            PriceHint = "Укажите номер, даты и тариф";
            return;
        }

        var nights = (CheckOutDate.Value.Date - CheckInDate.Value.Date).Days;
        if (nights <= 0)
        {
            EstimatedTotal = "—";
            PriceHint = "Дата выезда должна быть позже даты заезда";
            return;
        }

        var total = SelectedPriceStrategy.Create().CalculatePrice(SelectedRoom.Room.BasePrice, nights, CheckInDate.Value);
        EstimatedTotal = $"{total:N0} руб.";
        PriceHint = $"{nights} {GetNightsText(nights)} · {SelectedRoom.Room.BasePrice:N0} руб./ночь";
    }

    private void RefreshDashboard()
    {
        OnPropertyChanged(nameof(TotalRooms));
        OnPropertyChanged(nameof(AvailableRooms));
        OnPropertyChanged(nameof(ActiveBookings));
    }

    private static bool IsOptionalPositiveNumber(string value, bool isInteger)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        return isInteger
            ? int.TryParse(value, out var number) && number > 0
            : decimal.TryParse(value, NumberStyles.Number, CultureInfo.CurrentCulture, out var price) && price > 0;
    }

    private static string GetNightsText(int nights) => nights % 10 == 1 && nights % 100 != 11 ? "ночь" : nights % 10 is >= 2 and <= 4 && (nights % 100 < 10 || nights % 100 >= 20) ? "ночи" : "ночей";
    private static string GetRoomTypeName(RoomType type) => type switch
    {
        RoomType.Standard => "Стандарт",
        RoomType.Deluxe => "Делюкс",
        RoomType.Suite => "Люкс",
        RoomType.Family => "Семейный",
        _ => type.ToString()
    };

    private void ShowError(string message) => StyledDialog.ShowError(this, message);
    private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    private void SetField<T>(ref T field, T value, string propertyName)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        OnPropertyChanged(propertyName);
    }
}

public sealed class RoomRow : INotifyPropertyChanged
{
    public RoomRow(IRoom room) => Room = room;
    public event PropertyChangedEventHandler? PropertyChanged;
    public IRoom Room { get; }
    public string RoomNumber => Room.RoomNumber;
    public string TypeName => Room.Type switch { RoomType.Standard => "Стандарт", RoomType.Deluxe => "Делюкс", RoomType.Suite => "Люкс", RoomType.Family => "Семейный", _ => Room.Type.ToString() };
    public int Capacity => Room.Capacity;
    public string PriceText => $"{Room.BasePrice:N0} руб.";
    public string BookingDisplay => $"№ {RoomNumber} · {TypeName} · до {Capacity} гостей · {PriceText}/ночь";
    public string Status => Room.IsAvailable ? "Свободен" : "Забронирован";
    public void Refresh()
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
    }
}

public sealed class BookingRow : INotifyPropertyChanged
{
    public BookingRow(Booking booking) => Booking = booking;
    public event PropertyChangedEventHandler? PropertyChanged;
    public Booking Booking { get; }
    public int BookingId => Booking.BookingId;
    public string GuestName => Booking.GuestName;
    public string RoomNumber => Booking.Room.RoomNumber;
    public string Period => $"{Booking.CheckIn:dd.MM.yy} - {Booking.CheckOut:dd.MM.yy}";
    public string TotalPriceText => $"{Booking.TotalPrice:N0} руб.";
    public string Status => Booking.IsCancelled ? "Отменено" : "Подтверждено";
    public void Refresh() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Status)));
}

public sealed record RoomFilterOption(string Name, RoomType? RoomType);
public sealed record PriceStrategyOption(string Name, Func<IPriceStrategy> Create);
public sealed record NotificationEntry(string Header, string Message);

public sealed class UiNotification(Action<string, string, string> onSent) : INotification
{
    public NotificationType Type => NotificationType.Email;
    public bool Send(string message, string recipient)
    {
        onSent("Уведомление", recipient, message);
        return true;
    }
}
