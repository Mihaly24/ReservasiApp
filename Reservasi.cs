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
    public partial class Reservasi : Form
    {
        private SqlConnection conn;
        private string connectionString = "Data Source=MIHALY\\FAIRUZ013;Initial Catalog=ReservasiRestoran;Integrated Security=True";
        private int selectedReservasiId = 0;

        public Reservasi()
        {
            InitializeComponent();
            conn = new SqlConnection(connectionString);

            // Initialize the form
            InitializeForm();

            // Add event handlers
            this.Load += Reservasi_Load;
            dgvReservasi.CellClick += DgvReservasi_CellClick;
        }

        private void Reservasi_Load(object sender, EventArgs e)
        {
            LoadPelanggan();
            LoadMeja();
            LoadReservasi();

            // Set default values
            dtpTanggal.Value = DateTime.Today;
            dtpWaktu.Value = DateTime.Now;
            cmbStatus.SelectedIndex = 0; // 'Pending'
        }

        private void InitializeForm()
        {
            // Setup DataGridView
            dgvReservasi.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvReservasi.ReadOnly = true;
            dgvReservasi.AllowUserToAddRows = false;
            dgvReservasi.MultiSelect = false;
        }

        private void LoadPelanggan()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT pelanggan_id, nama FROM Pelanggan";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    cmbPelangganID.DataSource = dt;
                    cmbPelangganID.DisplayMember = "nama";
                    cmbPelangganID.ValueMember = "pelanggan_id";
                    cmbPelangganID.SelectedIndex = -1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading customers: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                finally
                {
                    conn.Close();
                }
            }
        }

        private void LoadMeja()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT meja_id, nomor_meja FROM Meja WHERE status_meja = 'Tersedia'";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    cmbMejaID.DataSource = dt;
                    cmbMejaID.DisplayMember = "nomor_meja";
                    cmbMejaID.ValueMember = "meja_id";
                    cmbMejaID.SelectedIndex = -1;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading tables: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        private void LoadReservasi()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT r.reservasi_id, p.nama AS Pelanggan, m.nomor_meja AS Meja, 
                       r.tanggal, 
                       CONVERT(VARCHAR(5), r.waktu, 108) AS waktu_formatted, -- Convert TIME to string (HH:mm)
                       r.status_reservasi 
                       FROM Reservasi r
                       LEFT JOIN Pelanggan p ON r.pelanggan_id = p.pelanggan_id
                       LEFT JOIN Meja m ON r.meja_id = m.meja_id
                       ORDER BY r.tanggal DESC, r.waktu DESC";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);

                    dgvReservasi.DataSource = dt;

                    // Hide the original "waktu" column if it still exists
                    if (dgvReservasi.Columns["waktu"] != null)
                        dgvReservasi.Columns["waktu"].Visible = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading reservations: " + ex.Message);
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (cmbPelangganID.SelectedIndex == -1 || cmbMejaID.SelectedIndex == -1 || cmbStatus.SelectedIndex == -1)
            {
                MessageBox.Show("Please select customer, table, and status", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                conn.Open();
                string query = @"INSERT INTO Reservasi (pelanggan_id, meja_id, tanggal, waktu, status_reservasi) 
                               VALUES (@pelanggan_id, @meja_id, @tanggal, @waktu, @status)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@pelanggan_id", cmbPelangganID.SelectedValue);
                cmd.Parameters.AddWithValue("@meja_id", cmbMejaID.SelectedValue);
                cmd.Parameters.AddWithValue("@tanggal", dtpTanggal.Value.Date);
                cmd.Parameters.AddWithValue("@waktu", dtpWaktu.Value.TimeOfDay);
                cmd.Parameters.AddWithValue("@status", cmbStatus.Text);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Reservation added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Update table status if reservation is confirmed
                if (cmbStatus.Text == "Confirmed")
                {
                    UpdateTableStatus((int)cmbMejaID.SelectedValue, "Tidak Tersedia"); // ✅ Correct
                }

                ClearForm();
                LoadReservasi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding reservation: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedReservasiId == 0)
            {
                MessageBox.Show("Please select a reservation to update", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbPelangganID.SelectedIndex == -1 || cmbMejaID.SelectedIndex == -1 || cmbStatus.SelectedIndex == -1)
            {
                MessageBox.Show("Please select customer, table, and status", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                conn.Open();

                // Get the current meja_id before updating
                SqlCommand getCmd = new SqlCommand("SELECT meja_id, status_reservasi FROM Reservasi WHERE reservasi_id = @id", conn);
                getCmd.Parameters.AddWithValue("@id", selectedReservasiId);
                SqlDataReader reader = getCmd.ExecuteReader();

                int currentMejaId = 0;
                string currentStatus = string.Empty;
                if (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                        currentMejaId = reader.GetInt32(0);
                    currentStatus = reader.GetString(1);
                }
                reader.Close();

                // Update the reservation
                string query = @"UPDATE Reservasi 
                               SET pelanggan_id = @pelanggan_id, 
                                   meja_id = @meja_id, 
                                   tanggal = @tanggal, 
                                   waktu = @waktu, 
                                   status_reservasi = @status
                               WHERE reservasi_id = @id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@pelanggan_id", cmbPelangganID.SelectedValue);
                cmd.Parameters.AddWithValue("@meja_id", cmbMejaID.SelectedValue);
                cmd.Parameters.AddWithValue("@tanggal", dtpTanggal.Value.Date);
                cmd.Parameters.AddWithValue("@waktu", dtpWaktu.Value.TimeOfDay);
                cmd.Parameters.AddWithValue("@status", cmbStatus.Text);
                cmd.Parameters.AddWithValue("@id", selectedReservasiId);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Reservation updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Update table statuses if needed
                int newMejaId = (int)cmbMejaID.SelectedValue;
                string newStatus = cmbStatus.Text;

                // If table was changed, set the old table to Available
                if (currentMejaId > 0 && currentMejaId != newMejaId)
                {
                    UpdateTableStatus(currentMejaId, "Available");
                }

                // If status changed to Confirmed, set the new table to Reserved
                if (newStatus == "Confirmed")
                {
                    UpdateTableStatus(newMejaId, "Tidak Tersedia"); // ✅ Correct
                }
                else if (currentStatus == "Confirmed" && newStatus == "Pending")
                {
                    UpdateTableStatus(newMejaId, "Tersedia"); // ✅ Correct
                }

                ClearForm();
                LoadReservasi();
                selectedReservasiId = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating reservation: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedReservasiId == 0)
            {
                MessageBox.Show("Please select a reservation to delete", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to delete this reservation?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    conn.Open();

                    // Get the meja_id before deleting
                    SqlCommand getCmd = new SqlCommand("SELECT meja_id, status_reservasi FROM Reservasi WHERE reservasi_id = @id", conn);
                    getCmd.Parameters.AddWithValue("@id", selectedReservasiId);
                    SqlDataReader reader = getCmd.ExecuteReader();

                    int mejaId = 0;
                    string status = string.Empty;
                    if (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                            mejaId = reader.GetInt32(0);
                        status = reader.GetString(1);
                    }
                    reader.Close();

                    // Delete the reservation
                    string query = "DELETE FROM Reservasi WHERE reservasi_id = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", selectedReservasiId);
                    cmd.ExecuteNonQuery();

                    // If the reservation was confirmed, set the table back to Available
                    if (mejaId > 0 && status == "Confirmed")
                    {
                        UpdateTableStatus(mejaId, "Tersedia"); // ✅ Correct
                    }

                    MessageBox.Show("Reservation deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    LoadReservasi();
                    selectedReservasiId = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting reservation: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadPelanggan();
            LoadMeja();
            LoadReservasi();
            ClearForm();
            selectedReservasiId = 0;
        }

        private void DgvReservasi_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;

            try
            {
                DataGridViewRow row = dgvReservasi.Rows[e.RowIndex];
                selectedReservasiId = Convert.ToInt32(row.Cells["reservasi_id"].Value);

                using (SqlConnection conn = new SqlConnection(connectionString))
                {

                    conn.Open();
                    string query = "SELECT pelanggan_id, meja_id, tanggal, waktu, status_reservasi FROM Reservasi WHERE reservasi_id = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", selectedReservasiId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        // Handle pelanggan_id (may be NULL)
                        if (!reader.IsDBNull(0))
                            cmbPelangganID.SelectedValue = reader.GetInt32(0);
                        else
                            cmbPelangganID.SelectedIndex = -1;

                        // Handle meja_id (may be NULL)
                        if (!reader.IsDBNull(1))
                            cmbMejaID.SelectedValue = reader.GetInt32(1);
                        else
                            cmbMejaID.SelectedIndex = -1;

                        // Date and Time
                        dtpTanggal.Value = reader.GetDateTime(2);
                        TimeSpan time = reader.GetTimeSpan(3);
                        dtpWaktu.Value = DateTime.Today.Add(time);

                        // Status
                        cmbStatus.Text = reader.GetString(4);
                    }
                    reader.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message);
            }
        }

        private void UpdateTableStatus(int mejaId, string status)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                string query = "UPDATE Meja SET status_meja = @status WHERE meja_id = @id";
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@status", status);
                cmd.Parameters.AddWithValue("@id", mejaId);
                cmd.ExecuteNonQuery();
            }
        }

        private void ClearForm()
        {
            cmbPelangganID.SelectedIndex = -1;
            cmbMejaID.SelectedIndex = -1;
            dtpTanggal.Value = DateTime.Today;
            dtpWaktu.Value = DateTime.Now;
            cmbStatus.SelectedIndex = 0;
            selectedReservasiId = 0;
        }
    }
}