namespace Finance.Api.Application
{
    public sealed class FinanziamentoLockedException() : InvalidOperationException("Remove the existing alignments before changing contractual data.");
}
