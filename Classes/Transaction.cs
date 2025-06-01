using System;

namespace LibraryManagementSystem1.Classes
{
    public class Transaction
    {
        public int TransactionID { get; set; }
        public int MemberID { get; set; }
        public int BookID { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime DueDate { get; set; }
        public DateTime? ReturnDate { get; set; }
        public string Status { get; set; }
        public decimal Fine { get; set; }

        // Additional properties for display
        public string MemberName { get; set; }
        public string BookTitle { get; set; }
    }
}