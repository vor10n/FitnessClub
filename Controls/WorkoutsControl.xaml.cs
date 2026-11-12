using FitnessClub.Data;
using FitnessClub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FitnessClub.Controls
{
    public partial class WorkoutsControl : UserControl
    {
        private List<Workout> _allWorkouts = new List<Workout>();

        public WorkoutsControl()
        {
            InitializeComponent();
            LoadWorkouts();
        }

        private void LoadWorkouts()
        {
            using (var db = new FitnessClubContext())
            {
                _allWorkouts = db.Workouts
                    .Select(w => new Workout
                    {
                        WorkoutID = w.WorkoutID,
                        Title = w.Title,
                        Description = w.Description,
                        TrainerID = w.TrainerID,
                        MaxParticipants = w.MaxParticipants,
                        DurationMinutes = w.DurationMinutes,
                        Trainer = new Trainer
                        {
                            TrainerID = w.Trainer.TrainerID,
                            FirstName = w.Trainer.FirstName,
                            LastName = w.Trainer.LastName
                        }
                    })
                    .ToList();

                DgWorkouts.ItemsSource = _allWorkouts;
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = TxtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(query))
            {
                DgWorkouts.ItemsSource = _allWorkouts;
                return;
            }

            var filtered = _allWorkouts.Where(w =>
                (w.Title != null && w.Title.ToLower().Contains(query)) ||
                (w.Description != null && w.Description.ToLower().Contains(query))
            ).ToList();

            DgWorkouts.ItemsSource = filtered;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            WorkoutEditWindow window = new WorkoutEditWindow();
            if (window.ShowDialog() == true)
            {
                LoadWorkouts();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DgWorkouts.SelectedItem == null)
            {
                MessageBox.Show("Выберите тренировку для редактирования.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var workout = DgWorkouts.SelectedItem as Workout;
            if (workout == null) return;

            WorkoutEditWindow window = new WorkoutEditWindow(workout.WorkoutID);
            if (window.ShowDialog() == true)
            {
                LoadWorkouts();
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (DgWorkouts.SelectedItem == null)
            {
                MessageBox.Show("Выберите тренировку для удаления.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var workout = DgWorkouts.SelectedItem as Workout;
            if (workout == null) return;

            var result = MessageBox.Show(
                "Удалить тренировку \"" + workout.Title + "\"?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new FitnessClubContext())
                    {
                        var toDelete = db.Workouts.Find(workout.WorkoutID);
                        if (toDelete != null)
                        {
                            db.Workouts.Remove(toDelete);
                            db.SaveChanges();
                        }
                    }
                    LoadWorkouts();
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