namespace SGEServer.Models
{
    public class ModalResultData
    {
        public bool Success { get; set; }
        public int Faturamento { get; set; }
        public int PrazoEntrega { get; set; }
        public string? Ncm { get; set; }
        public string? CaEpi { get; set; }
        public string? UnidadeMedidaDescricao { get; set; }
        public decimal ValorAbater {  get; set; }
        public decimal ValorRetirar { get; set; }
        public string? NomeResponsavel { get; set; }
        public string? MotivoRetirada { get; set; }
        public int QuantidadeEtiquetas { get; set; }
        public int QuantidadeUnitario { get; set; }
        public string SelectedPrinter { get; set; }
        public string Item { get; set; }
        public decimal ValorProduto { get; set; }
        public int CodProduto { get; set; }
        public string CodBarras { get; set; }
        public string NumOrdemCompra {  get; set; }
        public string NumCotacao { get; set; }
    }
}
