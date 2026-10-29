using FitnessClub.Data;
using FitnessClub.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FitnessClub
{
    public partial class ClientSelectWindow : Window
    {
        private List<Client> _allClients = new List<Client>();

        public int? SelectedClientId { get; private set; }
        public string SelectedClientName { get; private set; }

        public ClientSelectWindow()
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
            string query = TxtSearch.Text.Trim().ToLower();

            if (string.IsNullOrEmpty(query))
            {
                DgClients.ItemsSource = _allClients;
                return;
            }

            var filtered = _allClients.Where(c =>
                (c.FirstName != null && c.FirstName.ToLower().Contains(query)) ||
                (c.LastName != null && c.LastName.ToLower().Contains(query)) ||
                (c.Phone != null && c.Phone.Contains(query))
            ).ToList();

            DgClients.ItemsSource = filtered;
        }

        private void DgClients_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            SelectCurrent();
        }

        private void BtnSelect_Click(object sender, RoutedEventArgs e)
        {
            SelectCurrent();
        }

        private void SelectCurrent()
        {
            var client = DgClients.SelectedItem as Client;
            if (client == null)
            {
                MessageBox.Show("Выберите клиента.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SelectedClientId = client.ClientID;
            SelectedClientName = client.LastName + " " + client.FirstName + " (" + client.Phone + ")";
            this.DialogResult = true;
            this.Close();
        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}