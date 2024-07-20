
using System;
using System.Data;
using System.IO;
using OfficeOpenXml;
using OfficeOpenXml.Style;

class Program
{
    static DataTable LerDadosERA5(string caminhoArquivo)
    {
        var dt = new DataTable();
        using (var package = new ExcelPackage(new FileInfo(caminhoArquivo)))
        {
            var worksheet = package.Workbook.Worksheets[0];
            foreach (var firstRowCell in worksheet.Cells[1, 1, 1, worksheet.Dimension.End.Column])
            {
                dt.Columns.Add(firstRowCell.Text);
            }

            for (var rowNumber = 2; rowNumber <= worksheet.Dimension.End.Row; rowNumber++)
            {
                var row = dt.NewRow();
                for (var colNumber = 1; colNumber <= worksheet.Dimension.End.Column; colNumber++)
                {
                    row[colNumber - 1] = worksheet.Cells[rowNumber, colNumber].Value?.ToString();
                }
                dt.Rows.Add(row);
            }
        }
        return dt;
    }

    static void ExportarParaCSV(DataTable dt, string arquivoSaida)
    {
        using (var writer = new StreamWriter(arquivoSaida))
        {
            foreach (DataColumn col in dt.Columns)
            {
                writer.Write(col.ColumnName + ",");
            }
            writer.WriteLine();
            foreach (DataRow row in dt.Rows)
            {
                for (var i = 0; i < dt.Columns.Count; i++)
                {
                    writer.Write(row[i].ToString().Replace(",", ";") + ",");
                }
                writer.WriteLine();
            }
        }
        Console.WriteLine($"CSV exportado para {arquivoSaida}");
    }

    static void ExportarParaExcel(DataTable dt, string arquivoSaida)
    {
        using (var package = new ExcelPackage())
        {
            var worksheet = package.Workbook.Worksheets.Add("Dados ERA5");
            worksheet.Cells["A1"].LoadFromDataTable(dt, true);

            // Estilizando células com valores negativos em u10
            for (var row = 2; row <= dt.Rows.Count + 1; row++)
            {
                var cellValue = Convert.ToDouble(worksheet.Cells[row, 2].Value); // Assuming u10 is in the second column
                if (cellValue < 0)
                {
                    worksheet.Cells[row, 2].Style.Font.Color.SetColor(System.Drawing.Color.Red);
                }
            }

            package.SaveAs(new FileInfo(arquivoSaida));
        }
        Console.WriteLine($"Excel exportado para {arquivoSaida}");
    }

    static DataTable AgruparDados(DataTable dt, string modo = "Diário")
    {
        
        return dt;
    }

    static void Main()
    {
        // Step 1: Ler dados ERA5 do arquivo Excel
        var arquivo = "january-era5.xlsx";
        var dadosERA5 = LerDadosERA5(arquivo);

        // Step 2: Exportar para CSV
        var csvSaida = "dados_era5.csv";
        ExportarParaCSV(dadosERA5, csvSaida);

        // Step 3: Exportar para Excel
        var excelSaida = "dados_era5_output.xlsx";
        ExportarParaExcel(dadosERA5, excelSaida);

        // Step 4: Agrupar dados
        var modo = "Diário"; // ou "Semanal"
        var dadosAgrupados = AgruparDados(dadosERA5, modo);

        Console.WriteLine("Operações concluídas.");
    }
}
