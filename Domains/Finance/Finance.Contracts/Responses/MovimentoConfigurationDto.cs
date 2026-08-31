using Finance.DataModel.Models;

namespace Finance.Contracts.Responses
{
    public class MovimentoConfigurationDto(Movimento movimento)
    {
        public Guid Id { get; set; } = movimento.Id;
        public DateOnly Date { get; set; } = movimento.Date;
        public string Description { get; set; } = movimento.Description;
        public string Formula { get; set; } = movimento.Formula;
        public string ContoName { get; set; } = movimento.Conto.Name;
        public CategoriaReferenceDto? Category { get; set; } = movimento.Categoria is null ? null : new CategoriaReferenceDto(movimento.Categoria);
    }
}
