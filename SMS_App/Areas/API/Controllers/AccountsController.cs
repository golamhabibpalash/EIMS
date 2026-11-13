using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SMS_App.Areas.API.Controllers
{
    [Area("API")]
    [Route("API/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        // GET: API/Accounts
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("List of accounts");
        }

        // GET: API/Accounts/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            return Ok($"Account details for id {id}");
        }

        // POST: API/Accounts
        [HttpPost]
        public IActionResult Post([FromBody] AccountModel model)
        {
            // Code to create a new account
            return CreatedAtAction(nameof(Get), new { id = model.Id }, model);
        }

        // PUT: API/Accounts/5
        [HttpPut("{id}")]
        public IActionResult Put(int id, [FromBody] AccountModel model)
        {
            // Code to update an account
            return NoContent();
        }

        // DELETE: API/Accounts/5
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            // Code to delete an account
            return NoContent();
        }
    }

    public class AccountModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}
