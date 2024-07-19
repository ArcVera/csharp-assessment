using Parquet.Schema;
using Parquet;
using System.Data;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using DataColumn = System.Data.DataColumn;
using OxyPlot.Axes;
using System.Formats.Asn1;
using CsvHelper;
using CsvHelper.Configuration;

namespace ArcVera_Tech_Test
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private async void btnImportEra5_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Parquet files (*.parquet)|*.parquet|All files (*.*)|*.*";
                openFileDialog.Title = "Select a Parquet File";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    DataTable dataTable = await ReadParquetFileAsync(filePath);
                    dgImportedEra5.DataSource = dataTable;
                    PlotU10DailyValues(dataTable);
                }
            }
        }

        private async Task<DataTable> ReadParquetFileAsync(string filePath)
        {
            using (Stream fileStream = File.OpenRead(filePath))
            {
                using (var parquetReader = await ParquetReader.CreateAsync(fileStream))
                {
                    DataTable dataTable = new DataTable();

                    for (int i = 0; i < parquetReader.RowGroupCount; i++)
                    {
                        using (ParquetRowGroupReader groupReader = parquetReader.OpenRowGroupReader(i))
                        {
                            // Create columns
                            foreach (DataField field in parquetReader.Schema.GetDataFields())
                            {
                                if (!dataTable.Columns.Contains(field.Name))
                                {
                                    Type columnType = field.HasNulls ? typeof(object) : field.ClrType;
                                    dataTable.Columns.Add(field.Name, columnType);
                                }

                                // Read values from Parquet column
                                DataColumn column = dataTable.Columns[field.Name];
                                Array values = (await groupReader.ReadColumnAsync(field)).Data;
                                for (int j = 0; j < values.Length; j++)
                                {
                                    if (dataTable.Rows.Count <= j)
                                    {
                                        dataTable.Rows.Add(dataTable.NewRow());
                                    }
                                    dataTable.Rows[j][field.Name] = values.GetValue(j);
                                }
                            }
                        }
                    }

                    return dataTable;
                }
            }
        }

        private void PlotU10DailyValues(DataTable dataTable)
        {
            var plotModel = new PlotModel { Title = "Daily u10 Values" };
            var lineSeries = new LineSeries { Title = "u10" };

            var groupedData = dataTable.AsEnumerable()
                .GroupBy(row => DateTime.Parse(row["date"].ToString()))
                .Select(g => new
                {
                    Date = g.Key,
                    U10Average = g.Average(row => Convert.ToDouble(row["u10"]))
                })
                .OrderBy(data => data.Date);

            foreach (var data in groupedData)
            {
                lineSeries.Points.Add(new DataPoint(DateTimeAxis.ToDouble(data.Date), data.U10Average));
            }

            plotModel.Series.Add(lineSeries);
            plotView1.Model = plotModel;
        }

        private async void btnExportCsv_Click(object sender, EventArgs e)
        {
            // Specify the paths
            string parquetFilePath = "path_to_your_parquet_file.parquet";
            string csvFilePath = "path_to_save_csv_file.csv";

            try
            {
                // Read the Parquet file
                DataTable dataTable = await ReadParquetFileAsyncs(parquetFilePath);

                // Write the data to CSV
                WriteDataTableToCsv(dataTable, csvFilePath);

                MessageBox.Show("CSV file has been exported successfully.", "Export CSV", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while exporting the CSV file: " + ex.Message, "Export CSV", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task<DataTable> ReadParquetFileAsyncs(string parquetFilePath)
        {
            DataTable dataTable = new DataTable();

            using (Stream fileStream = System.IO.File.OpenRead(parquetFilePath))
            {
                using (var parquetReader = await ParquetReader.CreateAsync(fileStream))
                {
                    DataField[] dataFields = parquetReader.Schema.GetDataFields();
                    foreach (var dataField in dataFields)
                    {
                        dataTable.Columns.Add(dataField.Name);
                    }

                    for (int i = 0; i < parquetReader.RowGroupCount; i++)
                    {
                        using (ParquetRowGroupReader rowGroupReader = parquetReader.OpenRowGroupReader(i))
                        {
                            List<object[]> rows = new List<object[]>();
                            foreach (DataField dataField in dataFields)
                            {
                                DataColumn column = dataTable.Columns[dataField.Name];
                                Parquet.Data.DataColumn result = await rowGroupReader.ReadColumnAsync(dataField);
                                Array data = result.Data;
                                for (int j = 0; j < data.Length; j++)
                                {
                                    if (rows.Count <= j)
                                    {
                                        rows.Add(new object[dataFields.Length]);
                                    }
                                    rows[j][Array.IndexOf(dataFields, dataField)] = data.GetValue(j);
                                }
                            }
                            foreach (var row in rows)
                            {
                                dataTable.Rows.Add(row);
                            }
                        }
                    }
                }
            }

            return dataTable;
        }

        private void WriteDataTableToCsv(DataTable dataTable, string csvFilePath)
        {
            using (var writer = new StreamWriter(csvFilePath))
            using (var csv = new CsvWriter(writer, new CsvConfiguration(System.Globalization.CultureInfo.InvariantCulture)))
            {
                // Write the column headers
                foreach (DataColumn column in dataTable.Columns)
                {
                    csv.WriteField(column.ColumnName);
                }
                csv.NextRecord();

                // Write the data
                foreach (DataRow row in dataTable.Rows)
                {
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        csv.WriteField(row[column]);
                    }
                    csv.NextRecord();
                }
            }
        }
            private void btnExportExcel_Click(object sender, EventArgs e)
        {
            // Complete here
        }
    }
}
