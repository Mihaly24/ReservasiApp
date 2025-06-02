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
    public partial class Pelanggan : Form
    {
        private string connectionString = "Data Source=MIHALY\\FAIRUZ013;Initial Catalog=ReservasiRestoran;Integrated Security=True";
        private int selectedPelangganId = 0;

        public Pelanggan()
        {
            InitializeComponent();

            // Set DataGridView to select full row
            dgvPelanggan.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPelanggan.MultiSelect = false;
            dgvPelanggan.ReadOnly = true;
            dgvPelanggan.AllowUserToAddRows = false;
            dgvPelanggan.AllowUserToDeleteRows = false;
            dgvPelanggan.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

            // Add the event handler here instead of in Form1_Load
            dgvPelanggan.SelectionChanged += DgvPelanggan_SelectionChanged;
        }

        // Make sure you have a Load event for the form, e.g., in the designer or manually add:
        // this.Load += new System.EventHandler(this.Pelanggan_Load);
        // And then rename Form1_Load to Pelanggan_Load or whatever your form load event is named.
        private void Form1_Load(object sender, EventArgs e) // Or Pelanggan_Load
        {
            LoadData();
        }

        private void DgvPelanggan_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPelanggan.CurrentRow != null && dgvPelanggan.CurrentRow.Cells["ID"].Value != null) // Check for null cell value
            {
                try
                {
                    selectedPelangganId = Convert.ToInt32(dgvPelanggan.CurrentRow.Cells["ID"].Value);
                    txtNama.Text = dgvPelanggan.CurrentRow.Cells["Nama"].Value.ToString();
                    txtEmail.Text = dgvPelanggan.CurrentRow.Cells["Email"].Value == DBNull.Value ? "" : dgvPelanggan.CurrentRow.Cells["Email"].Value.ToString();
                    txtNoTelp.Text = dgvPelanggan.CurrentRow.Cells["No Telepon"].Value == DBNull.Value ? "" : dgvPelanggan.CurrentRow.Cells["No Telepon"].Value.ToString();
                    txtAlamat.Text = dgvPelanggan.CurrentRow.Cells["Alamat"].Value == DBNull.Value ? "" : dgvPelanggan.CurrentRow.Cells["Alamat"].Value.ToString();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error selecting row: " + ex.Message, "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    ClearForm(); // Clear form if selection causes an error
                }
            }
        }

        private void ClearForm()
        {
            txtNama.Clear();
            txtEmail.Clear();
            txtNoTelp.Clear();
            txtAlamat.Clear();
            selectedPelangganId = 0;
            // txtNama.Focus(); // Consider if focus is needed here or after specific actions
        }

        private void LoadData()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT pelanggan_id AS [ID], nama AS [Nama], email AS [Email], no_telp AS [No Telepon], alamat AS [Alamat] FROM Pelanggan";
                    SqlDataAdapter da = new SqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    da.Fill(dt);

                    dgvPelanggan.DataSource = dt;

                    if (dgvPelanggan.Rows.Count > 0)
                    {
                        dgvPelanggan.ClearSelection();
                        // dgvPelanggan.Rows[0].Selected = true; // This might trigger SelectionChanged before form is fully ready.
                        // It's often better to let the user make the first selection or handle initial population carefully.
                    }
                    else
                    {
                        ClearForm(); // Clear form if no data
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Terjadi kesalahan saat memuat data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private bool ValidatePelangganInput(out string errorMessage)
        {
            errorMessage = string.Empty;

            // Validate Nama
            if (string.IsNullOrWhiteSpace(txtNama.Text))
            {
                errorMessage = "Nama pelanggan harus diisi.";
                // txtNama.Focus(); // Focus can be handled by the calling method if needed
                return false;
            }
            if (!Regex.IsMatch(txtNama.Text, @"^[a-zA-Z\s]+$"))
            {
                errorMessage = "Nama pelanggan hanya boleh berisi huruf dan spasi.";
                return false;
            }
            if (txtNama.Text.Length > 30)
            {
                errorMessage = "Nama tidak boleh lebih dari 30 karakter.";
                return false;
            }

            // Validate Email (Optional field)
            if (!string.IsNullOrWhiteSpace(txtEmail.Text))
            {
                if (txtEmail.Text.Length > 50) // Increased max length for email
                {
                    errorMessage = "Email tidak boleh lebih dari 50 karakter.";
                    return false;
                }
                // Basic email format validation
                if (!Regex.IsMatch(txtEmail.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    errorMessage = "Format email tidak valid.";
                    return false;
                }
            }

            // Validate No Telepon (Optional field)
            if (!string.IsNullOrWhiteSpace(txtNoTelp.Text))
            {
                if (!Regex.IsMatch(txtNoTelp.Text, @"^\d+$"))
                {
                    errorMessage = "Nomor telepon hanya boleh berisi angka.";
                    return false;
                }
                if (txtNoTelp.Text.Length < 10 || txtNoTelp.Text.Length > 13) // Adjusted min length
                {
                    errorMessage = "Nomor telepon harus terdiri dari 10 hingga 13 digit.";
                    return false;
                }
            }

            // Validate Alamat (Optional field)
            if (!string.IsNullOrWhiteSpace(txtAlamat.Text) && txtAlamat.Text.Length > 100) // Increased max length for alamat
            {
                errorMessage = "Alamat tidak boleh lebih dari 100 karakter.";
                return false;
            }

            return true;
        }

        private void BtnTambah_Click(object sender, EventArgs e)
        {
            if (!ValidatePelangganInput(out string validationMessage))
            {
                MessageBox.Show(validationMessage, "Validasi Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                // Find the control to focus based on the error message, or a general one like txtNama
                if (validationMessage.ToLower().Contains("nama")) txtNama.Focus();
                else if (validationMessage.ToLower().Contains("email")) txtEmail.Focus();
                else if (validationMessage.ToLower().Contains("telepon")) txtNoTelp.Focus();
                else if (validationMessage.ToLower().Contains("alamat")) txtAlamat.Focus();
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Check if email already exists (if provided)
                    if (!string.IsNullOrWhiteSpace(txtEmail.Text))
                    {
                        string checkQuery = "SELECT COUNT(*) FROM Pelanggan WHERE email = @Email";
                        SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                        checkCmd.Parameters.AddWithValue("@Email", txtEmail.Text);

                        int count = (int)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            MessageBox.Show("Email sudah terdaftar. Harap gunakan email lain.", "Duplikasi Email", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            txtEmail.Focus();
                            return;
                        }
                    }
                    // Check if No Telepon already exists (if provided)
                    if (!string.IsNullOrWhiteSpace(txtNoTelp.Text))
                    {
                        string checkNoTelpQuery = "SELECT COUNT(*) FROM Pelanggan WHERE no_telp = @NoTelp";
                        SqlCommand checkNoTelpCmd = new SqlCommand(checkNoTelpQuery, conn);
                        checkNoTelpCmd.Parameters.AddWithValue("@NoTelp", txtNoTelp.Text);

                        int countNoTelp = (int)checkNoTelpCmd.ExecuteScalar();
                        if (countNoTelp > 0)
                        {
                            MessageBox.Show("Nomor telepon sudah terdaftar. Harap gunakan nomor lain.", "Duplikasi Nomor Telepon", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            txtNoTelp.Focus();
                            return;
                        }
                    }


                    string query = @"INSERT INTO Pelanggan (nama, email, no_telp, alamat) 
                                     VALUES (@Nama, @Email, @NoTelp, @Alamat)";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@Nama", txtNama.Text);
                    cmd.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(txtEmail.Text) ? DBNull.Value : (object)txtEmail.Text);
                    cmd.Parameters.AddWithValue("@NoTelp", string.IsNullOrWhiteSpace(txtNoTelp.Text) ? DBNull.Value : (object)txtNoTelp.Text);
                    cmd.Parameters.AddWithValue("@Alamat", string.IsNullOrWhiteSpace(txtAlamat.Text) ? DBNull.Value : (object)txtAlamat.Text);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Data pelanggan berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData();
                        ClearForm();
                    }
                    else
                    {
                        MessageBox.Show("Gagal menambahkan data pelanggan.", "Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Terjadi kesalahan saat menambah data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedPelangganId == 0)
            {
                MessageBox.Show("Silakan pilih pelanggan yang akan diperbarui.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (!ValidatePelangganInput(out string validationMessage))
            {
                MessageBox.Show(validationMessage, "Validasi Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                if (validationMessage.ToLower().Contains("nama")) txtNama.Focus();
                else if (validationMessage.ToLower().Contains("email")) txtEmail.Focus();
                else if (validationMessage.ToLower().Contains("telepon")) txtNoTelp.Focus();
                else if (validationMessage.ToLower().Contains("alamat")) txtAlamat.Focus();
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Check if email already exists for other customers (if provided)
                    if (!string.IsNullOrWhiteSpace(txtEmail.Text))
                    {
                        string checkQuery = "SELECT COUNT(*) FROM Pelanggan WHERE email = @Email AND pelanggan_id != @PelangganId";
                        SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                        checkCmd.Parameters.AddWithValue("@Email", txtEmail.Text);
                        checkCmd.Parameters.AddWithValue("@PelangganId", selectedPelangganId);

                        int count = (int)checkCmd.ExecuteScalar();
                        if (count > 0)
                        {
                            MessageBox.Show("Email sudah terdaftar untuk pelanggan lain. Harap gunakan email lain.", "Duplikasi Email", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            txtEmail.Focus();
                            return;
                        }
                    }
                    // Check if No Telepon already exists for other customers (if provided)
                    if (!string.IsNullOrWhiteSpace(txtNoTelp.Text))
                    {
                        string checkNoTelpQuery = "SELECT COUNT(*) FROM Pelanggan WHERE no_telp = @NoTelp AND pelanggan_id != @PelangganId";
                        SqlCommand checkNoTelpCmd = new SqlCommand(checkNoTelpQuery, conn);
                        checkNoTelpCmd.Parameters.AddWithValue("@NoTelp", txtNoTelp.Text);
                        checkNoTelpCmd.Parameters.AddWithValue("@PelangganId", selectedPelangganId);

                        int countNoTelp = (int)checkNoTelpCmd.ExecuteScalar();
                        if (countNoTelp > 0)
                        {
                            MessageBox.Show("Nomor telepon sudah terdaftar untuk pelanggan lain. Harap gunakan nomor lain.", "Duplikasi Nomor Telepon", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            txtNoTelp.Focus();
                            return;
                        }
                    }

                    string query = @"UPDATE Pelanggan 
                                     SET nama = @Nama, 
                                         email = @Email, 
                                         no_telp = @NoTelp, 
                                         alamat = @Alamat 
                                     WHERE pelanggan_id = @PelangganId";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@PelangganId", selectedPelangganId);
                    cmd.Parameters.AddWithValue("@Nama", txtNama.Text);
                    cmd.Parameters.AddWithValue("@Email", string.IsNullOrWhiteSpace(txtEmail.Text) ? DBNull.Value : (object)txtEmail.Text);
                    cmd.Parameters.AddWithValue("@NoTelp", string.IsNullOrWhiteSpace(txtNoTelp.Text) ? DBNull.Value : (object)txtNoTelp.Text);
                    cmd.Parameters.AddWithValue("@Alamat", string.IsNullOrWhiteSpace(txtAlamat.Text) ? DBNull.Value : (object)txtAlamat.Text);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Data pelanggan berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData();
                        ClearForm();
                    }
                    else
                    {
                        MessageBox.Show("Gagal memperbarui data pelanggan. Pelanggan mungkin tidak ditemukan.", "Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Terjadi kesalahan saat memperbarui data: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnHapus_Click(object sender, EventArgs e)
        {
            if (selectedPelangganId == 0)
            {
                MessageBox.Show("Silakan pilih pelanggan yang akan dihapus.", "Informasi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DialogResult result = MessageBox.Show("Apakah Anda yakin ingin menghapus pelanggan ini?", "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result != DialogResult.Yes) return;

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    string query = "DELETE FROM Pelanggan WHERE pelanggan_id = @PelangganId";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@PelangganId", selectedPelangganId);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        MessageBox.Show("Data pelanggan berhasil dihapus!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData();
                        ClearForm();
                    }
                    else
                    {
                        MessageBox.Show("Gagal menghapus data pelanggan. Pelanggan mungkin sudah dihapus.", "Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (SqlException sqlEx) // Catch specific SQL exceptions for foreign key constraints
                {
                    if (sqlEx.Number == 547) // Check for foreign key violation error number
                    {
                        MessageBox.Show("Gagal menghapus pelanggan. Pelanggan ini memiliki data terkait (misalnya reservasi) dan tidak dapat dihapus.", "Error Hapus", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        MessageBox.Show("Terjadi kesalahan database saat menghapus: " + sqlEx.Message, "Error Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Terjadi kesalahan umum saat menghapus: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
            ClearForm();
        }

        // This event handler seems unused, you can remove it if txtNama_TextChanged is not linked to any event.
        // Or if it was for label1_Click as in the original code, ensure it's named correctly.
        private void label1_Click(object sender, EventArgs e)
        {
            // Empty event handler
        }
    }
}