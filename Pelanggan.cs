using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Runtime.Caching; // Diperlukan untuk caching
using System.Text.RegularExpressions;

namespace Project
{
    public partial class Pelanggan : Form
    {
        // Membuat instance dari kelas Koneksi
        private Koneksi kn = new Koneksi();
        private int selectedPelangganId = 0;

        private readonly ObjectCache _cache = MemoryCache.Default;
        private readonly CacheItemPolicy _cachePolicy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(10)
        };
        private const string CacheKey = "PelangganDataCache";

        public Pelanggan()
        {
            InitializeComponent();
        }


        private void Pelanggan_Load(object sender, EventArgs e)
        {
            EnsureIndexes();
            LoadData();
            dgvPelanggan.SelectionChanged += DgvPelanggan_SelectionChanged;
        }

        private void DgvPelanggan_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvPelanggan.CurrentRow != null)
            {
                try
                {
                    selectedPelangganId = Convert.ToInt32(dgvPelanggan.CurrentRow.Cells["pelanggan_id"].Value);
                    txtNama.Text = dgvPelanggan.CurrentRow.Cells["nama"].Value.ToString();
                    txtEmail.Text = dgvPelanggan.CurrentRow.Cells["email"].Value.ToString();
                    txtNoTelp.Text = dgvPelanggan.CurrentRow.Cells["no_telp"].Value.ToString();
                    txtAlamat.Text = dgvPelanggan.CurrentRow.Cells["alamat"].Value.ToString();
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error selecting row: " + ex.Message);
                    ClearForm();
                }
            }
        }

        private void AnalyzeQuery(string query)
        {
            // Menggunakan kn.connectionString() untuk koneksi
            using (var conn = new SqlConnection(kn.connectionString()))
            {
                conn.InfoMessage += (s, e) => MessageBox.Show(e.Message, "STATISTIC INFO");
                conn.Open();
                var wrapped = $@"
                SET STATISTICS IO ON;
                SET STATISTICS TIME ON;
                {query};
                SET STATISTICS IO OFF;
                SET STATISTICS TIME OFF;";
                using (var cmd = new SqlCommand(wrapped, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void BtnTambah_Click(object sender, EventArgs e)
        {
            if (!ValidatePelangganInput(out string validationMessage))
            {
                MessageBox.Show(validationMessage, "Validasi Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Menggunakan kn.connectionString() untuk koneksi
            using (SqlConnection connection = new SqlConnection(kn.connectionString()))
            {
                SqlTransaction transaction = null;
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    // Panggil Stored Procedure AddPelanggan
                    SqlCommand command = new SqlCommand("AddPelanggan", connection, transaction);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@nama", txtNama.Text.Trim());
                    command.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                    command.Parameters.AddWithValue("@no_telp", txtNoTelp.Text.Trim());
                    command.Parameters.AddWithValue("@alamat", txtAlamat.Text.Trim());

                    command.ExecuteNonQuery();
                    transaction.Commit();

                    _cache.Remove(CacheKey);
                    MessageBox.Show("Data pelanggan berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                    ClearForm();
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    MessageBox.Show("Terjadi kesalahan saat menambah data. Perubahan dibatalkan.\nError: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                return;
            }

            // Menggunakan kn.connectionString() untuk koneksi
            using (SqlConnection connection = new SqlConnection(kn.connectionString()))
            {
                SqlTransaction transaction = null;
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    SqlCommand command = new SqlCommand("UpdatePelanggan", connection, transaction);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@pelanggan_id", selectedPelangganId);
                    command.Parameters.AddWithValue("@nama", txtNama.Text.Trim());
                    command.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                    command.Parameters.AddWithValue("@no_telp", txtNoTelp.Text.Trim());
                    command.Parameters.AddWithValue("@alamat", txtAlamat.Text.Trim());

                    int result = command.ExecuteNonQuery();
                    if (result > 0)
                    {
                        transaction.Commit();
                        _cache.Remove(CacheKey);
                        MessageBox.Show("Data pelanggan berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData();
                        ClearForm();
                    }
                    else
                    {
                        throw new Exception("Data tidak ditemukan atau gagal diperbarui.");
                    }
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    MessageBox.Show("Terjadi kesalahan saat memperbarui data. Perubahan dibatalkan.\nError: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            if (result == DialogResult.Yes)
            {
                // Menggunakan kn.connectionString() untuk koneksi
                using (SqlConnection connection = new SqlConnection(kn.connectionString()))
                {
                    SqlTransaction transaction = null;
                    try
                    {
                        connection.Open();
                        transaction = connection.BeginTransaction();

                        // Panggil Stored Procedure DeletePelanggan
                        SqlCommand command = new SqlCommand("DeletePelanggan", connection, transaction);
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@pelanggan_id", selectedPelangganId);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            transaction.Commit();
                            _cache.Remove(CacheKey);
                            MessageBox.Show("Data pelanggan berhasil dihapus!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadData();
                            ClearForm();
                        }
                        else
                        {
                            throw new Exception("Data tidak ditemukan di database.");
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        transaction?.Rollback();
                        if (sqlEx.Number == 547)
                        {
                            MessageBox.Show("Gagal menghapus. Pelanggan ini memiliki data terkait (reservasi) dan tidak dapat dihapus.", "Error Hapus", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            MessageBox.Show("Terjadi kesalahan database. Perubahan dibatalkan.\nError: " + sqlEx.Message, "Error Database", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction?.Rollback();
                        MessageBox.Show("Terjadi kesalahan umum. Perubahan dibatalkan.\nError: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            _cache.Remove(CacheKey);
            LoadData();
            ClearForm();
        }


        private void EnsureIndexes()
        {
            // Menggunakan kn.connectionString() untuk koneksi
            using (var conn = new SqlConnection(kn.connectionString()))
            {
                conn.Open();
                var indexScript = @"
                IF OBJECT_ID('dbo.Pelanggan', 'U') IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_Pelanggan_Nama')
                        CREATE NONCLUSTERED INDEX idx_Pelanggan_Nama ON dbo.Pelanggan(nama);
                END";
                using (var cmd = new SqlCommand(indexScript, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void LoadData()
        {
            try
            {
                DataTable pelangganDataTable;
                if (_cache.Contains(CacheKey))
                {
                    pelangganDataTable = _cache.Get(CacheKey) as DataTable;
                }
                else
                {
                    pelangganDataTable = new DataTable();
                    // Menggunakan kn.connectionString() untuk koneksi
                    using (var connection = new SqlConnection(kn.connectionString()))
                    {
                        connection.Open();
                        var query = "SELECT pelanggan_id, nama, email, no_telp, alamat FROM dbo.Pelanggan";
                        using (var adapter = new SqlDataAdapter(query, connection))
                        {
                            adapter.Fill(pelangganDataTable);
                        }
                    }
                    _cache.Add(CacheKey, pelangganDataTable, _cachePolicy);
                }
                dgvPelanggan.DataSource = pelangganDataTable;
                if (dgvPelanggan.Columns["pelanggan_id"] != null) dgvPelanggan.Columns["pelanggan_id"].HeaderText = "ID";
                if (dgvPelanggan.Columns["nama"] != null) dgvPelanggan.Columns["nama"].HeaderText = "Nama";
                if (dgvPelanggan.Columns["email"] != null) dgvPelanggan.Columns["email"].HeaderText = "Email";
                if (dgvPelanggan.Columns["no_telp"] != null) dgvPelanggan.Columns["no_telp"].HeaderText = "No Telepon";
                if (dgvPelanggan.Columns["alamat"] != null) dgvPelanggan.Columns["alamat"].HeaderText = "Alamat";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error memuat data pelanggan: " + ex.Message, "Data Loading Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAnalyze_Click(object sender, EventArgs e)
        {
            var heavyQuery = "SELECT nama, email, no_telp FROM dbo.Pelanggan WHERE nama LIKE 'A%'";
            AnalyzeQuery(heavyQuery);
        }

        private void BtnReport_Click(object sender, EventArgs e)
        {
            ReportPelanggan pelangganReportForm = new ReportPelanggan();
            pelangganReportForm.Show();
        }


        private void ClearForm()
        {
            txtNama.Clear();
            txtEmail.Clear();
            txtNoTelp.Clear();
            txtAlamat.Clear();
            selectedPelangganId = 0;
            dgvPelanggan.ClearSelection();
        }

        private bool ValidatePelangganInput(out string errorMessage)
        {
            errorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(txtNama.Text))
            {
                errorMessage = "Nama pelanggan harus diisi.";
                return false;
            }
            if (!Regex.IsMatch(txtNama.Text, @"^[a-zA-Z\s]+$"))
            {
                errorMessage = "Nama pelanggan hanya boleh berisi huruf dan spasi.";
                return false;
            }
            if (!string.IsNullOrWhiteSpace(txtEmail.Text) && !Regex.IsMatch(txtEmail.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                errorMessage = "Format email tidak valid.";
                return false;
            }
            if (!string.IsNullOrWhiteSpace(txtNoTelp.Text) && !Regex.IsMatch(txtNoTelp.Text, @"^\d{10,13}$"))
            {
                errorMessage = "Nomor telepon harus terdiri dari 10 hingga 13 digit angka.";
                return false;
            }
            return true;
        }
    }
}