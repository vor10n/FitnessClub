using System;
using System.Collections.Generic;

namespace FitnessClub.Models
{
    public class Client
    {
        public int ClientID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public DateTime RegistrationDate { get; set; }

        public ICollection<Membership> Memberships { get; set; }
        public ICollection<Booking> Bookings { get; set; }
    }
    public class Membership
    {
        public int MembershipID { get; set; }
        public int ClientID { get; set; }
        public string TypeName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal Price { get; set; }

        public Client Client { get; set; }
    }
    public class Trainer
    {
        public int TrainerID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Specialization { get; set; }
        public DateTime HireDate { get; set; }
        public int? EmployeeID { get; set; }

        public Employee Employee { get; set; }
        public ICollection<Workout> Workouts { get; set; }
    }
    public class Workout
    {
        public int WorkoutID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int TrainerID { get; set; }
        public int MaxParticipants { get; set; }
        public int DurationMinutes { get; set; }

        public Trainer Trainer { get; set; }
        public ICollection<Schedule> Schedules { get; set; }
    }
    public class Schedule
    {
        public int ScheduleID { get; set; }
        public int WorkoutID { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string Room { get; set; }

        public Workout Workout { get; set; }
        public ICollection<Booking> Bookings { get; set; }
    }
    public class Booking
    {
        public int BookingID { get; set; }
        public int ScheduleID { get; set; }
        public int ClientID { get; set; }
        public DateTime BookingDate { get; set; }
        public string Status { get; set; }

        public Schedule Schedule { get; set; }
        public Client Client { get; set; }
    }
    public class Employee
    {
        public int EmployeeID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Position { get; set; }
        public decimal? Salary { get; set; }
        public DateTime HireDate { get; set; }

        public Trainer Trainer { get; set; }
        public Admin Admin { get; set; }
    }
    public class Admin
    {
        public int AdminID { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public int? EmployeeID { get; set; }

        public Employee Employee { get; set; }
    }
}