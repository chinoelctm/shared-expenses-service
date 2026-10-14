using FluentValidation;
using SharedExpenses.Application.Commands.Categories;
using SharedExpenses.Application.Commands.Configuration.Update;
using SharedExpenses.Application.Commands.People;
using SharedExpenses.Application.DTOs;
using SharedExpenses.Domain.Entities;

namespace SharedExpenses.Application.Commands.Backup.Import
{
    public class ImportBackupCommandValidator : AbstractValidator<ImportBackupCommand>
    {
        public ImportBackupCommandValidator()
        {
            RuleFor(x => x.Backup)
                .NotNull().WithMessage("El respaldo es obligatorio.");

            When(x => x.Backup is not null, () =>
            {
                RuleFor(x => x.Backup.People)
                    .NotNull().WithMessage("El respaldo no tiene personas (puede ser una lista vacía).")
                    .Must(p => p.Select(x => x.Id).Distinct().Count() == p.Count)
                    .WithMessage("El respaldo tiene personas con Id repetido.")
                    .Must(p => p.Where(x => x.Enabled && x.Name is not null)
                                .GroupBy(x => Person.Normalize(x.Name))
                                .All(g => g.Count() == 1))
                    .WithMessage("El respaldo tiene dos personas activas con el mismo nombre.");

                RuleForEach(x => x.Backup.People).SetValidator(new BackupPersonDtoValidator());

                RuleFor(x => x.Backup.Settings)
                    .NotNull().WithMessage("El respaldo no tiene configuración.");

                RuleForEach(x => x.Backup.Settings.FixedCosts)
                    .SetValidator(new FixedCostDtoValidator())
                    .When(x => x.Backup.Settings?.FixedCosts is not null);

                RuleFor(x => x.Backup.Expenses)
                    .NotNull().WithMessage("El respaldo no tiene gastos (puede ser una lista vacía).")
                    .Must(e => e.Select(x => x.Id).Distinct().Count() == e.Count)
                    .WithMessage("El respaldo tiene gastos con Id repetido.");

                // Opcional: respaldos antiguos no traen categorías
                RuleFor(x => x.Backup.Categories!)
                    .Must(c => c.Select(x => x.Id).Distinct().Count() == c.Count)
                    .WithMessage("El respaldo tiene categorías con Id repetido.")
                    .Must(c => c.Where(x => x.Enabled && x.Name is not null)
                                .GroupBy(x => Category.Normalize(x.Name))
                                .All(g => g.Count() == 1))
                    .WithMessage("El respaldo tiene dos categorías activas con el mismo nombre.")
                    .When(x => x.Backup.Categories is not null);

                RuleForEach(x => x.Backup.Categories)
                    .SetValidator(new BackupCategoryDtoValidator())
                    .When(x => x.Backup.Categories is not null);
            });
        }
    }

    public class BackupCategoryDtoValidator : AbstractValidator<BackupCategoryDto>
    {
        public BackupCategoryDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("El Id de la categoría es obligatorio.");

            RuleFor(x => x.Name).MustBeValidCategoryName();
            RuleFor(x => x.Color).MustBeValidColor();
        }
    }

    public class BackupPersonDtoValidator : AbstractValidator<BackupPersonDto>
    {
        public BackupPersonDtoValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("El Id de la persona es obligatorio.");

            RuleFor(x => x.Name).MustBeValidPersonName();
            RuleFor(x => x.Contribution).MustBeValidContribution();
        }
    }
}
