using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Runtime.Caching;
using System.Linq;

namespace Project
{
    public partial class Meja : Form
    {
        // Menambahkan instance Koneksi
        private Koneksi kn = new Koneksi();
        private int selectedMejaId = 0;

        private readonly ObjectCache _cache = MemoryCache.Default;
        private readonly CacheItemPolicy _cachePolicy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(10)
        };
        private const string CacheKey = "MejaDataCache";

        public Meja()
        {
            InitializeComponent();
        } 

        private void Meja_Load(object sender, EventArgs e)
        {
            EnsureIndexes();
            LoadData();
            dgcMeja.CellClick += dgcMeja_CellClick;
        }

        private void dgcMeja_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgcMeja.Rows[e.RowIndex];
                selectedMejaId = Convert.ToInt32(row.Cells["meja_id"].Value);
                txtNomorMeja.Text = row.Cells["nomor_meja"].Value.ToString();
                txtKapasitas.Text = row.Cells["kapasitas"].Value.ToString().Replace(" orang", "").Trim();
                cmbStatusMeja.Text = row.Cells["status_meja"].Value.ToString();
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput(out int nomorMeja, out string kapasitasFormatted))
            {
                return;
            }

            // Menggunakan kn.connectionString()
            using (SqlConnection connection = new SqlConnection(kn.connectionString()))
            {
                SqlTransaction transaction = null;
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    SqlCommand command = new SqlCommand("AddMeja", connection, transaction);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@nomor_meja", nomorMeja);
                    command.Parameters.AddWithValue("@kapasitas", kapasitasFormatted);
                    command.Parameters.AddWithValue("@status_meja", cmbStatusMeja.Text);

                    command.ExecuteNonQuery();
                    transaction.Commit();

                    _cache.Remove(CacheKey);
                    MessageBox.Show("Data meja berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                    ClearFields();
                }
                catch (SqlException sqlEx) when (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                {
                    transaction?.Rollback();
                    MessageBox.Show("Nomor meja sudah ada. Silakan gunakan nomor lain.", "Error Duplikat", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    MessageBox.Show("Terjadi kesalahan saat menambah data. Perubahan dibatalkan.\nError: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedMejaId == 0)
            {
                MessageBox.Show("Pilih meja yang akan diupdate!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!ValidateInput(out int nomorMeja, out string kapasitasFormatted))
            {
                return;
            }

            // Menggunakan kn.connectionString()
            using (SqlConnection connection = new SqlConnection(kn.connectionString()))
            {
                SqlTransaction transaction = null;
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    SqlCommand command = new SqlCommand("UpdateMeja", connection, transaction);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@meja_id", selectedMejaId);
                    command.Parameters.AddWithValue("@nomor_meja", nomorMeja);
                    command.Parameters.AddWithValue("@kapasitas", kapasitasFormatted);
                    command.Parameters.AddWithValue("@status_meja", cmbStatusMeja.Text);

                    int result = command.ExecuteNonQuery();
                    if (result > 0)
                    {
                        transaction.Commit();
                        _cache.Remove(CacheKey);
                        MessageBox.Show("Data meja berhasil diupdate!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData();
                        ClearFields();
                    }
                    else
                    {
                        throw new Exception("Data tidak ditemukan atau gagal diperbarui.");
                    }
                }
                catch (SqlException sqlEx) when (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                {
                    transaction?.Rollback();
                    MessageBox.Show("Nomor meja sudah digunakan oleh meja lain.", "Error Duplikat", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    MessageBox.Show("Terjadi kesalahan saat memperbarui data. Perubahan dibatalkan.\nError: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedMejaId == 0)
            {
                MessageBox.Show("Pilih meja yang akan dihapus!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show("Anda yakin ingin menghapus meja ini?", "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                // Menggunakan kn.connectionString()
                using (SqlConnection connection = new SqlConnection(kn.connectionString()))
                {
                    SqlTransaction transaction = null;
                    try
                    {
                        connection.Open();
                        transaction = connection.BeginTransaction();

                        SqlCommand command = new SqlCommand("DeleteMeja", connection, transaction);
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@meja_id", selectedMejaId);

                        int result = command.ExecuteNonQuery();
                        if (result > 0)
                        {
                            transaction.Commit();
                            _cache.Remove(CacheKey);
                            MessageBox.Show("Data meja berhasil dihapus!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadData();
                            ClearFields();
                        }
                        else
                        {
                            throw new Exception("Data tidak ditemukan di database.");
                        }
                    }
                    catch (SqlException sqlEx) when (sqlEx.Number == 547)
                    {
                        transaction?.Rollback();
                        MessageBox.Show("Gagal menghapus. Meja ini mungkin sedang digunakan dalam data reservasi.", "Error Hapus", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        transaction?.Rollback();
                        MessageBox.Show("Terjadi kesalahan umum. Perubahan dibatalkan.\nError: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            _cache.Remove(CacheKey);
            LoadData();
            ClearFields();
        }

        private void EnsureIndexes()
        {
            // Menggunakan kn.connectionString()
            using (var conn = new SqlConnection(kn.connectionString()))
            {
                conn.Open();
                var indexScript = @"
                IF OBJECT_ID('dbo.Meja', 'U') IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_Meja_Status')
                        CREATE NONCLUSTERED INDEX idx_Meja_Status ON dbo.Meja(status_meja);
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
                DataTable mejaDataTable;
                if (_cache.Contains(CacheKey))
                {
                    mejaDataTable = _cache.Get(CacheKey) as DataTable;
                }
                else
                {
                    mejaDataTable = new DataTable();
                    // Menggunakan kn.connectionString()
                    using (var connection = new SqlConnection(kn.connectionString()))
                    {
                        var query = "SELECT meja_id, nomor_meja, kapasitas, status_meja FROM Meja";
                        using (var adapter = new SqlDataAdapter(query, connection))
                        {
                            adapter.Fill(mejaDataTable);
                        }
                    }
                    _cache.Add(CacheKey, mejaDataTable, _cachePolicy);
                }

                dgcMeja.DataSource = mejaDataTable;
                dgcMeja.Columns["meja_id"].Visible = false;
                dgcMeja.Columns["nomor_meja"].HeaderText = "Nomor Meja";
                dgcMeja.Columns["kapasitas"].HeaderText = "Kapasitas";
                dgcMeja.Columns["status_meja"].HeaderText = "Status Meja";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error memuat data meja: " + ex.Message, "Data Loading Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ClearFields()
        {
            txtNomorMeja.Clear();
            txtKapasitas.Clear();
            cmbStatusMeja.SelectedIndex = 0;
            selectedMejaId = 0;
            dgcMeja.ClearSelection();
            txtNomorMeja.Focus();
        }

        private bool ValidateInput(out int nomorMeja, out string kapasitasFormatted)
        {
            nomorMeja = 0;
            kapasitasFormatted = null;

            if (string.IsNullOrWhiteSpace(txtNomorMeja.Text) || !int.TryParse(txtNomorMeja.Text, out nomorMeja) || nomorMeja <= 0)
            {
                MessageBox.Show("Nomor meja harus diisi dengan angka positif!", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (string.IsNullOrWhiteSpace(txtKapasitas.Text) || !int.TryParse(txtKapasitas.Text, out int kapasitas) || kapasitas <= 0)
            {
                MessageBox.Show("Kapasitas harus diisi dengan angka positif!", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            kapasitasFormatted = FormatKapasitas(txtKapasitas.Text);
            return true;
        }

        private string FormatKapasitas(string input)
        {
            string digits = new string(input.Where(char.IsDigit).ToArray());
            string result = digits + " orang";
            return result.Length > 9 ? result.Substring(0, 9) : result;
        }
    }
}