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
    public partial class Dashboard : Form
    {
        public Dashboard()
        {
            InitializeComponent();
        }

        private void Dashboard_Load(object sender, EventArgs e)
        {
             // Any initialization code when the dashboard loads
        }

        private void BtnAdmin_Click(object sender, EventArgs e)
        {
            // Open the Admin form
            Admin adminForm = new Admin();
            adminForm.Show();
        }

        private void BtnStaff_Click(object sender, EventArgs e)
        {
            // Open the StaffRestoran form
            StaffRestoran staffForm = new StaffRestoran();
            staffForm.Show();
        }

        private void BtnPelanggan_Click(object sender, EventArgs e)
        {
            // Open the Pelanggan form
            Pelanggan pelangganForm = new Pelanggan();
            pelangganForm.Show();
        }

        private void BtnReservasi_Click(object sender, EventArgs e)
        {
            // Open the Reservasi form
            Reservasi reservasiForm = new Reservasi();
            reservasiForm.Show();
        }

        private void BtnPembayaran_Click(object sender, EventArgs e)
        {
            // Open the Pembayaran form
            Pembayaran pembayaranForm = new Pembayaran();
            pembayaranForm.Show();
        }

        private void BtnMeja_Click(object sender, EventArgs e)
        {
            // Open the Meja form
            Meja mejaForm = new Meja();
            mejaForm.Show();
        }

        private void BtnMenu_Click(object sender, EventArgs e)
        {
            // Open the Menu form
            Menu menuForm = new Menu();
            menuForm.Show();
        }
    }
}