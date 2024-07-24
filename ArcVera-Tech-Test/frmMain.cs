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
using OfficeOpenXml;
using OfficeOpenXml.Style;
using System.Drawing;
using ClosedXML.Excel;

namespace ArcVera_Tech_Test
{
    public partial class frmMain : Form
    {
        private ComboBox comboBoxViewType;
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

                    string viewType = comboBoxViewType.SelectedItem?.ToString() ?? "Daily";
                    PlotU10Values(dataTable, viewType);
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

        private void PlotU10Values(DataTable dataTable, string viewType)
        {
            var plotModel = new PlotModel { Title = viewType == "Daily" ? "Daily u10 Values" : "Weekly u10 Values" };
            var lineSeries = new LineSeries { Title = "u10" };

            var groupedData = dataTable.AsEnumerable()
                .GroupBy(row =>
                {
                    var date = DateTime.Parse(row["date"].ToString());
                    if (viewType == "Weekly")
                    {
                        // Group by week number and year
                        var firstDayOfWeek = date.AddDays(-(int)date.DayOfWeek);
                        return new DateTime(date.Year, 1, 1).AddDays((firstDayOfWeek - new DateTime(date.Year, 1, 1)).TotalDays / 7 * 7);
                    }
                    else
                    {
                        // Daily view
                        return date;
                    }
                })
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
            string inputExcelFilePath = "C:\\Users\\ASUS\\Downloads\\Excel.xlsx"; // Replace with actual input file path
            string outputExcelFilePath = "C:\\Users\\ASUS\\Downloads\\output.xlsx"; // Replace with desired output file path

            try
            {
                DataTable dataTable = ReadExcelFileClosedXML(inputExcelFilePath);
                WriteDataTableToExcelClosedXML(dataTable, outputExcelFilePath);

                MessageBox.Show("Excel file has been exported successfully.", "Export Excel", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while exporting the Excel file: " + ex.Message, "Export Excel", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private DataTable ReadExcelFileClosedXML(string excelFilePath)
        {
            DataTable dataTable = new DataTable();

            using (var workbook = new XLWorkbook(excelFilePath))
            {
                var worksheet = workbook.Worksheet(1); // Assuming data is in the first worksheet

                bool firstRow = true;
                foreach (var row in worksheet.RowsUsed())
                {
                    if (firstRow)
                    {
                        foreach (var cell in row.Cells())
                        {
                            dataTable.Columns.Add(cell.Value.ToString());
                        }
                        firstRow = false;
                    }
                    else
                    {
                        var dataRow = dataTable.NewRow();
                        int i = 0;
                        foreach (var cell in row.Cells())
                        {
                            dataRow[i++] = cell.Value.ToString();
                        }
                        dataTable.Rows.Add(dataRow);
                    }
                }
            }

            return dataTable;
        }

        private void WriteDataTableToExcelClosedXML(DataTable dataTable, string excelFilePath)
        {
            using (var workbook = new XLWorkbook())
            {
                var worksheet = workbook.Worksheets.Add("Sheet1");

                // Add column headers
                for (int col = 0; col < dataTable.Columns.Count; col++)
                {
                    worksheet.Cell(1, col + 1).Value = dataTable.Columns[col].ColumnName;
                }

                // Add rows and apply formatting
                for (int row = 0; row < dataTable.Rows.Count; row++)
                {
                    for (int col = 0; col < dataTable.Columns.Count; col++)
                    {
                        var cellValue = dataTable.Rows[row][col];
                        worksheet.Cell(row + 2, col + 1).Value = cellValue != DBNull.Value ? cellValue.ToString() : string.Empty;
                    }

                    // Check if the value in the 'u10' column is negative and color the row if so
                    if (dataTable.Columns.Contains("u10"))
                    {
                        var u10Value = dataTable.Rows[row]["u10"];
                        if (u10Value != DBNull.Value && Convert.ToDouble(u10Value) < 0)
                        {
                            var rowRange = worksheet.Range(row + 2, 1, row + 2, dataTable.Columns.Count);
                            rowRange.Style.Fill.BackgroundColor = XLColor.Red;
                        }
                    }
                }

                workbook.SaveAs(excelFilePath);
            }
        }

        private void comboBoxViewType_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }

}
