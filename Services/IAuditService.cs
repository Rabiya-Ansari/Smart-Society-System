using System.Threading.Tasks;

namespace SmartSociety.Services
{
    public interface IAuditService
    {
        Task LogAsync(string applicationUserId, string action, string entityName, string? entityId, string? details);
    }
}
