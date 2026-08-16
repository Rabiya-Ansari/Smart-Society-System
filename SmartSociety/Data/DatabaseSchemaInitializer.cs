using Microsoft.EntityFrameworkCore;

namespace SmartSociety.Data
{
    public static class DatabaseSchemaInitializer
    {
        public static async Task EnsureComplaintColumnsAsync(AppDbContext db)
        {
            const string sql = @"
IF COL_LENGTH('Complaints', 'WorkNotes') IS NULL
    ALTER TABLE Complaints ADD WorkNotes nvarchar(1000) NULL;
IF COL_LENGTH('Complaints', 'SlaTargetDate') IS NULL
    ALTER TABLE Complaints ADD SlaTargetDate datetime2 NULL;";
            try
            {
                await db.Database.ExecuteSqlRawAsync(sql);
            }
            catch
            {
                // Existing databases may be unavailable during design-time tooling.
                // Runtime requests will surface the normal database error instead of hiding it.
            }
        }
    }
}
