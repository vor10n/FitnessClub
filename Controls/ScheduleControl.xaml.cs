using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FitnessClub.Data;

namespace FitnessClub.Controls
{
    public partial class ScheduleControl : UserControl
    {
        private class ScheduleRow
        {
            public int ScheduleID { get; set; }
            public string WorkoutTitle { get; set; }
            public string TrainerName { get; set; }
            public DateTime StartTime { get; set; }
            public DateTime EndTime { get; set; }
            public string Room { get; set; }
            public string Slots { get; set; }
        }

        private List<ScheduleRow> _allRows = new List<ScheduleRow>();

        public ScheduleControl()
        {
            InitializeComponent();
            DpDate.SelectedDate = DateTime.Today;
            LoadSchedule();
        }

        private void LoadSchedule()
        {
            using (var db = new FitnessClubContext())
            {
                _allRows = db.Schedule
                    .Select(s => new ScheduleRow
                    {
                        ScheduleID = s.ScheduleID,
                        WorkoutTitle = s.Workout.Title,
                        TrainerName = s.Workout.Trainer.LastName + " " + s.Workout.Trainer.FirstName,
                        StartTime = s.StartTime,
                        EndTime = s.EndTime,
                        Room = s.Room,
                        Slots = db.Bookings.Count(b => b.ScheduleID == s.ScheduleID && b.Status == "Активно")
                                + " / " + s.Workout.MaxParticipants
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
                filtered = filtered.Where(s => s.StartTime.Date == date);
            }

            string query = TxtSearch.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(query))
            {
                filtered = filtered.Where(s =>
                    (s.WorkoutTitle != null && s.WorkoutTitle.ToLower().Contains(query)) ||
                    (s.TrainerName != null && s.TrainerName.ToLower().Contains(query)) ||
                    (s.Room != null && s.Room.ToLower().Contains(query)));
            }

            DgSchedule.ItemsSource = filtered.ToList();
        }

        private void DpDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
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
            ScheduleEditWindow window = new ScheduleEditWindow();
            if (window.ShowDialog() == true)
            {
                LoadSchedule();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DgSchedule.SelectedItem == null)
            {
                MessageBox.Show("Выберите занятие для редактирования.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var row = DgSchedule.SelectedItem as ScheduleRow;
            if (row == null) return;

            ScheduleEditWindow window = new ScheduleEditWindow(row.ScheduleID);
            if (window.ShowDialog() == true)
            {
                LoadSchedule();
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (DgSchedule.SelectedItem == null)
            {
                MessageBox.Show("Выберите занятие для удаления.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var row = DgSchedule.SelectedItem as ScheduleRow;
            if (row == null) return;

            var result = MessageBox.Show(
                "Удалить занятие \"" + row.WorkoutTitle + "\" (" + row.StartTime.ToString("dd.MM.yyyy HH:mm") + ")?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new FitnessClubContext())
                    {
                        var toDelete = db.Schedule.Find(row.ScheduleID);
                        if (toDelete != null)
                        {
                            db.Schedule.Remove(toDelete);
                            db.SaveChanges();
                        }
                    }
                    LoadSchedule();
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