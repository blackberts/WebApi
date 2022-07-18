using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApi.Interfaces;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBooks _IBooks;

        public BooksController(IBooks iBooks)
        {
            _IBooks = iBooks;
        }

        [HttpGet]
        public async Task<ActionResult<List<Books>>> Get()
        {
            return Ok(await Task.FromResult(_IBooks.GetAll()));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Books>> Get(int id)
        {
            var book = await Task.FromResult(_IBooks.GetById(id));
            
            if (book == null)
                return BadRequest("Book not found");

            return Ok(book);
        }

        [HttpPost]
        public async Task<ActionResult<List<Books>>> Add(Books book)
        {
            _IBooks.AddBook(book);

            return Ok(await Task.FromResult(book));
        }

        [HttpPut]
        public async Task<ActionResult<List<Books>>> Update(Books request)
        {
            _IBooks.UpdateBook(request);

            return Ok(await Task.FromResult(_IBooks.GetAll()));
        }

        [HttpDelete]
        public async Task<ActionResult<List<Books>>> Delete(int id)
        {
            var book = await Task.FromResult(_IBooks.DeleteById(id));

            if (book == null)
                return BadRequest("Book not found");

            return Ok(await Task.FromResult(_IBooks.GetAll()));
        }
    }
}
