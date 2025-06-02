using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Project
{
    public partial class MenuStaff : Form
    {
        // Database connection string (sama seperti di Menu.cs)
        private string connectionString = @"Data Source=MIHALY\FAIRUZ013;Initial Catalog=ReservasiRestoran;Integrated Security=True";
        private SqlConnection connection;
        private SqlCommand command;
        private SqlDataAdapter adapter;
        private DataTable dataTable;

        public MenuStaff()
        {
            InitializeComponent();

            // Tambahkan event handler untuk button refresh dan saat form load
            this.btnRefresh.Click += new EventHandler(this.btnRefresh_Click);
            this.Load += new EventHandler(this.MenuStaff_Load);
        }

        private void MenuStaff_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        // Load data dari database ke DataGridView
        private void LoadData()
        {
            try
            {
                using (connection = new SqlConnection(connectionString))
                {
                    // Query untuk mengambil semua data dari tabel Menu
                    string query = "SELECT menu_id, nama, deskripsi, harga, kategori FROM Menu";
                    command = new SqlCommand(query, connection);
                    adapter = new SqlDataAdapter(command);
                    dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    // Set DataSource untuk dgvMenuStaff (sesuai nama di MenuStaff.Designer.cs)
                    dgvMenuStaff.DataSource = dataTable;

                    // Format kolom header dan sembunyikan kolom menu_id
                    dgvMenuStaff.Columns["menu_id"].Visible = false;
                    dgvMenuStaff.Columns["nama"].HeaderText = "Nama Menu";
                    dgvMenuStaff.Columns["deskripsi"].HeaderText = "Deskripsi";
                    dgvMenuStaff.Columns["harga"].HeaderText = "Harga";
                    dgvMenuStaff.Columns["kategori"].HeaderText = "Kategori";

                    // Format kolom harga sebagai mata uang
                    dgvMenuStaff.Columns["harga"].DefaultCellStyle.Format = "N2"; // "N2" untuk format angka dengan 2 desimal

                    // Mengatur agar kolom mengisi DataGridView
                    dgvMenuStaff.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Event handler untuk tombol Refresh
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData(); // Cukup panggil LoadData untuk memuat ulang data
        }

        // Event handler CellContentClick untuk dgvMenuStaff (sudah ada di designer, bisa dikosongkan jika tidak ada aksi khusus)
        private void dgvMenu_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Kosongkan jika tidak ada interaksi spesifik saat sel diklik
            // Jika Anda ingin mengimplementasikan sesuatu di sini, Anda bisa melakukannya.
            // Misalnya, menampilkan detail item dalam MessageBox, dll.
            // Untuk saat ini, sesuai permintaan, hanya display dan refresh.
        }
    }
}