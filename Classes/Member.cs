using System;

namespace LibraryManagementSystem1.Classes
{
    public class Member
    {
        public int MemberID { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime MembershipDate { get; set; }
        public string Status { get; set; }

        public string FullName
        {
            get { return FirstName + " " + LastName; }
        }
    }
}