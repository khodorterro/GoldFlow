using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using BussinnessLayer;
using DataAccessLayer;
namespace gold_flow.Controllers
{
    //[Route("api/[controller]")]
    [Route("api/People")]
    [ApiController]
    public class PeopleController : ControllerBase
    {
        [HttpGet("Get Person",Name ="GetPersonByID")]

        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public ActionResult<PersonDTO> GetStudentById(int id)
        {

            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
            }
            BussinnessLayer.People Person = BussinnessLayer.People.Find(id);

            if (Person == null)
            {
                return NotFound($"Person with ID {id} not found.");
            }

            PersonDTO PDTO = Person.PDTO;

            return Ok(PDTO);

        }

        [HttpPost("Add Person",Name = "AddPerson")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public ActionResult<PersonDTO> AddPerson(PersonDTO newpersonDTO)
        {

            if (string.IsNullOrEmpty(newpersonDTO.Name) || string.IsNullOrEmpty(newpersonDTO.Email) || string.IsNullOrEmpty(newpersonDTO.Address) || string.IsNullOrEmpty(newpersonDTO.Phonenumber))
            {
                return BadRequest("Invalid person data.");
            }

            BussinnessLayer.People person = new BussinnessLayer.People(new PersonDTO(newpersonDTO.Id, newpersonDTO.Name, newpersonDTO.Address, newpersonDTO.Phonenumber,newpersonDTO.Email));
            person.Save();

            newpersonDTO.Id = person.ID;

            return CreatedAtRoute("GetPersonByID", new { id = newpersonDTO.Id }, newpersonDTO);

        }


        [HttpPut("Update Person", Name = "UpdatePerson")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<PersonDTO> UpdatePerson(int id, PersonDTO updatedPerson)
        {
            if (id < 1 || updatedPerson == null || string.IsNullOrEmpty(updatedPerson.Name) || string.IsNullOrEmpty(updatedPerson.Email) || string.IsNullOrEmpty(updatedPerson.Name) || string.IsNullOrEmpty(updatedPerson.Address) || string.IsNullOrEmpty(updatedPerson.Phonenumber))
            {
                return BadRequest("Invalid Person data.");
            }

            BussinnessLayer.People Person = BussinnessLayer.People.Find(id);


            if (Person == null)
            {
                return NotFound($"Person with ID {id} not found.");
            }


            Person.Name = updatedPerson.Name;
            Person.address = updatedPerson.Address;
            Person.email = updatedPerson.Email;
            Person.phone = updatedPerson.Phonenumber;
           
            Person.Save();

            return Ok(Person.PDTO);

        }

        [HttpDelete("DeletePerson", Name = "DeletePerson")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult DeletePerson(int id)
        {
            if (id < 1)
            {
                return BadRequest($"Not accepted ID {id}");
            }

            if (BussinnessLayer.People.DeletePerson(id))

                return Ok($"Persom with ID {id} has been deleted.");
            else
                return NotFound($"Person with ID {id} not found. no rows deleted!");
        }


    }
}
