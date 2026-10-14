namespace SharedExpenses.Domain.Exceptions
{
    public class DomainException(string message) : Exception(message);
    public class NotFoundException(string message) : DomainException(message);
    public class ConflictException(string message) : DomainException(message);
}
