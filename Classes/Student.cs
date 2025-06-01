using System;

namespace LibraryManagementSystem1.Classes
{
    public class Student
    {
        public int StudentID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string StudentNumber { get; set; }
        public string Department { get; set; }
        public string Semester { get; set; }
        public DateTime EnrollmentDate { get; set; }
        public string Status { get; set; }

        public string FullName
        {
            get { return FirstName + " " + LastName; }
        }
    }
}