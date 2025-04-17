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
    public partial class Form1: Form
    {
        private string connectionString = "Data Source=MIHALY\\FAIRUZ013;Initial Catalog=ReservasiRestoran;Integrated Security=True";
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            
        }

        private void ClearForm()
        {
            txtNama.Clear();
            txtEmail.Clear();
            txtNoTelp.Clear();
            txtAlamat.Clear();
            txtNama.Focus();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
    }
}
