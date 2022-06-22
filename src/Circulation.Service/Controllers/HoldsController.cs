using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Circulation.Domain.Holds;
using Microsoft.AspNetCore.Mvc;

namespace Circulation.Service.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HoldsController : ControllerBase
    {

        // POST /holds
        [HttpPost("cardholder/{cardholderNumber}/{isbn}")]
        public async Task<ActionResult<string>> Post(string cardHolderNumber, string isbn, [FromServices] PlaceHoldCommandHandler commandHandler)
        {
            var command = new PlaceHold {CardholderNumber = cardHolderNumber, Isbn = isbn};
            await commandHandler.Handle(command);

            return Ok();
        }

        // DELETE api/holds/5
        [HttpDelete("cardholder/{cardholderNumber}/{isbn}")]
        public void Delete()
        {

        }
    }
}
