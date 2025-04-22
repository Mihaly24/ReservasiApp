using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project
{
    public partial class Admin : Form
    {
        // Database connection string
        private string connectionString = "Data Source=MIHALY\\FAIRUZ013;Initial Catalog=ReservasiRestoran;Integrated Security=True";
        private int selectedAdminId = 0;

        public Admin()
        {
            InitializeComponent();
        }

        private void Admin_Load(object sender, EventArgs e)
        {
            // Load admin data when form loads
            LoadAdminData();

            // Set up event for selecting a row in the DataGridView
            dgvAdminResto.SelectionChanged += DgvAdminResto_SelectionChanged;
        }

        private void LoadAdminData()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // SQL query to get all admin data
                    string query = "SELECT * FROM AdminResto";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);

                        // Bind the DataTable to the DataGridView
                        dgvAdminResto.DataSource = dataTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading admin data: " + ex.Message);
            }
        }

        private void ClearInputFields()
        {
            txtNama.Text = "";
            txtUsername.Text = "";
            txtPassword.Text = "";
            selectedAdminId = 0;
        }

        private void DgvAdminResto_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvAdminResto.CurrentRow != null)
            {
                try
                {
                    // Get the ID of the selected admin
                    selectedAdminId = Convert.ToInt32(dgvAdminResto.CurrentRow.Cells["admin_id"].Value);

                    // Fill the text fields with the selected admin's data
                    txtNama.Text = dgvAdminResto.CurrentRow.Cells["nama"].Value.ToString();
                    txtUsername.Text = dgvAdminResto.CurrentRow.Cells["username"].Value.ToString();
                    txtPassword.Text = dgvAdminResto.CurrentRow.Cells["Password"].Value.ToString();
                }
                catch (Exception ex)
                {
                    // If there's an error (like column name doesn't exist), just log it
                    Console.WriteLine("Error in selection changed: " + ex.Message);
                }
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            // Reload admin data and clear input fields
            LoadAdminData();
            ClearInputFields();
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(txtNama.Text) ||
                string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check if username already exists
                    string checkQuery = "SELECT COUNT(*) FROM AdminResto WHERE username = @Username";
                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Username", txtUsername.Text);
                        int count = (int)checkCommand.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBox.Show("Username already exists. Please choose a different username.");
                            return;
                        }
                    }

                    // Insert new admin
                    string insertQuery = "INSERT INTO AdminResto (nama, username, Password) " +
                                         "VALUES (@Nama, @Username, @Password)";

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Nama", txtNama.Text);
                        command.Parameters.AddWithValue("@Username", txtUsername.Text);
                        command.Parameters.AddWithValue("@Password", txtPassword.Text);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Admin added successfully!");
                            ClearInputFields();
                            LoadAdminData();
                        }
                        else
                        {
                            MessageBox.Show("Failed to add admin.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding admin: " + ex.Message);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            // Validate selection
            if (selectedAdminId == 0)
            {
                MessageBox.Show("Please select an admin to update.");
                return;
            }

            // Validate input
            if (string.IsNullOrWhiteSpace(txtNama.Text) ||
                string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check if username already exists for other admins
                    string checkQuery = "SELECT COUNT(*) FROM AdminResto WHERE username = @Username AND admin_id != @AdminID";
                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Username", txtUsername.Text);
                        checkCommand.Parameters.AddWithValue("@AdminID", selectedAdminId);
                        int count = (int)checkCommand.ExecuteScalar();

                        if (count > 0)
                        {
                            MessageBox.Show("Username already exists. Please choose a different username.");
                            return;
                        }
                    }

                    // Update admin
                    string updateQuery = "UPDATE AdminResto SET nama = @Nama, username = @Username, " +
                                         "Password = @Password WHERE admin_id = @AdminID";

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@AdminID", selectedAdminId);
                        command.Parameters.AddWithValue("@Nama", txtNama.Text);
                        command.Parameters.AddWithValue("@Username", txtUsername.Text);
                        command.Parameters.AddWithValue("@Password", txtPassword.Text);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Admin updated successfully!");
                            ClearInputFields();
                            LoadAdminData();
                        }
                        else
                        {
                            MessageBox.Show("Failed to update admin.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating admin: " + ex.Message);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            // Validate selection
            if (selectedAdminId == 0)
            {
                MessageBox.Show("Please select an admin to delete.");
                return;
            }

            // Confirm deletion
            DialogResult confirmResult = MessageBox.Show("Are you sure you want to delete this admin?",
                                                        "Confirm Delete",
                                                        MessageBoxButtons.YesNo);
            if (confirmResult == DialogResult.No)
            {
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Delete admin
                    string deleteQuery = "DELETE FROM AdminResto WHERE admin_id = @AdminID";

                    using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@AdminID", selectedAdminId);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            MessageBox.Show("Admin deleted successfully!");
                            ClearInputFields();
                            LoadAdminData();
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete admin.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting admin: " + ex.Message);
            }
        }
    }
}