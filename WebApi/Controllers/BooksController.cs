using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        public readonly DataContext _context;

        public BooksController(DataContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<List<Books>>> Get()
        {
            return Ok(await _context.Books.ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Books>> Get(int id)
        {
            var book = await _context.Books.FindAsync(id);
            
            if (book == null)
                return BadRequest("Book not found");

            return Ok(book);
        }

        [HttpPost]
        public async Task<ActionResult<List<Books>>> AddBook(Books book)
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return Ok(await _context.Books.ToListAsync());
        }

        [HttpPut]
        public async Task<ActionResult<List<Books>>> UpdateBook(Books request)
        {
            var dbBook = await _context.Books.FindAsync(request.Id);
            
            if (dbBook == null)
                return BadRequest("Book not found");

            dbBook.Title = request.Title;
            dbBook.Description = request.Description;
            dbBook.TypeBook = request.TypeBook;
            dbBook.Price = request.Price;

            await _context.SaveChangesAsync();

            return Ok(await _context.Books.ToListAsync());
        }

        [HttpDelete]
        public async Task<ActionResult<List<Books>>> Delete(int id)
        {
            var dbBook = await _context.Books.FindAsync(id);

            if (dbBook == null)
                return BadRequest("Book not found");

            _context.Books.Remove(dbBook);

            await _context.SaveChangesAsync();
            return Ok(await _context.Books.ToListAsync());
        }
    }
}
