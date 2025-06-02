using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project
{
    public partial class ReserverPelanggan : Form
    {
        private SqlConnection conn;
        private string connectionString = "Data Source=MIHALY\\FAIRUZ013;Initial Catalog=ReservasiRestoran;Integrated Security=True";

        public ReserverPelanggan()
        {
            InitializeComponent();
            conn = new SqlConnection(connectionString);

            // Add event handlers
            this.Load += ReserverPelanggan_Load;
            this.btnAdd.Click += BtnAdd_Click; // Assuming your button in ReserverPelanggan.Designer.cs is named btnAdd

            // Configure DateTimePicker for time
            dateTimePicker2.Format = DateTimePickerFormat.Time;
            dateTimePicker2.ShowUpDown = true;
        }

        private void ReserverPelanggan_Load(object sender, EventArgs e)
        {
            LoadPelanggan();
            LoadMeja();
            InitializeStatusComboBox();

            // Set default values for date and time pickers
            dateTimePicker1.Value = DateTime.Today;
            dateTimePicker2.Value = DateTime.Now;
        }
        private void InitializeStatusComboBox()
        {
            comboBox3.SelectedItem = "Pending";
        }

        private void LoadPelanggan()
        {
            using (SqlConnection sqlConn = new SqlConnection(connectionString)) // Use a local connection object
            {
                try
                {
                    sqlConn.Open();
                    string query = "SELECT pelanggan_id, nama FROM Pelanggan";
                    SqlCommand cmd = new SqlCommand(query, sqlConn);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    comboBox1.DataSource = dt;
                    comboBox1.DisplayMember = "nama";
                    comboBox1.ValueMember = "pelanggan_id";
                    comboBox1.SelectedIndex = -1; // No default selection
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading customers: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                // No finally block needed for close with 'using' statement
            }
        }

        private void LoadMeja()
        {
            using (SqlConnection sqlConn = new SqlConnection(connectionString)) // Use a local connection object
            {
                try
                {
                    sqlConn.Open();
                    // Only load tables that are currently available
                    string query = "SELECT meja_id, nomor_meja FROM Meja WHERE status_meja = 'Tersedia'";
                    SqlCommand cmd = new SqlCommand(query, sqlConn);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    comboBox2.DataSource = dt;
                    comboBox2.DisplayMember = "nomor_meja";
                    comboBox2.ValueMember = "meja_id";
                    comboBox2.SelectedIndex = -1; // No default selection
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading tables: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex == -1 || comboBox2.SelectedIndex == -1 || comboBox3.SelectedIndex == -1)
            {
                MessageBox.Show("Please select customer, table, and status.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                conn.Open();
                string query = @"INSERT INTO Reservasi (pelanggan_id, meja_id, tanggal, waktu, status_reservasi) 
                               VALUES (@pelanggan_id, @meja_id, @tanggal, @waktu, @status)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@pelanggan_id", comboBox1.SelectedValue);
                cmd.Parameters.AddWithValue("@meja_id", comboBox2.SelectedValue);
                cmd.Parameters.AddWithValue("@tanggal", dateTimePicker1.Value.Date);
                cmd.Parameters.AddWithValue("@waktu", dateTimePicker2.Value.TimeOfDay); // Get TimeOfDay
                cmd.Parameters.AddWithValue("@status", comboBox3.Text);

                cmd.ExecuteNonQuery();

                MessageBox.Show("Your reservation was successfully created! Please contact Admin if you want to see your reservation data or maybe change it", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Update table status if reservation is directly confirmed by the customer
                // This logic is similar to Reservasi.cs. If customers should only make "Pending" reservations,
                // then this "Confirmed" check might not be hit, or `comboBox3` should be restricted to "Pending".
                if (comboBox3.Text == "Confirmed")
                {
                    UpdateTableStatus((int)comboBox2.SelectedValue, "Tidak Tersedia");
                }

                ClearForm();
                LoadMeja(); // Refresh available tables as one might have been taken
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding reservation: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
        }

        private void UpdateTableStatus(int mejaId, string status)
        {
            // This connection should be managed locally or ensure the class-level 'conn' is handled carefully.
            // For safety, using a local connection here.
            using (SqlConnection updateConn = new SqlConnection(connectionString))
            {
                try
                {
                    updateConn.Open();
                    string query = "UPDATE Meja SET status_meja = @status WHERE meja_id = @id";
                    SqlCommand cmd = new SqlCommand(query, updateConn);
                    cmd.Parameters.AddWithValue("@status", status);
                    cmd.Parameters.AddWithValue("@id", mejaId);
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating table status: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearForm()
        {
            comboBox1.SelectedIndex = -1;
            comboBox2.SelectedIndex = -1;
            dateTimePicker1.Value = DateTime.Today;
            dateTimePicker2.Value = DateTime.Now;
            // comboBox3.SelectedIndex = 0; // Reset to "Pending" or first item
            if (comboBox3.Items.Count > 0)
            {
                comboBox3.SelectedItem = "Pending"; // Or whatever the desired default is.
            }
            else
            {
                comboBox3.SelectedIndex = -1;
            }
        }

        // This empty event handler was in your original ReserverPelanggan.cs, 
        // you can remove it if it's not used.
        private void label6_Click(object sender, EventArgs e)
        {
            // If this label has no specific click action, this can be removed.
        }
    }
}