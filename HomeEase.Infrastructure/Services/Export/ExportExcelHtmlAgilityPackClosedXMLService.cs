using ClosedXML.Excel;
using HomeEase.Application.Interfaces.Services;
using HtmlAgilityPack;

namespace HomeEase.Infrastructure.Services.Export
{
    public class ExportExcelHtmlAgilityPackClosedXMLService : IExportExcelService
    {
        public async Task<byte[]> Export(string htmlContent)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlContent);

            byte[] bytes;
            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.AddWorksheet("Data");
                int rowIdx = 1;
                foreach (HtmlNode table in doc.DocumentNode.SelectNodes("//table"))
                {
                    string styleTable = table.GetAttributeValue("style", "");
                    table.SetAttributeValue("style", styleTable);

                    Console.WriteLine("Found: " + table.Id);
                    foreach (HtmlNode row in table.SelectNodes("//tr"))
                    {
                        int colIdx = 1;
                        Console.WriteLine("row");
                        foreach (HtmlNode cell in row.SelectNodes("th|td"))
                        {
                            string style = row.GetAttributeValue("style", "");
                            // Example: Add or update a style property
                            row.SetAttributeValue("style", style);
                            ws.Cell(rowIdx, colIdx).Value = cell.InnerText.Trim();
                            colIdx++;
                            Console.WriteLine("cell: " + cell.InnerText);
                        }
                        rowIdx++;
                    }
                }

                using (var memoryStream = new MemoryStream())
                {
                    workbook.SaveAs(memoryStream);
                    memoryStream.Position = 0;
                    bytes = memoryStream.ToArray();
                }
            }
            return bytes;
        }
    }
}
