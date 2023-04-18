using PrzykladoweKolokwium.Models;
using PrzykladoweKolokwium.Models.DTOs;

namespace PrzykladoweKolokwium.Repositories
{
    public interface IAnimalsRepository
    {
        Task<bool> DoesAnimalExist(int animalID);
        Task<bool> DoesOwnerExist(int ownerID);
        Task<bool> DoesProcedureExist(int procedureID);
        Task<AnimalWithOwner> GetAnimalWithOwner(int animalID);
        //V1
        Task AddNewAnimalWithProcedures(NewAnimalWithProcedures newAnimalWithProcedures);

        //V2
        Task AddAnimal(Animal animal);
        Task AddProcedureAnimal(int animalID, ProcedureWithDate procedure);
    }
}
