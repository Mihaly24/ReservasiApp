using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Runtime.Caching;

namespace Project
{
    public partial class Menu : Form
    {
        private readonly string connectionString = @"Data Source=MIHALY\FAIRUZ013;Initial Catalog=ReservasiRestoran;Integrated Security=True";
        private int selectedMenuId = 0;

        private readonly ObjectCache _cache = MemoryCache.Default;
        private readonly CacheItemPolicy _cachePolicy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(10)
        };
        private const string CacheKey = "MenuDataCache";

        public Menu()
        {
            InitializeComponent();
        }

        private void Menu_Load(object sender, EventArgs e)
        {
            EnsureIndexes();
            LoadData();
            dgcMenu.CellClick += dgcMenu_CellClick;
        }

        private void dgcMenu_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                try
                {
                    DataGridViewRow row = dgcMenu.Rows[e.RowIndex];
                    selectedMenuId = Convert.ToInt32(row.Cells["menu_id"].Value);
                    txtNama.Text = row.Cells["nama"].Value.ToString();
                    tctDeskripsi.Text = row.Cells["deskripsi"].Value?.ToString() ?? "";
                    txtHarga.Text = Convert.ToDecimal(row.Cells["harga"].Value).ToString("F2");

                    string kategori = row.Cells["kategori"].Value.ToString().Trim();
                    if (!string.IsNullOrEmpty(kategori))
                    {
                        cmbKategori.Text = char.ToUpper(kategori[0]) + kategori.Substring(1);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error selecting menu: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void AnalyzeQuery(string query)
        {
            using (var conn = new SqlConnection(connectionString))
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


        private void btnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput(out decimal harga))
            {
                return; // Pesan error sudah ditampilkan di dalam ValidateInput
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlTransaction transaction = null;
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    // Panggil Stored Procedure AddMenu
                    SqlCommand command = new SqlCommand("AddMenu", connection, transaction);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@nama", txtNama.Text.Trim());
                    command.Parameters.AddWithValue("@deskripsi", string.IsNullOrWhiteSpace(tctDeskripsi.Text) ? (object)DBNull.Value : tctDeskripsi.Text.Trim());
                    command.Parameters.AddWithValue("@harga", harga);
                    command.Parameters.AddWithValue("@kategori", cmbKategori.Text.ToLower().Trim());

                    command.ExecuteNonQuery();
                    transaction.Commit();

                    _cache.Remove(CacheKey);
                    MessageBox.Show("Menu berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                    ClearFields();
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    MessageBox.Show("Terjadi kesalahan saat menambah menu. Perubahan dibatalkan.\nError: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedMenuId == 0)
            {
                MessageBox.Show("Pilih menu yang akan diupdate!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!ValidateInput(out decimal harga))
            {
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlTransaction transaction = null;
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    // Panggil Stored Procedure UpdateMenu
                    SqlCommand command = new SqlCommand("UpdateMenu", connection, transaction);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@menu_id", selectedMenuId);
                    command.Parameters.AddWithValue("@nama", txtNama.Text.Trim());
                    command.Parameters.AddWithValue("@deskripsi", string.IsNullOrWhiteSpace(tctDeskripsi.Text) ? (object)DBNull.Value : tctDeskripsi.Text.Trim());
                    command.Parameters.AddWithValue("@harga", harga);
                    command.Parameters.AddWithValue("@kategori", cmbKategori.Text.ToLower().Trim());

                    int result = command.ExecuteNonQuery();
                    if (result > 0)
                    {
                        transaction.Commit();
                        _cache.Remove(CacheKey);
                        MessageBox.Show("Menu berhasil diupdate!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadData();
                        ClearFields();
                    }
                    else
                    {
                        throw new Exception("Data tidak ditemukan atau gagal diperbarui.");
                    }
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    MessageBox.Show("Terjadi kesalahan saat memperbarui menu. Perubahan dibatalkan.\nError: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (selectedMenuId == 0)
            {
                MessageBox.Show("Pilih menu yang akan dihapus!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show("Anda yakin ingin menghapus menu ini?", "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlTransaction transaction = null;
                    try
                    {
                        connection.Open();
                        transaction = connection.BeginTransaction();

                        // Panggil Stored Procedure DeleteMenu
                        SqlCommand command = new SqlCommand("DeleteMenu", connection, transaction);
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@menu_id", selectedMenuId);

                        int result = command.ExecuteNonQuery();
                        if (result > 0)
                        {
                            transaction.Commit();
                            _cache.Remove(CacheKey);
                            MessageBox.Show("Menu berhasil dihapus!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadData();
                            ClearFields();
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
                            MessageBox.Show("Gagal menghapus. Menu ini mungkin sedang digunakan dalam data reservasi.", "Error Hapus", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                        else
                        {
                            MessageBox.Show("Terjadi kesalahan database. Perubahan dibatalkan.\nError: " + sqlEx.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            _cache.Remove(CacheKey);
            LoadData();
            ClearFields();
        }


        private void EnsureIndexes()
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var indexScript = @"
                IF OBJECT_ID('dbo.Menu', 'U') IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_Menu_Nama')
                        CREATE NONCLUSTERED INDEX idx_Menu_Nama ON dbo.Menu(nama);
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
                DataTable menuDataTable;
                if (_cache.Contains(CacheKey))
                {
                    menuDataTable = _cache.Get(CacheKey) as DataTable;
                }
                else
                {
                    menuDataTable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        var query = "SELECT menu_id, nama, deskripsi, harga, kategori FROM Menu";
                        using (var adapter = new SqlDataAdapter(query, connection))
                        {
                            adapter.Fill(menuDataTable);
                        }
                    }
                    _cache.Add(CacheKey, menuDataTable, _cachePolicy);
                }

                dgcMenu.DataSource = menuDataTable;
                dgcMenu.Columns["menu_id"].Visible = false;
                dgcMenu.Columns["nama"].HeaderText = "Nama Menu";
                dgcMenu.Columns["deskripsi"].HeaderText = "Deskripsi";
                dgcMenu.Columns["harga"].HeaderText = "Harga";
                dgcMenu.Columns["kategori"].HeaderText = "Kategori";
                dgcMenu.Columns["harga"].DefaultCellStyle.Format = "N2";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error memuat data menu: " + ex.Message, "Data Loading Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnAnalyze_Click(object sender, EventArgs e)
        {
            // Menganalisis query yang mencari menu dengan kategori 'makanan'
            var heavyQuery = "SELECT nama, deskripsi, harga, kategori FROM dbo.Menu WHERE kategori = 'makanan'";
            AnalyzeQuery(heavyQuery);
        }

        private void BtnReport_Click(object sender, EventArgs e)
        {
            ReportMenu menuReportForm = new ReportMenu();
            menuReportForm.Show();
        }

        private void ClearFields()
        {
            txtNama.Clear();
            tctDeskripsi.Clear();
            txtHarga.Clear();
            cmbKategori.SelectedIndex = 0;
            selectedMenuId = 0;
            dgcMenu.ClearSelection();
            txtNama.Focus();
        }

        private bool ValidateInput(out decimal harga)
        {
            harga = 0;
            if (string.IsNullOrWhiteSpace(txtNama.Text) || txtNama.Text.Length > 20)
            {
                MessageBox.Show("Nama menu harus diisi dan maksimal 20 karakter!", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!string.IsNullOrEmpty(tctDeskripsi.Text) && tctDeskripsi.Text.Length > 50)
            {
                MessageBox.Show("Deskripsi maksimal 50 karakter!", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            if (!decimal.TryParse(txtHarga.Text, out harga) || harga <= 0)
            {
                MessageBox.Show("Harga harus berupa angka positif!", "Input Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            return true;
        }
    }
}