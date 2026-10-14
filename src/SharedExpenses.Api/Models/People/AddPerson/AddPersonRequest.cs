namespace SharedExpenses.Api.Models.People.AddPerson
{
    /// <param name="Name">Nombre de la persona.</param>
    /// <param name="Contribution">Dinero (CLP) con el que puede aportar a los gastos compartidos.</param>
    public record AddPersonRequest(
        string Name,
        long Contribution);
}
