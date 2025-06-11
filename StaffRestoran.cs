using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;
using System.Runtime.Caching;
using System.Text.RegularExpressions;

namespace Project
{
    public partial class StaffRestoran : Form
    {
        private readonly string connectionString = "Data Source=MIHALY\\FAIRUZ013;Initial Catalog=ReservasiRestoran;Integrated Security=True";
        private int selectedStaffId = 0;

        private readonly ObjectCache _cache = MemoryCache.Default;
        private readonly CacheItemPolicy _cachePolicy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(10)
        };
        private const string CacheKey = "StaffDataCache";

        public StaffRestoran()
        {
            InitializeComponent();
        }


        private void StaffRestoran_Load(object sender, EventArgs e)
        {
            EnsureIndexes();
            LoadStaffData(); 
            dgvStaff.SelectionChanged += DgvStaff_SelectionChanged;
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

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput(out string validationMessage))
            {
                MessageBox.Show(validationMessage, "Input Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlTransaction transaction = null;
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    SqlCommand command = new SqlCommand("AddStaff", connection, transaction);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@nama", txtNama.Text.Trim());
                    command.Parameters.AddWithValue("@posisi", txtPosisi.Text.Trim());
                    command.Parameters.AddWithValue("@username", txtUsername.Text.Trim());
                    command.Parameters.AddWithValue("@passwords", txtPassword.Text.Trim());
                    command.Parameters.AddWithValue("@no_telp", txtNoTelp.Text.Trim());

                    command.ExecuteNonQuery();
                    transaction.Commit();

                    _cache.Remove(CacheKey);
                    MessageBox.Show("Staff berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadStaffData();
                    ClearInputs();
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    MessageBox.Show("Terjadi kesalahan saat menambahkan staff. Perubahan telah dibatalkan.\nError: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedStaffId == 0)
            {
                MessageBox.Show("Pilih staff yang akan diubah.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!ValidateInput(out string validationMessage))
            {
                MessageBox.Show(validationMessage, "Input Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlTransaction transaction = null;
                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    SqlCommand command = new SqlCommand("UpdateStaff", connection, transaction);
                    command.CommandType = CommandType.StoredProcedure;
                    command.Parameters.AddWithValue("@staff_id", selectedStaffId);
                    command.Parameters.AddWithValue("@nama", txtNama.Text.Trim());
                    command.Parameters.AddWithValue("@posisi", txtPosisi.Text.Trim());
                    command.Parameters.AddWithValue("@username", txtUsername.Text.Trim());
                    command.Parameters.AddWithValue("@passwords", txtPassword.Text.Trim());
                    command.Parameters.AddWithValue("@no_telp", txtNoTelp.Text.Trim());

                    int result = command.ExecuteNonQuery();
                    if (result > 0)
                    {
                        transaction.Commit();
                        _cache.Remove(CacheKey);
                        MessageBox.Show("Data staff berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadStaffData();
                        ClearInputs();
                    }
                    else
                    {
                        throw new Exception("Data tidak ditemukan atau gagal diperbarui.");
                    }
                }
                catch (Exception ex)
                {
                    transaction?.Rollback();
                    MessageBox.Show("Terjadi kesalahan saat memperbarui staff. Perubahan telah dibatalkan.\nError: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedStaffId == 0)
            {
                MessageBox.Show("Pilih staff yang akan dihapus.", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show("Apakah Anda yakin ingin menghapus staff ini?", "Konfirmasi Hapus", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm == DialogResult.Yes)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlTransaction transaction = null;
                    try
                    {
                        connection.Open();
                        transaction = connection.BeginTransaction();

                        SqlCommand command = new SqlCommand("DeleteStaff", connection, transaction);
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@staff_id", selectedStaffId);

                        int result = command.ExecuteNonQuery();
                        if (result > 0)
                        {
                            transaction.Commit();
                            _cache.Remove(CacheKey);
                            MessageBox.Show("Data staff berhasil dihapus!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadStaffData();
                            ClearInputs();
                        }
                        else
                        {
                            throw new Exception("Data tidak ditemukan di database.");
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction?.Rollback();
                        MessageBox.Show("Terjadi kesalahan saat menghapus staff. Perubahan telah dibatalkan.\nError: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            _cache.Remove(CacheKey);
            LoadStaffData();
            ClearInputs();
        }

        private void BtnAnalyze_Click(object sender, EventArgs e)
        {
            // Menganalisis query yang mencari staff dengan posisi 'kasir'
            var heavyQuery = "SELECT nama, username, posisi FROM dbo.Staff_Restoran WHERE posisi = 'kasir'";
            AnalyzeQuery(heavyQuery);
        }

        private void BtnReport_Click(object sender, EventArgs e)
        {
            ReportStaff staffReportForm = new ReportStaff();
            staffReportForm.Show();
        }

        private void EnsureIndexes()
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var indexScript = @"
                IF OBJECT_ID('dbo.Staff_Restoran', 'U') IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_Staff_Nama')
                        CREATE NONCLUSTERED INDEX idx_Staff_Nama ON dbo.Staff_Restoran(nama);
                END";
                using (var cmd = new SqlCommand(indexScript, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        private void LoadStaffData()
        {
            try
            {
                DataTable staffDataTable;
                if (_cache.Contains(CacheKey))
                {
                    staffDataTable = _cache.Get(CacheKey) as DataTable;
                }
                else
                {
                    staffDataTable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();
                        var query = "SELECT staff_id, nama, posisi, username, passwords, no_telp FROM dbo.Staff_Restoran";
                        using (var adapter = new SqlDataAdapter(query, connection))
                        {
                            adapter.Fill(staffDataTable);
                        }
                    }
                    _cache.Add(CacheKey, staffDataTable, _cachePolicy);
                }
                dgvStaff.DataSource = staffDataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error memuat data staff: " + ex.Message, "Data Loading Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        // Metode validasi input dari kode asli Anda, tetap relevan.
        private bool ValidateInput(out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(txtNama.Text) || !Regex.IsMatch(txtNama.Text, @"^[a-zA-Z\s]+$") || txtNama.Text.Length > 30)
            {
                errorMessage = "Nama hanya boleh berisi huruf, spasi, dan maksimal 30 karakter.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtPosisi.Text))
            {
                errorMessage = "Posisi tidak boleh kosong.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                errorMessage = "Username tidak boleh kosong.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                errorMessage = "Password tidak boleh kosong.";
                return false;
            }
            if (string.IsNullOrWhiteSpace(txtNoTelp.Text) || !Regex.IsMatch(txtNoTelp.Text, @"^\d{11,13}$"))
            {
                errorMessage = "No Telp hanya boleh berisi angka dengan panjang 11-13 digit.";
                return false;
            }
            return true;
        }

        private void txtUsername_TextChanged(object sender, EventArgs e)
        {

        }
    }
}