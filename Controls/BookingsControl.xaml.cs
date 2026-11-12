using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FitnessClub.Data;

namespace FitnessClub.Controls
{
    public partial class BookingsControl : UserControl
    {
        private class BookingRow
        {
            public int BookingID { get; set; }
            public string ClientFullName { get; set; }
            public string ClientPhone { get; set; }
            public string WorkoutTitle { get; set; }
            public string TrainerName { get; set; }
            public DateTime StartTime { get; set; }
            public string Room { get; set; }
            public DateTime BookingDate { get; set; }
            public string Status { get; set; }
        }

        private List<BookingRow> _allRows = new List<BookingRow>();

        public BookingsControl()
        {

            InitializeComponent();
            DpDate.SelectedDate = null;
            LoadBookings();
        }

        private void AutoCompleteBookings()
        {
            try
            {
                using (var db = new FitnessClubContext())
                {
                    
                    var expiredBookings = db.Bookings
                        .Where(b => b.Status == "Активно" && b.Schedule.EndTime < DateTime.Now)
                        .ToList();

                    if (expiredBookings.Any())
                    {
                        foreach (var booking in expiredBookings)
                        {
                            booking.Status = "Завершено";
                        }
                        db.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка автообновления статусов:\n" + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadBookings()
        {
            AutoCompleteBookings();

            using (var db = new FitnessClubContext())
            {
                _allRows = db.Bookings
                    .Select(b => new BookingRow
                    {
                        BookingID = b.BookingID,
                        ClientFullName = b.Client.LastName + " " + b.Client.FirstName,
                        ClientPhone = b.Client.Phone,
                        WorkoutTitle = b.Schedule.Workout.Title,
                        TrainerName = b.Schedule.Workout.Trainer.LastName + " " + b.Schedule.Workout.Trainer.FirstName,
                        StartTime = b.Schedule.StartTime,
                        Room = b.Schedule.Room,
                        BookingDate = b.BookingDate,
                        Status = b.Status
                    })
                    .ToList();

                ApplyFilter();
            }
        }

        private void ApplyFilter()
        {
            var filtered = _allRows.AsEnumerable();

            if (DpDate.SelectedDate.HasValue)
            {
                var date = DpDate.SelectedDate.Value.Date;
                filtered = filtered.Where(b => b.StartTime.Date == date);
            }

            string status = (CmbStatus.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (!string.IsNullOrEmpty(status) && status != "Все")
            {
                filtered = filtered.Where(b => b.Status == status);
            }

            string query = TxtSearch.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(query))
            {
                filtered = filtered.Where(b =>
                    (b.ClientFullName != null && b.ClientFullName.ToLower().Contains(query)) ||
                    (b.ClientPhone != null && b.ClientPhone.Contains(query)) ||
                    (b.WorkoutTitle != null && b.WorkoutTitle.ToLower().Contains(query)) ||
                    (b.TrainerName != null && b.TrainerName.ToLower().Contains(query)));
            }

            DgBookings.ItemsSource = filtered.ToList();
        }

        private void DpDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            ApplyFilter();
        }

        private void CmbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            ApplyFilter();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            BookingEditWindow window = new BookingEditWindow();
            if (window.ShowDialog() == true)
            {
                LoadBookings();
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            if (DgBookings.SelectedItem == null)
            {
                MessageBox.Show("Выберите бронирование для отмены.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var row = DgBookings.SelectedItem as BookingRow;
            if (row == null) return;

            if (row.Status != "Активно")
            {
                MessageBox.Show("Отменить можно только активное бронирование.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var result = MessageBox.Show(
                "Отменить бронь клиента " + row.ClientFullName + "?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new FitnessClubContext())
                    {
                        var booking = db.Bookings.Find(row.BookingID);
                        if (booking != null)
                        {
                            booking.Status = "Отменено";
                            db.SaveChanges();
                        }
                    }
                    LoadBookings();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка отмены:\n" + ex.Message, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (DgBookings.SelectedItem == null)
            {
                MessageBox.Show("Выберите бронирование для удаления.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var row = DgBookings.SelectedItem as BookingRow;
            if (row == null) return;

            var result = MessageBox.Show(
                "Удалить бронирование клиента " + row.ClientFullName + "?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new FitnessClubContext())
                    {
                        var toDelete = db.Bookings.Find(row.BookingID);
                        if (toDelete != null)
                        {
                            db.Bookings.Remove(toDelete);
                            db.SaveChanges();
                        }
                    }
                    LoadBookings();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка удаления:\n" + ex.Message, "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}