using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using LibraryManagementSystem1.Classes;

namespace LibraryManagementSystem1.Forms
{
    public partial class TransactionForm : Form
    {
        private int selectedTransactionID = 0;

        public TransactionForm()
        {
            InitializeComponent();
            LoadTransactions();
            LoadUserTypes();
            LoadAvailableBooks();
        }

        private void LoadTransactions()
        {
            string query = @"SELECT t.TransactionID, t.UserType,
                            CASE 
                                WHEN t.UserType = 'Member' THEN m.FirstName + ' ' + m.LastName
                                WHEN t.UserType = 'Student' THEN s.FirstName + ' ' + s.LastName
                                WHEN t.UserType = 'Faculty' THEN f.FirstName + ' ' + f.LastName
                            END AS UserName,
                            CASE 
                                WHEN t.UserType = 'Student' THEN s.StudentNumber
                                WHEN t.UserType = 'Faculty' THEN f.EmployeeID
                                ELSE NULL
                            END AS UserNumber,
                            b.Title AS BookTitle, b.Author, t.IssueDate, t.DueDate, t.ReturnDate, t.Status, t.Fine
                            FROM Transactions t
                            LEFT JOIN Members m ON t.UserType = 'Member' AND t.UserID = m.MemberID
                            LEFT JOIN Students s ON t.UserType = 'Student' AND t.UserID = s.StudentID
                            LEFT JOIN Faculty f ON t.UserType = 'Faculty' AND t.UserID = f.FacultyID
                            JOIN Books b ON t.BookID = b.BookID
                            ORDER BY t.IssueDate DESC";

            DataTable dt = DatabaseConnection.ExecuteQuery(query);
            dgvTransactions.DataSource = dt;

            if (dgvTransactions.Columns.Count > 0)
            {
                foreach (DataGridViewColumn column in dgvTransactions.Columns)
                {
                    column.HeaderCell.Style.BackColor = System.Drawing.Color.FromArgb(50, 50, 100);
                    column.HeaderCell.Style.ForeColor = System.Drawing.Color.White;
                }
            }
        }

        private void LoadUserTypes()
        {
            cmbUserType.Items.Clear();
            cmbUserType.Items.AddRange(new string[] { "Member", "Student", "Faculty" });
            cmbUserType.SelectedIndex = 0;
        }

        private void LoadUsers()
        {
            if (cmbUserType.SelectedIndex == -1) return;

            string userType = cmbUserType.SelectedItem.ToString();
            string query = "";

            switch (userType)
            {
                case "Member":
                    query = "SELECT MemberID AS ID, FirstName + ' ' + LastName AS FullName FROM Members WHERE Status = 'Active'";
                    break;
                case "Student":
                    query = "SELECT StudentID AS ID, FirstName + ' ' + LastName + ' (' + StudentNumber + ')' AS FullName FROM Students WHERE Status = 'Active'";
                    break;
                case "Faculty":
                    query = "SELECT FacultyID AS ID, FirstName + ' ' + LastName + ' (' + EmployeeID + ')' AS FullName FROM Faculty WHERE Status = 'Active'";
                    break;
            }

            DataTable dt = DatabaseConnection.ExecuteQuery(query);
            cmbUsers.DataSource = dt;
            cmbUsers.DisplayMember = "FullName";
            cmbUsers.ValueMember = "ID";
        }

        private void LoadAvailableBooks()
        {
            string query = "SELECT BookID, Title + ' - ' + Author AS BookInfo FROM Books WHERE AvailableCopies > 0";
            DataTable dt = DatabaseConnection.ExecuteQuery(query);

            cmbBooks.DataSource = dt;
            cmbBooks.DisplayMember = "BookInfo";
            cmbBooks.ValueMember = "BookID";
        }

        private void cmbUserType_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadUsers();
        }

        private void btnIssue_Click(object sender, EventArgs e)
        {
            if (cmbUserType.SelectedIndex == -1 || cmbUsers.SelectedIndex == -1 || cmbBooks.SelectedIndex == -1)
            {
                MessageBox.Show("Please select user type, user, and book to issue.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if user has any overdue books or unpaid fines
            if (HasOverdueBooks() || HasUnpaidFines())
            {
                MessageBox.Show("User has overdue books or unpaid fines. Please resolve before issuing new books.", "Issue Blocked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check user's borrowing limit
            if (HasReachedBorrowingLimit())
            {
                MessageBox.Show("User has reached the maximum borrowing limit.", "Issue Blocked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string userType = cmbUserType.SelectedItem.ToString();
            int userID = Convert.ToInt32(cmbUsers.SelectedValue);
            int bookID = Convert.ToInt32(cmbBooks.SelectedValue);
            DateTime issueDate = dtpIssueDate.Value.Date;
            DateTime dueDate = dtpDueDate.Value.Date;

            string insertQuery = @"INSERT INTO Transactions (BookID, UserType, UserID, IssueDate, DueDate, Status) 
                                 VALUES (@BookID, @UserType, @UserID, @IssueDate, @DueDate, 'Issued')";

            string updateBookQuery = @"UPDATE Books SET AvailableCopies = AvailableCopies - 1 WHERE BookID = @BookID";

            SqlParameter[] insertParams = {
                new SqlParameter("@BookID", bookID),
                new SqlParameter("@UserType", userType),
                new SqlParameter("@UserID", userID),
                new SqlParameter("@IssueDate", issueDate),
                new SqlParameter("@DueDate", dueDate)
            };

            SqlParameter[] updateParams = {
                new SqlParameter("@BookID", bookID)
            };

            try
            {
                // Start transaction
                using (SqlConnection conn = new SqlConnection(DatabaseConnection.connectionString))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Insert transaction record
                            SqlCommand insertCmd = new SqlCommand(insertQuery, conn, transaction);
                            insertCmd.Parameters.AddRange(insertParams);
                            insertCmd.ExecuteNonQuery();

                            // Update book availability
                            SqlCommand updateCmd = new SqlCommand(updateBookQuery, conn, transaction);
                            updateCmd.Parameters.AddRange(updateParams);
                            updateCmd.ExecuteNonQuery();

                            transaction.Commit();
                            MessageBox.Show("Book issued successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadTransactions();
                            LoadAvailableBooks();
                            ClearFields();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show("Error issuing book: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database connection error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnReturn_Click(object sender, EventArgs e)
        {
            if (selectedTransactionID == 0)
            {
                MessageBox.Show("Please select a transaction to return.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if book is already returned
            string checkQuery = "SELECT Status FROM Transactions WHERE TransactionID = @TransactionID";
            SqlParameter[] checkParams = { new SqlParameter("@TransactionID", selectedTransactionID) };
            DataTable dt = DatabaseConnection.ExecuteQuery(checkQuery, checkParams);

            if (dt.Rows.Count > 0 && dt.Rows[0]["Status"].ToString() == "Returned")
            {
                MessageBox.Show("This book has already been returned.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DateTime returnDate = dtpReturnDate.Value.Date;
            decimal fine = CalculateFine();

            string updateTransactionQuery = @"UPDATE Transactions SET ReturnDate = @ReturnDate, Status = 'Returned', Fine = @Fine 
                                            WHERE TransactionID = @TransactionID";

            string updateBookQuery = @"UPDATE Books SET AvailableCopies = AvailableCopies + 1 
                                     WHERE BookID = (SELECT BookID FROM Transactions WHERE TransactionID = @TransactionID)";

            SqlParameter[] transactionParams = {
                new SqlParameter("@ReturnDate", returnDate),
                new SqlParameter("@Fine", fine),
                new SqlParameter("@TransactionID", selectedTransactionID)
            };

            SqlParameter[] bookParams = {
                new SqlParameter("@TransactionID", selectedTransactionID)
            };

            try
            {
                using (SqlConnection conn = new SqlConnection(DatabaseConnection.connectionString))
                {
                    conn.Open();
                    using (SqlTransaction transaction = conn.BeginTransaction())
                    {
                        try
                        {
                            // Update transaction record
                            SqlCommand transactionCmd = new SqlCommand(updateTransactionQuery, conn, transaction);
                            transactionCmd.Parameters.AddRange(transactionParams);
                            transactionCmd.ExecuteNonQuery();

                            // Update book availability
                            SqlCommand bookCmd = new SqlCommand(updateBookQuery, conn, transaction);
                            bookCmd.Parameters.AddRange(bookParams);
                            bookCmd.ExecuteNonQuery();

                            transaction.Commit();

                            string message = "Book returned successfully!";
                            if (fine > 0)
                            {
                                message += $"\nFine amount: ${fine:F2}";
                            }

                            MessageBox.Show(message, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadTransactions();
                            LoadAvailableBooks();
                            ClearFields();
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback();
                            MessageBox.Show("Error returning book: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Database connection error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnRenew_Click(object sender, EventArgs e)
        {
            if (selectedTransactionID == 0)
            {
                MessageBox.Show("Please select a transaction to renew.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if book can be renewed (not overdue, no pending requests, etc.)
            if (!CanRenewBook())
            {
                MessageBox.Show("This book cannot be renewed. It may be overdue or have pending requests.", "Renewal Blocked", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DateTime newDueDate = DateTime.Now.AddDays(14); // Extend by 14 days

            string renewQuery = @"UPDATE Transactions SET DueDate = @NewDueDate WHERE TransactionID = @TransactionID";
            SqlParameter[] parameters = {
                new SqlParameter("@NewDueDate", newDueDate),
                new SqlParameter("@TransactionID", selectedTransactionID)
            };

            int result = DatabaseConnection.ExecuteNonQuery(renewQuery, parameters);
            if (result > 0)
            {
                MessageBox.Show($"Book renewed successfully! New due date: {newDueDate:yyyy-MM-dd}", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadTransactions();
                ClearFields();
            }
            else
            {
                MessageBox.Show("Failed to renew book.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(keyword))
            {
                LoadTransactions();
                return;
            }

            string query = @"SELECT t.TransactionID, t.UserType,
                            CASE 
                                WHEN t.UserType = 'Member' THEN m.FirstName + ' ' + m.LastName
                                WHEN t.UserType = 'Student' THEN s.FirstName + ' ' + s.LastName
                                WHEN t.UserType = 'Faculty' THEN f.FirstName + ' ' + f.LastName
                            END AS UserName,
                            CASE 
                                WHEN t.UserType = 'Student' THEN s.StudentNumber
                                WHEN t.UserType = 'Faculty' THEN f.EmployeeID
                                ELSE NULL
                            END AS UserNumber,
                            b.Title AS BookTitle, b.Author, t.IssueDate, t.DueDate, t.ReturnDate, t.Status, t.Fine
                            FROM Transactions t
                            LEFT JOIN Members m ON t.UserType = 'Member' AND t.UserID = m.MemberID
                            LEFT JOIN Students s ON t.UserType = 'Student' AND t.UserID = s.StudentID
                            LEFT JOIN Faculty f ON t.UserType = 'Faculty' AND t.UserID = f.FacultyID
                            JOIN Books b ON t.BookID = b.BookID
                            WHERE b.Title LIKE @keyword OR b.Author LIKE @keyword 
                            OR m.FirstName LIKE @keyword OR m.LastName LIKE @keyword
                            OR s.FirstName LIKE @keyword OR s.LastName LIKE @keyword OR s.StudentNumber LIKE @keyword
                            OR f.FirstName LIKE @keyword OR f.LastName LIKE @keyword OR f.EmployeeID LIKE @keyword
                            ORDER BY t.IssueDate DESC";

            SqlParameter[] parameters = { new SqlParameter("@keyword", "%" + keyword + "%") };
            DataTable dt = DatabaseConnection.ExecuteQuery(query, parameters);
            dgvTransactions.DataSource = dt;
        }

        private void dgvTransactions_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvTransactions.Rows[e.RowIndex];
                selectedTransactionID = Convert.ToInt32(row.Cells["TransactionID"].Value);

                // Update UI based on selected transaction
                string status = row.Cells["Status"].Value.ToString();
                if (status == "Returned")
                {
                    btnReturn.Enabled = false;
                    btnRenew.Enabled = false;
                }
                else
                {
                    btnReturn.Enabled = true;
                    btnRenew.Enabled = true;
                }

                // Set return date to today for convenience
                dtpReturnDate.Value = DateTime.Now;

                // Calculate and display fine if overdue
                if (row.Cells["DueDate"].Value != DBNull.Value && status != "Returned")
                {
                    DateTime dueDate = Convert.ToDateTime(row.Cells["DueDate"].Value);
                    if (DateTime.Now.Date > dueDate.Date)
                    {
                        decimal fine = CalculateFineForTransaction(selectedTransactionID);
                        lblFineAmount.Text = $"Fine: ${fine:F2}";
                        lblFineAmount.Visible = true;
                    }
                    else
                    {
                        lblFineAmount.Visible = false;
                    }
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void ClearFields()
        {
            selectedTransactionID = 0;
            cmbUserType.SelectedIndex = 0;
            cmbUsers.SelectedIndex = -1;
            cmbBooks.SelectedIndex = -1;
            dtpIssueDate.Value = DateTime.Now;
            dtpDueDate.Value = DateTime.Now.AddDays(14);
            dtpReturnDate.Value = DateTime.Now;
            txtSearch.Clear();
            lblFineAmount.Visible = false;
            btnReturn.Enabled = true;
            btnRenew.Enabled = true;
        }

        private bool HasOverdueBooks()
        {
            if (cmbUsers.SelectedValue == null) return false;

            string userType = cmbUserType.SelectedItem.ToString();
            int userID = Convert.ToInt32(cmbUsers.SelectedValue);

            string query = @"SELECT COUNT(*) FROM Transactions 
                           WHERE UserType = @UserType AND UserID = @UserID 
                           AND Status = 'Issued' AND DueDate < @CurrentDate";

            SqlParameter[] parameters = {
                new SqlParameter("@UserType", userType),
                new SqlParameter("@UserID", userID),
                new SqlParameter("@CurrentDate", DateTime.Now.Date)
            };

            DataTable dt = DatabaseConnection.ExecuteQuery(query, parameters);
            return Convert.ToInt32(dt.Rows[0][0]) > 0;
        }

        private bool HasUnpaidFines()
        {
            if (cmbUsers.SelectedValue == null) return false;

            string userType = cmbUserType.SelectedItem.ToString();
            int userID = Convert.ToInt32(cmbUsers.SelectedValue);

            string query = @"SELECT ISNULL(SUM(Fine), 0) FROM Transactions 
                           WHERE UserType = @UserType AND UserID = @UserID AND Fine > 0";

            SqlParameter[] parameters = {
                new SqlParameter("@UserType", userType),
                new SqlParameter("@UserID", userID)
            };

            DataTable dt = DatabaseConnection.ExecuteQuery(query, parameters);
            decimal totalFines = Convert.ToDecimal(dt.Rows[0][0]);
            return totalFines > 0;
        }

        private bool HasReachedBorrowingLimit()
        {
            if (cmbUsers.SelectedValue == null) return false;

            string userType = cmbUserType.SelectedItem.ToString();
            int userID = Convert.ToInt32(cmbUsers.SelectedValue);

            // Different limits for different user types
            int maxBooks = 5; // Default for members
            switch (userType)
            {
                case "Student":
                    maxBooks = 3;
                    break;
                case "Faculty":
                    maxBooks = 10;
                    break;
            }

            string query = @"SELECT COUNT(*) FROM Transactions 
                           WHERE UserType = @UserType AND UserID = @UserID AND Status = 'Issued'";

            SqlParameter[] parameters = {
                new SqlParameter("@UserType", userType),
                new SqlParameter("@UserID", userID)
            };

            DataTable dt = DatabaseConnection.ExecuteQuery(query, parameters);
            int currentBooks = Convert.ToInt32(dt.Rows[0][0]);
            return currentBooks >= maxBooks;
        }

        private bool CanRenewBook()
        {
            string query = @"SELECT DueDate, Status FROM Transactions WHERE TransactionID = @TransactionID";
            SqlParameter[] parameters = { new SqlParameter("@TransactionID", selectedTransactionID) };
            DataTable dt = DatabaseConnection.ExecuteQuery(query, parameters);

            if (dt.Rows.Count == 0) return false;

            string status = dt.Rows[0]["Status"].ToString();
            DateTime dueDate = Convert.ToDateTime(dt.Rows[0]["DueDate"]);

            // Can't renew if already returned or overdue
            return status == "Issued" && DateTime.Now.Date <= dueDate.Date;
        }

        private decimal CalculateFine()
        {
            return CalculateFineForTransaction(selectedTransactionID);
        }

        private decimal CalculateFineForTransaction(int transactionID)
        {
            string query = "SELECT DueDate FROM Transactions WHERE TransactionID = @TransactionID";
            SqlParameter[] parameters = { new SqlParameter("@TransactionID", transactionID) };
            DataTable dt = DatabaseConnection.ExecuteQuery(query, parameters);

            if (dt.Rows.Count == 0) return 0;

            DateTime dueDate = Convert.ToDateTime(dt.Rows[0]["DueDate"]);
            DateTime returnDate = dtpReturnDate.Value.Date;

            if (returnDate > dueDate)
            {
                int overdueDays = (returnDate - dueDate).Days;
                decimal finePerDay = 0.50m; // $0.50 per day
                return overdueDays * finePerDay;
            }

            return 0;
        }

        private void TransactionForm_Load(object sender, EventArgs e)
        {
            // Set default dates
            dtpIssueDate.Value = DateTime.Now;
            dtpDueDate.Value = DateTime.Now.AddDays(14);
            dtpReturnDate.Value = DateTime.Now;

            // Initialize fine label
            lblFineAmount.Visible = false;
        }

        private void dtpIssueDate_ValueChanged(object sender, EventArgs e)
        {
            // Automatically set due date to 14 days from issue date
            dtpDueDate.Value = dtpIssueDate.Value.AddDays(14);
        }

        private void dtpReturnDate_ValueChanged(object sender, EventArgs e)
        {
            // Recalculate fine when return date changes
            if (selectedTransactionID > 0)
            {
                decimal fine = CalculateFine();
                if (fine > 0)
                {
                    lblFineAmount.Text = $"Fine: ${fine:F2}";
                    lblFineAmount.Visible = true;
                }
                else
                {
                    lblFineAmount.Visible = false;
                }
            }
        }
    }
}