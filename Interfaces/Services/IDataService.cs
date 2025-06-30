using Votae.Models;

namespace Votae.Interfaces.Services
{
    public interface IDataService
    {
        #region Eleicoes

        Task<List<Eleicao>> GetEleicoesAsync();

        Task<Eleicao?> GetEleicaoByIdAsync(int id);

        Task<int> SaveEleicaoAsync(Eleicao eleicao);

        Task DeleteEleicaoAsync(int id);

        #endregion Eleicoes

        #region Candidatos

        Task<List<Candidato>> GetCandidatosByEleicaoIdAsync(int eleicaoId);

        Task<int> SaveCandidatoAsync(Candidato candidato);

        Task DeleteCandidatoAsync(int candidatoId);

        #endregion Candidatos
    }
}