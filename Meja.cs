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
    public partial class Meja : Form
    {
        // Database connection string
        private string connectionString = @"Data Source=MIHALY\FAIRUZ013;Initial Catalog=ReservasiRestoran;Integrated Security=True";
        private SqlConnection connection;
        private SqlCommand command;
        private SqlDataAdapter adapter;
        private DataTable dataTable;
        private int selectedMejaId = 0;

        public Meja()
        {
            InitializeComponent();

            // Set initial selection for status combobox
            cmbStatusMeja.SelectedIndex = 0;

            // Add event handlers for buttons and grid
            dgcMeja.CellClick += new DataGridViewCellEventHandler(dgcMeja_CellClick);
        }

        private void Meja_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        // Load data from database to DataGridView
        private void LoadData()
        {
            try
            {
                using (connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT meja_id, nomor_meja, kapasitas, status_meja FROM Meja";
                    command = new SqlCommand(query, connection);
                    adapter = new SqlDataAdapter(command);
                    dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    dgcMeja.DataSource = dataTable;

                    // Optional: Format column headers and hide meja_id column
                    dgcMeja.Columns["meja_id"].Visible = false;
                    dgcMeja.Columns["nomor_meja"].HeaderText = "Nomor Meja";
                    dgcMeja.Columns["kapasitas"].HeaderText = "Kapasitas";
                    dgcMeja.Columns["status_meja"].HeaderText = "Status Meja";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Add new table record
        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(txtNomorMeja.Text) || string.IsNullOrWhiteSpace(txtKapasitas.Text))
                {
                    MessageBox.Show("Nomor meja dan kapasitas harus diisi!", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(txtNomorMeja.Text, out int nomorMeja))
                {
                    MessageBox.Show("Nomor meja harus berupa angka!", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Format kapasitas to match the CHECK constraint
                string kapasitasFormatted = FormatKapasitas(txtKapasitas.Text);

                using (connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO Meja (nomor_meja, kapasitas, status_meja) VALUES (@nomor_meja, @kapasitas, @status_meja)";
                    command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@nomor_meja", nomorMeja);
                    command.Parameters.AddWithValue("@kapasitas", kapasitasFormatted);
                    command.Parameters.AddWithValue("@status_meja", cmbStatusMeja.Text);
                    command.ExecuteNonQuery();

                    MessageBox.Show("Data berhasil ditambahkan!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFields();
                    LoadData();
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601) // Unique constraint error
                {
                    MessageBox.Show("Nomor meja sudah digunakan!", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Error adding data: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Update selected table record
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (selectedMejaId == 0)
                {
                    MessageBox.Show("Pilih data yang akan diupdate!", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate input
                if (string.IsNullOrWhiteSpace(txtNomorMeja.Text) || string.IsNullOrWhiteSpace(txtKapasitas.Text))
                {
                    MessageBox.Show("Nomor meja dan kapasitas harus diisi!", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!int.TryParse(txtNomorMeja.Text, out int nomorMeja))
                {
                    MessageBox.Show("Nomor meja harus berupa angka!", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Format kapasitas to match the CHECK constraint
                string kapasitasFormatted = FormatKapasitas(txtKapasitas.Text);

                using (connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE Meja SET nomor_meja = @nomor_meja, kapasitas = @kapasitas, status_meja = @status_meja WHERE meja_id = @meja_id";
                    command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@meja_id", selectedMejaId);
                    command.Parameters.AddWithValue("@nomor_meja", nomorMeja);
                    command.Parameters.AddWithValue("@kapasitas", kapasitasFormatted);
                    command.Parameters.AddWithValue("@status_meja", cmbStatusMeja.Text);
                    command.ExecuteNonQuery();

                    MessageBox.Show("Data berhasil diupdate!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFields();
                    LoadData();
                    selectedMejaId = 0;
                }
            }
            catch (SqlException ex)
            {
                if (ex.Number == 2627 || ex.Number == 2601) // Unique constraint error
                {
                    MessageBox.Show("Nomor meja sudah digunakan!", "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show("Error updating data: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Delete selected table record
        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (selectedMejaId == 0)
                {
                    MessageBox.Show("Pilih data yang akan dihapus!", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult result = MessageBox.Show("Anda yakin ingin menghapus data ini?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    using (connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string query = "DELETE FROM Meja WHERE meja_id = @meja_id";
                        command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@meja_id", selectedMejaId);
                        command.ExecuteNonQuery();

                        MessageBox.Show("Data berhasil dihapus!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearFields();
                        LoadData();
                        selectedMejaId = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting data: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Refresh the data in DataGridView
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            ClearFields();
            LoadData();
            selectedMejaId = 0;
        }

        // Handle cell click in DataGridView to select a record
        private void dgcMeja_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgcMeja.Rows[e.RowIndex];
                    selectedMejaId = Convert.ToInt32(row.Cells["meja_id"].Value);
                    txtNomorMeja.Text = row.Cells["nomor_meja"].Value.ToString();

                    // Remove " orang" text from kapasitas for the textbox
                    string kapasitas = row.Cells["kapasitas"].Value.ToString().Trim();
                    txtKapasitas.Text = kapasitas.Replace(" orang", "");

                    cmbStatusMeja.Text = row.Cells["status_meja"].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Clear all input fields
        private void ClearFields()
        {
            txtNomorMeja.Clear();
            txtKapasitas.Clear();
            cmbStatusMeja.SelectedIndex = 0;
            txtNomorMeja.Focus();
        }

        // Format kapasitas to match CHECK constraint (X orang)
        private string FormatKapasitas(string input)
        {
            // Remove any non-digit characters
            string digits = new string(input.Where(char.IsDigit).ToArray());

            // Format as "X orang"
            string result = digits + " orang";

            // Ensure it fits the CHAR(9) constraint
            if (result.Length > 9)
            {
                result = result.Substring(0, 9);
            }

            return result;
        }

        private void button1_Click(object sender, EventArgs e)
        {

        }
    }
}