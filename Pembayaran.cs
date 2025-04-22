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
    public partial class Pembayaran : Form
    {
        private SqlConnection conn;
        private string connectionString = "Data Source=MIHALY\\FAIRUZ013;Initial Catalog=ReservasiRestoran;Integrated Security=True";
        private int selectedPembayaranId = 0;

        public Pembayaran()
        {
            InitializeComponent();
            conn = new SqlConnection(connectionString);

            // Initialize the form
            InitializeForm();

            // Add event handlers
            this.Load += Pembayaran_Load;
            dgvPembayaran.CellClick += DgvPembayaran_CellClick;
            cmbReservasiID.SelectedIndexChanged += CmbReservasiID_SelectedIndexChanged;
            txtJumlah.KeyPress += TxtJumlah_KeyPress;
        }

        private void Pembayaran_Load(object sender, EventArgs e)
        {
            LoadReservasi();
            LoadPembayaran();

            // Set default values
            cmbMetode.SelectedIndex = 0; // 'Cash'
            cmbStatus.SelectedIndex = 0; // 'Pending'
        }

        private void InitializeForm()
        {
            // Setup DataGridView
            dgvPembayaran.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvPembayaran.ReadOnly = true;
            dgvPembayaran.AllowUserToAddRows = false;
            dgvPembayaran.MultiSelect = false;
        }

        private void LoadReservasi()
        {
            try
            {
                conn.Open();
                // Load all reservations without any filtering
                string query = @"SELECT r.reservasi_id, 
                                CONCAT(r.reservasi_id, ' - ', p.nama, ' (', FORMAT(r.tanggal, 'dd/MM/yyyy'), ') - ', r.status_reservasi) AS info,
                                (SELECT TOP 1 pb.status_pembayaran FROM Pembayaran pb WHERE pb.reservasi_id = r.reservasi_id) AS payment_status
                               FROM Reservasi r
                               LEFT JOIN Pelanggan p ON r.pelanggan_id = p.pelanggan_id
                               ORDER BY r.tanggal DESC, r.waktu DESC";

                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                // Add a payment status indicator to the displayed text if a payment exists
                foreach (DataRow row in dt.Rows)
                {
                    if (row["payment_status"] != DBNull.Value)
                    {
                        string paymentStatus = row["payment_status"].ToString();
                        row["info"] = row["info"] + " [" + paymentStatus + "]";
                    }
                }

                cmbReservasiID.DataSource = dt;
                cmbReservasiID.DisplayMember = "info";
                cmbReservasiID.ValueMember = "reservasi_id";
                cmbReservasiID.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading reservations: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }
        }

        private void LoadPembayaran()
        {
            try
            {
                conn.Open();
                string query = @"SELECT pb.pembayaran_id, 
                               CONCAT(r.reservasi_id, ' - ', p.nama, ' (', FORMAT(r.tanggal, 'dd/MM/yyyy'), ') - ', r.status_reservasi) AS Reservasi,
                               pb.jumlah, pb.metode, pb.status_pembayaran
                               FROM Pembayaran pb
                               LEFT JOIN Reservasi r ON pb.reservasi_id = r.reservasi_id
                               LEFT JOIN Pelanggan p ON r.pelanggan_id = p.pelanggan_id
                               ORDER BY pb.pembayaran_id DESC";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                adapter.Fill(dt);

                dgvPembayaran.DataSource = dt;

                // Format the amount column
                if (dgvPembayaran.Columns["jumlah"] != null)
                    dgvPembayaran.Columns["jumlah"].DefaultCellStyle.Format = "N2";
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading payments: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }
        }

        private void CmbReservasiID_SelectedIndexChanged(object sender, EventArgs e)
        {
            // If a reservation is selected, try to get any associated order total
            if (cmbReservasiID.SelectedIndex != -1)
            {
                try
                {
                    conn.Open();

                    // First check if there's already a payment for this reservation
                    string checkQuery = @"SELECT jumlah, status_pembayaran 
                                        FROM Pembayaran 
                                        WHERE reservasi_id = @reservasi_id";
                    SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                    checkCmd.Parameters.AddWithValue("@reservasi_id", cmbReservasiID.SelectedValue);
                    SqlDataReader reader = checkCmd.ExecuteReader();

                    if (reader.Read())
                    {
                        // If there's already a payment, use its amount
                        decimal amount = reader.GetDecimal(0);
                        string status = reader.GetString(1);
                        txtJumlah.Text = amount.ToString("N2");

                        // Find and set the status
                        for (int i = 0; i < cmbStatus.Items.Count; i++)
                        {
                            if (string.Equals(cmbStatus.Items[i].ToString(), status, StringComparison.OrdinalIgnoreCase))
                            {
                                cmbStatus.SelectedIndex = i;
                                break;
                            }
                        }

                        reader.Close();
                    }
                    else
                    {
                        // No existing payment, check for order total
                        reader.Close();

                        // This is a placeholder - replace with actual logic to calculate the amount from orders
                        string query = @"SELECT ISNULL(SUM(harga), 0) as total
                                       FROM Pesanan
                                       WHERE reservasi_id = @reservasi_id";
                        SqlCommand cmd = new SqlCommand(query, conn);
                        cmd.Parameters.AddWithValue("@reservasi_id", cmbReservasiID.SelectedValue);

                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            decimal total = Convert.ToDecimal(result);
                            txtJumlah.Text = total.ToString("N2");
                        }
                        else
                        {
                            txtJumlah.Text = "0.00";
                        }

                        // Reset status to pending for new payments
                        cmbStatus.SelectedIndex = 0;
                    }
                }
                catch (Exception ex)
                {
                    // Just ignore errors in getting the total - user can enter manually
                    txtJumlah.Text = "0.00";
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        private void TxtJumlah_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Allow only digits, decimal point, and control characters
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar) && e.KeyChar != '.')
            {
                e.Handled = true;
            }

            // Only allow one decimal point
            if (e.KeyChar == '.' && (sender as TextBox).Text.IndexOf('.') > -1)
            {
                e.Handled = true;
            }
        }

        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (cmbReservasiID.SelectedIndex == -1 || string.IsNullOrWhiteSpace(txtJumlah.Text) ||
                cmbMetode.SelectedIndex == -1 || cmbStatus.SelectedIndex == -1)
            {
                MessageBox.Show("Please fill in all fields", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtJumlah.Text, out decimal jumlah))
            {
                MessageBox.Show("Please enter a valid amount", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                conn.Open();
                // Check if payment for this reservation already exists
                string checkQuery = "SELECT COUNT(*) FROM Pembayaran WHERE reservasi_id = @reservasi_id";
                SqlCommand checkCmd = new SqlCommand(checkQuery, conn);
                checkCmd.Parameters.AddWithValue("@reservasi_id", cmbReservasiID.SelectedValue);
                int count = (int)checkCmd.ExecuteScalar();

                if (count > 0)
                {
                    DialogResult result = MessageBox.Show(
                        "A payment for this reservation already exists. Would you like to update the existing payment instead?",
                        "Duplicate Payment", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (result == DialogResult.Yes)
                    {
                        // Get the existing payment ID
                        string getIdQuery = "SELECT pembayaran_id FROM Pembayaran WHERE reservasi_id = @reservasi_id";
                        SqlCommand getIdCmd = new SqlCommand(getIdQuery, conn);
                        getIdCmd.Parameters.AddWithValue("@reservasi_id", cmbReservasiID.SelectedValue);
                        selectedPembayaranId = (int)getIdCmd.ExecuteScalar();

                        // Update instead of add
                        string updateQuery = @"UPDATE Pembayaran 
                                            SET jumlah = @jumlah, 
                                                metode = @metode, 
                                                status_pembayaran = @status
                                            WHERE pembayaran_id = @id";

                        SqlCommand updateCmd = new SqlCommand(updateQuery, conn);
                        updateCmd.Parameters.AddWithValue("@jumlah", jumlah);
                        updateCmd.Parameters.AddWithValue("@metode", cmbMetode.Text.ToLower());
                        updateCmd.Parameters.AddWithValue("@status", cmbStatus.Text.ToLower());
                        updateCmd.Parameters.AddWithValue("@id", selectedPembayaranId);

                        updateCmd.ExecuteNonQuery();
                        MessageBox.Show("Payment updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    conn.Close();
                    ClearForm();
                    LoadPembayaran();
                    LoadReservasi();
                    return;
                }

                string query = @"INSERT INTO Pembayaran (reservasi_id, jumlah, metode, status_pembayaran) 
                               VALUES (@reservasi_id, @jumlah, @metode, @status)";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@reservasi_id", cmbReservasiID.SelectedValue);
                cmd.Parameters.AddWithValue("@jumlah", jumlah);
                cmd.Parameters.AddWithValue("@metode", cmbMetode.Text.ToLower());
                cmd.Parameters.AddWithValue("@status", cmbStatus.Text.ToLower());

                cmd.ExecuteNonQuery();
                MessageBox.Show("Payment added successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Update reservation status if payment is completed
                if (cmbStatus.Text.ToLower() == "lunas")
                {
                    UpdateReservationAfterPayment((int)cmbReservasiID.SelectedValue);
                }

                ClearForm();
                LoadPembayaran();
                LoadReservasi();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error adding payment: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            if (selectedPembayaranId == 0)
            {
                MessageBox.Show("Please select a payment to update", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (cmbReservasiID.SelectedIndex == -1 || string.IsNullOrWhiteSpace(txtJumlah.Text) ||
                cmbMetode.SelectedIndex == -1 || cmbStatus.SelectedIndex == -1)
            {
                MessageBox.Show("Please fill in all fields", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!decimal.TryParse(txtJumlah.Text, out decimal jumlah))
            {
                MessageBox.Show("Please enter a valid amount", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                conn.Open();

                // Get the current status and reservation ID before updating
                string getQuery = "SELECT status_pembayaran, reservasi_id FROM Pembayaran WHERE pembayaran_id = @id";
                SqlCommand getCmd = new SqlCommand(getQuery, conn);
                getCmd.Parameters.AddWithValue("@id", selectedPembayaranId);
                SqlDataReader reader = getCmd.ExecuteReader();

                string currentStatus = string.Empty;
                int reservasiId = 0;
                if (reader.Read())
                {
                    currentStatus = reader.GetString(0);
                    reservasiId = reader.GetInt32(1);
                }
                reader.Close();

                // Update the payment
                string query = @"UPDATE Pembayaran 
                               SET reservasi_id = @reservasi_id, 
                                   jumlah = @jumlah, 
                                   metode = @metode, 
                                   status_pembayaran = @status
                               WHERE pembayaran_id = @id";

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.Parameters.AddWithValue("@reservasi_id", cmbReservasiID.SelectedValue);
                cmd.Parameters.AddWithValue("@jumlah", jumlah);
                cmd.Parameters.AddWithValue("@metode", cmbMetode.Text.ToLower());
                cmd.Parameters.AddWithValue("@status", cmbStatus.Text.ToLower());
                cmd.Parameters.AddWithValue("@id", selectedPembayaranId);

                cmd.ExecuteNonQuery();
                MessageBox.Show("Payment updated successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // If payment status changed to "lunas", update the reservation 
                if (cmbStatus.Text.ToLower() == "lunas" && currentStatus != "lunas")
                {
                    UpdateReservationAfterPayment((int)cmbReservasiID.SelectedValue);
                }

                ClearForm();
                LoadPembayaran();
                LoadReservasi();
                selectedPembayaranId = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error updating payment: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                conn.Close();
            }
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            if (selectedPembayaranId == 0)
            {
                MessageBox.Show("Please select a payment to delete", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            DialogResult result = MessageBox.Show("Are you sure you want to delete this payment?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                try
                {
                    conn.Open();
                    string query = "DELETE FROM Pembayaran WHERE pembayaran_id = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", selectedPembayaranId);
                    cmd.ExecuteNonQuery();

                    MessageBox.Show("Payment deleted successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ClearForm();
                    LoadPembayaran();
                    LoadReservasi();
                    selectedPembayaranId = 0;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error deleting payment: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadReservasi();
            LoadPembayaran();
            ClearForm();
            selectedPembayaranId = 0;
        }

        private void DgvPembayaran_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvPembayaran.Rows[e.RowIndex];
                selectedPembayaranId = Convert.ToInt32(row.Cells["pembayaran_id"].Value);

                // Load the payment details for editing
                try
                {
                    conn.Open();
                    string query = "SELECT reservasi_id, jumlah, metode, status_pembayaran FROM Pembayaran WHERE pembayaran_id = @id";
                    SqlCommand cmd = new SqlCommand(query, conn);
                    cmd.Parameters.AddWithValue("@id", selectedPembayaranId);
                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.Read())
                    {
                        int reservasiId = reader.GetInt32(0);

                        // Try to find the reservation in the ComboBox
                        if (cmbReservasiID.Items.Count > 0)
                        {
                            DataTable dt = (DataTable)cmbReservasiID.DataSource;
                            DataRow[] foundRows = dt.Select($"reservasi_id = {reservasiId}");

                            if (foundRows.Length > 0)
                            {
                                // Found it, set the selected item
                                cmbReservasiID.SelectedValue = reservasiId;
                            }
                        }

                        txtJumlah.Text = reader.GetDecimal(1).ToString("N2");

                        string metode = reader.GetString(2);
                        // Find metode in ComboBox (case insensitive)
                        for (int i = 0; i < cmbMetode.Items.Count; i++)
                        {
                            if (string.Equals(cmbMetode.Items[i].ToString(), metode, StringComparison.OrdinalIgnoreCase))
                            {
                                cmbMetode.SelectedIndex = i;
                                break;
                            }
                        }

                        string status = reader.GetString(3);
                        // Find status in ComboBox (case insensitive)
                        for (int i = 0; i < cmbStatus.Items.Count; i++)
                        {
                            if (string.Equals(cmbStatus.Items[i].ToString(), status, StringComparison.OrdinalIgnoreCase))
                            {
                                cmbStatus.SelectedIndex = i;
                                break;
                            }
                        }
                    }
                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading payment details: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    conn.Close();
                }
            }
        }

        private void UpdateReservationAfterPayment(int reservasiId)
        {
            // This function would update any relevant reservation details after a payment is completed
            // For example, you might want to update the reservation status or related tables

            // The implementation depends on your business logic. Here's a simple example:
            /*
            string updateQuery = "UPDATE Reservasi SET status_reservasi = 'Paid' WHERE reservasi_id = @id";
            SqlCommand updateCmd = new SqlCommand(updateQuery, conn);
            updateCmd.Parameters.AddWithValue("@id", reservasiId);
            updateCmd.ExecuteNonQuery();
            */

            // Note: This is commented out as it's just an example. Implement according to your needs.
        }

        private void ClearForm()
        {
            cmbReservasiID.SelectedIndex = -1;
            txtJumlah.Text = "";
            cmbMetode.SelectedIndex = 0;
            cmbStatus.SelectedIndex = 0;
            selectedPembayaranId = 0;
        }
    }
}