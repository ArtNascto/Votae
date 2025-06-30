namespace Votae.Models
{
    public class Eleicao
    {
        public int Id { get; set; }
        public string Nome { get; set; } = "";
        public string Status { get; set; } = "Pendente";
        public DateTime? DataInicio { get; set; } = DateTime.Now;
        public DateTime? DataFim { get; set; } = null;
    }
}