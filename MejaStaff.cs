using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Project
{
    public partial class MejaStaff : Form
    {
        // Database connection string (sesuaikan dengan milik Anda)
        private string connectionString = @"Data Source=MIHALY\FAIRUZ013;Initial Catalog=ReservasiRestoran;Integrated Security=True";
        private SqlConnection connection;
        private SqlCommand command;
        private SqlDataAdapter adapter;
        private DataTable dataTable;

        public MejaStaff()
        {
            InitializeComponent();
            // Tambahkan event handler untuk Tombol Refresh secara manual jika belum ada di Designer
            // this.btnRefresh.Click += new System.EventHandler(this.btnRefresh_Click);

            // Tambahkan event handler untuk Form Load secara manual jika belum ada di Designer
            // this.Load += new System.EventHandler(this.MejaStaff_Load);
        }

        private void MejaStaff_Load(object sender, EventArgs e)
        {
            LoadData();
        }

        // Load data from database to DataGridView
        private void LoadData()
        {
            try
            {
                using (connection = new SqlConnection(connectionString))
                {
                    string query = "SELECT meja_id, nomor_meja, kapasitas, status_meja FROM Meja"; // Query yang sama dengan Meja.cs
                    command = new SqlCommand(query, connection);
                    adapter = new SqlDataAdapter(command);
                    dataTable = new DataTable();
                    adapter.Fill(dataTable);
                    dgvMeja.DataSource = dataTable; // Menggunakan dgvMeja sesuai Designer

                    // Optional: Format column headers and hide meja_id column
                    if (dgvMeja.Columns["meja_id"] != null)
                        dgvMeja.Columns["meja_id"].Visible = false;
                    if (dgvMeja.Columns["nomor_meja"] != null)
                        dgvMeja.Columns["nomor_meja"].HeaderText = "Nomor Meja";
                    if (dgvMeja.Columns["kapasitas"] != null)
                        dgvMeja.Columns["kapasitas"].HeaderText = "Kapasitas";
                    if (dgvMeja.Columns["status_meja"] != null)
                        dgvMeja.Columns["status_meja"].HeaderText = "Status Meja";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Refresh the data in DataGridView
        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        // Event handler dari Designer, biarkan kosong jika tidak ada logika khusus saat sel diklik
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
           
        }

        private void MejaStaff_Load_1(object sender, EventArgs e)
        {

        }
    }
}