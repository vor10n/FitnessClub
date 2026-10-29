using FitnessClub.Data;
using FitnessClub.Models;
using System;
using System.Windows;
using System.Windows.Controls;

namespace FitnessClub
{
    public partial class EmployeeEditWindow : Window
    {
        private int? _employeeId;

        public EmployeeEditWindow()
        {
            InitializeComponent();
            _employeeId = null;
            TxtTitle.Text = "Новый сотрудник";
            DpHireDate.SelectedDate = DateTime.Today;
        }

        public EmployeeEditWindow(int employeeId)
        {
            InitializeComponent();
            _employeeId = employeeId;
            TxtTitle.Text = "Редактирование сотрудника";
            LoadEmployee();
        }

        private void LoadEmployee()
        {
            using (var db = new FitnessClubContext())
            {
                var emp = db.Employees.Find(_employeeId);
                if (emp == null)
                {
                    MessageBox.Show("Сотрудник не найден.", "Ошибка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    this.Close();
                    return;
                }

                TxtFirstName.Text = emp.FirstName;
                TxtLastName.Text = emp.LastName;
                TxtPhone.Text = emp.Phone;
                TxtEmail.Text = emp.Email;
                TxtSalary.Text = emp.Salary.HasValue ? emp.Salary.Value.ToString("0.00") : "";
                DpHireDate.SelectedDate = emp.HireDate;

                foreach (ComboBoxItem item in CmbPosition.Items)
                {
                    if ((string)item.Content == emp.Position)
                    {
                        CmbPosition.SelectedItem = item;
                        break;
                    }
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            string firstName = TxtFirstName.Text.Trim();
            string lastName = TxtLastName.Text.Trim();
            string phone = TxtPhone.Text.Trim();
            string email = TxtEmail.Text.Trim();
            string salaryText = TxtSalary.Text.Trim();

            if (string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(phone))
            {
                MessageBox.Show("Заполните обязательные поля (Имя, Фамилия, Телефон).",
                    "Внимание", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string position = (CmbPosition.SelectedItem as ComboBoxItem)?.Content?.ToString();
            if (string.IsNullOrEmpty(position))
            {
                MessageBox.Show("Выберите должность.", "Внимание",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            decimal? salary = null;
            if (!string.IsNullOrEmpty(salaryText))
            {
                decimal parsed;
                if (!decimal.TryParse(salaryText, out parsed) || parsed < 0)
                {
                    MessageBox.Show("Введите корректную зарплату.", "Внимание",
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                salary = parsed;
            }

            try
            {
                using (var db = new FitnessClubContext())
                {
                    if (_employeeId == null)
                    {
                        var emp = new Employee
                        {
                            FirstName = firstName,
                            LastName = lastName,
                            Phone = phone,
                            Email = string.IsNullOrEmpty(email) ? null : email,
                            Position = position,
                            Salary = salary,
                            HireDate = DpHireDate.SelectedDate ?? DateTime.Today
                        };
                        db.Employees.Add(emp);
                    }
                    else
                    {
                        var emp = db.Employees.Find(_employeeId);
                        if (emp != null)
                        {
                            emp.FirstName = firstName;
                            emp.LastName = lastName;
                            emp.Phone = phone;
                            emp.Email = string.IsNullOrEmpty(email) ? null : email;
                            emp.Position = position;
                            emp.Salary = salary;
                            emp.HireDate = DpHireDate.SelectedDate ?? DateTime.Today;
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