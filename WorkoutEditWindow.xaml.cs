using FitnessClub.Data;
using FitnessClub.Models;
using System;
using System.Linq;
using System.Windows;

namespace FitnessClub
{
    public partial class WorkoutEditWindow : Window
    {
        private int? _workoutId;

        public WorkoutEditWindow()
        {
            InitializeComponent();
            _workoutId = null;
            TxtTitle.Text = "Новая тренировка";
            LoadTrainers();
        }

        public WorkoutEditWindow(int workoutId)
        {
            InitializeComponent();
            _workoutId = workoutId;
            TxtTitle.Text = "Редактирование тренировки";
            LoadTrainers();
            LoadWorkout();
        }

        private void LoadTrainers()
        {
            using (var db = new FitnessClubContext())
            {
                var trainers = db.Trainers
                    .Select(t => new
                    {
                        t.TrainerID,
                        FullName = t.LastName + " " + t.FirstName
                    })
                    .ToList();

                CmbTrainer.ItemsSource = trainers;
                CmbTrainer.DisplayMemberPath = "FullName";
                CmbTrainer.SelectedValuePath = "TrainerID";
            }
        }

        private void LoadWorkout()
        {
            using (var db = new FitnessClubContext())
            {
                var w = db.Workouts.Find(_workoutId);
                if (w == null)
                {
                    MessageBox.Show("Тренировка не найдена.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                TxtTitle2.Text = w.Title;
                TxtDescription.Text = w.Description;
                CmbTrainer.SelectedValue = w.TrainerID;
                TxtMaxParticipants.Text = w.MaxParticipants.ToString();
                TxtDuration.Text = w.DurationMinutes.ToString();
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string title = TxtTitle2.Text.Trim();
            string description = TxtDescription.Text.Trim();

            if (string.IsNullOrEmpty(title))
            {
                MessageBox.Show("Введите название тренировки.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CmbTrainer.SelectedValue == null)
            {
                MessageBox.Show("Выберите тренера.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int maxParticipants;
            if (!int.TryParse(TxtMaxParticipants.Text, out maxParticipants) || maxParticipants <= 0)
            {
                MessageBox.Show("Введите корректное число участников.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            int duration;
            if (!int.TryParse(TxtDuration.Text, out duration) || duration <= 0)
            {
                MessageBox.Show("Введите корректную длительность.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new FitnessClubContext())
                {
                    if (_workoutId == null)
                    {
                        var workout = new Workout
                        {
                            Title = title,
                            Description = string.IsNullOrEmpty(description) ? null : description,
                            TrainerID = (int)CmbTrainer.SelectedValue,
                            MaxParticipants = maxParticipants,
                            DurationMinutes = duration
                        };
                        db.Workouts.Add(workout);
                    }
                    else
                    {
                        var workout = db.Workouts.Find(_workoutId);
                        if (workout != null)
                        {
                            workout.Title = title;
                            workout.Description = string.IsNullOrEmpty(description) ? null : description;
                            workout.TrainerID = (int)CmbTrainer.SelectedValue;
                            workout.MaxParticipants = maxParticipants;
                            workout.DurationMinutes = duration;
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