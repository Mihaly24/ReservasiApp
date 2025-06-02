using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project
{
    public partial class DashStaff : Form
    {
        public DashStaff()
        {
            InitializeComponent();
        }

        private void DashStaff_Load(object sender, EventArgs e)
        {
            // Kode inisialisasi saat form DashStaff dimuat, jika ada.
        }

        private void BtnRes_Click(object sender, EventArgs e)
        {
            // Membuka form Reservasi
            ReserveStaff reserveForm = new ReserveStaff();
            reserveForm.Show();
        }

        private void BtnMenu_Click(object sender, EventArgs e)
        {
            // Membuka form Menu
            MenuStaff menustaffForm = new MenuStaff();
            menustaffForm.Show();
        }

        private void BtnMeja_Click(object sender, EventArgs e)
        {
            // Membuka form Meja
            MejaStaff mejastaffForm = new MejaStaff();
            mejastaffForm.Show();
        }
    }
}