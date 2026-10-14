using MediatR;

namespace SharedExpenses.Application.Queries.Configuration.Get
{
    public record GetSettingsQuery : IRequest<GetSettingsResponse>;
}
