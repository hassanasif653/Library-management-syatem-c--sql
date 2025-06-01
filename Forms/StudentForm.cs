using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using LibraryManagementSystem1.Classes;

namespace LibraryManagementSystem1.Forms
{
    public partial class StudentForm : Form
    {
        private int selectedStudentID = 0;

        public StudentForm()
        {
            InitializeComponent();
            LoadStudents();
        }

        private void LoadStudents()
        {
            string query = "SELECT StudentID, FirstName, LastName, Email, Phone, Address, StudentNumber, Department, Semester, EnrollmentDate, Status FROM Students";
            DataTable dt = DatabaseConnection.ExecuteQuery(query);
            dgvStudents.DataSource = dt;

            if (dgvStudents.Columns.Count > 0)
            {
                foreach (DataGridViewColumn column in dgvStudents.Columns)
                {
                    column.HeaderCell.Style.BackColor = System.Drawing.Color.FromArgb(50, 50, 100);
                    column.HeaderCell.Style.ForeColor = System.Drawing.Color.White;
                }
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (ValidateInput())
            {
                string query = @"INSERT INTO Students (FirstName, LastName, Email, Phone, Address, StudentNumber, Department, Semester) 
                               VALUES (@FirstName, @LastName, @Email, @Phone, @Address, @StudentNumber, @Department, @Semester)";

                SqlParameter[] parameters = {
                    new SqlParameter("@FirstName", txtFirstName.Text),
                    new SqlParameter("@LastName", txtLastName.Text),
                    new SqlParameter("@Email", txtEmail.Text),
                    new SqlParameter("@Phone", txtPhone.Text),
                    new SqlParameter("@Address", txtAddress.Text),
                    new SqlParameter("@StudentNumber", txtStudentNumber.Text),
                    new SqlParameter("@Department", txtDepartment.Text),
                    new SqlParameter("@Semester", txtSemester.Text)
                };

                int result = DatabaseConnection.ExecuteNonQuery(query, parameters);
                if (result > 0)
                {
                    MessageBox.Show("Student added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadStudents();
                    ClearFields();
                }
                else
                {
                    MessageBox.Show("Failed to add student.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedStudentID == 0)
            {
                MessageBox.Show("Please select a student to update.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (ValidateInput())
            {
                string query = @"UPDATE Students SET FirstName = @FirstName, LastName = @LastName, Email = @Email, 
                               Phone = @Phone, Address = @Address, StudentNumber = @StudentNumber, 
                               Department = @Department, Semester = @Semester, Status = @Status 
                               WHERE StudentID = @StudentID";

                SqlParameter[] parameters = {
                    new SqlParameter("@FirstName", txtFirstName.Text),
                    new SqlParameter("@LastName", txtLastName.Text),
                    new SqlParameter("@Email", txtEmail.Text),
                    new SqlParameter("@Phone", txtPhone.Text),
                    new SqlParameter("@Address", txtAddress.Text),
                    new SqlParameter("@StudentNumber", txtStudentNumber.Text),
                    new SqlParameter("@Department", txtDepartment.Text),
                    new SqlParameter("@Semester", txtSemester.Text),
                    new SqlParameter("@Status", cmbStatus.SelectedItem.ToString()),
                    new SqlParameter("@StudentID", selectedStudentID)
                };

                int result = DatabaseConnection.ExecuteNonQuery(query, parameters);
                if (result > 0)
                {
                    MessageBox.Show("Student updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadStudents();
                    ClearFields();
                }
                else
                {
                    MessageBox.Show("Failed to update student.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedStudentID == 0)
            {
                MessageBox.Show("Please select a student to delete.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show("Are you sure you want to delete this student?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                string query = "DELETE FROM Students WHERE StudentID = @StudentID";
                SqlParameter[] parameters = { new SqlParameter("@StudentID", selectedStudentID) };

                int result = DatabaseConnection.ExecuteNonQuery(query, parameters);
                if (result > 0)
                {
                    MessageBox.Show("Student deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadStudents();
                    ClearFields();
                }
                else
                {
                    MessageBox.Show("Failed to delete student.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearFields();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim();
            if (string.IsNullOrEmpty(keyword))
            {
                LoadStudents();
                return;
            }

            string query = @"SELECT * FROM Students WHERE FirstName LIKE @keyword OR LastName LIKE @keyword 
                           OR Email LIKE @keyword OR StudentNumber LIKE @keyword OR Department LIKE @keyword";
            SqlParameter[] parameters = { new SqlParameter("@keyword", "%" + keyword + "%") };
            DataTable dt = DatabaseConnection.ExecuteQuery(query, parameters);
            dgvStudents.DataSource = dt;
        }

        private void dgvStudents_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvStudents.Rows[e.RowIndex];
                selectedStudentID = Convert.ToInt32(row.Cells["StudentID"].Value);
                txtFirstName.Text = row.Cells["FirstName"].Value.ToString();
                txtLastName.Text = row.Cells["LastName"].Value.ToString();
                txtEmail.Text = row.Cells["Email"].Value.ToString();
                txtPhone.Text = row.Cells["Phone"].Value.ToString();
                txtAddress.Text = row.Cells["Address"].Value.ToString();
                txtStudentNumber.Text = row.Cells["StudentNumber"].Value.ToString();
                txtDepartment.Text = row.Cells["Department"].Value.ToString();
                txtSemester.Text = row.Cells["Semester"].Value.ToString();
                cmbStatus.SelectedItem = row.Cells["Status"].Value.ToString();
            }
        }

        private void ClearFields()
        {
            selectedStudentID = 0;
            txtFirstName.Clear();
            txtLastName.Clear();
            txtEmail.Clear();
            txtPhone.Clear();
            txtAddress.Clear();
            txtStudentNumber.Clear();
            txtDepartment.Clear();
            txtSemester.Clear();
            txtSearch.Clear();
            cmbStatus.SelectedIndex = 0; // Default to Active
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtStudentNumber.Text) ||
                string.IsNullOrWhiteSpace(txtDepartment.Text))
            {
                MessageBox.Show("Please fill in all required fields (First Name, Last Name, Email, Student Number, Department).",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            // Check for valid email format
            if (!IsValidEmail(txtEmail.Text))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            return true;
        }

        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private void StudentForm_Load(object sender, EventArgs e)
        {
            // Initialize status combo box
            cmbStatus.Items.Clear();
            cmbStatus.Items.AddRange(new string[] { "Active", "Inactive", "Graduated", "Suspended" });
            cmbStatus.SelectedIndex = 0;
        }
    }
}