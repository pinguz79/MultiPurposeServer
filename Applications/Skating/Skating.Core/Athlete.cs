namespace Skating.Core
{
    /// <summary>Anagrafica personale, indipendente da scuola e partecipazione alle coreografie. Gli omonimi hanno identità distinte.</summary>
    public sealed class Athlete
    {
        public Guid Id { get; }

        public string FirstName { get; }

        public string LastName { get; }

        public Athlete(Guid id, string firstName, string lastName)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("L'identificativo dell'atleta è obbligatorio.", nameof(id));
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(firstName);
            ArgumentException.ThrowIfNullOrWhiteSpace(lastName);
            Id = id;
            FirstName = firstName.Trim();
            LastName = lastName.Trim();
        }
    }
}
