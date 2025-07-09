namespace Project
{
    partial class Dashboard
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.btnAdmin = new System.Windows.Forms.Button();
            this.btnStaff = new System.Windows.Forms.Button();
            this.btnPelanggan = new System.Windows.Forms.Button();
            this.btnReservasi = new System.Windows.Forms.Button();
            this.btnPembayaran = new System.Windows.Forms.Button();
            this.btnMeja = new System.Windows.Forms.Button();
            this.btnMenu = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(247, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(317, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Welcome Admin, which data you want to see or edit?";
            // 
            // btnAdmin
            // 
            this.btnAdmin.BackColor = System.Drawing.Color.PaleTurquoise;
            this.btnAdmin.Location = new System.Drawing.Point(107, 93);
            this.btnAdmin.Name = "btnAdmin";
            this.btnAdmin.Size = new System.Drawing.Size(128, 69);
            this.btnAdmin.TabIndex = 8;
            this.btnAdmin.Text = "ADMIN";
            this.btnAdmin.UseVisualStyleBackColor = false;
            this.btnAdmin.Click += new System.EventHandler(this.BtnAdmin_Click);
            // 
            // btnStaff
            // 
            this.btnStaff.BackColor = System.Drawing.Color.Wheat;
            this.btnStaff.Location = new System.Drawing.Point(339, 93);
            this.btnStaff.Name = "btnStaff";
            this.btnStaff.Size = new System.Drawing.Size(128, 69);
            this.btnStaff.TabIndex = 9;
            this.btnStaff.Text = "STAFF";
            this.btnStaff.UseVisualStyleBackColor = false;
            this.btnStaff.Click += new System.EventHandler(this.BtnStaff_Click);
            // 
            // btnPelanggan
            // 
            this.btnPelanggan.BackColor = System.Drawing.Color.LightGreen;
            this.btnPelanggan.Location = new System.Drawing.Point(572, 93);
            this.btnPelanggan.Name = "btnPelanggan";
            this.btnPelanggan.Size = new System.Drawing.Size(128, 69);
            this.btnPelanggan.TabIndex = 10;
            this.btnPelanggan.Text = "PELANGGAN";
            this.btnPelanggan.UseVisualStyleBackColor = false;
            this.btnPelanggan.Click += new System.EventHandler(this.BtnPelanggan_Click);
            // 
            // btnReservasi
            // 
            this.btnReservasi.BackColor = System.Drawing.Color.LightPink;
            this.btnReservasi.Location = new System.Drawing.Point(107, 348);
            this.btnReservasi.Name = "btnReservasi";
            this.btnReservasi.Size = new System.Drawing.Size(128, 69);
            this.btnReservasi.TabIndex = 11;
            this.btnReservasi.Text = "RESERVASI";
            this.btnReservasi.UseVisualStyleBackColor = false;
            this.btnReservasi.Click += new System.EventHandler(this.BtnReservasi_Click);
            // 
            // btnPembayaran
            // 
            this.btnPembayaran.BackColor = System.Drawing.Color.MediumPurple;
            this.btnPembayaran.Location = new System.Drawing.Point(339, 348);
            this.btnPembayaran.Name = "btnPembayaran";
            this.btnPembayaran.Size = new System.Drawing.Size(128, 69);
            this.btnPembayaran.TabIndex = 12;
            this.btnPembayaran.Text = "PEMBAYARAN";
            this.btnPembayaran.UseVisualStyleBackColor = false;
            this.btnPembayaran.Click += new System.EventHandler(this.BtnPembayaran_Click);
            // 
            // btnMeja
            // 
            this.btnMeja.BackColor = System.Drawing.Color.MediumSpringGreen;
            this.btnMeja.Location = new System.Drawing.Point(339, 219);
            this.btnMeja.Name = "btnMeja";
            this.btnMeja.Size = new System.Drawing.Size(128, 69);
            this.btnMeja.TabIndex = 13;
            this.btnMeja.Text = "MEJA";
            this.btnMeja.UseVisualStyleBackColor = false;
            this.btnMeja.Click += new System.EventHandler(this.BtnMeja_Click);
            // 
            // btnMenu
            // 
            this.btnMenu.BackColor = System.Drawing.Color.Salmon;
            this.btnMenu.Location = new System.Drawing.Point(572, 348);
            this.btnMenu.Name = "btnMenu";
            this.btnMenu.Size = new System.Drawing.Size(128, 69);
            this.btnMenu.TabIndex = 14;
            this.btnMenu.Text = "MENU";
            this.btnMenu.UseVisualStyleBackColor = false;
            this.btnMenu.Click += new System.EventHandler(this.BtnMenu_Click);
            // 
            // Dashboard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.btnMenu);
            this.Controls.Add(this.btnMeja);
            this.Controls.Add(this.btnPembayaran);
            this.Controls.Add(this.btnReservasi);
            this.Controls.Add(this.btnPelanggan);
            this.Controls.Add(this.btnStaff);
            this.Controls.Add(this.btnAdmin);
            this.Controls.Add(this.label1);
            this.Name = "Dashboard";
            this.Text = "Dashboard";
            this.Load += new System.EventHandler(this.Dashboard_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnAdmin;
        private System.Windows.Forms.Button btnStaff;
        private System.Windows.Forms.Button btnPelanggan;
        private System.Windows.Forms.Button btnReservasi;
        private System.Windows.Forms.Button btnPembayaran;
        private System.Windows.Forms.Button btnMeja;
        private System.Windows.Forms.Button btnMenu;
    }
}