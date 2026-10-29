using System.Windows;
using FitnessClub.Controls;

namespace FitnessClub
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            TxtUser.Text = "Администратор";
        }

        private void BtnClients_Click(object sender, RoutedEventArgs e)
        {
            TxtHeader.Text = "Клиенты";
            MainContent.Content = new ClientsControl();
        }

        private void BtnMemberships_Click(object sender, RoutedEventArgs e)
        {
            TxtHeader.Text = "Абонементы";
            MainContent.Content = new MembershipsControl();
        }

        private void BtnTrainers_Click(object sender, RoutedEventArgs e)
        {
            TxtHeader.Text = "Тренеры";
            MainContent.Content = new TrainersControl();
        }

        private void BtnWorkouts_Click(object sender, RoutedEventArgs e)
        {
            TxtHeader.Text = "Тренировки";
            MainContent.Content = new WorkoutsControl();
        }

        private void BtnSchedule_Click(object sender, RoutedEventArgs e)
        {
            TxtHeader.Text = "Расписание";
            MainContent.Content = new ScheduleControl();
        }

        private void BtnBookings_Click(object sender, RoutedEventArgs e)
        {
            TxtHeader.Text = "Бронирования";
            MainContent.Content = new BookingsControl();
        }

        private void BtnEmployees_Click(object sender, RoutedEventArgs e)
        {
            TxtHeader.Text = "Сотрудники";
            MainContent.Content = new EmployeesControl();
        }

        private void BtnLogout_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Вы уверены, что хотите выйти?",
                "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                LoginWindow login = new LoginWindow();
                login.Show();
                this.Close();
            }
            if (result == MessageBoxResult.No) { }
        }
    }
}