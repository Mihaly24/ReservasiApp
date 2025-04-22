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
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            LoadData();
            dgvPelanggan.SelectionChanged += DgvPelanggan_SelectionChanged;
        }

        private void DgvPelanggan_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPelanggan.CurrentRow != null)
            {
                selectedPelangganId = Convert.ToInt32(dgvPelanggan.CurrentRow.Cells["ID"].Value);
                txtNama.Text = dgvPelanggan.CurrentRow.Cells["Nama"].Value.ToString();
                txtEmail.Text = dgvPelanggan.CurrentRow.Cells["Email"].Value == DBNull.Value ? "" : dgvPelanggan.CurrentRow.Cells["Email"].Value.ToString();
                txtNoTelp.Text = dgvPelanggan.CurrentRow.Cells["No Telepon"].Value == DBNull.Value ? "" : dgvPelanggan.CurrentRow.Cells["No Telepon"].Value.ToString();
                txtAlamat.Text = dgvPelanggan.CurrentRow.Cells["Alamat"].Value == DBNull.Value ? "" : dgvPelanggan.CurrentRow.Cells["Alamat"].Value.ToString();
            }
        }

        private void ClearForm()
        {
            txtNama.Clear();
            txtEmail.Clear();
            txtNoTelp.Clear();
            txtAlamat.Clear();
            selectedPelangganId = 0;
            txtNama.Focus();
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
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Terjadi kesalahan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private bool ValidateInput()
        {
            // Validate required fields
            if (string.IsNullOrWhiteSpace(txtNama.Text))
            {
                MessageBox.Show("Nama pelanggan harus diisi.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNama.Focus();
                return false;
            }

            // Validate length constraints
            if (txtNama.Text.Length > 30)
            {
                MessageBox.Show("Nama tidak boleh lebih dari 30 karakter.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNama.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtEmail.Text) && txtEmail.Text.Length > 20)
            {
                MessageBox.Show("Email tidak boleh lebih dari 20 karakter.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtEmail.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtNoTelp.Text) && txtNoTelp.Text.Length > 13)
            {
                MessageBox.Show("Nomor telepon tidak boleh lebih dari 13 karakter.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtNoTelp.Focus();
                return false;
            }

            if (!string.IsNullOrWhiteSpace(txtAlamat.Text) && txtAlamat.Text.Length > 50)
            {
                MessageBox.Show("Alamat tidak boleh lebih dari 50 karakter.", "Validasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtAlamat.Focus();
                return false;
            }

            return true;
        }

        private void BtnTambah_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

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
                            MessageBox.Show("Email sudah terdaftar. Harap gunakan email lain.", "Duplikasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            txtEmail.Focus();
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
                    MessageBox.Show("Terjadi kesalahan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            if (!ValidateInput()) return;

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
                            MessageBox.Show("Email sudah terdaftar. Harap gunakan email lain.", "Duplikasi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            txtEmail.Focus();
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
                        MessageBox.Show("Gagal memperbarui data pelanggan.", "Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Terjadi kesalahan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

            DialogResult result = MessageBox.Show("Apakah Anda yakin ingin menghapus pelanggan ini?", "Konfirmasi", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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
                        MessageBox.Show("Gagal menghapus data pelanggan.", "Gagal", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Terjadi kesalahan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
            ClearForm();
        }

        private void label1_Click(object sender, EventArgs e)
        {
            // Empty event handler
        }
    }
}