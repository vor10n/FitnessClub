using FitnessClub.Data;
using FitnessClub.Models;
using System;
using System.Windows;

namespace FitnessClub
{
    public partial class ClientEditWindow : Window
    {
        private int? _clientId;

        public ClientEditWindow()
        {
            InitializeComponent();
            _clientId = null;
            TxtTitle.Text = "Новый клиент";
        }

        public ClientEditWindow(int clientId)
        {
            InitializeComponent();
            _clientId = clientId;
            TxtTitle.Text = "Редактирование клиента";
            LoadClient();
        }

        private void LoadClient()
        {
            using (var db = new FitnessClubContext())
            {
                var client = db.Clients.Find(_clientId);
                if (client == null)
                {
                    MessageBox.Show("Клиент не найден.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                TxtFirstName.Text = client.FirstName;
                TxtLastName.Text = client.LastName;
                TxtPhone.Text = client.Phone;
                TxtEmail.Text = client.Email;
                DpDateOfBirth.SelectedDate = client.DateOfBirth;
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string firstName = TxtFirstName.Text.Trim();
            string lastName = TxtLastName.Text.Trim();
            string phone = TxtPhone.Text.Trim();
            string email = TxtEmail.Text.Trim();

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
                    if (_clientId == null)
                    {
                        var client = new Client
                        {
                            FirstName = firstName,
                            LastName = lastName,
                            Phone = phone,
                            Email = string.IsNullOrEmpty(email) ? null : email,
                            DateOfBirth = DpDateOfBirth.SelectedDate,
                            RegistrationDate = DateTime.Today
                        };
                        db.Clients.Add(client);
                    }
                    else
                    {
                        var client = db.Clients.Find(_clientId);
                        if (client != null)
                        {
                            client.FirstName = firstName;
                            client.LastName = lastName;
                            client.Phone = phone;
                            client.Email = string.IsNullOrEmpty(email) ? null : email;
                            client.DateOfBirth = DpDateOfBirth.SelectedDate;
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