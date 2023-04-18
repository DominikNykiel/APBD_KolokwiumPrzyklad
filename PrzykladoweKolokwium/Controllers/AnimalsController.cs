using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using PrzykladoweKolokwium.Models.DTOs;
using PrzykladoweKolokwium.Repositories;
using System.Transactions;

namespace PrzykladoweKolokwium.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnimalsController : ControllerBase
    {
        private readonly IAnimalsRepository _animalsRepository;
        public AnimalsController(IAnimalsRepository animalsRepository)
        {

            _animalsRepository = animalsRepository;

        }
        [HttpGet]
        [Route("{animalID}")]
        public async Task<IActionResult> GetAnimal(int animalID)
        {
            if(!await _animalsRepository.DoesAnimalExist(animalID)) 
                return NotFound($"Zwierzę o podanym ID - {animalID} nie istnieje");

            var animal = await _animalsRepository.GetAnimalWithOwner(animalID);

            return Ok(animal);
        }

        [HttpPost]
        public async Task<IActionResult> AddAnimal(NewAnimalWithProcedures newAnimalWithProcedures)
        {
            if (await _animalsRepository.DoesAnimalExist(newAnimalWithProcedures.ID))
                return NotFound($"Zwierzę o podanym ID - {newAnimalWithProcedures.ID} już istnieje");

            if (!await _animalsRepository.DoesOwnerExist(newAnimalWithProcedures.Owner_ID))
                return NotFound($"Właściciel o podanym ID - {newAnimalWithProcedures.Owner_ID} nie istnieje");

            foreach (var procedure in newAnimalWithProcedures.Procedures)
            {
                if (!await _animalsRepository.DoesProcedureExist(procedure.Procedure_ID))
                    return NotFound($"Procedura o podanym ID - {procedure.Procedure_ID} nie istnieje");
            }

            await _animalsRepository.AddNewAnimalWithProcedures(newAnimalWithProcedures);

            return Created(Request.Path.Value ?? "api/animals", newAnimalWithProcedures);
        }

        [HttpPost]
        [Route("v2")]
        public async Task<IActionResult> AddAnimalV2(NewAnimalWithProcedures newAnimalWithProcedures)
        {
            if (await _animalsRepository.DoesAnimalExist(newAnimalWithProcedures.ID))
                return NotFound($"Zwierzę o podanym ID - {newAnimalWithProcedures.ID} już istnieje");

            if (!await _animalsRepository.DoesOwnerExist(newAnimalWithProcedures.Owner_ID))
                return NotFound($"Właściciel o podanym ID - {newAnimalWithProcedures.Owner_ID} nie istnieje");

            foreach (var procedure in newAnimalWithProcedures.Procedures)
            {
                if (!await _animalsRepository.DoesProcedureExist(procedure.Procedure_ID))
                    return NotFound($"Procedura o podanym ID - {procedure.Procedure_ID} nie istnieje");
            }

            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await _animalsRepository.AddAnimal(new Models.Animal
                {
                    ID = newAnimalWithProcedures.ID,
                    Name = newAnimalWithProcedures.Name,
                    Type = newAnimalWithProcedures.Type,
                    AdmissionDate = newAnimalWithProcedures.AdmissionDate,
                    Owner_ID = newAnimalWithProcedures.Owner_ID
                });

                foreach (var procedure in newAnimalWithProcedures.Procedures)
                {
                    await _animalsRepository.AddProcedureAnimal(newAnimalWithProcedures.ID, procedure);
                }

                scope.Complete();
            }

            return Created(Request.Path.Value ?? "api/animals", newAnimalWithProcedures);
        }
    }
}
