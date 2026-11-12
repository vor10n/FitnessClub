using Microsoft.EntityFrameworkCore;
using FitnessClub.Models;

namespace FitnessClub.Data
{
    public class FitnessClubContext : DbContext
    {
        public DbSet<Client> Clients { get; set; }
        public DbSet<Membership> Memberships { get; set; }
        public DbSet<Trainer> Trainers { get; set; }
        public DbSet<Workout> Workouts { get; set; }
        public DbSet<Schedule> Schedule { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Admin> Admins { get; set; }

        protected override void OnConfiguring(
     DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string server = "(local)";

                string path = System.IO.Path.Combine(
                    AppContext.BaseDirectory,
                    "server.txt");

                if (System.IO.File.Exists(path))
                {
                    server = System.IO.File.ReadAllText(path).Trim();
                }

                optionsBuilder.UseSqlServer(
                    $"Data Source={server};" +
                    "Initial Catalog=FitnessClub;" +
                    "Integrated Security=True;" +
                    "TrustServerCertificate=True;");
            }
        }
    }
}