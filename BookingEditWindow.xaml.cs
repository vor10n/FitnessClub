using FitnessClub.Data;
using FitnessClub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FitnessClub
{
    public partial class BookingEditWindow : Window
    {
        private class ScheduleItem
        {
            public int ScheduleID { get; set; }
            public string Display { get; set; }
        }

        private int? _selectedClientId;
        private List<ScheduleItem> _schedules = new List<ScheduleItem>();

        public BookingEditWindow()
        {
            InitializeComponent();
            LoadSchedules();
        }

        private void LoadSchedules()
        {
            using (var db = new FitnessClubContext())
            {
                var today = DateTime.Today;

                _schedules = db.Schedule
                    .Where(s => s.StartTime >= today)
                    .OrderBy(s => s.StartTime)
                    .Select(s => new ScheduleItem
                    {
                        ScheduleID = s.ScheduleID,
                        Display = s.Workout.Title + " | " +
                                  s.StartTime.ToString("dd.MM.yyyy HH:mm") + " | " +
                                  "Зал: " + s.Room
                    })
                    .ToList();

                CmbSchedule.ItemsSource = _schedules;
                CmbSchedule.DisplayMemberPath = "Display";
                CmbSchedule.SelectedValuePath = "ScheduleID";
            }
        }

        private void BtnChooseClient_Click(object sender, RoutedEventArgs e)
        {
            ClientSelectWindow window = new ClientSelectWindow();
            if (window.ShowDialog() == true && window.SelectedClientId.HasValue)
            {
                _selectedClientId = window.SelectedClientId.Value;
                TxtClient.Text = window.SelectedClientName;
                LoadMembershipInfo();
            }
        }

        private void LoadMembershipInfo()
        {
            if (!_selectedClientId.HasValue)
            {
                TxtMembership.Text = "";
                return;
            }

            using (var db = new FitnessClubContext())
            {
                var today = DateTime.Today;
                var activeMembership = db.Memberships
                    .Where(m => m.ClientID == _selectedClientId.Value && m.EndDate >= today)
                    .OrderByDescending(m => m.EndDate)
                    .FirstOrDefault();

                if (activeMembership != null)
                {
                    TxtMembership.Text = activeMembership.TypeName +
                                         " до " + activeMembership.EndDate.ToString("dd.MM.yyyy");
                }
                else
                {
                    TxtMembership.Text = "Нет активного абонемента";
                }
            }
        }

        private void CmbSchedule_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateSlots();
        }

        private void UpdateSlots()
        {
            if (CmbSchedule.SelectedValue == null)
            {
                TxtSlots.Text = "";
                return;
            }

            int scheduleId = (int)CmbSchedule.SelectedValue;

            using (var db = new FitnessClubContext())
            {
                var schedule = db.Schedule.Find(scheduleId);
                if (schedule == null) return;

                var workout = db.Workouts.Find(schedule.WorkoutID);
                if (workout == null) return;

                int booked = db.Bookings.Count(b => b.ScheduleID == scheduleId && b.Status == "Активно");
                int free = workout.MaxParticipants - booked;

                TxtSlots.Text = free + " из " + workout.MaxParticipants;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!_selectedClientId.HasValue)
            {
                MessageBox.Show("Выберите клиента.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CmbSchedule.SelectedValue == null)
            {
                MessageBox.Show("Выберите занятие.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int scheduleId = (int)CmbSchedule.SelectedValue;

            // Проверка абонемента
            using (var db = new FitnessClubContext())
            {
                var today = DateTime.Today;
                var activeMembership = db.Memberships
                    .FirstOrDefault(m => m.ClientID == _selectedClientId.Value && m.EndDate >= today);

                if (activeMembership == null)
                {
                    MessageBox.Show("У клиента нет активного абонемента. Запись невозможна.",
                        "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка мест
                var schedule = db.Schedule.Find(scheduleId);
                if (schedule == null)
                {
                    MessageBox.Show("Занятие не найдено.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var workout = db.Workouts.Find(schedule.WorkoutID);
                int booked = db.Bookings.Count(b => b.ScheduleID == scheduleId && b.Status == "Активно");

                if (booked >= workout.MaxParticipants)
                {
                    MessageBox.Show("На это занятие больше нет свободных мест.",
                        "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Проверка, не записан ли уже клиент на это занятие
                var existing = db.Bookings
                    .FirstOrDefault(b => b.ScheduleID == scheduleId
                                      && b.ClientID == _selectedClientId.Value
                                      && b.Status == "Активно");

                if (existing != null)
                {
                    MessageBox.Show("Клиент уже записан на это занятие.",
                        "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
            }

            string status = (CmbStatus.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (string.IsNullOrEmpty(status)) status = "Активно";

            try
            {
                using (var db = new FitnessClubContext())
                {
                    var booking = new Booking
                    {
                        ClientID = _selectedClientId.Value,
                        ScheduleID = scheduleId,
                        BookingDate = DateTime.Now,
                        Status = status
                    };
                    db.Bookings.Add(booking);
                    db.SaveChanges();
                }

                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка сохранения:\n" + ex.Message, "Ошибка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}