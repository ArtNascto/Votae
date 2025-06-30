using Microsoft.Data.Sqlite;
using Votae.Interfaces.Services;
using Votae.Models;

namespace Votae.Services
{
    public class DataService : IDataService
    {
        private readonly string _dbPath;

        public DataService(string appDataPath)
        {
            _dbPath = Path.Combine(appDataPath, "eleicoes.db");
            InitDatabase();
        }

        private SqliteConnection GetConnection() => new SqliteConnection($"Data Source={_dbPath}");

        private void InitDatabase()
        {
            using var connection = GetConnection();
            connection.Open();

            var command = connection.CreateCommand();
            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Eleicoes (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Nome TEXT NOT NULL,
                    Status TEXT NOT NULL,
                    DataInicio TEXT NOT NULL
                );";
            command.ExecuteNonQuery();

            command.CommandText = @"
                CREATE TABLE IF NOT EXISTS Candidatos (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    IdEleicao INTEGER NOT NULL,
                    Numero INTEGER NOT NULL,
                    NomeCandidato TEXT NOT NULL,
                    FotoCandidatoBase64 TEXT NULL,
                    NomeVice TEXT NULL,
                    FotoViceBase64 TEXT NULL,
                    FOREIGN KEY (IdEleicao) REFERENCES Eleicoes(Id) ON DELETE CASCADE
                );";
            command.ExecuteNonQuery();
        }

        #region Eleicoes

        public async Task<List<Eleicao>> GetEleicoesAsync()
        {
            var eleicoes = new List<Eleicao>();
            using var connection = GetConnection();
            await connection.OpenAsync();

            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Nome, Status, DataInicio FROM Eleicoes";

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                eleicoes.Add(new Eleicao
                {
                    Id = reader.GetInt32(0),
                    Nome = reader.GetString(1),
                    Status = reader.GetString(2),
                    DataInicio = DateTime.Parse(reader.GetString(3))
                });
            }
            return eleicoes;
        }

        public async Task<Eleicao> GetEleicaoByIdAsync(int id)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, Nome, Status, DataInicio FROM Eleicoes WHERE Id = @id";
            command.Parameters.AddWithValue("@id", id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Eleicao
                {
                    Id = reader.GetInt32(0),
                    Nome = reader.GetString(1),
                    Status = reader.GetString(2),
                    DataInicio = DateTime.Parse(reader.GetString(3))
                };
            }
            return null;
        }

        public async Task<int> SaveEleicaoAsync(Eleicao eleicao)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            var command = connection.CreateCommand();

            if (eleicao.Id == 0) // Criar nova
            {
                command.CommandText = "INSERT INTO Eleicoes (Nome, Status, DataInicio) VALUES (@nome, @status, @dataInicio); SELECT last_insert_rowid();";
            }
            else // Atualizar existente
            {
                command.CommandText = "UPDATE Eleicoes SET Nome = @nome, Status = @status, DataInicio = @dataInicio WHERE Id = @id";
                command.Parameters.AddWithValue("@id", eleicao.Id);
            }

            command.Parameters.AddWithValue("@nome", eleicao.Nome);
            command.Parameters.AddWithValue("@status", eleicao.Status);
            command.Parameters.AddWithValue("@dataInicio", eleicao.DataInicio?.ToString("o")); // Formato ISO 8601

            if (eleicao.Id == 0)
            {
                return Convert.ToInt32(await command.ExecuteScalarAsync());
            }
            else
            {
                await command.ExecuteNonQueryAsync();
                return eleicao.Id;
            }
        }

        public async Task DeleteEleicaoAsync(int id)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Eleicoes WHERE Id = @id AND Status = 'Pendente'";
            command.Parameters.AddWithValue("@id", id);
            await command.ExecuteNonQueryAsync();
        }

        #endregion Eleicoes

        #region Candidatos

        public async Task<List<Candidato>> GetCandidatosByEleicaoIdAsync(int eleicaoId)
        {
            var candidatos = new List<Candidato>();
            using var connection = GetConnection();
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "SELECT Id, IdEleicao, Numero, NomeCandidato, FotoCandidatoBase64, NomeVice, FotoViceBase64 FROM Candidatos WHERE IdEleicao = @eleicaoId";
            command.Parameters.AddWithValue("@eleicaoId", eleicaoId);

            using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                candidatos.Add(new Candidato
                {
                    Id = reader.GetInt32(0),
                    IdEleicao = reader.GetInt32(1),
                    Numero = reader.GetInt32(2),
                    NomeCandidato = reader.GetString(3),
                    FotoCandidatoBase64 = reader.IsDBNull(4) ? null : reader.GetString(4),
                    NomeVice = reader.IsDBNull(5) ? null : reader.GetString(5),
                    FotoViceBase64 = reader.IsDBNull(6) ? null : reader.GetString(6),
                });
            }
            return candidatos;
        }

        public async Task<int> SaveCandidatoAsync(Candidato candidato)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            var command = connection.CreateCommand();

            if (candidato.Id == 0) // Criar
            {
                command.CommandText = @"
                    INSERT INTO Candidatos (IdEleicao, Numero, NomeCandidato, FotoCandidatoBase64, NomeVice, FotoViceBase64)
                    VALUES (@idEleicao, @numero, @nome, @foto, @nomeVice, @fotoVice);
                    SELECT last_insert_rowid();";
            }
            else // Atualizar
            {
                command.CommandText = @"
                    UPDATE Candidatos SET
                        Numero = @numero,
                        NomeCandidato = @nome,
                        FotoCandidatoBase64 = @foto,
                        NomeVice = @nomeVice,
                        FotoViceBase64 = @fotoVice
                    WHERE Id = @id";
                command.Parameters.AddWithValue("@id", candidato.Id);
            }

            command.Parameters.AddWithValue("@idEleicao", candidato.IdEleicao);
            command.Parameters.AddWithValue("@numero", candidato.Numero);
            command.Parameters.AddWithValue("@nome", candidato.NomeCandidato);
            command.Parameters.AddWithValue("@foto", (object)candidato.FotoCandidatoBase64 ?? DBNull.Value);
            command.Parameters.AddWithValue("@nomeVice", (object)candidato.NomeVice ?? DBNull.Value);
            command.Parameters.AddWithValue("@fotoVice", (object)candidato.FotoViceBase64 ?? DBNull.Value);

            if (candidato.Id == 0)
            {
                return Convert.ToInt32(await command.ExecuteScalarAsync());
            }
            else
            {
                await command.ExecuteNonQueryAsync();
                return candidato.Id;
            }
        }

        public async Task DeleteCandidatoAsync(int candidatoId)
        {
            using var connection = GetConnection();
            await connection.OpenAsync();
            var command = connection.CreateCommand();
            command.CommandText = "DELETE FROM Candidatos WHERE Id = @id";
            command.Parameters.AddWithValue("@id", candidatoId);
            await command.ExecuteNonQueryAsync();
        }

        #endregion Candidatos
    }
}