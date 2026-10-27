namespace Skating.Core
{
    /// <summary>Disciplina configurabile, senza categorie o livelli incorporati.</summary>
    public sealed class Discipline
    {
        public Guid Id { get; }

        public string Name { get; }

        public Discipline(Guid id, string name)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("L'identificativo della disciplina è obbligatorio.", nameof(id));
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Id = id;
            Name = name.Trim();
        }
    }
}
