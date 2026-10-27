namespace Skating.Core
{
    /// <summary>Livello condiviso; il percorso PRO è esplicito e non dipende dalla denominazione.</summary>
    public sealed class Level
    {
        public Guid Id { get; }

        public string Name { get; }

        public bool IsPro { get; }

        public Level(Guid id, string name, bool isPro)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("L'identificativo del livello è obbligatorio.", nameof(id));
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Id = id;
            Name = name.Trim();
            IsPro = isPro;
        }
    }
}
