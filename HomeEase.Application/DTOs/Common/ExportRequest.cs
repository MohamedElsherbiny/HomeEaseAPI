using HomeEase.Domain.Enums;

namespace HomeEase.Application.DTOs.Common
{
    public class ExportRequest<T> where T : class
    {
        public IReadOnlyCollection<T> Data { get; set; }
        public string TemplatePath { get; set; }
        public EnumExportFormat ExportFormat { get; set; }
        public Dictionary<string, Func<T, string>> ColumnMappings { get; set; }
    }
}
