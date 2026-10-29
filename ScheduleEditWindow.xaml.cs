using FitnessClub.Data;
using FitnessClub.Models;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FitnessClub
{
    public partial class ScheduleEditWindow : Window
    {
        private int? _scheduleId;

        public ScheduleEditWindow()
        {
            InitializeComponent();
            _scheduleId = null;
            TxtTitle.Text = "Новое занятие";
            DpDate.SelectedDate = DateTime.Today;
            LoadWorkouts();
        }

        public ScheduleEditWindow(int scheduleId)
        {
            InitializeComponent();
            _scheduleId = scheduleId;
            TxtTitle.Text = "Редактирование занятия";
            LoadWorkouts();
            LoadSchedule();
        }

        private void LoadWorkouts()
        {
            using (var db = new FitnessClubContext())
            {
                var workouts = db.Workouts
                    .Select(w => new
                    {
                        w.WorkoutID,
                        Title = w.Title
                    })
                    .ToList();

                CmbWorkout.ItemsSource = workouts;
                CmbWorkout.DisplayMemberPath = "Title";
                CmbWorkout.SelectedValuePath = "WorkoutID";
            }
        }

        private void LoadSchedule()
        {
            using (var db = new FitnessClubContext())
            {
                var s = db.Schedule.Find(_scheduleId);
                if (s == null)
                {
                    MessageBox.Show("Занятие не найдено.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                CmbWorkout.SelectedValue = s.WorkoutID;
                DpDate.SelectedDate = s.StartTime.Date;
                TxtStartTime.Text = s.StartTime.ToString("HH:mm");
                TxtEndTime.Text = s.EndTime.ToString("HH:mm");
                TxtRoom.Text = s.Room;
            }
        }

        private void CmbWorkout_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            UpdateEndTime();
        }

        private void DpDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            UpdateEndTime();
        }

        private void UpdateEndTime()
        {
            if (CmbWorkout.SelectedValue == null) return;
            if (!DpDate.SelectedDate.HasValue) return;

            var startText = TxtStartTime.Text.Trim();
            TimeSpan start;
            if (!TimeSpan.TryParseExact(startText, "hh\\:mm", CultureInfo.InvariantCulture, out start))
                return;

            int duration;
            using (var db = new FitnessClubContext())
            {
                int workoutId = (int)CmbWorkout.SelectedValue;
                var w = db.Workouts.Find(workoutId);
                if (w == null) return;
                duration = w.DurationMinutes;
            }

            var end = start.Add(TimeSpan.FromMinutes(duration));
            TxtEndTime.Text = end.ToString(@"hh\:mm");
        }

        private void TxtStartTime_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateEndTime();
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (CmbWorkout.SelectedValue == null)
            {
                MessageBox.Show("Выберите тренировку.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DpDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Укажите дату.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            TimeSpan start;
            if (!TimeSpan.TryParseExact(TxtStartTime.Text.Trim(), "hh\\:mm", CultureInfo.InvariantCulture, out start))
            {
                MessageBox.Show("Введите корректное время начала (чч:мм).", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string room = TxtRoom.Text.Trim();
            if (string.IsNullOrEmpty(room))
            {
                MessageBox.Show("Укажите зал.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime startDateTime = DpDate.SelectedDate.Value.Date + start;

            int duration;
            using (var db = new FitnessClubContext())
            {
                int workoutId = (int)CmbWorkout.SelectedValue;
                var w = db.Workouts.Find(workoutId);
                if (w == null)
                {
                    MessageBox.Show("Тренировка не найдена.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                duration = w.DurationMinutes;
            }

            DateTime endDateTime = startDateTime.AddMinutes(duration);

            try
            {
                using (var db = new FitnessClubContext())
                {
                    if (_scheduleId == null)
                    {
                        var s = new Schedule
                        {
                            WorkoutID = (int)CmbWorkout.SelectedValue,
                            StartTime = startDateTime,
                            EndTime = endDateTime,
                            Room = room
                        };
                        db.Schedule.Add(s);
                    }
                    else
                    {
                        var s = db.Schedule.Find(_scheduleId);
                        if (s != null)
                        {
                            s.WorkoutID = (int)CmbWorkout.SelectedValue;
                            s.StartTime = startDateTime;
                            s.EndTime = endDateTime;
                            s.Room = room;
                        }
                    }

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