using PrzykladoweKolokwium.Models;
using PrzykladoweKolokwium.Models.DTOs;
using System.Data.SqlClient;

namespace PrzykladoweKolokwium.Repositories
{
    public class AnimalsRepository : IAnimalsRepository
    {
        private readonly string _connectionString;

        public AnimalsRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Default") 
                ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task AddAnimal(Animal animal)
        {
            var query = $"INSERT INTO Animal VALUES(@ID, @Name, @Type, @AdmissionDate, @OwnerID);";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                var command = connection.CreateCommand();
                command.CommandText = query;
                command.Parameters.AddWithValue("@ID", animal.ID);
                command.Parameters.AddWithValue("@Name", animal.Name);
                command.Parameters.AddWithValue("@Type", animal.Type);
                command.Parameters.AddWithValue("@AdmissionDate", animal.AdmissionDate);
                command.Parameters.AddWithValue("@OwnerID", animal.Owner_ID);

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task AddNewAnimalWithProcedures(NewAnimalWithProcedures newAnimalWithProcedures)
        {
            var insert = $"INSERT INTO Animal VALUES(@ID, @Name, @Type, @AdmissionDate, @OwnerID);";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                var command = connection.CreateCommand();
                command.CommandText = insert;
                command.Parameters.AddWithValue("@ID", newAnimalWithProcedures.ID);
                command.Parameters.AddWithValue("@Name", newAnimalWithProcedures.Name);
                command.Parameters.AddWithValue("@Type", newAnimalWithProcedures.Type);
                command.Parameters.AddWithValue("@AdmissionDate", newAnimalWithProcedures.AdmissionDate);
                command.Parameters.AddWithValue("@OwnerID", newAnimalWithProcedures.Owner_ID);

                await connection.OpenAsync();

                var transaction = await connection.BeginTransactionAsync();
                command.Transaction = transaction as SqlTransaction;
                try
                {
                    await command.ExecuteNonQueryAsync();
                    
                    foreach (var procedure in newAnimalWithProcedures.Procedures)
                    {
                        command.Parameters.Clear();
                        command.CommandText = "INSERT INTO Procedure_Animal VALUES(@ProcedureID, @AnimalID, @Date)";
                        command.Parameters.AddWithValue("@ProcedureID", procedure.Procedure_ID);
                        command.Parameters.AddWithValue("@AnimalID", newAnimalWithProcedures.ID);
                        command.Parameters.AddWithValue("@Date", procedure.Date);

                        await command.ExecuteNonQueryAsync();
                    }

                    await transaction.CommitAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task AddProcedureAnimal(int animalID, ProcedureWithDate procedure)
        {
            var query = $"INSERT INTO Procedure_Animal VALUES(@ProcedureID, @AnimalID, @Date)";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                var command = connection.CreateCommand();
                command.CommandText = query;
                command.Parameters.AddWithValue("@ProcedureID", procedure.Procedure_ID);
                command.Parameters.AddWithValue("@AnimalID", animalID);
                command.Parameters.AddWithValue("@Date", procedure.Date);

                await connection.OpenAsync();

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<bool> DoesAnimalExist(int animalID)
        {
            var query = $"SELECT 1 FROM Animal WHERE ID = {animalID}";

            using(SqlConnection connection = new SqlConnection(_connectionString))
            {
                var command = connection.CreateCommand();
                command.CommandText = query;

                await connection.OpenAsync();

                object? res = await command.ExecuteScalarAsync();

                return res is not null;
            }
        }

        public async Task<bool> DoesOwnerExist(int ownerID)
        {
            var query = $"SELECT 1 FROM Owner WHERE ID = {ownerID}";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                var command = connection.CreateCommand();
                command.CommandText = query;

                await connection.OpenAsync();

                object? res = await command.ExecuteScalarAsync();

                return res is not null;
            }
        }

        public async Task<bool> DoesProcedureExist(int procedureID)
        {
            var query = $"SELECT 1 FROM [Procedure] WHERE ID = {procedureID}";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                var command = connection.CreateCommand();
                command.CommandText = query;

                await connection.OpenAsync();

                object? res = await command.ExecuteScalarAsync();

                return res is not null;
            }
        }

        public async Task<AnimalWithOwner> GetAnimalWithOwner(int animalID)
        {
            var query = $"SELECT Animal.ID AS AnimalID, Animal.Name, Animal.Type, Animal.AdmissionDate, Owner.ID As OwnerID, Owner.FirstName, Owner.LastName FROM Animal JOIN Owner ON Animal.Owner_ID = Owner.ID WHERE Animal.ID = {animalID}";

            using (SqlConnection connection = new SqlConnection(_connectionString))
            {
                var command = connection.CreateCommand();
                command.CommandText = query;

                await connection.OpenAsync();

                var reader = await command.ExecuteReaderAsync();

                var animalIDOrdinal = reader.GetOrdinal("AnimalID");
                var nameOrdinal = reader.GetOrdinal("Name");
                var typeOrdinal = reader.GetOrdinal("Type");
                var admissionDateOrdinal = reader.GetOrdinal("AdmissionDate");
                var ownerIDOrdinal = reader.GetOrdinal("OwnerID");
                var firstNameOrdinal = reader.GetOrdinal("FirstName");
                var lastNameOrdinal = reader.GetOrdinal("LastName");

                await reader.ReadAsync();

                return new AnimalWithOwner
                {
                    ID = reader.GetInt32(animalIDOrdinal),
                    Name = reader.GetString(nameOrdinal),
                    Type = reader.GetString(typeOrdinal),
                    AdmissionDate = reader.GetDateTime(admissionDateOrdinal),
                    Owner = new Owner
                    {
                        ID = reader.GetInt32(ownerIDOrdinal),
                        FirstName = reader.GetString(firstNameOrdinal),
                        LastName = reader.GetString(lastNameOrdinal),   
                    }
                };
            }
        }
    }
}
