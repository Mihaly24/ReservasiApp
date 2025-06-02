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
            if (this.btnAdd != null) // Ensure btnAdd is not null before adding handler
            {
                this.btnAdd.Click += BtnAdd_Click;
            }

            // Configure DateTimePicker for time
            if (this.dateTimePicker2 != null) // Ensure dateTimePicker2 is not null
            {
                dateTimePicker2.Format = DateTimePickerFormat.Time;
                dateTimePicker2.ShowUpDown = true;
            }
        }

        private void ReserverPelanggan_Load(object sender, EventArgs e)
        {
            LoadPelanggan();
            LoadMeja();
            InitializeStatusComboBox();

            // Set default values and constraints for date and time pickers
            if (this.dateTimePicker1 != null) // Ensure dateTimePicker1 is not null
            {
                dateTimePicker1.MinDate = DateTime.Today; // Set minimum date to today
                dateTimePicker1.MaxDate = DateTime.Today.AddMonths(3); // Set maximum date to 3 months from today
                dateTimePicker1.Value = DateTime.Today; // Set default value to today (will be adjusted if it's outside new min/max, but shouldn't be)
            }

            if (this.dateTimePicker2 != null) // Ensure dateTimePicker2 is not null
            {
                dateTimePicker2.Value = DateTime.Now;
            }
        }
        private void InitializeStatusComboBox()
        {
            if (this.comboBox3 != null)
            {
                comboBox3.SelectedItem = "Pending";
            }
        }

        private void LoadPelanggan()
        {
            if (this.comboBox1 == null) return; // Guard clause

            using (SqlConnection sqlConn = new SqlConnection(connectionString))
            {
                try
                {
                    sqlConn.Open();
                    string query = "SELECT pelanggan_id, nama FROM Pelanggan"; //
                    SqlCommand cmd = new SqlCommand(query, sqlConn);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    comboBox1.DataSource = dt; //
                    comboBox1.DisplayMember = "nama"; //
                    comboBox1.ValueMember = "pelanggan_id"; //
                    comboBox1.SelectedIndex = -1; // No default selection //
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading customers: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void LoadMeja()
        {
            if (this.comboBox2 == null) return; // Guard clause

            using (SqlConnection sqlConn = new SqlConnection(connectionString))
            {
                try
                {
                    sqlConn.Open();
                    // Only load tables that are currently available
                    string query = "SELECT meja_id, nomor_meja FROM Meja WHERE status_meja = 'Tersedia'"; //
                    SqlCommand cmd = new SqlCommand(query, sqlConn);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    comboBox2.DataSource = dt; //
                    comboBox2.DisplayMember = "nomor_meja"; //
                    comboBox2.ValueMember = "meja_id"; //
                    comboBox2.SelectedIndex = -1; // No default selection //
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading tables: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (this.comboBox1 == null || this.comboBox2 == null || this.comboBox3 == null ||
                this.dateTimePicker1 == null || this.dateTimePicker2 == null)
            {
                MessageBox.Show("A required control is missing. Please contact support.", "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (comboBox1.SelectedIndex == -1 || comboBox2.SelectedIndex == -1 || comboBox3.SelectedIndex == -1) //
            {
                MessageBox.Show("Please select customer, table, and status.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); //
                return;
            }

            // Additional validation for date to ensure it's within the allowed range (although DateTimePicker should enforce it)
            if (dateTimePicker1.Value < DateTime.Today || dateTimePicker1.Value > DateTime.Today.AddMonths(3))
            {
                MessageBox.Show($"Please select a date between {DateTime.Today:d} and {DateTime.Today.AddMonths(3):d}.", "Date Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (conn.State != ConnectionState.Open) // Ensure connection is open only when needed
                {
                    conn.Open();
                }
                string query = @"INSERT INTO Reservasi (pelanggan_id, meja_id, tanggal, waktu, status_reservasi) 
                               VALUES (@pelanggan_id, @meja_id, @tanggal, @waktu, @status)"; //

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@pelanggan_id", comboBox1.SelectedValue); //
                cmd.Parameters.AddWithValue("@meja_id", comboBox2.SelectedValue); //
                cmd.Parameters.AddWithValue("@tanggal", dateTimePicker1.Value.Date); //
                cmd.Parameters.AddWithValue("@waktu", dateTimePicker2.Value.TimeOfDay); //
                cmd.Parameters.AddWithValue("@status", comboBox3.Text); //

                cmd.ExecuteNonQuery(); //

                MessageBox.Show("Your reservation was successfully created! Please contact Admin if you want to see your reservation data or maybe change it", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); //

                if (comboBox3.Text == "Confirmed") //
                {
                    UpdateTableStatus((int)comboBox2.SelectedValue, "Tidak Tersedia"); //
                }

                ClearForm(); //
                LoadMeja(); // Refresh available tables as one might have been taken //
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding reservation: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); //
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close(); //
                }
            }
        }

        private void UpdateTableStatus(int mejaId, string status)
        {
            using (SqlConnection updateConn = new SqlConnection(connectionString))
            {
                try
                {
                    updateConn.Open();
                    string query = "UPDATE Meja SET status_meja = @status WHERE meja_id = @id"; //
                    SqlCommand cmd = new SqlCommand(query, updateConn);
                    cmd.Parameters.AddWithValue("@status", status); //
                    cmd.Parameters.AddWithValue("@id", mejaId); //
                    cmd.ExecuteNonQuery(); //
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating table status: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); //
                }
            }
        }

        private void ClearForm()
        {
            if (this.comboBox1 != null) comboBox1.SelectedIndex = -1; //
            if (this.comboBox2 != null) comboBox2.SelectedIndex = -1; //

            if (this.dateTimePicker1 != null)
            {
                // Reset to today, respecting MinDate and MaxDate.
                // If MinDate is in the future, DateTime.Today might be invalid.
                // However, MinDate is set to DateTime.Today, so this is fine.
                dateTimePicker1.Value = DateTime.Today; //
            }
            if (this.dateTimePicker2 != null) dateTimePicker2.Value = DateTime.Now; //

            if (this.comboBox3 != null) //
            {
                if (comboBox3.Items.Count > 0)
                {
                    comboBox3.SelectedItem = "Pending"; //
                }
                else
                {
                    comboBox3.SelectedIndex = -1; //
                }
            }
        }

        // This empty event handler was in your original ReserverPelanggan.cs, 
        // you can remove it if it's not used.
        // Based on ReserverPelanggan.Designer.cs, this event is tied to label6.
        private void label6_Click(object sender, EventArgs e)
        {
            // If this label has no specific click action, this can be removed.
        }
    }
}