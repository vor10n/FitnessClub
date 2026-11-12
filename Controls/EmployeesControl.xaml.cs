using FitnessClub.Data;
using FitnessClub.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace FitnessClub.Controls
{
    public partial class EmployeesControl : UserControl
    {
        private List<Employee> _allEmployees = new List<Employee>();

        public EmployeesControl()
        {
            InitializeComponent();
            LoadEmployees();
        }

        private void LoadEmployees()
        {
            using (var db = new FitnessClubContext())
            {
                _allEmployees = db.Employees.ToList();
                ApplyFilter();
            }
        }

        private void ApplyFilter()
        {
            var filtered = _allEmployees.AsEnumerable();

            string query = TxtSearch.Text.Trim().ToLower();
            if (!string.IsNullOrEmpty(query))
            {
                filtered = filtered.Where(e =>
                    (e.FirstName != null && e.FirstName.ToLower().Contains(query)) ||
                    (e.LastName != null && e.LastName.ToLower().Contains(query)) ||
                    (e.Phone != null && e.Phone.Contains(query)) ||
                    (e.Email != null && e.Email.ToLower().Contains(query)));
            }

            string position = (CmbPosition.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (!string.IsNullOrEmpty(position) && position != "Все должности")
            {
                filtered = filtered.Where(e => e.Position == position);
            }

            DgEmployees.ItemsSource = filtered.ToList();
        }

        private void TxtSearch_TextChanged(object sender, TextChangedEventArgs e)
        {
            ApplyFilter();
        }

        private void CmbPosition_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded) return;
            ApplyFilter();
        }

        private void BtnAdd_Click(object sender, RoutedEventArgs e)
        {
            EmployeeEditWindow window = new EmployeeEditWindow();
            if (window.ShowDialog() == true)
            {
                LoadEmployees();
            }
        }

        private void BtnEdit_Click(object sender, RoutedEventArgs e)
        {
            if (DgEmployees.SelectedItem == null)
            {
                MessageBox.Show("Выберите сотрудника для редактирования.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var emp = DgEmployees.SelectedItem as Employee;
            if (emp == null) return;

            EmployeeEditWindow window = new EmployeeEditWindow(emp.EmployeeID);
            if (window.ShowDialog() == true)
            {
                LoadEmployees();
            }
        }

        private void BtnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (DgEmployees.SelectedItem == null)
            {
                MessageBox.Show("Выберите сотрудника для удаления.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var employee = DgEmployees.SelectedItem as Employee;
            if (employee == null) return;

            var result = MessageBox.Show(
                "Удалить сотрудника " + employee.FirstName + " " + employee.LastName + "?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new FitnessClubContext())
                    {
                        var toDelete = db.Employees.Find(employee.EmployeeID);
                        if (toDelete != null)
                        {
                            db.Employees.Remove(toDelete);
                            db.SaveChanges();
                        }
                    }
                    LoadEmployees();
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