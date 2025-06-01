using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using LibraryManagementSystem1.Classes;

namespace LibraryManagementSystem1.Forms
{
    public partial class BookForm : Form
    {
        private int selectedBookID = 0;

        public BookForm()
        {
            InitializeComponent();
            LoadBooks();
        }

        private void LoadBooks()
        {
            string query = "SELECT BookID, Title, Author, ISBN, Publisher, Category, TotalCopies, AvailableCopies, DateAdded FROM Books";
            DataTable dt = DatabaseConnection.ExecuteQuery(query);
            dgvBooks.DataSource = dt;

            if (dgvBooks.Columns.Count > 0)
            {
                foreach (DataGridViewColumn column in dgvBooks.Columns)
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
                int totalCopies = Convert.ToInt32(txtTotalCopies.Text);
                string query = @"INSERT INTO Books (Title, Author, ISBN, Publisher, Category, TotalCopies, AvailableCopies) 
                               VALUES (@Title, @Author, @ISBN, @Publisher, @Category, @TotalCopies, @AvailableCopies)";

                SqlParameter[] parameters = {
                    new SqlParameter("@Title", txtTitle.Text),
                    new SqlParameter("@Author", txtAuthor.Text),
                    new SqlParameter("@ISBN", txtISBN.Text),
                    new SqlParameter("@Publisher", txtPublisher.Text),
                    new SqlParameter("@Category", txtCategory.Text),
                    new SqlParameter("@TotalCopies", totalCopies),
                    new SqlParameter("@AvailableCopies", totalCopies)
                };

                int result = DatabaseConnection.ExecuteNonQuery(query, parameters);
                if (result > 0)
                {
                    MessageBox.Show("Book added successfully!");
                    LoadBooks();
                    ClearFields();
                }
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedBookID == 0)
            {
                MessageBox.Show("Please select a book to update.");
                return;
            }

            if (ValidateInput())
            {
                string query = @"UPDATE Books SET Title = @Title, Author = @Author, ISBN = @ISBN, 
                               Publisher = @Publisher, Category = @Category, TotalCopies = @TotalCopies 
                               WHERE BookID = @BookID";

                SqlParameter[] parameters = {
                    new SqlParameter("@Title", txtTitle.Text),
                    new SqlParameter("@Author", txtAuthor.Text),
                    new SqlParameter("@ISBN", txtISBN.Text),
                    new SqlParameter("@Publisher", txtPublisher.Text),
                    new SqlParameter("@Category", txtCategory.Text),
                    new SqlParameter("@TotalCopies", Convert.ToInt32(txtTotalCopies.Text)),
                    new SqlParameter("@BookID", selectedBookID)
                };

                int result = DatabaseConnection.ExecuteNonQuery(query, parameters);
                if (result > 0)
                {
                    MessageBox.Show("Book updated successfully!");
                    LoadBooks();
                    ClearFields();
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedBookID == 0)
            {
                MessageBox.Show("Please select a book to delete.");
                return;
            }

            DialogResult confirm = MessageBox.Show("Are you sure you want to delete this book?", "Confirm Delete", MessageBoxButtons.YesNo);
            if (confirm == DialogResult.Yes)
            {
                string query = "DELETE FROM Books WHERE BookID = @BookID";
                SqlParameter[] parameters = { new SqlParameter("@BookID", selectedBookID) };
                int result = DatabaseConnection.ExecuteNonQuery(query, parameters);
                if (result > 0)
                {
                    MessageBox.Show("Book deleted successfully!");
                    LoadBooks();
                    ClearFields();
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
            string query = "SELECT * FROM Books WHERE Title LIKE @keyword OR Author LIKE @keyword OR ISBN LIKE @keyword";
            SqlParameter[] parameters = { new SqlParameter("@keyword", "%" + keyword + "%") };
            DataTable dt = DatabaseConnection.ExecuteQuery(query, parameters);
            dgvBooks.DataSource = dt;
        }

        private void dgvBooks_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvBooks.Rows[e.RowIndex];
                selectedBookID = Convert.ToInt32(row.Cells["BookID"].Value);
                txtTitle.Text = row.Cells["Title"].Value.ToString();
                txtAuthor.Text = row.Cells["Author"].Value.ToString();
                txtISBN.Text = row.Cells["ISBN"].Value.ToString();
                txtPublisher.Text = row.Cells["Publisher"].Value.ToString();
                txtCategory.Text = row.Cells["Category"].Value.ToString();
                txtTotalCopies.Text = row.Cells["TotalCopies"].Value.ToString();
            }
        }

        private void ClearFields()
        {
            selectedBookID = 0;
            txtTitle.Clear();
            txtAuthor.Clear();
            txtISBN.Clear();
            txtPublisher.Clear();
            txtCategory.Clear();
            txtTotalCopies.Clear();
            txtSearch.Clear();
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text) ||
                string.IsNullOrWhiteSpace(txtAuthor.Text) ||
                string.IsNullOrWhiteSpace(txtISBN.Text) ||
                string.IsNullOrWhiteSpace(txtPublisher.Text) ||
                string.IsNullOrWhiteSpace(txtCategory.Text) ||
                string.IsNullOrWhiteSpace(txtTotalCopies.Text))
            {
                MessageBox.Show("All fields are required.");
                return false;
            }

            if (!int.TryParse(txtTotalCopies.Text, out _))
            {
                MessageBox.Show("Total copies must be a valid number.");
                return false;
            }

            return true;
        }

        private void BookForm_Load(object sender, EventArgs e)
        {

        }
    }
}
