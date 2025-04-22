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

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNama.Text) || string.IsNullOrWhiteSpace(txtPosisi.Text) ||
                string.IsNullOrWhiteSpace(txtUsername.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Please fill all required fields.");
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
                        MessageBox.Show("Username already exists.");
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
                        MessageBox.Show("Staff added successfully!");
                        ClearInputs();
                        LoadStaffData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding staff: " + ex.Message);
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedStaffId == 0)
            {
                MessageBox.Show("Select a staff to update.");
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
                        MessageBox.Show("Username already exists.");
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
                        MessageBox.Show("Staff updated successfully!");
                        ClearInputs();
                        LoadStaffData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating staff: " + ex.Message);
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedStaffId == 0)
            {
                MessageBox.Show("Select a staff to delete.");
                return;
            }

            DialogResult confirm = MessageBox.Show("Are you sure?", "Confirm Delete", MessageBoxButtons.YesNo);
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
                        MessageBox.Show("Staff deleted successfully!");
                        ClearInputs();
                        LoadStaffData();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting staff: " + ex.Message);
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadStaffData();
            ClearInputs();
        }
    }
}

