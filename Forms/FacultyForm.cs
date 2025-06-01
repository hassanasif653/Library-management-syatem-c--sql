using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using LibraryManagementSystem1.Classes;

namespace LibraryManagementSystem1.Forms
{
    public partial class FacultyForm : Form
    {
        private int selectedFacultyID = 0;

        public FacultyForm()
        {
            InitializeComponent();
            LoadFaculty();
        }

        private void LoadFaculty()
        {
            string query = "SELECT FacultyID, FirstName, LastName, Email, Phone, Address, EmployeeID, Department, Designation, JoiningDate, Status FROM Faculty";
            DataTable dt = DatabaseConnection.ExecuteQuery(query);
            dgvFaculty.DataSource = dt;

            if (dgvFaculty.Columns.Count > 0)
            {
                foreach (DataGridViewColumn column in dgvFaculty.Columns)
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
                string query = @"INSERT INTO Faculty (FirstName, LastName, Email, Phone, Address, EmployeeID, Department, Designation) 
                               VALUES (@FirstName, @LastName, @Email, @Phone, @Address, @EmployeeID, @Department, @Designation)";

                SqlParameter[] parameters = {
                    new SqlParameter("@FirstName", txtFirstName.Text),
                    new SqlParameter("@LastName", txtLastName.Text),
                    new SqlParameter("@Email", txtEmail.Text),
                    new SqlParameter("@Phone", txtPhone.Text),
                    new SqlParameter("@Address", txtAddress.Text),
                    new SqlParameter("@EmployeeID", txtEmployeeID.Text),
                    new SqlParameter("@Department", txtDepartment.Text),
                    new SqlParameter("@Designation", txtDesignation.Text)
                };

                int result = DatabaseConnection.ExecuteNonQuery(query, parameters);
                if (result > 0)
                {
                    MessageBox.Show("Faculty member added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadFaculty();
                    ClearFields();
                }
                else
                {
                    MessageBox.Show("Failed to add faculty member.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedFacultyID == 0)
            {
                MessageBox.Show("Please select a faculty member to update.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (ValidateInput())
            {
                string query = @"UPDATE Faculty SET FirstName = @FirstName, LastName = @LastName, Email = @Email, 
                               Phone = @Phone, Address = @Address, EmployeeID = @EmployeeID, 
                               Department = @Department, Designation = @Designation, Status = @Status 
                               WHERE FacultyID = @FacultyID";

                SqlParameter[] parameters = {
                    new SqlParameter("@FirstName", txtFirstName.Text),
                    new SqlParameter("@LastName", txtLastName.Text),
                    new SqlParameter("@Email", txtEmail.Text),
                    new SqlParameter("@Phone", txtPhone.Text),
                    new SqlParameter("@Address", txtAddress.Text),
                    new SqlParameter("@EmployeeID", txtEmployeeID.Text),
                    new SqlParameter("@Department", txtDepartment.Text),
                    new SqlParameter("@Designation", txtDesignation.Text),
                    new SqlParameter("@Status", cmbStatus.SelectedItem.ToString()),
                    new SqlParameter("@FacultyID", selectedFacultyID)
                };

                int result = DatabaseConnection.ExecuteNonQuery(query, parameters);
                if (result > 0)
                {
                    MessageBox.Show("Faculty member updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadFaculty();
                    ClearFields();
                }
                else
                {
                    MessageBox.Show("Failed to update faculty member.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedFacultyID == 0)
            {
                MessageBox.Show("Please select a faculty member to delete.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show("Are you sure you want to delete this faculty member?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                string query = "DELETE FROM Faculty WHERE FacultyID = @FacultyID";
                SqlParameter[] parameters = { new SqlParameter("@FacultyID", selectedFacultyID) };

                int result = DatabaseConnection.ExecuteNonQuery(query, parameters);
                if (result > 0)
                {
                    MessageBox.Show("Faculty member deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadFaculty();
                    ClearFields();
                }
                else
                {
                    MessageBox.Show("Failed to delete faculty member.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                LoadFaculty();
                return;
            }

            string query = @"SELECT * FROM Faculty WHERE FirstName LIKE @keyword OR LastName LIKE @keyword 
                           OR Email LIKE @keyword OR EmployeeID LIKE @keyword OR Department LIKE @keyword OR Designation LIKE @keyword";
            SqlParameter[] parameters = { new SqlParameter("@keyword", "%" + keyword + "%") };
            DataTable dt = DatabaseConnection.ExecuteQuery(query, parameters);
            dgvFaculty.DataSource = dt;
        }

        private void dgvFaculty_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvFaculty.Rows[e.RowIndex];
                selectedFacultyID = Convert.ToInt32(row.Cells["FacultyID"].Value);
                txtFirstName.Text = row.Cells["FirstName"].Value.ToString();
                txtLastName.Text = row.Cells["LastName"].Value.ToString();
                txtEmail.Text = row.Cells["Email"].Value.ToString();
                txtPhone.Text = row.Cells["Phone"].Value.ToString();
                txtAddress.Text = row.Cells["Address"].Value.ToString();
                txtEmployeeID.Text = row.Cells["EmployeeID"].Value.ToString();
                txtDepartment.Text = row.Cells["Department"].Value.ToString();
                txtDesignation.Text = row.Cells["Designation"].Value.ToString();
                cmbStatus.SelectedItem = row.Cells["Status"].Value.ToString();
            }
        }

        private void ClearFields()
        {
            selectedFacultyID = 0;
            txtFirstName.Clear();
            txtLastName.Clear();
            txtEmail.Clear();
            txtPhone.Clear();
            txtAddress.Clear();
            txtEmployeeID.Clear();
            txtDepartment.Clear();
            txtDesignation.Clear();
            txtSearch.Clear();
            cmbStatus.SelectedIndex = 0; // Default to Active
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtFirstName.Text) ||
                string.IsNullOrWhiteSpace(txtLastName.Text) ||
                string.IsNullOrWhiteSpace(txtEmail.Text) ||
                string.IsNullOrWhiteSpace(txtEmployeeID.Text) ||
                string.IsNullOrWhiteSpace(txtDepartment.Text) ||
                string.IsNullOrWhiteSpace(txtDesignation.Text))
            {
                MessageBox.Show("Please fill in all required fields (First Name, Last Name, Email, Employee ID, Department, Designation).",
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

        private void FacultyForm_Load(object sender, EventArgs e)
        {
            // Initialize status combo box
            cmbStatus.Items.Clear();
            cmbStatus.Items.AddRange(new string[] { "Active", "Inactive", "Retired", "On Leave" });
            cmbStatus.SelectedIndex = 0;
        }
    }
}