using HomeEase.Application.DTOs.Common;

namespace HomeEase.Application.Interfaces.Services
{
    public interface IDataExportService
    {
        Task<EntityResult> ExportData<T>(ExportRequest<T> request) where T : class;
        Task<EntityResult> GeneratePdf<T>(ExportRequest<T> request) where T : class;
    }
    public interface IExportExcelService
    {
        Task<byte[]> Export(string htmlContent);
    }
}
