using Microsoft.Reporting.WinForms;
using System;
using System.Data;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;

namespace Project
{
    public partial class ReportAdmin : Form
    {
        public ReportAdmin()
        {
            InitializeComponent();
        }

        // --- Metode yang sudah ada dari file Anda ---
        private void ReportAdmin_Load(object sender, EventArgs e)
        {
            // Panggil method untuk setup dan memuat data ke ReportViewer
            SetupReportViewer();
            // Refresh report untuk menampilkan data
            this.reportViewer1.RefreshReport();
        }

        /// <summary>
        /// Mengatur koneksi, query, dan sumber data untuk ReportViewer.
        /// </summary>
        private void SetupReportViewer()
        {
            // Koneksi dan query dari file Anda
            string connectionString = "Data Source=MIHALY\\FAIRUZ013;Initial Catalog=ReservasiRestoran;Integrated Security=True";
            string query = "SELECT admin_id, nama, username, Passwords FROM AdminResto";
            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.Fill(dt);
            }

            ReportDataSource rds = new ReportDataSource("DataSetAdmin", dt);
            reportViewer1.LocalReport.DataSources.Clear();
            reportViewer1.LocalReport.DataSources.Add(rds);
            reportViewer1.LocalReport.ReportPath = @"C:\Users\LENOVO\Documents\PABD\ProjectPABD\Project\AdminReport.rdlc";
            reportViewer1.RefreshReport();
        }

        // --- FUNGSI EKSPOR BARU ---

        private void BtnExport_Click(object sender, EventArgs e)
        {
            // Buat menu konteks untuk pilihan format
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            ToolStripMenuItem pdfMenuItem = new ToolStripMenuItem("Export ke PDF");
            ToolStripMenuItem csvMenuItem = new ToolStripMenuItem("Export ke CSV");

            contextMenu.Items.Add(pdfMenuItem);
            contextMenu.Items.Add(csvMenuItem);

            // Tambahkan event handler untuk setiap pilihan menu
            pdfMenuItem.Click += (s, args) => ExportReport("PDF");
            csvMenuItem.Click += (s, args) => ExportReport("CSV");

            // Tampilkan menu di bawah tombol export
            contextMenu.Show(BtnExport, new System.Drawing.Point(0, BtnExport.Height));
        }

        /// <summary>
        /// Mengekspor laporan dari ReportViewer ke format yang ditentukan (PDF atau CSV).
        /// </summary>
        /// <param name="format">Format output ("PDF" atau "CSV").</param>
        private void ExportReport(string format)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                // Atur filter dan nama file default berdasarkan format
                if (format == "PDF")
                {
                    saveFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
                    saveFileDialog.FileName = $"LaporanAdmin_{DateTime.Now:yyyyMMdd}.pdf";
                }
                else if (format == "CSV")
                {
                    saveFileDialog.Filter = "CSV Files (*.csv)|*.csv";
                    saveFileDialog.FileName = $"LaporanAdmin_{DateTime.Now:yyyyMMdd}.csv";
                }

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Gunakan metode Render bawaan dari ReportViewer
                        byte[] bytes = reportViewer1.LocalReport.Render(
                            format,
                            null,
                            out string mimeType,
                            out string encoding,
                            out string fileNameExtension,
                            out string[] streams,
                            out Warning[] warnings
                        );

                        // Tulis hasil render ke file yang dipilih pengguna
                        using (FileStream fs = new FileStream(saveFileDialog.FileName, FileMode.Create))
                        {
                            fs.Write(bytes, 0, bytes.Length);
                        }

                        MessageBox.Show("Laporan berhasil diekspor!", "Sukses", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Terjadi kesalahan saat mengekspor laporan: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
    }
}