using System;
using System.Windows.Forms;
using LibraryManagementSystem1.Forms;

namespace LibraryManagementSystem1
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (Classes.DatabaseConnection.TestConnection())
            {
                Application.Run(new LoginForm());
            }
            else
            {
                MessageBox.Show("Failed to connect to database. Please check your connection settings.");
            }
        }
    }
}