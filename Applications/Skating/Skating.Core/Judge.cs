namespace Skating.Core
{
    /// <summary>Giudice della manifestazione.</summary>
    public sealed class Judge
    {
        public Guid Id { get; }

        public string Name { get; }

        public bool IsAbsent { get; }

        public Judge(Guid id, string name, bool isAbsent = false)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Identificativo obbligatorio.", nameof(id));
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Id = id;
            Name = name.Trim();
            IsAbsent = isAbsent;
        }
    }
}
