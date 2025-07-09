using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Project
{
    public partial class Login : Form
    {
        private string baseConnectionString = "Data Source=MIHALY\\FAIRUZ013;Initial Catalog=ReservasiRestoran;";

        public Login()
        {
            InitializeComponent();
        }

        private void Login_Load(object sender, EventArgs e)
        {
            // Initialization logic if needed
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            string username = txtUsername.Text.Trim();
            string password = txtPassword.Text;
            string userRole = null;

            try
            {
                string connectionString = baseConnectionString + $"User ID={username};Password={password};";

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    try
                    {
                        connection.Open();


                        string roleCheckQuery = @"
                            SELECT CASE
                                WHEN IS_MEMBER('AdminAppRole') = 1 THEN 'Admin'
                                WHEN IS_MEMBER('StaffAppRole') = 1 THEN 'Staff'
                                WHEN IS_MEMBER('PelangganAppRole') = 1 THEN 'Pelanggan'
                                ELSE NULL
                            END AS UserRole";

                        using (SqlCommand cmd = new SqlCommand(roleCheckQuery, connection))
                        {
                            object result = cmd.ExecuteScalar();
                            if (result != DBNull.Value && result != null)
                            {
                                userRole = result.ToString();
                            }
                        }

                        if (userRole != null)
                        {
                            MessageBox.Show($"Login berhasil sebagai {userRole}");
                            this.Hide();

                            if (userRole == "Admin")
                            {
                                Dashboard dashboard = new Dashboard();
                                dashboard.Show();
                            }
                            else if (userRole == "Staff")
                            {
                                DashStaff dashForm = new DashStaff();
                                dashForm.Show();
                            }
                            else if (userRole == "Pelanggan")
                            {
                                ReserverPelanggan reservepelForm = new ReserverPelanggan();
                                reservepelForm.Show();
                            }
                            else
                            {
                                MessageBox.Show("Login berhasil, tetapi peran tidak terdefinisi untuk pengarahan.");
                                // Application.Exit(); // Atau tindakan fallback
                            }
                        }
                        else
                        {
                            MessageBox.Show("Login berhasil, tetapi peran pengguna tidak terkonfigurasi di sistem. Hubungi administrator.");
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        if (sqlEx.Number == 18456) // Login failed for user
                        {
                            MessageBox.Show("Kredensial login salah.");
                        }
                        else
                        {
                            MessageBox.Show("Kesalahan database: " + sqlEx.Message);
                        }
                    }
                    // `using` akan otomatis menutup koneksi
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Terjadi kesalahan: " + ex.Message);
            }
        }
    }
}