using AngleSharp;
using ClosedXML.Excel;
using HomeEase.Application.Interfaces.Services;
using System.Drawing;

namespace HomeEase.Infrastructure.Services.Export
{
    public class ExportExcelAngleSharpClosedXMLService : IExportExcelService
    {
        public async Task<byte[]> Export(string htmlContent)
        {
            var config = Configuration.Default;
            var context = BrowsingContext.New(config);
            var document = await context.OpenAsync(req => req.Content(htmlContent));

            var table = document.QuerySelector("table");
            if (table == null)
                return null;

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Sheet1");

            int currentRow = 1;
            foreach (var row in table.QuerySelectorAll("tr"))
            {
                int currentCol = 1;
                foreach (var cell in row.Children)
                {
                    var excelCell = worksheet.Cell(currentRow, currentCol);
                    excelCell.Value = cell.TextContent.Trim();

                    var style = cell.GetAttribute("style");
                    ApplyHtmlStyleToExcel(excelCell, style, cell.NodeName);

                    excelCell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                    currentCol++;
                }
                currentRow++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream.ToArray();
        }
        private void ApplyHtmlStyleToExcel(IXLCell excelCell, string style, string nodeName)
        {
            if (string.IsNullOrWhiteSpace(style))
            {
                if (nodeName == "TH")
                {
                    excelCell.Style.Font.Bold = true;
                    excelCell.Style.Fill.BackgroundColor = XLColor.LightGray;
                }
                return;
            }

            var rules = style.Split(';', StringSplitOptions.RemoveEmptyEntries);

            foreach (var rule in rules)
            {
                var parts = rule.Split(':');
                if (parts.Length != 2) continue;

                var property = parts[0].Trim().ToLower();
                var value = parts[1].Trim().ToLower();

                switch (property)
                {
                    case "font-weight":
                        if (value == "bold" || value == "700")
                            excelCell.Style.Font.Bold = true;
                        break;

                    case "text-align":
                        if (value == "center")
                            excelCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                        else if (value == "right")
                            excelCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                        else
                            excelCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                        break;

                    case "vertical-align":
                        if (value == "middle")
                            excelCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        else if (value == "bottom")
                            excelCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Bottom;
                        else
                            excelCell.Style.Alignment.Vertical = XLAlignmentVerticalValues.Top;
                        break;

                    case "background-color":
                        if (TryConvertColor(value, out var bgColor))
                            excelCell.Style.Fill.BackgroundColor = XLColor.FromColor(bgColor);
                        break;

                    case "color":
                        if (TryConvertColor(value, out var fontColor))
                            excelCell.Style.Font.FontColor = XLColor.FromColor(fontColor);
                        break;
                }
            }
        }
        private bool TryConvertColor(string value, out Color color)
        {
            try
            {
                color = ColorTranslator.FromHtml(value);
                return true;
            }
            catch
            {
                color = Color.Black;
                return false;
            }
        }
    }
}
