using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using FitnessClub.Data;

namespace FitnessClub.Controls
{
    public partial class MembershipsControl : UserControl
    {
        private class MembershipRow
        {
            public int MembershipID { get; set; }
            public string ClientFullName { get; set; }
            public string ClientPhone { get; set; }
            public string TypeName { get; set; }
            public DateTime StartDate { get; set; }
            public DateTime EndDate { get; set; }
            public decimal Price { get; set; }
            public string StatusText { get; set; }
        }

        private List<MembershipRow> _allRows = new List<MembershipRow>();

        public MembershipsControl()
        {
            InitializeComponent();
            LoadMemberships();
        }

        private void LoadMemberships()
        {
            using (var db = new FitnessClubContext())
            {
                var today = DateTime.Today;

                _allRows = db.Memberships
                    .Select(m => new MembershipRow
                    {
                        MembershipID = m.MembershipID,
                        ClientFullName = m.Client.LastName + " " + m.Client.FirstName,
                        ClientPhone = m.Client.Phone,
                        TypeName = m.TypeName,
                        StartDate = m.StartDate,
                        EndDate = m.EndDate,
                        Price = m.Price,
                        StatusText = m.EndDate >= today ? "Активен" : "Истёк"
                    })
                    .ToList();

                ApplyFilter();
            }
        }

        private void ApplyFilter()
        {
            string query = TxtSearch.Text.Trim().ToLower();
            var filtered = _allRows.AsEnumerable();

            if (!string.IsNullOrEmpty(query))
            {
                filtered = filtered.Where(m =>
                    (m.ClientFullName != null && m.ClientFullName.ToLower().Contains(query)) ||
                    (m.ClientPhone != null && m.ClientPhone.Contains(query)) ||
                    (m.TypeName != null && m.TypeName.ToLower().Contains(query)));
            }

            if (CmbStatus.SelectedIndex == 1)
                filtered = filtered.Where(m => m.StatusText == "Активен");
            else if (CmbStatus.SelectedIndex == 2)
                filtered = filtered.Where(m => m.StatusText == "Истёк");

            DgMemberships.ItemsSource = filtered.ToList();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void CmbStatus_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            ApplyFilter();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            MembershipEditWindow window = new MembershipEditWindow();
            if (window.ShowDialog() == true)
            {
                LoadMemberships();
            }
        }

        private void BtnExtend_Click(object sender, RoutedEventArgs e)
        {
            if (DgMemberships.SelectedItem == null)
            {
                MessageBox.Show("Выберите абонемент для продления/редактирования.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            dynamic row = DgMemberships.SelectedItem;
            int membershipId = row.MembershipID;

            MembershipEditWindow window = new MembershipEditWindow(membershipId);
            if (window.ShowDialog() == true)
            {
                LoadMemberships();
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (DgMemberships.SelectedItem == null)
            {
                MessageBox.Show("Выберите абонемент для удаления.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var row = DgMemberships.SelectedItem as MembershipRow;
            if (row == null) return;

            var result = MessageBox.Show(
                "Удалить абонемент \"" + row.TypeName + "\" клиента " + row.ClientFullName + "?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new FitnessClubContext())
                    {
                        var toDelete = db.Memberships.Find(row.MembershipID);
                        if (toDelete != null)
                        {
                            db.Memberships.Remove(toDelete);
                            db.SaveChanges();
                        }
                    }
                    LoadMemberships();
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