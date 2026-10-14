using MediatR;
using SharedExpenses.Application.DTOs;

namespace SharedExpenses.Application.Queries.Backup.Export
{
    public record ExportBackupQuery : IRequest<BackupDto>;
}
