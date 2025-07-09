using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Runtime.Caching;

namespace Project
{
    public partial class Pembayaran : Form
    {
        // Menambahkan instance Koneksi
        private Koneksi kn = new Koneksi();
        private int selectedPembayaranId = 0;

        private readonly ObjectCache _cache = MemoryCache.Default;
        private readonly CacheItemPolicy _cachePolicy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5)
        };
        private const string PembayaranCacheKey = "PembayaranDataCache";
        private const string ReservasiCacheKey = "ReservasiForPembayaranCache";


        public Pembayaran()
        {
            InitializeComponent();
        }

        private void Pembayaran_Load(object sender, EventArgs e)
        {
            EnsureIndexes();
            LoadReservasi();
            LoadPembayaran();
            dgvPembayaran.CellClick += DgvPembayaran_CellClick;
        }

        private void CmbReservasiID_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbReservasiID.SelectedIndex == -1 || cmbReservasiID.SelectedValue == null)
            {
                ClearForm(false);
                return;
            }

            int reservasiId = (int)cmbReservasiID.SelectedValue;
            CheckPaymentExistsForReservation(reservasiId);
        }

        private void DgvPembayaran_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                PopulateFormFromGrid(e.RowIndex);
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput(out decimal jumlah)) return;

            // Menggunakan kn.connectionString()
            using (SqlConnection connection = new SqlConnection(kn.connectionString()))
            {
                SqlTransaction transaction = null;
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    SqlCommand command = new SqlCommand("AddPembayaran", connection, transaction);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@reservasi_id", cmbReservasiID.SelectedValue);
                    command.Parameters.AddWithValue("@jumlah", jumlah);
                    command.Parameters.AddWithValue("@metode", cmbMetode.Text);
                    command.Parameters.AddWithValue("@status_pembayaran", cmbStatus.Text);

                    command.ExecuteNonQuery();
                    transaction.Commit();

                    InvalidateCaches();
                    MessageBox.Show("Pembayaran berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            if (!ValidateInput(out decimal jumlah)) return;

            // Menggunakan kn.connectionString()
            using (SqlConnection connection = new SqlConnection(kn.connectionString()))
            {
                SqlTransaction transaction = null;
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    SqlCommand command = new SqlCommand("UpdatePembayaran", connection, transaction);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@pembayaran_id", selectedPembayaranId);
                    command.Parameters.AddWithValue("@reservasi_id", cmbReservasiID.SelectedValue);
                    command.Parameters.AddWithValue("@jumlah", jumlah);
                    command.Parameters.AddWithValue("@metode", cmbMetode.Text);
                    command.Parameters.AddWithValue("@status_pembayaran", cmbStatus.Text);

                    int result = command.ExecuteNonQuery();
                    if (result > 0)
                    {
                        transaction.Commit();
                        InvalidateCaches();
                        MessageBox.Show("Pembayaran berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadAllData();
                    }
                    else
                    {
                        throw new Exception("Data tidak ditemukan atau gagal diperbarui.");
                    }
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
            DialogResult confirm = MessageBox.Show("Apakah Anda yakin ingin menghapus pembayaran ini?", "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
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

                        SqlCommand command = new SqlCommand("DeletePembayaran", connection, transaction);
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@pembayaran_id", selectedPembayaranId);

                        int result = command.ExecuteNonQuery();
                        if (result > 0)
                        {
                            transaction.Commit();
                            InvalidateCaches();
                            MessageBox.Show("Pembayaran berhasil dihapus!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadAllData();
                        }
                        else
                        {
                            throw new Exception("Data tidak ditemukan di database.");
                        }
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
                IF OBJECT_ID('dbo.Pembayaran', 'U') IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_Pembayaran_Status')
                        CREATE NONCLUSTERED INDEX idx_Pembayaran_Status ON dbo.Pembayaran(status_pembayaran);
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_Pembayaran_ReservasiId')
                        CREATE NONCLUSTERED INDEX idx_Pembayaran_ReservasiId ON dbo.Pembayaran(reservasi_id);
                END";
                using (var cmd = new SqlCommand(indexScript, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void LoadReservasi()
        {
            try
            {
                DataTable reservasiDt;
                if (_cache.Contains(ReservasiCacheKey))
                {
                    reservasiDt = _cache.Get(ReservasiCacheKey) as DataTable;
                }
                else
                {
                    reservasiDt = new DataTable();
                    string query = @"SELECT r.reservasi_id, 
                            CONCAT(r.reservasi_id, ' - ', p.nama, ' (', FORMAT(r.tanggal, 'dd/MM/yyyy'), ') - ', r.status_reservasi) AS info,
                            (SELECT TOP 1 pb.status_pembayaran FROM Pembayaran pb WHERE pb.reservasi_id = r.reservasi_id) AS payment_status
                           FROM Reservasi r
                           JOIN Pelanggan p ON r.pelanggan_id = p.pelanggan_id
                           ORDER BY r.tanggal DESC, r.waktu DESC";

                    // Menggunakan kn.connectionString()
                    using (var connection = new SqlConnection(kn.connectionString()))
                    using (var adapter = new SqlDataAdapter(query, connection))
                    {
                        adapter.Fill(reservasiDt);
                        foreach (DataRow row in reservasiDt.Rows)
                        {
                            if (row["payment_status"] != DBNull.Value)
                            {
                                row["info"] += $" [{row["payment_status"]}]";
                            }
                        }
                    }
                    _cache.Add(ReservasiCacheKey, reservasiDt, _cachePolicy);
                }

                cmbReservasiID.DataSource = reservasiDt;
                cmbReservasiID.DisplayMember = "info";
                cmbReservasiID.ValueMember = "reservasi_id";
                cmbReservasiID.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error memuat reservasi: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadPembayaran()
        {
            try
            {
                DataTable pembayaranDt;
                if (_cache.Contains(PembayaranCacheKey))
                {
                    pembayaranDt = _cache.Get(PembayaranCacheKey) as DataTable;
                }
                else
                {
                    pembayaranDt = new DataTable();
                    string query = @"SELECT pb.pembayaran_id, pb.reservasi_id,
                           CONCAT(r.reservasi_id, ' - ', p.nama) AS Reservasi,
                           pb.jumlah, pb.metode, pb.status_pembayaran
                           FROM Pembayaran pb
                           JOIN Reservasi r ON pb.reservasi_id = r.reservasi_id
                           JOIN Pelanggan p ON r.pelanggan_id = p.pelanggan_id
                           ORDER BY pb.pembayaran_id DESC";
                    // Menggunakan kn.connectionString()
                    using (var connection = new SqlConnection(kn.connectionString()))
                    using (var adapter = new SqlDataAdapter(query, connection))
                    {
                        adapter.Fill(pembayaranDt);
                    }
                    _cache.Add(PembayaranCacheKey, pembayaranDt, _cachePolicy);
                }

                dgvPembayaran.DataSource = pembayaranDt;
                if (dgvPembayaran.Columns["jumlah"] != null) dgvPembayaran.Columns["jumlah"].DefaultCellStyle.Format = "N2";
                if (dgvPembayaran.Columns["pembayaran_id"] != null) dgvPembayaran.Columns["pembayaran_id"].Visible = false;
                if (dgvPembayaran.Columns["reservasi_id"] != null) dgvPembayaran.Columns["reservasi_id"].Visible = false;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error memuat pembayaran: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void LoadAllData()
        {
            LoadReservasi();
            LoadPembayaran();
            ClearForm(true);
        }

        private void InvalidateCaches()
        {
            _cache.Remove(PembayaranCacheKey);
            _cache.Remove(ReservasiCacheKey);
        }

        private void ClearForm(bool clearComboBox)
        {
            if (clearComboBox)
            {
                cmbReservasiID.SelectedIndex = -1;
            }
            txtJumlah.Clear();
            cmbMetode.SelectedIndex = -1;
            cmbStatus.SelectedIndex = -1;
            selectedPembayaranId = 0;
            dgvPembayaran.ClearSelection();
        }

        private void CheckPaymentExistsForReservation(int reservasiId)
        {
            DataView dv = new DataView(dgvPembayaran.DataSource as DataTable);
            dv.RowFilter = $"reservasi_id = {reservasiId}";

            if (dv.Count > 0)
            {
                foreach (DataGridViewRow row in dgvPembayaran.Rows)
                {
                    if (Convert.ToInt32(row.Cells["reservasi_id"].Value) == reservasiId)
                    {
                        PopulateFormFromGrid(row.Index);
                        break;
                    }
                }
            }
            else
            {
                selectedPembayaranId = 0;
                // Menggunakan kn.connectionString()
                using (var connection = new SqlConnection(kn.connectionString()))
                {
                    var query = "SELECT ISNULL(SUM(harga), 0) as total FROM Pesanan WHERE reservasi_id = @id";
                    using (var cmd = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        cmd.Parameters.AddWithValue("@id", reservasiId);
                        decimal total = Convert.ToDecimal(cmd.ExecuteScalar());
                        txtJumlah.Text = total.ToString("N2");
                    }
                }
                cmbMetode.SelectedIndex = 0;
                cmbStatus.SelectedIndex = 0;
            }
        }

        private void PopulateFormFromGrid(int rowIndex)
        {
            DataGridViewRow row = dgvPembayaran.Rows[rowIndex];
            selectedPembayaranId = Convert.ToInt32(row.Cells["pembayaran_id"].Value);

            int reservasiId = Convert.ToInt32(row.Cells["reservasi_id"].Value);
            cmbReservasiID.SelectedValue = reservasiId;

            txtJumlah.Text = Convert.ToDecimal(row.Cells["jumlah"].Value).ToString("N2");

            string metode = row.Cells["metode"].Value.ToString();
            cmbMetode.SelectedIndex = cmbMetode.FindStringExact(metode);

            string status = row.Cells["status_pembayaran"].Value.ToString();
            cmbStatus.SelectedIndex = cmbStatus.FindStringExact(status);
        }

        private bool ValidateInput(out decimal jumlah)
        {
            jumlah = 0;
            if (cmbReservasiID.SelectedIndex == -1)
            {
                MessageBox.Show("Pilih reservasi terlebih dahulu.", "Validasi Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!decimal.TryParse(txtJumlah.Text, out jumlah) || jumlah < 0)
            {
                MessageBox.Show("Jumlah harus berupa angka positif.", "Validasi Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (cmbMetode.SelectedIndex == -1 || cmbStatus.SelectedIndex == -1)
            {
                MessageBox.Show("Pilih metode dan status pembayaran.", "Validasi Gagal", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }
    }
}