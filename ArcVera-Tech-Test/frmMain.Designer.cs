using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CsvHelper;
using OfficeOpenXml;
using Parquet;
using Parquet.Data;

namespace ArcVera_Tech_Test
{
    public partial class frmMain : Form
    {
        private string filePath; // Stores the path of the selected Parquet file

        public frmMain()
        {
            InitializeComponent();
        }

        private void btnImportEra5_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Parquet Files (*.parquet)|*.parquet|All Files (*.*)|*.*";
            openFileDialog.Title = "Select a Parquet File";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                filePath = openFileDialog.FileName;
                DataTable dt = ReadParquetFile(filePath);
                dgImportedEra5.DataSource = dt; // Display data in DataGridView
            }
        }

        private DataTable ReadParquetFile(string filePath)
        {
            DataTable dataTable = new DataTable();

            using (Stream parquetStream = File.OpenRead(filePath))
            {
                using (var parquetReader = new ParquetReader(parquetStream))
                {
                    DataColumn[] columns = parquetReader.Schema
                        .Fields
                        .Select(field => new DataColumn(field.Name, field.ClrType))
                        .ToArray();

                    dataTable.Columns.AddRange(columns);

                    while (true)
                    {
                        try
                        {
                            DataRow row = dataTable.NewRow();
                            if (!parquetReader.Read())
                                break;

                            for (int i = 0; i < parquetReader.Schema.Fields.Count; i++)
                            {
                                row[i] = parquetReader.ReadColumn(i);
                            }

                            dataTable.Rows.Add(row);
                        }
                        catch (EndOfStreamException)
                        {
                            break;
                        }
                    }
                }
            }

            return dataTable;
        }

        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            if (dgImportedEra5.DataSource == null || dgImportedEra5.Rows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (StreamWriter streamWriter = new StreamWriter("exported_data.csv"))
            {
                using (CsvWriter csvWriter = new CsvWriter(streamWriter, System.Globalization.CultureInfo.InvariantCulture))
                {
                    csvWriter.WriteRecords((IEnumerable<object>)dgImportedEra5.DataSource);
                }
            }

            MessageBox.Show("Data exported to exported_data.csv", "Export Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            if (dgImportedEra5.DataSource == null || dgImportedEra5.Rows.Count == 0)
            {
                MessageBox.Show("No data to export.", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (ExcelPackage excelPackage = new ExcelPackage())
            {
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("Data");

                // Write header row
                for (int i = 0; i < dgImportedEra5.Columns.Count; i++)
                {
                    worksheet.Cells[1, i + 1].Value = dgImportedEra5.Columns[i].HeaderText;
                }

                // Write data rows
                for (int i = 0; i < dgImportedEra5.Rows.Count; i++)
                {
                    for (int j = 0; j < dgImportedEra5.Columns.Count; j++)
                    {
                        worksheet.Cells[i + 2, j + 1].Value = dgImportedEra5.Rows[i].Cells[j].Value?.ToString();
                    }
                }

                // Save Excel file
                excelPackage.SaveAs(new FileInfo("exported_data.xlsx"));
            }

            MessageBox.Show("Data exported to exported_data.xlsx", "Export Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void InitializeComponent()
        {
            this.btnImportEra5 = new System.Windows.Forms.Button();
            this.btnExportCsv = new System.Windows.Forms.Button();
            this.btnExportExcel = new System.Windows.Forms.Button();
            this.dgImportedEra5 = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.dgImportedEra5)).BeginInit();
            this.SuspendLayout();
            // 
            // btnImportEra5
            // 
            this.btnImportEra5.Location = new System.Drawing.Point(12, 12);
            this.btnImportEra5.Name = "btnImportEra5";
            this.btnImportEra5.Size = new System.Drawing.Size(125, 23);
            this.btnImportEra5.TabIndex = 0;
            this.btnImportEra5.Text = "Import ERA5 Data";
            this.btnImportEra5.UseVisualStyleBackColor = true;
            this.btnImportEra5.Click += new System.EventHandler(this.btnImportEra5_Click);
            // 
            // btnExportCsv
            // 
            this.btnExportCsv.Location = new System.Drawing.Point(143, 12);
            this.btnExportCsv.Name = "btnExportCsv";
            this.btnExportCsv.Size = new System.Drawing.Size(125, 23);
            this.btnExportCsv.TabIndex = 1;
            this.btnExportCsv.Text = "Export CSV";
            this.btnExportCsv.UseVisualStyleBackColor = true;
            this.btnExportCsv.Click += new System.EventHandler(this.btnExportCsv_Click);
            // 
            // btnExportExcel
            // 
            this.btnExportExcel.Location = new System.Drawing.Point(274, 12);
            this.btnExportExcel.Name = "btnExportExcel";
            this.btnExportExcel.Size = new System.Drawing.Size(125, 23);
            this.btnExportExcel.TabIndex = 2;
            this.btnExportExcel.Text = "Export Excel";
            this.btnExportExcel.UseVisualStyleBackColor = true;
            this.btnExportExcel.Click += new System.EventHandler(this.btnExportExcel_Click);
            // 
            // dgImportedEra5
            // 
            this.dgImportedEra5.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgImportedEra5.Location = new System.Drawing.Point(12, 50);
            this.dgImportedEra5.Name = "dgImportedEra5";
            this.dgImportedEra5.RowHeadersWidth = 51;
            this.dgImportedEra5.RowTemplate.Height = 24;
            this.dgImportedEra5.Size = new System.Drawing.Size(776, 388);
            this.dgImportedEra5.TabIndex = 3;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.dgImportedEra5);
            this.Controls.Add(this.btnExportExcel);
            this.Controls.Add(this.btnExportCsv);
            this.Controls.Add(this.btnImportEra5);
            this.Name = "frmMain";
            this.Text = "ERA5 Data Viewer";
            ((System.ComponentModel.ISupportInitialize)(this.dgImportedEra5)).EndInit();
            this.ResumeLayout(false);

        }

        private Button btnImportEra5;
        private Button btnExportCsv;
        private Button btnExportExcel;
        private DataGridView dgImportedEra5;
    }
}
