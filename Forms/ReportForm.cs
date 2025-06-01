using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;
using LibraryManagementSystem1.Classes;

namespace LibraryManagementSystem1.Forms
{
    public partial class ReportForm : Form
    {
        public ReportForm()
        {
            InitializeComponent();
        }

        private void btnGenerate_Click(object sender, EventArgs e)
        {
            if (cmbReportType.SelectedIndex == -1)
            {
                MessageBox.Show("Please select a report type.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string reportType = cmbReportType.SelectedItem.ToString();
            string query = GetReportQuery(reportType);

            try
            {
                DataTable dt = DatabaseConnection.ExecuteQuery(query);
                dgvReports.DataSource = dt;

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("No data found for the selected report.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show($"Report generated successfully! Found {dt.Rows.Count} records.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generating report: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private string GetReportQuery(string reportType)
        {
            switch (reportType)
            {
                case "Books Report":
                    return @"SELECT b.BookID, b.Title, b.Author, b.ISBN, b.Publisher, 
                           b.Category, b.TotalCopies, b.AvailableCopies,
                           (b.TotalCopies - b.AvailableCopies) AS IssuedCopies
                           FROM Books b
                           ORDER BY b.Title";

                case "Users Report":
                    return @"SELECT 
                           'Member' AS UserType,
                           MemberID AS ID,
                           FirstName + ' ' + LastName AS FullName,
                           Email, Phone, Address,
                           MembershipDate AS RegistrationDate,
                           Status,
                           (SELECT COUNT(*) FROM Transactions t WHERE t.UserType = 'Member' AND t.UserID = m.MemberID) AS TotalTransactions
                           FROM Members m
                           
                           UNION ALL
                           
                           SELECT 
                           'Student' AS UserType,
                           StudentID AS ID,
                           FirstName + ' ' + LastName AS FullName,
                           Email, Phone, Address,
                           EnrollmentDate AS RegistrationDate,
                           Status,
                           (SELECT COUNT(*) FROM Transactions t WHERE t.UserType = 'Student' AND t.UserID = s.StudentID) AS TotalTransactions
                           FROM Students s
                           
                           UNION ALL
                           
                           SELECT 
                           'Faculty' AS UserType,
                           FacultyID AS ID,
                           FirstName + ' ' + LastName AS FullName,
                           Email, Phone, Address,
                           JoiningDate AS RegistrationDate,
                           Status,
                           (SELECT COUNT(*) FROM Transactions t WHERE t.UserType = 'Faculty' AND t.UserID = f.FacultyID) AS TotalTransactions
                           FROM Faculty f
                           
                           ORDER BY UserType, FullName";

                case "Currently Issued Books":
                    return @"SELECT 
                           t.TransactionID, 
                           CASE 
                               WHEN t.UserType = 'Member' THEN m.FirstName + ' ' + m.LastName
                               WHEN t.UserType = 'Student' THEN s.FirstName + ' ' + s.LastName
                               WHEN t.UserType = 'Faculty' THEN f.FirstName + ' ' + f.LastName
                           END AS UserName,
                           t.UserType,
                           b.Title AS BookTitle, 
                           b.Author, 
                           t.IssueDate, 
                           t.DueDate,
                           DATEDIFF(day, GETDATE(), t.DueDate) AS DaysRemaining,
                           CASE 
                               WHEN t.UserType = 'Member' THEN m.Email
                               WHEN t.UserType = 'Student' THEN s.Email
                               WHEN t.UserType = 'Faculty' THEN f.Email
                           END AS UserEmail
                           FROM Transactions t
                           LEFT JOIN Members m ON t.UserType = 'Member' AND t.UserID = m.MemberID
                           LEFT JOIN Students s ON t.UserType = 'Student' AND t.UserID = s.StudentID
                           LEFT JOIN Faculty f ON t.UserType = 'Faculty' AND t.UserID = f.FacultyID
                           JOIN Books b ON t.BookID = b.BookID
                           WHERE t.Status = 'Issued'
                           ORDER BY t.DueDate";

                case "Overdue Books":
                    return @"SELECT 
                           t.TransactionID, 
                           CASE 
                               WHEN t.UserType = 'Member' THEN m.FirstName + ' ' + m.LastName
                               WHEN t.UserType = 'Student' THEN s.FirstName + ' ' + s.LastName
                               WHEN t.UserType = 'Faculty' THEN f.FirstName + ' ' + f.LastName
                           END AS UserName,
                           t.UserType,
                           CASE 
                               WHEN t.UserType = 'Member' THEN m.Phone
                               WHEN t.UserType = 'Student' THEN s.Phone
                               WHEN t.UserType = 'Faculty' THEN f.Phone
                           END AS ContactNumber,
                           b.Title AS BookTitle, 
                           b.Author,
                           t.IssueDate, 
                           t.DueDate, 
                           DATEDIFF(day, t.DueDate, GETDATE()) AS DaysOverdue,
                           (DATEDIFF(day, t.DueDate, GETDATE()) * 1.0) AS EstimatedFine,
                           CASE 
                               WHEN t.UserType = 'Member' THEN m.Email
                               WHEN t.UserType = 'Student' THEN s.Email
                               WHEN t.UserType = 'Faculty' THEN f.Email
                           END AS UserEmail
                           FROM Transactions t
                           LEFT JOIN Members m ON t.UserType = 'Member' AND t.UserID = m.MemberID
                           LEFT JOIN Students s ON t.UserType = 'Student' AND t.UserID = s.StudentID
                           LEFT JOIN Faculty f ON t.UserType = 'Faculty' AND t.UserID = f.FacultyID
                           JOIN Books b ON t.BookID = b.BookID
                           WHERE t.Status = 'Issued' AND t.DueDate < GETDATE()
                           ORDER BY DaysOverdue DESC";

                case "Transaction History":
                    return @"SELECT 
                           t.TransactionID, 
                           CASE 
                               WHEN t.UserType = 'Member' THEN m.FirstName + ' ' + m.LastName
                               WHEN t.UserType = 'Student' THEN s.FirstName + ' ' + s.LastName
                               WHEN t.UserType = 'Faculty' THEN f.FirstName + ' ' + f.LastName
                           END AS UserName,
                           t.UserType,
                           b.Title AS BookTitle, 
                           b.Author, 
                           t.IssueDate, 
                           t.DueDate, 
                           t.ReturnDate, 
                           t.Status, 
                           ISNULL(t.Fine, 0) AS Fine,
                           CASE 
                               WHEN t.UserType = 'Member' THEN m.Email
                               WHEN t.UserType = 'Student' THEN s.Email
                               WHEN t.UserType = 'Faculty' THEN f.Email
                           END AS UserEmail
                           FROM Transactions t
                           LEFT JOIN Members m ON t.UserType = 'Member' AND t.UserID = m.MemberID
                           LEFT JOIN Students s ON t.UserType = 'Student' AND t.UserID = s.StudentID
                           LEFT JOIN Faculty f ON t.UserType = 'Faculty' AND t.UserID = f.FacultyID
                           JOIN Books b ON t.BookID = b.BookID
                           ORDER BY t.IssueDate DESC";

                default:
                    throw new ArgumentException("Invalid report type selected");
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (dgvReports.DataSource == null)
            {
                MessageBox.Show("Please generate a report first before exporting.", "No Data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "CSV files (*.csv)|*.csv|Excel files (*.xlsx)|*.xlsx";
            saveFileDialog.Title = "Save Report";
            saveFileDialog.FileName = $"LibraryReport_{DateTime.Now:yyyyMMdd_HHmmss}";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    if (saveFileDialog.FilterIndex == 1) // CSV
                    {
                        ExportToCSV(dgvReports, saveFileDialog.FileName);
                    }
                    else // Excel
                    {
                        ExportToExcel(dgvReports, saveFileDialog.FileName);
                    }
                    MessageBox.Show("Report exported successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error exporting report: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ExportToCSV(DataGridView dataGridView, string fileName)
        {
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(fileName))
            {
                // Write headers
                for (int i = 0; i < dataGridView.Columns.Count; i++)
                {
                    writer.Write(dataGridView.Columns[i].HeaderText);
                    if (i < dataGridView.Columns.Count - 1)
                        writer.Write(",");
                }
                writer.WriteLine();

                // Write data rows
                foreach (DataGridViewRow row in dataGridView.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        for (int i = 0; i < dataGridView.Columns.Count; i++)
                        {
                            object cellValue = row.Cells[i].Value;
                            string value = cellValue?.ToString() ?? "";

                            // Escape commas and quotes in CSV
                            if (value.Contains(",") || value.Contains("\""))
                            {
                                value = "\"" + value.Replace("\"", "\"\"") + "\"";
                            }

                            writer.Write(value);
                            if (i < dataGridView.Columns.Count - 1)
                                writer.Write(",");
                        }
                        writer.WriteLine();
                    }
                }
            }
        }

        private void ExportToExcel(DataGridView dataGridView, string fileName)
        {
            try
            {
                Microsoft.Office.Interop.Excel.Application excel = new Microsoft.Office.Interop.Excel.Application();
                excel.Visible = false;
                Microsoft.Office.Interop.Excel.Workbook workbook = excel.Workbooks.Add(Type.Missing);
                Microsoft.Office.Interop.Excel.Worksheet worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.ActiveSheet;

                // Header
                for (int i = 1; i <= dataGridView.Columns.Count; i++)
                {
                    worksheet.Cells[1, i] = dataGridView.Columns[i - 1].HeaderText;
                }

                // Data
                for (int i = 0; i < dataGridView.Rows.Count; i++)
                {
                    if (!dataGridView.Rows[i].IsNewRow)
                    {
                        for (int j = 0; j < dataGridView.Columns.Count; j++)
                        {
                            worksheet.Cells[i + 2, j + 1] = dataGridView.Rows[i].Cells[j].Value?.ToString();
                        }
                    }
                }

                // Auto-fit columns
                worksheet.Columns.AutoFit();

                // Save and close
                workbook.SaveAs(fileName);
                workbook.Close();
                excel.Quit();

                // Release COM objects
                System.Runtime.InteropServices.Marshal.ReleaseComObject(worksheet);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(workbook);
                System.Runtime.InteropServices.Marshal.ReleaseComObject(excel);
            }
            catch (Exception ex)
            {
                throw new Exception("Excel export failed: " + ex.Message);
            }
        }

        private void ReportForm_Load(object sender, EventArgs e)
        {
            cmbReportType.Items.Clear();
            cmbReportType.Items.AddRange(new string[] {
                "Books Report",
                "Users Report",
                "Currently Issued Books",
                "Overdue Books",
                "Transaction History"
            });
            cmbReportType.SelectedIndex = 0;

            // Configure DataGridView
            dgvReports.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvReports.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvReports.MultiSelect = false;
            dgvReports.ReadOnly = true;
            dgvReports.AllowUserToAddRows = false;
            dgvReports.AllowUserToDeleteRows = false;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}