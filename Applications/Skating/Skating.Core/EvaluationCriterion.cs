namespace Skating.Core
{
    /// <summary>Criterio comune a tutte le gare della manifestazione.</summary>
    public sealed class EvaluationCriterion
    {
        public Guid Id { get; }

        public string Name { get; }

        public EvaluationCriterion(Guid id, string name)
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Identificativo obbligatorio.", nameof(id));
            }

            ArgumentException.ThrowIfNullOrWhiteSpace(name);
            Id = id;
            Name = name.Trim();
        }
    }
}
