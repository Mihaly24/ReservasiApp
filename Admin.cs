using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions; // Required for Regex
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project
{
    public partial class Admin : Form
    {
        // Database connection string
        private string connectionString = "Data Source=MIHALY\\FAIRUZ013;Initial Catalog=ReservasiRestoran;Integrated Security=True"; //
        private int selectedAdminId = 0; //

        public Admin()
        {
            InitializeComponent();
        }

        private void Admin_Load(object sender, EventArgs e)
        {
            // Load admin data when form loads
            LoadAdminData(); //

            // Set up event for selecting a row in the DataGridView
            dgvAdminResto.SelectionChanged += DgvAdminResto_SelectionChanged; //
        }

        private void LoadAdminData()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString)) //
                {
                    connection.Open(); //

                    // SQL query to get all admin data
                    string query = "SELECT * FROM AdminResto"; //

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, connection)) //
                    {
                        DataTable dataTable = new DataTable(); //
                        adapter.Fill(dataTable); //

                        // Bind the DataTable to the DataGridView
                        dgvAdminResto.DataSource = dataTable; //
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading admin data: " + ex.Message, "Data Loading Error", MessageBoxButtons.OK, MessageBoxIcon.Error); //
            }
        }

        private void ClearInputFields()
        {
            txtNama.Text = ""; //
            txtUsername.Text = ""; //
            txtPassword.Text = ""; //
            selectedAdminId = 0; //
        }

        private void DgvAdminResto_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvAdminResto.CurrentRow != null) //
            {
                try
                {
                    // Get the ID of the selected admin
                    selectedAdminId = Convert.ToInt32(dgvAdminResto.CurrentRow.Cells["admin_id"].Value); //

                    // Fill the text fields with the selected admin's data
                    txtNama.Text = dgvAdminResto.CurrentRow.Cells["nama"].Value.ToString(); //
                    txtUsername.Text = dgvAdminResto.CurrentRow.Cells["username"].Value.ToString(); //
                    txtPassword.Text = dgvAdminResto.CurrentRow.Cells["Passwords"].Value.ToString(); //
                }
                catch (Exception ex)
                {
                    // If there's an error (like column name doesn't exist), just log it
                    Console.WriteLine("Error in selection changed: " + ex.Message); //
                    MessageBox.Show("Error processing selection: " + ex.Message, "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        // Helper method for input validation
        private bool ValidateAdminInput(out string errorMessage)
        {
            errorMessage = string.Empty;

            // Validate Nama
            if (string.IsNullOrWhiteSpace(txtNama.Text))
            {
                errorMessage = "Nama field cannot be empty.";
                return false;
            }
            if (!Regex.IsMatch(txtNama.Text, @"^[a-zA-Z\s]+$"))
            {
                errorMessage = "Nama can only contain letters and spaces.";
                return false;
            }
            if (txtNama.Text.Length > 30)
            {
                errorMessage = "Nama cannot exceed 30 characters.";
                return false;
            }

            // Validate Username
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                errorMessage = "Username field cannot be empty.";
                return false;
            }
            // Add more specific username validation if needed (e.g., length, no special characters other than underscore)
            // For example: if (!Regex.IsMatch(txtUsername.Text, @"^[a-zA-Z0-9_]+$")) { errorMessage = "Username can only contain letters, numbers, and underscores."; return false; }


            // Validate Password
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                errorMessage = "Password field cannot be empty.";
                return false;
            }
            // Add password complexity rules if needed (e.g., minimum length, special characters)
            // For example: if (txtPassword.Text.Length < 6) { errorMessage = "Password must be at least 6 characters long."; return false; }

            return true;
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            // Reload admin data and clear input fields
            LoadAdminData(); //
            ClearInputFields(); //
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateAdminInput(out string validationMessage))
            {
                MessageBox.Show(validationMessage, "Input Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString)) //
                {
                    connection.Open(); //

                    // Check if username already exists
                    string checkQuery = "SELECT COUNT(*) FROM AdminResto WHERE username = @Username"; //
                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection)) //
                    {
                        checkCommand.Parameters.AddWithValue("@Username", txtUsername.Text); //
                        int count = (int)checkCommand.ExecuteScalar(); //

                        if (count > 0)
                        {
                            MessageBox.Show("Username already exists. Please choose a different username.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); //
                            return;
                        }
                    }

                    // Insert new admin
                    string insertQuery = "INSERT INTO AdminResto (nama, username, Passwords) " + //
                                         "VALUES (@Nama, @Username, @Password)"; //

                    using (SqlCommand command = new SqlCommand(insertQuery, connection)) //
                    {
                        command.Parameters.AddWithValue("@Nama", txtNama.Text); //
                        command.Parameters.AddWithValue("@Username", txtUsername.Text); //
                        command.Parameters.AddWithValue("@Password", txtPassword.Text); //

                        int result = command.ExecuteNonQuery(); //

                        if (result > 0)
                        {
                            MessageBox.Show("Admin added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); //
                            ClearInputFields(); //
                            LoadAdminData(); //
                        }
                        else
                        {
                            MessageBox.Show("Failed to add admin.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); //
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding admin: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); //
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            // Validate selection
            if (selectedAdminId == 0) //
            {
                MessageBox.Show("Please select an admin to update.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information); //
                return;
            }

            if (!ValidateAdminInput(out string validationMessage))
            {
                MessageBox.Show(validationMessage, "Input Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString)) //
                {
                    connection.Open(); //

                    // Check if username already exists for other admins
                    string checkQuery = "SELECT COUNT(*) FROM AdminResto WHERE username = @Username AND admin_id != @AdminID"; //
                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection)) //
                    {
                        checkCommand.Parameters.AddWithValue("@Username", txtUsername.Text); //
                        checkCommand.Parameters.AddWithValue("@AdminID", selectedAdminId); //
                        int count = (int)checkCommand.ExecuteScalar(); //

                        if (count > 0)
                        {
                            MessageBox.Show("Username already exists. Please choose a different username.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); //
                            return;
                        }
                    }

                    // Update admin
                    string updateQuery = "UPDATE AdminResto SET nama = @Nama, username = @Username, " + //
                                         "Passwords = @Password WHERE admin_id = @AdminID"; //

                    using (SqlCommand command = new SqlCommand(updateQuery, connection)) //
                    {
                        command.Parameters.AddWithValue("@AdminID", selectedAdminId); //
                        command.Parameters.AddWithValue("@Nama", txtNama.Text); //
                        command.Parameters.AddWithValue("@Username", txtUsername.Text); //
                        command.Parameters.AddWithValue("@Password", txtPassword.Text); //

                        int result = command.ExecuteNonQuery(); //

                        if (result > 0)
                        {
                            MessageBox.Show("Admin updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); //
                            ClearInputFields(); //
                            LoadAdminData(); //
                        }
                        else
                        {
                            MessageBox.Show("Failed to update admin.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); //
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating admin: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); //
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            // Validate selection
            if (selectedAdminId == 0) //
            {
                MessageBox.Show("Please select an admin to delete.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information); //
                return;
            }

            // Confirm deletion
            DialogResult confirmResult = MessageBox.Show("Are you sure you want to delete this admin?", //
                                                        "Confirm Delete", //
                                                        MessageBoxButtons.YesNo, MessageBoxIcon.Question); //
            if (confirmResult == DialogResult.No) //
            {
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString)) //
                {
                    connection.Open(); //

                    // Delete admin
                    string deleteQuery = "DELETE FROM AdminResto WHERE admin_id = @AdminID"; //

                    using (SqlCommand command = new SqlCommand(deleteQuery, connection)) //
                    {
                        command.Parameters.AddWithValue("@AdminID", selectedAdminId); //

                        int result = command.ExecuteNonQuery(); //

                        if (result > 0)
                        {
                            MessageBox.Show("Admin deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); //
                            ClearInputFields(); //
                            LoadAdminData(); //
                        }
                        else
                        {
                            MessageBox.Show("Failed to delete admin.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); //
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting admin: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); //
            }
        }
    }
}