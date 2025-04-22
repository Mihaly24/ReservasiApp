using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Project
{
    public partial class Login : Form
    {
        // Base connection string without credentials
        private string baseConnectionString = "Data Source=MIHALY\\FAIRUZ013;Initial Catalog=ReservasiRestoran;";

        public Login()
        {
            InitializeComponent();
        }

        private void Login_Load(object sender, EventArgs e)
        {
            // You can add initialization logic here, if needed.
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            // Retrieve the username and password from the textboxes.
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;

            try
            {
                // Create a connection string with the provided credentials
                string connectionString = baseConnectionString + $"User ID={username};Password={password}";

                // Test the connection to see if authentication works
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();
                        // If we get here, the connection was successful
                        MessageBox.Show("Login successful");
                        this.Hide(); // Close or hide the login form

                        // Store the authenticated connection string somewhere if needed for further use
                        // For example, in a static property or in a configuration
                        // AppSettings.ConnectionString = connectionString;

                        Dashboard dashboard = new Dashboard();
                        dashboard.Show();
                    }
                    catch (SqlException sqlEx)
                    {
                        // SQL error codes for login failures:
                        // 18456: Login failed for user
                        if (sqlEx.Number == 18456)
                        {
                            MessageBox.Show("Invalid login credentials");
                        }
                        else
                        {
                            MessageBox.Show("Database error: " + sqlEx.Message);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception details
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }
    }
}