namespace Skating.Core
{
    /// <summary>Identità della manifestazione, indipendente dall'interfaccia e dalla persistenza.</summary>
    public sealed class Manifestation
    {
        public Guid Id { get; }

        public string Name { get; }

        public Manifestation(Guid id, string name)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("L'identificativo della manifestazione è obbligatorio.", nameof(id));
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Id = id;
            Name = name.Trim();
        }
    }
}
