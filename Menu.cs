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
    public partial class Menu : Form
    {
        // Database connection string
        private string connectionString = @"Data Source=MIHALY\FAIRUZ013;Initial Catalog=ReservasiRestoran;Integrated Security=True";
        private SqlConnection connection;
        private SqlCommand command;
        private SqlDataAdapter adapter;
        private DataTable dataTable;
        private int selectedMenuId = 0;

        public Menu()
        {
            InitializeComponent();

            // Set initial selection for kategori combobox
            cmbKategori.SelectedIndex = 0;

            // Add event handlers for buttons and grid
            dgcMenu.CellClick += new DataGridViewCellEventHandler(dgcMenu_CellClick);

            // Load data when form loads
            this.Load += new EventHandler(Menu_Load);
        }

        private void Menu_Load(object sender, EventArgs e)
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
                    string query = "SELECT menu_id, nama, deskripsi, harga, kategori FROM Menu";
                    command = new SqlCommand(query, connection);
                    adapter = new SqlDataAdapter(command);
                    dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    dgcMenu.DataSource = dataTable;

                    // Format column headers and hide menu_id column
                    dgcMenu.Columns["menu_id"].Visible = false;
                    dgcMenu.Columns["nama"].HeaderText = "Nama Menu";
                    dgcMenu.Columns["deskripsi"].HeaderText = "Deskripsi";
                    dgcMenu.Columns["harga"].HeaderText = "Harga";
                    dgcMenu.Columns["kategori"].HeaderText = "Kategori";

                    // Format the harga column as currency
                    dgcMenu.Columns["harga"].DefaultCellStyle.Format = "N2";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Add new menu record
        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(txtNama.Text) || string.IsNullOrWhiteSpace(txtHarga.Text))
                {
                    MessageBox.Show("Nama dan harga harus diisi!", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!decimal.TryParse(txtHarga.Text, out decimal harga) || harga <= 0)
                {
                    MessageBox.Show("Harga harus berupa angka positif!", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate nama length constraint
                if (txtNama.Text.Length > 20)
                {
                    MessageBox.Show("Nama menu maksimal 20 karakter!", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate deskripsi length constraint
                if (!string.IsNullOrEmpty(tctDeskripsi.Text) && tctDeskripsi.Text.Length > 50)
                {
                    MessageBox.Show("Deskripsi maksimal 50 karakter!", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Format kategori to match CHECK constraint (lowercase)
                string kategoriFormatted = cmbKategori.Text.ToLower();

                using (connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "INSERT INTO Menu (nama, deskripsi, harga, kategori) VALUES (@nama, @deskripsi, @harga, @kategori)";
                    command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@nama", txtNama.Text);
                    command.Parameters.AddWithValue("@deskripsi", string.IsNullOrWhiteSpace(tctDeskripsi.Text) ? DBNull.Value : (object)tctDeskripsi.Text);
                    command.Parameters.AddWithValue("@harga", harga);
                    command.Parameters.AddWithValue("@kategori", kategoriFormatted);
                    command.ExecuteNonQuery();

                    MessageBox.Show("Menu berhasil ditambahkan!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFields();
                    LoadData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding menu: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Update selected menu record
        private void btnUpdate_Click(object sender, EventArgs e)
        {
            try
            {
                if (selectedMenuId == 0)
                {
                    MessageBox.Show("Pilih menu yang akan diupdate!", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate input
                if (string.IsNullOrWhiteSpace(txtNama.Text) || string.IsNullOrWhiteSpace(txtHarga.Text))
                {
                    MessageBox.Show("Nama dan harga harus diisi!", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (!decimal.TryParse(txtHarga.Text, out decimal harga) || harga <= 0)
                {
                    MessageBox.Show("Harga harus berupa angka positif!", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate nama length constraint
                if (txtNama.Text.Length > 20)
                {
                    MessageBox.Show("Nama menu maksimal 20 karakter!", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validate deskripsi length constraint
                if (!string.IsNullOrEmpty(tctDeskripsi.Text) && tctDeskripsi.Text.Length > 50)
                {
                    MessageBox.Show("Deskripsi maksimal 50 karakter!", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Format kategori to match CHECK constraint (lowercase)
                string kategoriFormatted = cmbKategori.Text.ToLower();

                using (connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = "UPDATE Menu SET nama = @nama, deskripsi = @deskripsi, harga = @harga, kategori = @kategori WHERE menu_id = @menu_id";
                    command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@menu_id", selectedMenuId);
                    command.Parameters.AddWithValue("@nama", txtNama.Text);
                    command.Parameters.AddWithValue("@deskripsi", string.IsNullOrWhiteSpace(tctDeskripsi.Text) ? DBNull.Value : (object)tctDeskripsi.Text);
                    command.Parameters.AddWithValue("@harga", harga);
                    command.Parameters.AddWithValue("@kategori", kategoriFormatted);
                    command.ExecuteNonQuery();

                    MessageBox.Show("Menu berhasil diupdate!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearFields();
                    LoadData();
                    selectedMenuId = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating menu: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Delete selected menu record
        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (selectedMenuId == 0)
                {
                    MessageBox.Show("Pilih menu yang akan dihapus!", "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult result = MessageBox.Show("Anda yakin ingin menghapus menu ini?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    using (connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        string query = "DELETE FROM Menu WHERE menu_id = @menu_id";
                        command = new SqlCommand(query, connection);
                        command.Parameters.AddWithValue("@menu_id", selectedMenuId);
                        command.ExecuteNonQuery();

                        MessageBox.Show("Menu berhasil dihapus!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ClearFields();
                        LoadData();
                        selectedMenuId = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error deleting menu: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Refresh the data in DataGridView
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            ClearFields();
            LoadData();
            selectedMenuId = 0;
        }

        // Handle cell click in DataGridView to select a record
        private void dgcMenu_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgcMenu.Rows[e.RowIndex];
                    selectedMenuId = Convert.ToInt32(row.Cells["menu_id"].Value);
                    txtNama.Text = row.Cells["nama"].Value.ToString();

                    // Handle possible null value in deskripsi
                    if (row.Cells["deskripsi"].Value != DBNull.Value)
                    {
                        tctDeskripsi.Text = row.Cells["deskripsi"].Value.ToString();
                    }
                    else
                    {
                        tctDeskripsi.Text = string.Empty;
                    }

                    txtHarga.Text = row.Cells["harga"].Value.ToString();

                    // Set the kategori combobox based on the value (capitalize first letter for display)
                    string kategori = row.Cells["kategori"].Value.ToString().Trim();
                    if (!string.IsNullOrEmpty(kategori))
                    {
                        string kategoriDisplayText = char.ToUpper(kategori[0]) + kategori.Substring(1);
                        cmbKategori.Text = kategoriDisplayText;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error selecting menu: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Clear all input fields
        private void ClearFields()
        {
            txtNama.Clear();
            tctDeskripsi.Clear();
            txtHarga.Clear();
            cmbKategori.SelectedIndex = 0;
            txtNama.Focus();
        }
    }
}