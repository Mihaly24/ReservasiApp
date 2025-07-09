using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Runtime.Caching;

namespace Project
{
    public partial class Reservasi : Form
    {
        // Menambahkan instance Koneksi
        private Koneksi kn = new Koneksi();
        private int selectedReservasiId = 0;

        private readonly ObjectCache _cache = MemoryCache.Default;
        private readonly CacheItemPolicy _cachePolicy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5)
        };
        private const string ReservasiCacheKey = "ReservasiDataCache";
        private const string PelangganCacheKey = "PelangganForReservasiCache";
        private const string MejaCacheKey = "MejaForReservasiCache";

        public Reservasi()
        {
            InitializeComponent();
        }

        private void Reservasi_Load(object sender, EventArgs e)
        {
            EnsureIndexes();
            LoadAllData();
            dtpTanggal.MinDate = DateTime.Today;
            if (dgvReservasi != null) dgvReservasi.CellClick += DgvReservasi_CellClick;
        }

        private void DgvReservasi_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                PopulateFormFromGrid(e.RowIndex);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput()) return;

            // Menggunakan kn.connectionString()
            using (SqlConnection connection = new SqlConnection(kn.connectionString()))
            {
                SqlTransaction transaction = null;
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    var cmd = new SqlCommand("AddReservasi", connection, transaction) { CommandType = CommandType.StoredProcedure };
                    cmd.Parameters.AddWithValue("@pelanggan_id", cmbPelangganID.SelectedValue);
                    cmd.Parameters.AddWithValue("@meja_id", cmbMejaID.SelectedValue);
                    cmd.Parameters.AddWithValue("@tanggal", dtpTanggal.Value.Date);
                    cmd.Parameters.AddWithValue("@waktu", dtpWaktu.Value.TimeOfDay);
                    cmd.Parameters.AddWithValue("@status_reservasi", cmbStatus.Text);
                    cmd.ExecuteNonQuery();

                    if (cmbStatus.Text == "Confirmed")
                    {
                        UpdateTableStatus((int)cmbMejaID.SelectedValue, "Tidak Tersedia", connection, transaction);
                    }

                    transaction.Commit();

                    InvalidateCaches();
                    MessageBox.Show("Reservasi berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadAllData();
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    MessageBox.Show("Terjadi kesalahan. Perubahan dibatalkan.\nError: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedReservasiId == 0)
            {
                MessageBox.Show("Pilih reservasi yang akan diupdate.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!ValidateInput()) return;

            // Menggunakan kn.connectionString()
            using (SqlConnection connection = new SqlConnection(kn.connectionString()))
            {
                SqlTransaction transaction = null;
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    var getCmd = new SqlCommand("SELECT meja_id, status_reservasi FROM Reservasi WHERE reservasi_id = @id", connection, transaction);
                    getCmd.Parameters.AddWithValue("@id", selectedReservasiId);
                    int oldMejaId = 0;
                    string oldStatus = "";
                    using (var reader = getCmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            oldMejaId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);
                            oldStatus = reader.GetString(1);
                        }
                    }

                    var updateCmd = new SqlCommand("UpdateReservasi", connection, transaction) { CommandType = CommandType.StoredProcedure };
                    updateCmd.Parameters.AddWithValue("@reservasi_id", selectedReservasiId);
                    updateCmd.Parameters.AddWithValue("@pelanggan_id", cmbPelangganID.SelectedValue);
                    updateCmd.Parameters.AddWithValue("@meja_id", cmbMejaID.SelectedValue);
                    updateCmd.Parameters.AddWithValue("@tanggal", dtpTanggal.Value.Date);
                    updateCmd.Parameters.AddWithValue("@waktu", dtpWaktu.Value.TimeOfDay);
                    updateCmd.Parameters.AddWithValue("@status_reservasi", cmbStatus.Text);
                    updateCmd.ExecuteNonQuery();

                    int newMejaId = (int)cmbMejaID.SelectedValue;
                    string newStatus = cmbStatus.Text;
                    if (oldMejaId != 0 && oldMejaId != newMejaId)
                    {
                        UpdateTableStatus(oldMejaId, "Tersedia", connection, transaction);
                    }
                    if (newStatus == "Confirmed")
                    {
                        UpdateTableStatus(newMejaId, "Tidak Tersedia", connection, transaction);
                    }
                    else if (oldStatus == "Confirmed" && (newStatus == "Pending" || newStatus == "Cancelled"))
                    {
                        UpdateTableStatus(newMejaId, "Tersedia", connection, transaction);
                    }

                    transaction.Commit();

                    InvalidateCaches();
                    MessageBox.Show("Reservasi berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadAllData();
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    MessageBox.Show("Terjadi kesalahan. Perubahan dibatalkan.\nError: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedReservasiId == 0) { MessageBox.Show("Pilih reservasi untuk dihapus."); return; }

            DialogResult confirm = MessageBox.Show("Anda yakin ingin menghapus reservasi ini?", "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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

                        var getCmd = new SqlCommand("SELECT meja_id, status_reservasi FROM Reservasi WHERE reservasi_id = @id", connection, transaction);
                        getCmd.Parameters.AddWithValue("@id", selectedReservasiId);
                        int mejaId = 0;
                        string status = "";
                        using (var reader = getCmd.ExecuteReader())
                        {
                            if (reader.Read()) { mejaId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0); status = reader.GetString(1); }
                        }

                        var deleteCmd = new SqlCommand("DeleteReservasi", connection, transaction) { CommandType = CommandType.StoredProcedure };
                        deleteCmd.Parameters.AddWithValue("@reservasi_id", selectedReservasiId);
                        deleteCmd.ExecuteNonQuery();

                        if (mejaId != 0 && status == "Confirmed")
                        {
                            UpdateTableStatus(mejaId, "Tersedia", connection, transaction);
                        }

                        transaction.Commit();

                        InvalidateCaches();
                        MessageBox.Show("Reservasi berhasil dihapus!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadAllData();
                    }
                    catch (Exception ex)
                    {
                        transaction?.Rollback();
                        MessageBox.Show("Terjadi kesalahan. Perubahan dibatalkan.\nError: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            InvalidateCaches();
            LoadAllData();
        }

        private void EnsureIndexes()
        {
            // Menggunakan kn.connectionString()
            using (var conn = new SqlConnection(kn.connectionString()))
            {
                conn.Open();
                var indexScript = @"
                IF OBJECT_ID('dbo.Reservasi', 'U') IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_Reservasi_Tanggal')
                        CREATE NONCLUSTERED INDEX idx_Reservasi_Tanggal ON dbo.Reservasi(tanggal);
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_Reservasi_Status')
                        CREATE NONCLUSTERED INDEX idx_Reservasi_Status ON dbo.Reservasi(status_reservasi);
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_Reservasi_PelangganId')
                        CREATE NONCLUSTERED INDEX idx_Reservasi_PelangganId ON dbo.Reservasi(pelanggan_id);
                END";
                using (var cmd = new SqlCommand(indexScript, conn)) { cmd.ExecuteNonQuery(); }
            }
        }

        private void LoadPelanggan()
        {
            try
            {
                DataTable dt;
                if (_cache.Contains(PelangganCacheKey)) { dt = _cache.Get(PelangganCacheKey) as DataTable; }
                else
                {
                    dt = new DataTable();
                    // Menggunakan kn.connectionString()
                    using (var adapter = new SqlDataAdapter("SELECT pelanggan_id, nama FROM Pelanggan ORDER BY nama", kn.connectionString()))
                    {
                        adapter.Fill(dt);
                    }
                    _cache.Add(PelangganCacheKey, dt, _cachePolicy);
                }
                cmbPelangganID.DataSource = dt;
                cmbPelangganID.DisplayMember = "nama";
                cmbPelangganID.ValueMember = "pelanggan_id";
            }
            catch (Exception ex) { MessageBox.Show("Error loading customers: " + ex.Message); }
        }

        private void LoadMeja(int? mejaIdUntukDitampilkan = null)
        {
            try
            {
                DataTable dt;
                string cacheKey = MejaCacheKey + (mejaIdUntukDitampilkan?.ToString() ?? "");

                if (_cache.Contains(cacheKey)) { dt = _cache.Get(cacheKey) as DataTable; }
                else
                {
                    dt = new DataTable();
                    string query = "SELECT meja_id, nomor_meja FROM Meja WHERE status_meja = 'Tersedia'";
                    if (mejaIdUntukDitampilkan.HasValue)
                    {
                        query += $" OR meja_id = {mejaIdUntukDitampilkan.Value}";
                    }
                    // Menggunakan kn.connectionString()
                    using (var adapter = new SqlDataAdapter(query, kn.connectionString()))
                    {
                        adapter.Fill(dt);
                    }
                    _cache.Add(cacheKey, dt, _cachePolicy);
                }
                cmbMejaID.DataSource = dt;
                cmbMejaID.DisplayMember = "nomor_meja";
                cmbMejaID.ValueMember = "meja_id";
            }
            catch (Exception ex) { MessageBox.Show("Error loading tables: " + ex.Message); }
        }

        private void LoadReservasi()
        {
            try
            {
                DataTable dt;
                if (_cache.Contains(ReservasiCacheKey)) { dt = _cache.Get(ReservasiCacheKey) as DataTable; }
                else
                {
                    dt = new DataTable();
                    string query = @"SELECT r.reservasi_id, r.pelanggan_id, r.meja_id, r.tanggal, r.waktu, r.status_reservasi, p.nama AS Pelanggan, m.nomor_meja AS Meja 
                                   FROM Reservasi r 
                                   LEFT JOIN Pelanggan p ON r.pelanggan_id = p.pelanggan_id 
                                   LEFT JOIN Meja m ON r.meja_id = m.meja_id 
                                   ORDER BY r.tanggal DESC, r.waktu DESC";
                    // Menggunakan kn.connectionString()
                    using (var adapter = new SqlDataAdapter(query, kn.connectionString()))
                    {
                        adapter.Fill(dt);
                    }
                    _cache.Add(ReservasiCacheKey, dt, _cachePolicy);
                }

                DataTable displayDt = dt.Copy();
                displayDt.Columns.Add("waktu_formatted", typeof(string));
                foreach (DataRow row in displayDt.Rows)
                {
                    row["waktu_formatted"] = ((TimeSpan)row["waktu"]).ToString(@"hh\:mm");
                }

                dgvReservasi.DataSource = displayDt;

                string[] columnsToHide = { "reservasi_id", "pelanggan_id", "meja_id", "waktu" };
                foreach (var colName in columnsToHide)
                {
                    if (dgvReservasi.Columns.Contains(colName)) dgvReservasi.Columns[colName].Visible = false;
                }
            }
            catch (Exception ex) { MessageBox.Show("Error loading reservations: " + ex.Message); }
        }

        private void UpdateTableStatus(int mejaId, string status, SqlConnection connection, SqlTransaction transaction)
        {
            try
            {
                var cmd = new SqlCommand("UPDATE Meja SET status_meja = @status WHERE meja_id = @id", connection, transaction);
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@id", mejaId);
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw new Exception("Gagal mengupdate status meja.", ex);
            }
        }

        private void PopulateFormFromGrid(int rowIndex)
        {
            try
            {
                DataGridViewRow row = dgvReservasi.Rows[rowIndex];
                selectedReservasiId = Convert.ToInt32(row.Cells["reservasi_id"].Value);

                int mejaId = row.Cells["meja_id"].Value is DBNull ? 0 : Convert.ToInt32(row.Cells["meja_id"].Value);
                LoadMeja(mejaId);

                cmbPelangganID.SelectedValue = row.Cells["pelanggan_id"].Value;
                cmbMejaID.SelectedValue = mejaId;
                dtpTanggal.Value = Convert.ToDateTime(row.Cells["tanggal"].Value);
                dtpWaktu.Value = DateTime.Today.Add((TimeSpan)row.Cells["waktu"].Value);
                cmbStatus.Text = row.Cells["status_reservasi"].Value.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saat memilih baris: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadAllData()
        {
            LoadPelanggan();
            LoadMeja();
            LoadReservasi();
            ClearForm();
        }

        private void InvalidateCaches()
        {
            _cache.Remove(ReservasiCacheKey);
            _cache.Remove(PelangganCacheKey);
            _cache.Remove(MejaCacheKey);
        }

        private void ClearForm()
        {
            cmbPelangganID.SelectedIndex = -1;
            cmbMejaID.SelectedIndex = -1;
            dtpTanggal.Value = DateTime.Today;
            dtpWaktu.Value = DateTime.Now;
            cmbStatus.SelectedIndex = 0;
            selectedReservasiId = 0;
            dgvReservasi.ClearSelection();
        }

        private bool ValidateInput()
        {
            if (cmbPelangganID.SelectedIndex == -1 || cmbMejaID.SelectedIndex == -1 || cmbStatus.SelectedIndex == -1)
            {
                MessageBox.Show("Pelanggan, Meja, dan Status harus dipilih.", "Validasi Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }
    }
}