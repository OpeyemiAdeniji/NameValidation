using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace NameValidationApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NameValidationController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get(string name)
        {
            // Check if the name is null or whitespace
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest("Name is required.");
            }

            // Validate that the name contains only letters
            if (!Regex.IsMatch(name, @"^[a-zA-Z]+$"))
            {
                return BadRequest("Invalid characters in name. Only letters are allowed.");
            }

            return Ok($"Hello, {name}!"); // Return a 200 OK response
        }
    }
}
