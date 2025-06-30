namespace Votae.Models
{
    public class Candidato
    {
        public int Id { get; set; }
        public int IdEleicao { get; set; }
        public int Numero { get; set; }
        public string NomeCandidato { get; set; } = "";

        public string? FotoCandidatoBase64 { get; set; }

        public string? NomeVice { get; set; }
        public string? FotoViceBase64 { get; set; }
    }
}