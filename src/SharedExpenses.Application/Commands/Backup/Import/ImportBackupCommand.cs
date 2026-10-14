using MediatR;
using SharedExpenses.Application.DTOs;

namespace SharedExpenses.Application.Commands.Backup.Import
{
    /// <summary>Reemplaza toda la configuración y todos los gastos por el contenido del respaldo.</summary>
    public record ImportBackupCommand(BackupDto Backup) : IRequest;
}
