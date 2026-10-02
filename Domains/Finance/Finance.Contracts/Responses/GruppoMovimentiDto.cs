using Finance.DataModel.Models;

namespace Finance.Contracts.Responses
{
    public class GruppoMovimentiDto(GruppoMovimenti group)
    {
        public Guid Id { get; set; } = group.Id;
        public IReadOnlyList<MovimentoConfigurationDto> Items { get; set; } = group.Movimenti.OrderBy(item => item.Date).ThenBy(item => item.Id)
            .Select(item => new MovimentoConfigurationDto(item)).ToList();
    }
}
