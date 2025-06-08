using Microsoft.Reporting.WinForms;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Windows.Forms;

namespace Project
{
    public partial class ReportMenu: Form
    {
        public ReportMenu()
        {
            InitializeComponent();
        }

        private void ReportMenu_Load(object sender, EventArgs e)
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
            // GANTI dengan string koneksi database Anda jika berbeda
            string connectionString = "Data Source=MIHALY\\FAIRUZ013;Initial Catalog=ReservasiRestoran;Integrated Security=True";

            // Query untuk mengambil data dari tabel Menu
            string query = "SELECT menu_id, nama, deskripsi, harga, kategori FROM Menu";

            DataTable dt = new DataTable();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                SqlDataAdapter da = new SqlDataAdapter(query, conn);
                da.Fill(dt);
            }

            // Buat ReportDataSource. Pastikan "DataSetMenu" sesuai dengan nama
            // DataSet di dalam file .rdlc Anda.
            ReportDataSource rds = new ReportDataSource("DataSetMenu", dt);

            // Bersihkan data source yang ada dan tambahkan yang baru
            reportViewer1.LocalReport.DataSources.Clear();
            reportViewer1.LocalReport.DataSources.Add(rds);

            // GANTI dengan lokasi file ReportMenu.rdlc Anda
            reportViewer1.LocalReport.ReportPath = @"C:\Users\LENOVO\Documents\PABD\ProjectPABD\Project\ReportMenu.rdlc";

            // Refresh ReportViewer untuk menampilkan laporan
            reportViewer1.RefreshReport();
        }

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
            // DIUBAH: Gunakan format "Excel" untuk menghasilkan file CSV
            csvMenuItem.Click += (s, args) => ExportReport("Excel");

            // Tampilkan menu di bawah tombol export
            contextMenu.Show(BtnExport, new System.Drawing.Point(0, BtnExport.Height));
        }

        private void ExportReport(string format)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                // Logika di sini tidak perlu diubah, karena kita tetap ingin
                // ekstensi filenya .csv untuk kemudahan pengguna.
                if (format == "PDF")
                {
                    saveFileDialog.Filter = "PDF Files (*.pdf)|*.pdf";
                    saveFileDialog.FileName = $"LaporanMenu_{DateTime.Now:yyyyMMdd}.pdf";
                }
                else if (format == "Excel") // Kondisi diubah ke "Excel"
                {
                    saveFileDialog.Filter = "CSV (Comma delimited) (*.csv)|*.csv";
                    saveFileDialog.FileName = $"LaporanMenu_{DateTime.Now:yyyyMMdd}.csv";
                }

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Gunakan metode Render bawaan dari ReportViewer
                        byte[] bytes = reportViewer1.LocalReport.Render(
                            format, // 'format' sekarang akan berisi "PDF" atau "Excel"
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
