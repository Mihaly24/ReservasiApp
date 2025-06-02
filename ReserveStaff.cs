using System;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace Project
{
    public partial class ReserveStaff : Form
    {
        // Connection string ke database Anda
        private string connectionString = "Data Source=MIHALY\\FAIRUZ013;Initial Catalog=ReservasiRestoran;Integrated Security=True";

        // Asumsikan Anda memiliki DataGridView bernama dgvReservasi
        // dan Button bernama BtnRefresh di form ReserveStaff Anda.
        // Kontrol-kontrol ini harus ditambahkan melalui Form Designer.

        public ReserveStaff()
        {
            InitializeComponent(); // Ini memanggil metode dari ReserveStaff.Designer.cs

            // Panggil InitializeForm untuk mengatur properti DataGridView
            InitializeFormProperties();

            // Tambahkan event handler untuk Load form
            this.Load += ReserveStaff_Load;

            // Jika Anda menambahkan BtnRefresh melalui designer,
            // event click handler-nya (BtnRefresh_Click) akan terhubung di ReserveStaff.Designer.cs
            // atau Anda bisa menghubungkannya di sini jika belum:
            // this.BtnRefresh.Click += new System.EventHandler(this.BtnRefresh_Click);
        }

        private void ReserveStaff_Load(object sender, EventArgs e)
        {
            // Saat form dimuat, langsung tampilkan data reservasi
            LoadReservasiData();
        }

        private void InitializeFormProperties()
        {
            // Pastikan dgvReservasi sudah ada di form designer Anda
            // Jika Anda menggunakan nama lain untuk DataGridView, sesuaikan di sini.
            if (this.Controls.Find("dgvReservasi", true).FirstOrDefault() is DataGridView dgv)
            {
                dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
                dgv.ReadOnly = true;
                dgv.AllowUserToAddRows = false;
                dgv.MultiSelect = false;
                dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill; // Agar kolom menyesuaikan lebar
            }
        }

        private void LoadReservasiData()
        {
            // Pastikan dgvReservasi sudah ada di form designer Anda
            if (!(this.Controls.Find("dgvReservasi", true).FirstOrDefault() is DataGridView dgv))
            {
                MessageBox.Show("DataGridView 'dgvReservasi' tidak ditemukan di form.", "Error Desain", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // Query untuk mengambil data reservasi yang relevan untuk ditampilkan
                    // (Sama seperti query di Reservasi.cs)
                    string query = @"SELECT r.reservasi_id AS 'ID Reservasi', 
                                           p.nama AS Pelanggan, 
                                           m.nomor_meja AS 'Nomor Meja', 
                                           r.tanggal AS Tanggal, 
                                           CONVERT(VARCHAR(5), r.waktu, 108) AS Waktu, 
                                           r.status_reservasi AS 'Status Reservasi'
                                   FROM Reservasi r
                                   LEFT JOIN Pelanggan p ON r.pelanggan_id = p.pelanggan_id
                                   LEFT JOIN Meja m ON r.meja_id = m.meja_id
                                   ORDER BY r.tanggal DESC, r.waktu DESC";

                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgv.DataSource = dt; // Tampilkan data di DataGridView

                    // Sembunyikan kolom asli 'waktu' jika masih ada dan query mengambil 'waktu_formatted'
                    // Namun query di atas sudah mengalias 'waktu' menjadi 'Waktu', jadi ini mungkin tidak perlu
                    // jika Anda tidak memiliki kolom 'waktu' yang tidak terformat di SELECT list.
                    if (dgv.Columns.Contains("waktu_formatted") && dgv.Columns.Contains("waktu"))
                    {
                        dgv.Columns["waktu"].Visible = false;
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error memuat data reservasi: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                // Koneksi akan ditutup otomatis oleh 'using' statement
            }
        }

        // Event handler untuk tombol Refresh
        // Pastikan tombol ini bernama BtnRefresh di designer dan event Click-nya terhubung ke metode ini.
        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadReservasiData(); // Muat ulang data dari database
            MessageBox.Show("Data reservasi telah diperbarui.", "Refresh", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}