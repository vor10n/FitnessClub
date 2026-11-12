using FitnessClub.Data;
using FitnessClub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FitnessClub.Controls
{
    public partial class TrainersControl : UserControl
    {
        private List<Trainer> _allTrainers = new List<Trainer>();

        public TrainersControl()
        {
            InitializeComponent();
            LoadTrainers();
        }

        private void LoadTrainers()
        {
            using (var db = new FitnessClubContext())
            {
                _allTrainers = db.Trainers.ToList();
                DgTrainers.ItemsSource = _allTrainers;
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            string query = TxtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(query))
            {
                DgTrainers.ItemsSource = _allTrainers;
                return;
            }

            var filtered = _allTrainers.Where(t =>
                (t.FirstName != null && t.FirstName.ToLower().Contains(query)) ||
                (t.LastName != null && t.LastName.ToLower().Contains(query)) ||
                (t.Specialization != null && t.Specialization.ToLower().Contains(query))
            ).ToList();

            DgTrainers.ItemsSource = filtered;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            TrainerEditWindow window = new TrainerEditWindow();
            if (window.ShowDialog() == true)
            {
                LoadTrainers();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DgTrainers.SelectedItem == null)
            {
                MessageBox.Show("Выберите тренера для редактирования.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var trainer = DgTrainers.SelectedItem as Trainer;
            if (trainer == null) return;

            TrainerEditWindow window = new TrainerEditWindow(trainer.TrainerID);
            if (window.ShowDialog() == true)
            {
                LoadTrainers();
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (DgTrainers.SelectedItem == null)
            {
                MessageBox.Show("Выберите тренера для удаления.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var trainer = DgTrainers.SelectedItem as Trainer;
            if (trainer == null) return;

            var result = MessageBox.Show(
                "Удалить тренера " + trainer.FirstName + " " + trainer.LastName + "?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new FitnessClubContext())
                    {
                        var toDelete = db.Trainers.Find(trainer.TrainerID);
                        if (toDelete != null)
                        {
                            db.Trainers.Remove(toDelete);
                            db.SaveChanges();
                        }
                    }
                    LoadTrainers();
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