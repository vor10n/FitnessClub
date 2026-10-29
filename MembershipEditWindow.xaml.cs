using FitnessClub.Data;
using FitnessClub.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace FitnessClub
{
    public partial class MembershipEditWindow : Window
    {
        private int? _membershipId;
        private int? _selectedClientId;

        public MembershipEditWindow()
        {
            InitializeComponent();
            _membershipId = null;
            _selectedClientId = null;
            TxtTitle.Text = "Новый абонемент";
            DpStartDate.SelectedDate = DateTime.Today;
            UpdateEndDate();
        }

        public MembershipEditWindow(int membershipId)
        {
            InitializeComponent();
            _membershipId = membershipId;
            TxtTitle.Text = "Редактирование абонемента";
            LoadMembership();
        }

        private void LoadMembership()
        {
            using (var db = new FitnessClubContext())
            {
                var m = db.Memberships.Find(_membershipId);
                if (m == null)
                {
                    MessageBox.Show("Абонемент не найден.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                _selectedClientId = m.ClientID;
                var client = db.Clients.Find(m.ClientID);
                if (client != null)
                    TxtClient.Text = client.LastName + " " + client.FirstName + " (" + client.Phone + ")";

                foreach (ComboBoxItem item in CmbType.Items)
                {
                    if ((string)item.Content == m.TypeName)
                    {
                        CmbType.SelectedItem = item;
                        break;
                    }
                }

                DpStartDate.SelectedDate = m.StartDate;
                DpEndDate.SelectedDate = m.EndDate;
                TxtPrice.Text = m.Price.ToString("0,00");
            }
        }

        private void BtnChooseClient_Click(object sender, RoutedEventArgs e)
        {
            ClientSelectWindow window = new ClientSelectWindow();
            if (window.ShowDialog() == true && window.SelectedClientId.HasValue)
            {
                _selectedClientId = window.SelectedClientId.Value;
                TxtClient.Text = window.SelectedClientName;
            }
        }

        private void CmbType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            UpdateEndDate();
            UpdatePrice();
        }

        private void DpStartDate_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            UpdateEndDate();
        }

        private void UpdateEndDate()
        {
            if (!DpStartDate.SelectedDate.HasValue) return;

            string type = (CmbType.SelectedItem as ComboBoxItem)?.Content?.ToString();
            DateTime start = DpStartDate.SelectedDate.Value;

            if (type == "Месячный")
                DpEndDate.SelectedDate = start.AddMonths(1);
            else if (type == "Полугодовой")
                DpEndDate.SelectedDate = start.AddMonths(6);
            else if (type == "Годовой")
                DpEndDate.SelectedDate = start.AddYears(1);
        }

        private void UpdatePrice()
        {
            string type = (CmbType.SelectedItem as ComboBoxItem)?.Content?.ToString();

            if (type == "Месячный")
                TxtPrice.Text = "3000.00";
            else if (type == "Полугодовой")
                TxtPrice.Text = "14000.00";
            else if (type == "Годовой")
                TxtPrice.Text = "25000.00";
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!_selectedClientId.HasValue)
            {
                MessageBox.Show("Выберите клиента.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string type = (CmbType.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (string.IsNullOrEmpty(type))
            {
                MessageBox.Show("Выберите тип абонемента.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!DpStartDate.SelectedDate.HasValue || !DpEndDate.SelectedDate.HasValue)
            {
                MessageBox.Show("Укажите даты начала и окончания.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal price;
            if (!decimal.TryParse(TxtPrice.Text, out price) || price < 0)
            {
                MessageBox.Show("Введите корректную цену.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                using (var db = new FitnessClubContext())
                {
                    if (_membershipId == null)
                    {
                        var membership = new Membership
                        {
                            ClientID = _selectedClientId.Value,
                            TypeName = type,
                            StartDate = DpStartDate.SelectedDate.Value,
                            EndDate = DpEndDate.SelectedDate.Value,
                            Price = price
                        };
                        db.Memberships.Add(membership);
                    }
                    else
                    {
                        var membership = db.Memberships.Find(_membershipId);
                        if (membership != null)
                        {
                            membership.ClientID = _selectedClientId.Value;
                            membership.TypeName = type;
                            membership.StartDate = DpStartDate.SelectedDate.Value;
                            membership.EndDate = DpEndDate.SelectedDate.Value;
                            membership.Price = price;
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