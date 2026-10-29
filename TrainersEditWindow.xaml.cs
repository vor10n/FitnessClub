using FitnessClub.Data;
using FitnessClub.Models;
using System;
using System.Windows;

namespace FitnessClub
{
    public partial class TrainerEditWindow : Window
    {
        private int? _trainerId;

        public TrainerEditWindow()
        {
            InitializeComponent();
            _trainerId = null;
            TxtTitle.Text = "Новый тренер";
            DpHireDate.SelectedDate = DateTime.Today;
        }

        public TrainerEditWindow(int trainerId)
        {
            InitializeComponent();
            _trainerId = trainerId;
            TxtTitle.Text = "Редактирование тренера";
            LoadTrainer();
        }

        private void LoadTrainer()
        {
            using (var db = new FitnessClubContext())
            {
                var trainer = db.Trainers.Find(_trainerId);
                if (trainer == null)
                {
                    MessageBox.Show("Тренер не найден.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                TxtFirstName.Text = trainer.FirstName;
                TxtLastName.Text = trainer.LastName;
                TxtPhone.Text = trainer.Phone;
                TxtSpecialization.Text = trainer.Specialization;
                DpHireDate.SelectedDate = trainer.HireDate;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string firstName = TxtFirstName.Text.Trim();
            string lastName = TxtLastName.Text.Trim();
            string phone = TxtPhone.Text.Trim();
            string specialization = TxtSpecialization.Text.Trim();

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(phone))
            {
                MessageBox.Show("Заполните обязательные поля (Имя, Фамилия, Телефон).",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new FitnessClubContext())
                {
                    if (_trainerId == null)
                    {
                        var trainer = new Trainer
                        {
                            FirstName = firstName,
                            LastName = lastName,
                            Phone = phone,
                            Specialization = string.IsNullOrEmpty(specialization) ? null : specialization,
                            HireDate = DpHireDate.SelectedDate ?? DateTime.Today
                        };
                        db.Trainers.Add(trainer);
                    }
                    else
                    {
                        var trainer = db.Trainers.Find(_trainerId);
                        if (trainer != null)
                        {
                            trainer.FirstName = firstName;
                            trainer.LastName = lastName;
                            trainer.Phone = phone;
                            trainer.Specialization = string.IsNullOrEmpty(specialization) ? null : specialization;
                            trainer.HireDate = DpHireDate.SelectedDate ?? DateTime.Today;
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