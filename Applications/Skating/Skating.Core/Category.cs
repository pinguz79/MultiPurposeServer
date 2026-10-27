namespace Skating.Core
{
    /// <summary>Categoria condivisa fra tutte le discipline, senza vincoli anagrafici impliciti.</summary>
    public sealed class Category
    {
        public Guid Id { get; }

        public string Name { get; }

        public Category(Guid id, string name)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("L'identificativo della categoria è obbligatorio.", nameof(id));
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Id = id;
            Name = name.Trim();
        }
    }
}
