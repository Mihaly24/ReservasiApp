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
        private string connectionString = "Data Source=MIHALY\\FAIRUZ013;Initial Catalog=ReservasiRestoran;Integrated Security=True"; //
        private int selectedReservasiId = 0; //

        public Reservasi()
        {
            InitializeComponent();
            conn = new SqlConnection(connectionString); //

            // Initialize the form
            InitializeForm(); //

            // Add event handlers
            this.Load += Reservasi_Load; //
            if (dgvReservasi != null)
            {
                dgvReservasi.CellClick += DgvReservasi_CellClick; //
            }
        }

        private void Reservasi_Load(object sender, EventArgs e)
        {
            LoadPelanggan(); //
            LoadMeja(); //
            LoadReservasi(); //

            // Set default values and constraints for date and time pickers
            if (dtpTanggal != null)
            {
                dtpTanggal.MinDate = DateTime.Today; // Set minimum date to today
                dtpTanggal.MaxDate = DateTime.Today.AddMonths(3); // Set maximum date to 3 months from today
                dtpTanggal.Value = DateTime.Today; //
            }

            if (dtpWaktu != null)
            {
                dtpWaktu.Value = DateTime.Now; //
            }

            if (cmbStatus != null && cmbStatus.Items.Count > 0)
            {
                cmbStatus.SelectedIndex = 0; // 'Pending' //
            }
            else if (cmbStatus != null)
            {
                // Optional: Add items if they are not added in the designer
                // cmbStatus.Items.AddRange(new object[] { "Pending", "Confirmed", "Cancelled" });
                // cmbStatus.SelectedIndex = 0;
            }
        }

        private void InitializeForm()
        {
            if (dgvReservasi != null)
            {
                // Setup DataGridView
                dgvReservasi.SelectionMode = DataGridViewSelectionMode.FullRowSelect; //
                dgvReservasi.ReadOnly = true; //
                dgvReservasi.AllowUserToAddRows = false; //
                dgvReservasi.MultiSelect = false; //
            }
        }

        private void LoadPelanggan()
        {
            if (cmbPelangganID == null) return;

            using (SqlConnection localConn = new SqlConnection(connectionString)) //
            {
                try
                {
                    localConn.Open(); //
                    string query = "SELECT pelanggan_id, nama FROM Pelanggan"; //
                    SqlCommand cmd = new SqlCommand(query, localConn); //
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd); //
                    DataTable dt = new DataTable(); //
                    adapter.Fill(dt); //

                    cmbPelangganID.DataSource = dt; //
                    cmbPelangganID.DisplayMember = "nama"; //
                    cmbPelangganID.ValueMember = "pelanggan_id"; //
                    cmbPelangganID.SelectedIndex = -1; //
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading customers: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); //
                }
                // finally block with conn.Close() is not strictly needed due to 'using' statement.
            }
        }

        private void LoadMeja()
        {
            if (cmbMejaID == null) return;

            using (SqlConnection localConn = new SqlConnection(connectionString)) //
            {
                try
                {
                    localConn.Open(); //
                    string query = "SELECT meja_id, nomor_meja FROM Meja WHERE status_meja = 'Tersedia'"; //
                    SqlCommand cmd = new SqlCommand(query, localConn); //
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd); //
                    DataTable dt = new DataTable(); //
                    adapter.Fill(dt); //

                    cmbMejaID.DataSource = dt; //
                    cmbMejaID.DisplayMember = "nomor_meja"; //
                    cmbMejaID.ValueMember = "meja_id"; //
                    cmbMejaID.SelectedIndex = -1; //
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading tables: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); //
                }
            }
        }

        private void LoadReservasi()
        {
            if (dgvReservasi == null) return;

            using (SqlConnection localConn = new SqlConnection(connectionString)) //
            {
                try
                {
                    localConn.Open(); //
                    string query = @"SELECT r.reservasi_id, p.nama AS Pelanggan, m.nomor_meja AS Meja, 
                       r.tanggal, 
                       CONVERT(VARCHAR(5), r.waktu, 108) AS waktu_formatted, -- Convert TIME to string (HH:mm)
                       r.status_reservasi 
                       FROM Reservasi r
                       LEFT JOIN Pelanggan p ON r.pelanggan_id = p.pelanggan_id
                       LEFT JOIN Meja m ON r.meja_id = m.meja_id
                       ORDER BY r.tanggal DESC, r.waktu DESC"; //
                    SqlCommand cmd = new SqlCommand(query, localConn); //
                    SqlDataAdapter adapter = new SqlDataAdapter(cmd); //
                    DataTable dt = new DataTable(); //
                    adapter.Fill(dt); //

                    dgvReservasi.DataSource = dt; //

                    // Hide the original "waktu" column if it still exists
                    if (dgvReservasi.Columns.Contains("waktu") && dgvReservasi.Columns["waktu"] != null) //
                        dgvReservasi.Columns["waktu"].Visible = false; //
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading reservations: " + ex.Message); //
                }
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (cmbPelangganID == null || cmbMejaID == null || cmbStatus == null || dtpTanggal == null || dtpWaktu == null) return;

            if (cmbPelangganID.SelectedIndex == -1 || cmbMejaID.SelectedIndex == -1 || cmbStatus.SelectedIndex == -1) //
            {
                MessageBox.Show("Please select customer, table, and status", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); //
                return; //
            }

            // Validate date
            if (dtpTanggal.Value < DateTime.Today || dtpTanggal.Value > DateTime.Today.AddMonths(3))
            {
                MessageBox.Show($"Please select a date between {DateTime.Today:d} and {DateTime.Today.AddMonths(3):d}.", "Date Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                if (conn.State != ConnectionState.Open) conn.Open(); //
                string query = @"INSERT INTO Reservasi (pelanggan_id, meja_id, tanggal, waktu, status_reservasi) 
                               VALUES (@pelanggan_id, @meja_id, @tanggal, @waktu, @status)"; //

                SqlCommand cmd = new SqlCommand(query, conn); //
                cmd.Parameters.AddWithValue("@pelanggan_id", cmbPelangganID.SelectedValue); //
                cmd.Parameters.AddWithValue("@meja_id", cmbMejaID.SelectedValue); //
                cmd.Parameters.AddWithValue("@tanggal", dtpTanggal.Value.Date); //
                cmd.Parameters.AddWithValue("@waktu", dtpWaktu.Value.TimeOfDay); //
                cmd.Parameters.AddWithValue("@status", cmbStatus.Text); //

                cmd.ExecuteNonQuery(); //
                MessageBox.Show("Reservation added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); //

                if (cmbStatus.Text == "Confirmed") //
                {
                    UpdateTableStatus((int)cmbMejaID.SelectedValue, "Tidak Tersedia"); //
                }

                ClearForm(); //
                LoadReservasi(); //
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding reservation: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); //
            }
            finally
            {
                if (conn.State == ConnectionState.Open) conn.Close(); //
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (cmbPelangganID == null || cmbMejaID == null || cmbStatus == null || dtpTanggal == null || dtpWaktu == null) return;

            if (selectedReservasiId == 0) //
            {
                MessageBox.Show("Please select a reservation to update", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); //
                return; //
            }

            if (cmbPelangganID.SelectedIndex == -1 || cmbMejaID.SelectedIndex == -1 || cmbStatus.SelectedIndex == -1) //
            {
                MessageBox.Show("Please select customer, table, and status", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); //
                return; //
            }

            // Validate date
            if (dtpTanggal.Value < dtpTanggal.MinDate || dtpTanggal.Value > dtpTanggal.MaxDate) // Check against actual MinDate/MaxDate
            {
                MessageBox.Show($"Please select a date between {dtpTanggal.MinDate:d} and {dtpTanggal.MaxDate:d}.", "Date Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            try
            {
                if (conn.State != ConnectionState.Open) conn.Open(); //

                SqlCommand getCmd = new SqlCommand("SELECT meja_id, status_reservasi FROM Reservasi WHERE reservasi_id = @id", conn); //
                getCmd.Parameters.AddWithValue("@id", selectedReservasiId); //
                SqlDataReader reader = getCmd.ExecuteReader(); //

                int currentMejaId = 0; //
                string currentStatus = string.Empty; //
                if (reader.Read()) //
                {
                    if (!reader.IsDBNull(0)) //
                        currentMejaId = reader.GetInt32(0); //
                    currentStatus = reader.GetString(1); //
                }
                reader.Close(); //

                string query = @"UPDATE Reservasi 
                               SET pelanggan_id = @pelanggan_id, 
                                   meja_id = @meja_id, 
                                   tanggal = @tanggal, 
                                   waktu = @waktu, 
                                   status_reservasi = @status
                               WHERE reservasi_id = @id"; //

                SqlCommand cmd = new SqlCommand(query, conn); //
                cmd.Parameters.AddWithValue("@pelanggan_id", cmbPelangganID.SelectedValue); //
                cmd.Parameters.AddWithValue("@meja_id", cmbMejaID.SelectedValue); //
                cmd.Parameters.AddWithValue("@tanggal", dtpTanggal.Value.Date); //
                cmd.Parameters.AddWithValue("@waktu", dtpWaktu.Value.TimeOfDay); //
                cmd.Parameters.AddWithValue("@status", cmbStatus.Text); //
                cmd.Parameters.AddWithValue("@id", selectedReservasiId); //

                cmd.ExecuteNonQuery(); //
                MessageBox.Show("Reservation updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); //

                int newMejaId = (int)cmbMejaID.SelectedValue; //
                string newStatus = cmbStatus.Text; //

                if (currentMejaId > 0 && currentMejaId != newMejaId) //
                {
                    UpdateTableStatus(currentMejaId, "Tersedia"); // Changed "Available" to "Tersedia" for consistency
                }

                if (newStatus == "Confirmed") //
                {
                    UpdateTableStatus(newMejaId, "Tidak Tersedia"); //
                }
                else if (currentStatus == "Confirmed" && (newStatus == "Pending" || newStatus == "Cancelled")) // Consider "Cancelled" too
                {
                    UpdateTableStatus(newMejaId, "Tersedia"); //
                }

                ClearForm(); //
                LoadReservasi(); //
                selectedReservasiId = 0; //
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating reservation: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); //
            }
            finally
            {
                if (conn.State == ConnectionState.Open) conn.Close(); //
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedReservasiId == 0) //
            {
                MessageBox.Show("Please select a reservation to delete", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning); //
                return; //
            }

            DialogResult result = MessageBox.Show("Are you sure you want to delete this reservation?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question); //
            if (result == DialogResult.Yes) //
            {
                try
                {
                    if (conn.State != ConnectionState.Open) conn.Open(); //

                    SqlCommand getCmd = new SqlCommand("SELECT meja_id, status_reservasi FROM Reservasi WHERE reservasi_id = @id", conn); //
                    getCmd.Parameters.AddWithValue("@id", selectedReservasiId); //
                    SqlDataReader reader = getCmd.ExecuteReader(); //

                    int mejaId = 0; //
                    string status = string.Empty; //
                    if (reader.Read()) //
                    {
                        if (!reader.IsDBNull(0)) //
                            mejaId = reader.GetInt32(0); //
                        status = reader.GetString(1); //
                    }
                    reader.Close(); //

                    string query = "DELETE FROM Reservasi WHERE reservasi_id = @id"; //
                    SqlCommand cmd = new SqlCommand(query, conn); //
                    cmd.Parameters.AddWithValue("@id", selectedReservasiId); //
                    cmd.ExecuteNonQuery(); //

                    if (mejaId > 0 && status == "Confirmed") //
                    {
                        UpdateTableStatus(mejaId, "Tersedia"); //
                    }

                    MessageBox.Show("Reservation deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information); //
                    ClearForm(); //
                    LoadReservasi(); //
                    selectedReservasiId = 0; //
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting reservation: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error); //
                }
                finally
                {
                    if (conn.State == ConnectionState.Open) conn.Close(); //
                }
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadPelanggan(); //
            LoadMeja(); //
            LoadReservasi(); //
            ClearForm(); //
            selectedReservasiId = 0; //
        }

        private void DgvReservasi_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; //
            if (dgvReservasi == null || cmbPelangganID == null || cmbMejaID == null || dtpTanggal == null || dtpWaktu == null || cmbStatus == null) return;


            try
            {
                DataGridViewRow row = dgvReservasi.Rows[e.RowIndex]; //
                selectedReservasiId = Convert.ToInt32(row.Cells["reservasi_id"].Value); //

                using (SqlConnection localConn = new SqlConnection(connectionString)) //
                {
                    localConn.Open(); //
                    string query = "SELECT pelanggan_id, meja_id, tanggal, waktu, status_reservasi FROM Reservasi WHERE reservasi_id = @id"; //
                    SqlCommand cmd = new SqlCommand(query, localConn); //
                    cmd.Parameters.AddWithValue("@id", selectedReservasiId); //
                    SqlDataReader reader = cmd.ExecuteReader(); //

                    if (reader.Read()) //
                    {
                        if (!reader.IsDBNull(0)) //
                            cmbPelangganID.SelectedValue = reader.GetInt32(0); //
                        else
                            cmbPelangganID.SelectedIndex = -1; //

                        if (!reader.IsDBNull(1)) //
                        {
                            // Temporarily load all tables to find the selected one if it's not 'Tersedia'
                            LoadAllMejaForSelection(reader.GetInt32(1));
                            cmbMejaID.SelectedValue = reader.GetInt32(1); //
                        }
                        else
                            cmbMejaID.SelectedIndex = -1; //

                        DateTime reservationDate = reader.GetDateTime(2); //
                        // Check if the reservationDate is within the current Min/Max range of dtpTanggal
                        if (reservationDate >= dtpTanggal.MinDate && reservationDate <= dtpTanggal.MaxDate)
                        {
                            dtpTanggal.Value = reservationDate;
                        }
                        else
                        {
                            // If out of range, you might want to set it to MinDate or MaxDate,
                            // or inform the user. For now, let DateTimePicker clamp it.
                            // Or, temporarily adjust Min/Max to show the actual date.
                            // For simplicity here, we just assign. DateTimePicker will clamp.
                            dtpTanggal.Value = reservationDate;
                            // Optionally, inform user:
                            // MessageBox.Show($"The selected reservation date ({reservationDate:d}) is outside the editable range. It will be clamped if you try to modify other fields and save.", "Date Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }


                        TimeSpan time = reader.GetTimeSpan(3); //
                        dtpWaktu.Value = DateTime.Today.Add(time); //

                        cmbStatus.Text = reader.GetString(4); //
                    }
                    reader.Close(); //
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading data: " + ex.Message); //
            }
        }

        private void LoadAllMejaForSelection(int selectedMejaId)
        {
            if (cmbMejaID == null) return;
            DataTable currentTableSource = cmbMejaID.DataSource as DataTable;

            bool found = false;
            if (currentTableSource != null)
            {
                foreach (DataRow row in currentTableSource.Rows)
                {
                    if ((int)row["meja_id"] == selectedMejaId)
                    {
                        found = true;
                        break;
                    }
                }
            }

            if (!found) // If the selectedMejaId is not in the current 'Tersedia' list
            {
                using (SqlConnection localConn = new SqlConnection(connectionString))
                {
                    try
                    {
                        localConn.Open();
                        // Load ALL tables, or at least the selected one plus all 'Tersedia'
                        string query = $"SELECT meja_id, nomor_meja FROM Meja WHERE status_meja = 'Tersedia' OR meja_id = {selectedMejaId}";
                        SqlDataAdapter adapter = new SqlDataAdapter(query, localConn);
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        cmbMejaID.DataSource = dt; // Temporarily set to this broader list
                        // DisplayMember and ValueMember should already be set
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error loading specific table for selection: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }


        private void UpdateTableStatus(int mejaId, string status)
        {
            using (SqlConnection localConn = new SqlConnection(connectionString)) //
            {
                try
                {
                    localConn.Open(); //
                    string query = "UPDATE Meja SET status_meja = @status WHERE meja_id = @id"; //
                    SqlCommand cmd = new SqlCommand(query, localConn); //
                    cmd.Parameters.AddWithValue("@status", status); //
                    cmd.Parameters.AddWithValue("@id", mejaId); //
                    cmd.ExecuteNonQuery(); //
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error updating table status: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void ClearForm()
        {
            if (cmbPelangganID != null) cmbPelangganID.SelectedIndex = -1; //
            if (cmbMejaID != null)
            {
                // Reload only available tables after clearing
                LoadMeja(); // This ensures cmbMejaID only shows 'Tersedia' tables
                cmbMejaID.SelectedIndex = -1; //
            }
            if (dtpTanggal != null) dtpTanggal.Value = DateTime.Today; //
            if (dtpWaktu != null) dtpWaktu.Value = DateTime.Now; //
            if (cmbStatus != null && cmbStatus.Items.Count > 0) cmbStatus.SelectedIndex = 0; //
            selectedReservasiId = 0; //
        }
    }
}