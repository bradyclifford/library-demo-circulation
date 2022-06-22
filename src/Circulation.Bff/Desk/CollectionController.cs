using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Circulation.Domain;
using Circulation.Domain.PublicationCopies;
using Circulation.Domain.Publications;
using Dapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Circulation.Bff.Desk
{
    [Route("api/[controller]")]
    [ApiController]
    public class CollectionController : ControllerBase
    {
        private readonly IDbConnection _dbConnection;

        public CollectionController(IDbConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        [HttpPost("publication/{isbn}/copies")]
        public async Task<ActionResult<int>> Post(string isbn, [FromServices] AddPublicationCopyCommandHandler commandHandler)
        {
            var addPublicationCopyCommand = new AddPublicationCopyCommandDto {Isbn = isbn};

            IEnumerable events = await commandHandler.Handle(addPublicationCopyCommand);
            var savedCopyId = events.OfType<PublicationCopyAdded>().First().PublicationCopyId;
            return Ok(savedCopyId);
        }

        [HttpHead("publication/{isbn}")]
        public async Task<ActionResult> GetPublication(string isbn)
        {
            // look in our database to see if we have this isbn already.
            var found = await _dbConnection.QueryAsync("SELECT * FROM dbo.Publication where isbn = @isbn", new {isbn});
            if(found.Any())
                return Ok();
            return NotFound();
        }

        [HttpPost("publication")]
        public async Task<ActionResult> AddPublication([FromBody] AddPublicationDto publicationDto, [FromServices] AddPublicationCommandHandler commandHandler)
        {
            var addPublicationCommand = publicationDto.ToCommand();
            await commandHandler.Handle(addPublicationCommand);
            return Ok();
        }
    }

    public class AddPublicationDto
    {
        public string Isbn { get; set; }
        public string Title { get; set; }
        public string Authors { get; set; }
        public string CoverImageUrl { get; set; }

        public AddPublication ToCommand()
        {
            return new AddPublication
            {
                Isbn = Isbn,
                Title = Title,
                Authors = Authors,
                CoverImageUrl = CoverImageUrl
            };
        }
    }
}
