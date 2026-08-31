using Finance.DataModel.Models;

namespace Finance.Contracts.Responses
{
    public sealed record CategoriaReferenceDto(string Name, string DisplayName)
    {
        public CategoriaReferenceDto(Categoria categoria) : this(categoria.Name, categoria.DisplayName)
        {
        }
    }
}
