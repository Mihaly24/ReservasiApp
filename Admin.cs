using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Drawing;
using System.Linq;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.Caching;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.XWPF.UserModel;
using System.Drawing.Printing;
using System.Xml.Linq;

namespace Project
{
    public partial class Admin : Form
    {
        // Database connection string
        private readonly string connectionString = "Data Source=MIHALY\\FAIRUZ013;Initial Catalog=ReservasiRestoran;Integrated Security=True"; //
        private int selectedAdminId = 0;

        private readonly MemoryCache _cache = MemoryCache.Default;
        private readonly CacheItemPolicy _cachePolicy = new CacheItemPolicy
        {
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(10)
        };
        private const string CacheKey = "AdminDataCache";

        public Admin()
        {
            InitializeComponent();
        }

        private void Admin_Load(object sender, EventArgs e)
        {
            EnsureIndexes();
            LoadAdminData();

            dgvAdminResto.SelectionChanged += DgvAdminResto_SelectionChanged; //
        }

        private void EnsureIndexes()
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var indexScript = @"
                IF OBJECT_ID('dbo.AdminResto', 'U') IS NOT NULL
                BEGIN
                    IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'idx_Admin_Nama')
                        CREATE NONCLUSTERED INDEX idx_Admin_Nama ON dbo.AdminResto(nama);
                END";
                using (var cmd = new SqlCommand(indexScript, conn)) //
                {
                    cmd.ExecuteNonQuery(); //
                }
            }
        }

        private void LoadAdminData()
        {
            try
            {
                DataTable adminDataTable;

                // Langkah 1: Periksa apakah data ada di dalam cache
                if (_cache.Contains(CacheKey))
                {
                    // Jika ada, ambil data dari cache
                    adminDataTable = _cache.Get(CacheKey) as DataTable;
                }
                else
                {
                    // Jika tidak ada, ambil data dari database
                    adminDataTable = new DataTable();
                    using (var connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        // Query yang lebih spesifik, mirip dengan di gambar
                        var query = "SELECT admin_id, nama, username, Passwords FROM dbo.AdminResto";
                        using (var adapter = new SqlDataAdapter(query, connection))
                        {
                            adapter.Fill(adminDataTable);
                        }
                    }

                    // Langkah 2: Simpan data yang baru diambil ke dalam cache
                    _cache.Add(CacheKey, adminDataTable, _cachePolicy);
                }

                // Langkah 3: Tampilkan data ke DataGridView
                dgvAdminResto.DataSource = adminDataTable;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error memuat data admin: " + ex.Message, "Data Loading Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void ClearInputFields()
        {
            txtNama.Text = ""; //
            txtUsername.Text = ""; //
            txtPassword.Text = ""; //
            selectedAdminId = 0; //
        }

        private void DgvAdminResto_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvAdminResto.CurrentRow != null) //
            {
                try
                {
                    // Get the ID of the selected admin
                    selectedAdminId = Convert.ToInt32(dgvAdminResto.CurrentRow.Cells["admin_id"].Value); //

                    // Fill the text fields with the selected admin's data
                    txtNama.Text = dgvAdminResto.CurrentRow.Cells["nama"].Value.ToString(); //
                    txtUsername.Text = dgvAdminResto.CurrentRow.Cells["username"].Value.ToString(); //
                    txtPassword.Text = dgvAdminResto.CurrentRow.Cells["Passwords"].Value.ToString(); //
                }
                catch (Exception ex)
                {
                    // If there's an error (like column name doesn't exist), just log it
                    Console.WriteLine("Error in selection changed: " + ex.Message); //
                    MessageBox.Show("Error processing selection: " + ex.Message, "Selection Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        // Helper method for input validation
        private bool ValidateAdminInput(out string errorMessage)
        {
            errorMessage = string.Empty;

            // Validate Nama
            if (string.IsNullOrWhiteSpace(txtNama.Text))
            {
                errorMessage = "Nama field cannot be empty.";
                return false;
            }
            if (!Regex.IsMatch(txtNama.Text, @"^[a-zA-Z\s]+$"))
            {
                errorMessage = "Nama can only contain letters and spaces.";
                return false;
            }
            if (txtNama.Text.Length > 30)
            {
                errorMessage = "Nama cannot exceed 30 characters.";
                return false;
            }

            // Validate Username
            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                errorMessage = "Username field cannot be empty.";
                return false;
            }
            // Add more specific username validation if needed (e.g., length, no special characters other than underscore)
            // For example: if (!Regex.IsMatch(txtUsername.Text, @"^[a-zA-Z0-9_]+$")) { errorMessage = "Username can only contain letters, numbers, and underscores."; return false; }


            // Validate Password
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                errorMessage = "Password field cannot be empty.";
                return false;
            }
            // Add password complexity rules if needed (e.g., minimum length, special characters)
            // For example: if (txtPassword.Text.Length < 6) { errorMessage = "Password must be at least 6 characters long."; return false; }

            return true;
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            _cache.Remove(CacheKey);
            LoadAdminData(); //
            ClearInputFields(); //
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            // Validasi input tetap di luar, ini sudah bagus
            if (string.IsNullOrWhiteSpace(txtNama.Text) ||
                string.IsNullOrWhiteSpace(txtUsername.Text) ||
                string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Harap isi semua data!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlTransaction transaction = null;

                try
                {
                    connection.Open();
                    transaction = connection.BeginTransaction();

                    SqlCommand command = new SqlCommand("AddAdmin", connection, transaction);
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    // Menambahkan parameter seperti biasa
                    command.Parameters.AddWithValue("@nama", txtNama.Text.Trim());
                    command.Parameters.AddWithValue("@username", txtUsername.Text.Trim());
                    command.Parameters.AddWithValue("@Passwords", txtPassword.Text.Trim());

                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        transaction.Commit();

                        _cache.Remove(CacheKey);
                        MessageBox.Show("Admin berhasil ditambahkan!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadAdminData();
                        ClearInputFields();
                    }
                    else
                    {
                        throw new Exception("Operasi penambahan data tidak mempengaruhi baris manapun.");
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        transaction?.Rollback();
                    }
                    catch (Exception exRollback)
                    {
                        MessageBox.Show("Error saat rollback transaksi: " + exRollback.Message);
                    }

                    MessageBox.Show("Terjadi kesalahan. Perubahan telah dibatalkan.\nError: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedAdminId == 0)
            {
                MessageBox.Show("Pilih data admin yang akan diubah!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(txtNama.Text) || string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                MessageBox.Show("Nama dan Username tidak boleh kosong.", "Input Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                SqlTransaction transaction = null;

                try
                {
                    connection.Open();
                    // Memulai transaksi
                    transaction = connection.BeginTransaction();

                    SqlCommand command = new SqlCommand("UpdateAdmin", connection, transaction);
                    command.CommandType = System.Data.CommandType.StoredProcedure;

                    // Menambahkan parameter untuk Stored Procedure
                    command.Parameters.AddWithValue("@admin_id", selectedAdminId);
                    command.Parameters.AddWithValue("@nama", txtNama.Text.Trim());
                    command.Parameters.AddWithValue("@username", txtUsername.Text.Trim());
                    command.Parameters.AddWithValue("@Passwords", txtPassword.Text.Trim());

                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        transaction.Commit();

                        _cache.Remove(CacheKey);
                        MessageBox.Show("Data admin berhasil diperbarui!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        LoadAdminData();
                        ClearInputFields();
                    }
                    else
                    {
                        throw new Exception("Data tidak ditemukan atau gagal diperbarui.");
                    }
                }
                catch (Exception ex)
                {
                    try
                    {
                        transaction?.Rollback();
                    }
                    catch (Exception exRollback)
                    {
                        MessageBox.Show("Error saat rollback transaksi: " + exRollback.Message);
                    }
                    MessageBox.Show("Terjadi kesalahan. Perubahan telah dibatalkan.\nError: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedAdminId == 0)
            {
                MessageBox.Show("Pilih data admin yang akan dihapus!", "Peringatan", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult confirm = MessageBox.Show("Apakah Anda yakin ingin menghapus data admin ini?", "Konfirmasi Hapus",
                                                    MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    SqlTransaction transaction = null;

                    try
                    {
                        connection.Open();
                        // Memulai transaksi
                        transaction = connection.BeginTransaction();

                        // Membuat command dan menghubungkannya ke koneksi dan transaksi
                        SqlCommand command = new SqlCommand("DeleteAdmin", connection, transaction);
                        command.CommandType = System.Data.CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@admin_id", selectedAdminId);

                        int result = command.ExecuteNonQuery();

                        if (result > 0)
                        {
                            transaction.Commit();

                            _cache.Remove(CacheKey);
                            MessageBox.Show("Data admin berhasil dihapus!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            LoadAdminData();
                            ClearInputFields();
                        }
                        else
                        {
                            throw new Exception("Data tidak ditemukan di database.");
                        }
                    }
                    catch (Exception ex)
                    {
                        try
                        {
                            transaction?.Rollback();
                        }
                        catch (Exception exRollback)
                        {
                            MessageBox.Show("Error saat rollback transaksi: " + exRollback.Message);
                        }

                        MessageBox.Show("Terjadi kesalahan. Perubahan telah dibatalkan.\nError: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void BtnAnalyze(object sender, EventArgs e)
        {
            var heavyQuery = "SELECT nama, username, passwords FROM dbo.AdminResto WHERE nama LIKE 'A%'";
            AnalyzeQuery(heavyQuery);
        }

        private void BtnReport_Click(object sender, EventArgs e)
        {
            ReportAdmin adminForm = new ReportAdmin();
            adminForm.Show();
        }
    }
}