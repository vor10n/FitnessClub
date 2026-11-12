using FitnessClub.Data;
using FitnessClub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FitnessClub.Controls
{
    public partial class ClientsControl : UserControl
    {
        private List<Client> _allClients = new List<Client>();

        public ClientsControl()
        {
            InitializeComponent();
            LoadClients();
        }

        private void LoadClients()
        {
            using (var db = new FitnessClubContext())
            {
                _allClients = db.Clients.ToList();
                DgClients.ItemsSource = _allClients;
            }
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void BtnSearch_Click(object sender, RoutedEventArgs e)
        {
            ApplyFilter();
        }

        private void ApplyFilter()
        {
            string query = TxtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(query))
            {
                DgClients.ItemsSource = _allClients;
                return;
            }

            var filtered = _allClients.Where(c =>
                (c.FirstName != null && c.FirstName.ToLower().Contains(query)) ||
                (c.LastName != null && c.LastName.ToLower().Contains(query)) ||
                (c.Phone != null && c.Phone.Contains(query)) ||
                (c.Email != null && c.Email.ToLower().Contains(query))
            ).ToList();

            DgClients.ItemsSource = filtered;
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            ClientEditWindow window = new ClientEditWindow();
            if (window.ShowDialog() == true)
            {
                LoadClients();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DgClients.SelectedItem == null)
            {
                MessageBox.Show("Выберите клиента для редактирования.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var client = DgClients.SelectedItem as Client;
            if (client == null) return;

            ClientEditWindow window = new ClientEditWindow(client.ClientID);
            if (window.ShowDialog() == true)
            {
                LoadClients();
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (DgClients.SelectedItem == null)
            {
                MessageBox.Show("Выберите клиента для удаления.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var client = DgClients.SelectedItem as Client;
            if (client == null) return;

            var result = MessageBox.Show(
                "Удалить клиента " + client.FirstName + " " + client.LastName + "?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new FitnessClubContext())
                    {
                        var toDelete = db.Clients.Find(client.ClientID);
                        if (toDelete != null)
                        {
                            db.Clients.Remove(toDelete);
                            db.SaveChanges();
                        }
                    }
                    LoadClients();
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