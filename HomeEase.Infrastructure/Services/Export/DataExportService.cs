using DinkToPdf;
using DinkToPdf.Contracts;
using HomeEase.Application.DTOs.Common;
using HomeEase.Application.Interfaces.Services;
using HomeEase.Domain.Enums;
using System.Text;

namespace HomeEase.Infrastructure.Services.Export
{
    public class DataExportService(
        IConverter _converter,
        Func<EnumExportExcelType, IExportExcelService> _exportExportExcelFactory) : IDataExportService
    {
        public async Task<EntityResult> ExportData<T>(ExportRequest<T> request) where T : class
        {
            switch (request.ExportFormat)
            {
                case EnumExportFormat.Excel:
                    return await ExportToExcel(request.Data, request.TemplatePath, request.ColumnMappings);
                case EnumExportFormat.CSV:
                    return ExportToCsv(request.Data, request.ColumnMappings);
                case EnumExportFormat.PDF:
                    var excelResult = await GeneratePdf(request);
                    if (!excelResult.Succeeded)
                        return excelResult;

                    var pdfBytes = (byte[])excelResult.Data;
                    return EntityResult.SuccessWithData(Convert.ToBase64String(pdfBytes));
                default:
                    return EntityResult.Failed("Unsupported export format");
            }
        }

        public async Task<EntityResult> GeneratePdf<T>(ExportRequest<T> request) where T : class
        {
            var htmlResult = await ExportToHTML(request.Data, request.TemplatePath, request.ColumnMappings);
            if (!htmlResult.Succeeded) return htmlResult;

            var globalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
            };

            var objectSettings = new ObjectSettings
            {
                HtmlContent = (string)htmlResult.Data,
                WebSettings = { DefaultEncoding = "utf-8" },
            };

            var pdf = new HtmlToPdfDocument()
            {
                GlobalSettings = globalSettings,
                Objects = { objectSettings }
            };

            return EntityResult.SuccessWithData(await Task.FromResult(_converter.Convert(pdf)));
        }
        private static async Task<EntityResult> ExportToHTML<T>(IReadOnlyCollection<T> data, string templatePath, Dictionary<string, Func<T, string>> columnMappings)
        {
            try
            {
                var pathIndex = Path.Combine(templatePath, "index.html");
                var pathRow = Path.Combine(templatePath, "row.html");

                var html = await File.ReadAllTextAsync(pathIndex, Encoding.UTF8);
                var rowTemplate = await File.ReadAllTextAsync(pathRow, Encoding.UTF8);

                var rows = new List<string>();
                foreach (var item in data)
                {
                    string row = rowTemplate;
                    foreach (var mapping in columnMappings)
                    {
                        row = row.Replace(mapping.Key, mapping.Value(item));
                    }
                    rows.Add(row);
                }
                html = html.Replace("{transactions}", string.Join("\n", rows.ToArray()));

                return EntityResult.SuccessWithData(html);
            }
            catch (Exception ex)
            {
                return EntityResult.Failed(ex.Message);
            }
        }

        private async Task<EntityResult> ExportToExcel<T>(IReadOnlyCollection<T> data, string templatePath,  Dictionary<string, Func<T, string>> columnMappings)
        {
            try
            {
                var htmlResult = await ExportToHTML(data, templatePath, columnMappings);
                if (!htmlResult.Succeeded) return htmlResult;

                var senderExport = _exportExportExcelFactory(EnumExportExcelType.AngleSharpClosedXML);
                byte[] byesConverts = await senderExport.Export((string)htmlResult.Data);

                return byesConverts is not null ? EntityResult.SuccessWithData(byesConverts) : EntityResult.Failed("no data");
            }
            catch (Exception ex)
            {
                return EntityResult.Failed(ex.Message);
            }
        }

        private static EntityResult ExportToCsv<T>(IReadOnlyCollection<T> data, Dictionary<string, Func<T, string>> columnMappings)
        {
            try
            {
                var rows = new List<string>() { "العنوان,تاريخ البدء,تاريخ الإنتهاء,مسؤل الرحلة,الحالة" };
                foreach (var item in data)
                {
                    var values = columnMappings.Values.Select(mapping => mapping(item));
                    rows.Add(string.Join(",", values));
                }

                return EntityResult.SuccessWithData(string.Join("\n", rows.ToArray()));
            }
            catch (Exception ex)
            {
                return EntityResult.Failed(ex.Message);
            }
        }
    }
}
