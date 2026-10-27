namespace Skating.Core
{
    /// <summary>Scuola rappresentata da una coreografia, non appartenenza esclusiva dell'atleta.</summary>
    public sealed class School
    {
        public Guid Id { get; }

        public string Name { get; }

        public School(Guid id, string name)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("L'identificativo della scuola è obbligatorio.", nameof(id));
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Id = id;
            Name = name.Trim();
        }
    }
}
