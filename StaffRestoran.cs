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
    public partial class StaffRestoran : Form
    {
        private string connectionString = "Data Source=MIHALY\\FAIRUZ013;Initial Catalog=ReservasiRestoran;Integrated Security=True";
        private int selectedStaffId = 0;

        public StaffRestoran()
        {
            InitializeComponent();
        }

        private void StaffRestoran_Load(object sender, EventArgs e)
        {
            LoadStaffData();
            dgvStaff.SelectionChanged += DgvStaff_SelectionChanged;
        }

        private void LoadStaffData()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "SELECT * FROM Staff_Restoran";
                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dgvStaff.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading staff data: " + ex.Message);
            }
        }

        private void ClearInputs()
        {
            txtNama.Text = "";
            txtPosisi.Text = "";
            txtUsername.Text = "";
            txtPassword.Text = "";
            txtNoTelp.Text = "";
            selectedStaffId = 0;
        }

        private void DgvStaff_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvStaff.CurrentRow != null)
            {
                selectedStaffId = Convert.ToInt32(dgvStaff.CurrentRow.Cells["staff_id"].Value);
                txtNama.Text = dgvStaff.CurrentRow.Cells["nama"].Value.ToString();
                txtPosisi.Text = dgvStaff.CurrentRow.Cells["posisi"].Value.ToString();
                txtUsername.Text = dgvStaff.CurrentRow.Cells["username"].Value.ToString();
                txtPassword.Text = dgvStaff.CurrentRow.Cells["passwords"].Value.ToString();
                txtNoTelp.Text = dgvStaff.CurrentRow.Cells["no_telp"].Value.ToString();
            }
        }

        // Helper method for input validation
        private bool ValidateInput(out string errorMessage)
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

            // Validate Posisi (assuming it's required but no specific format)
            if (string.IsNullOrWhiteSpace(txtPosisi.Text))
            {
                errorMessage = "Posisi field cannot be empty.";
                return false;
            }

            // Validate Username
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                errorMessage = "Username field cannot be empty.";
                return false;
            }

            // Validate Password
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                errorMessage = "Password field cannot be empty.";
                return false;
            }

            // Validate NoTelp
            if (string.IsNullOrWhiteSpace(txtNoTelp.Text))
            {
                errorMessage = "No Telp field cannot be empty.";
                return false;
            }
            if (!Regex.IsMatch(txtNoTelp.Text, @"^\d+$"))
            {
                errorMessage = "No Telp can only contain numbers.";
                return false;
            }
            if (txtNoTelp.Text.Length < 11 || txtNoTelp.Text.Length > 13)
            {
                errorMessage = "No Telp must be between 11 and 13 digits.";
                return false;
            }

            return true;
        }


        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput(out string validationMessage))
            {
                MessageBox.Show(validationMessage, "Input Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check username uniqueness
                    string checkQuery = "SELECT COUNT(*) FROM Staff_Restoran WHERE username = @Username";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@Username", txtUsername.Text);
                    int count = (int)checkCmd.ExecuteScalar();
                    if (count > 0)
                    {
                        MessageBox.Show("Username already exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Insert new staff
                    string insertQuery = @"INSERT INTO Staff_Restoran (nama, posisi, username, passwords, no_telp) 
                                         VALUES (@Nama, @Posisi, @Username, @Password, @NoTelp)";
                    SqlCommand cmd = new SqlCommand(insertQuery, connection);
                    cmd.Parameters.AddWithValue("@Nama", txtNama.Text);
                    cmd.Parameters.AddWithValue("@Posisi", txtPosisi.Text);
                    cmd.Parameters.AddWithValue("@Username", txtUsername.Text);
                    cmd.Parameters.AddWithValue("@Password", txtPassword.Text);
                    cmd.Parameters.AddWithValue("@NoTelp", txtNoTelp.Text);

                    int result = cmd.ExecuteNonQuery();
                    if (result > 0)
                    {
                        MessageBox.Show("Staff added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearInputs();
                        LoadStaffData();
                    }
                    else
                    {
                        MessageBox.Show("Failed to add staff.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding staff: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedStaffId == 0)
            {
                MessageBox.Show("Select a staff to update.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!ValidateInput(out string validationMessage))
            {
                MessageBox.Show(validationMessage, "Input Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    // Check username uniqueness (excluding current staff)
                    string checkQuery = "SELECT COUNT(*) FROM Staff_Restoran WHERE username = @Username AND staff_id != @StaffId";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, connection);
                    checkCmd.Parameters.AddWithValue("@Username", txtUsername.Text);
                    checkCmd.Parameters.AddWithValue("@StaffId", selectedStaffId);
                    int count = (int)checkCmd.ExecuteScalar();
                    if (count > 0)
                    {
                        MessageBox.Show("Username already exists for another staff member.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    // Update staff
                    string updateQuery = @"UPDATE Staff_Restoran 
                                         SET nama = @Nama, posisi = @Posisi, username = @Username, 
                                             passwords = @Password, no_telp = @NoTelp 
                                         WHERE staff_id = @StaffId";
                    SqlCommand cmd = new SqlCommand(updateQuery, connection);
                    cmd.Parameters.AddWithValue("@StaffId", selectedStaffId);
                    cmd.Parameters.AddWithValue("@Nama", txtNama.Text);
                    cmd.Parameters.AddWithValue("@Posisi", txtPosisi.Text);
                    cmd.Parameters.AddWithValue("@Username", txtUsername.Text);
                    cmd.Parameters.AddWithValue("@Password", txtPassword.Text);
                    cmd.Parameters.AddWithValue("@NoTelp", txtNoTelp.Text);

                    int result = cmd.ExecuteNonQuery();
                    if (result > 0)
                    {
                        MessageBox.Show("Staff updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearInputs();
                        LoadStaffData();
                    }
                    else
                    {
                        MessageBox.Show("Failed to update staff. Make sure the selected staff exists.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating staff: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedStaffId == 0)
            {
                MessageBox.Show("Select a staff to delete.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult confirm = MessageBox.Show("Are you sure you want to delete this staff member?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.No) return;

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string deleteQuery = "DELETE FROM Staff_Restoran WHERE staff_id = @StaffId";
                    SqlCommand cmd = new SqlCommand(deleteQuery, connection);
                    cmd.Parameters.AddWithValue("@StaffId", selectedStaffId);
                    int result = cmd.ExecuteNonQuery();
                    if (result > 0)
                    {
                        MessageBox.Show("Staff deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearInputs();
                        LoadStaffData();
                    }
                    else
                    {
                        MessageBox.Show("Failed to delete staff. The staff member may have already been deleted.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting staff: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadStaffData();
            ClearInputs();
        }
    }
}