using System;

namespace LibraryManagementSystem1.Classes
{
    public class Faculty
    {
        public int FacultyID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string EmployeeID { get; set; }
        public string Department { get; set; }
        public string Designation { get; set; }
        public DateTime JoiningDate { get; set; }
        public string Status { get; set; }

        public string FullName
        {
            get { return FirstName + " " + LastName; }
        }
    }
}