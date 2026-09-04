namespace Finance.Desktop.Models
{
    public interface ITemporalDefinition
    {
        DateOnly? ValidFrom { get; }
        DateOnly? ValidTo { get; }
    }
}
