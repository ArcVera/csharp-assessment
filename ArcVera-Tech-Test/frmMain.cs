using Parquet.Schema;
using Parquet;
using System.Data;
using OxyPlot;
using OxyPlot.Series;
using OxyPlot.WindowsForms;
using DataColumn = System.Data.DataColumn;
using OxyPlot.Axes;
using ClosedXML.Excel;
using System.Text;

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
                    bool weekly = cbViewType.SelectedItem?.ToString() == "Weekly";
                    PlotU10Values(dataTable, weekly);
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
                                    Type columnType = field.IsNullable ? typeof(object) : field.ClrType;
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

        private void PlotU10Values(DataTable dataTable, bool weekly)
        {
            var plotModel = new PlotModel { Title = weekly ? "Weekly u10 Values" : "Daily u10 Values" };
            var lineSeries = new LineSeries { Title = "u10" };

            var groupedData = dataTable.AsEnumerable()
                .Where(row => row["date"] != null && row["u10"] != null) // Verificação de nulidade
                .GroupBy(row => weekly ? GetWeekStartDate(DateTime.Parse(row["date"].ToString())) : DateTime.Parse(row["date"].ToString()))
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

        private DateTime GetWeekStartDate(DateTime date)
        {
            int diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
            return date.AddDays(-1 * diff).Date;
        }

        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                saveFileDialog.Title = "Save as CSV";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    StringBuilder csvContent = new StringBuilder();
                    foreach (DataColumn column in ((DataTable)dgImportedEra5.DataSource).Columns)
                    {
                        csvContent.Append(column.ColumnName + ",");
                    }
                    csvContent.AppendLine();

                    foreach (DataRow row in ((DataTable)dgImportedEra5.DataSource).Rows)
                    {
                        foreach (DataColumn column in ((DataTable)dgImportedEra5.DataSource).Columns)
                        {
                            csvContent.Append(row[column.ColumnName].ToString() + ",");
                        }
                        csvContent.AppendLine();
                    }

                    File.WriteAllText(saveFileDialog.FileName, csvContent.ToString());
                }
            }
        }

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*";
                saveFileDialog.Title = "Save as Excel";

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    using (var workbook = new XLWorkbook())
                    {
                        var worksheet = workbook.Worksheets.Add("Imported Data");

                        DataTable dataTable = (DataTable)dgImportedEra5.DataSource;

                        // Add column names
                        for (int i = 0; i < dataTable.Columns.Count; i++)
                        {
                            worksheet.Cell(1, i + 1).Value = dataTable.Columns[i].ColumnName;
                        }

                        // Add rows
                        for (int i = 0; i < dataTable.Rows.Count; i++)
                        {
                            for (int j = 0; j < dataTable.Columns.Count; j++)
                            {
                                var cell = worksheet.Cell(i + 2, j + 1);
                                cell.Value = dataTable.Rows[i][j]?.ToString() ?? ""; // Cast explícito para string

                                if (dataTable.Columns[j].ColumnName == "u10" && double.TryParse(dataTable.Rows[i][j]?.ToString(), out double u10Value) && u10Value < 0)
                                {
                                    cell.Style.Font.FontColor = XLColor.Red;
                                }
                            }
                        }

                        workbook.SaveAs(saveFileDialog.FileName);
                    }
                }
            }
        }
    }
}
