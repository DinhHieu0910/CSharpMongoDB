using DemoMongoDB.Dto;
using DemoMongoDB.Entities;
using DemoMongoDB.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace DemoMongoDB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BooksController : ControllerBase
    {
        private readonly BooksService _booksService;

        public BooksController(BooksService booksService) =>
            _booksService = booksService;

        [HttpGet("get-all")]
        public async Task<List<Book>> Get([FromQuery] PageResultBookDto input) =>
            await _booksService.GetAsync(input);

        [HttpGet("get-with-filter")]
        public async Task<List<Book>> GetWithFillter(int pageNumber, int? pageSize, string filterJson) =>
            await _booksService.GetAsyncWithFilter(pageNumber, pageSize ?? 100, filterJson);

        [HttpGet("get-linQ")]
        public List<Book> GetLinQ([FromQuery] PageResultBookDto input) =>
            _booksService.GetAsyncLinQ(input);

        [HttpPost("create-json-params")]
        public string CreateJsonParams(string category, string author) =>
            _booksService.CreateJsonParams(category, author);

        [HttpGet("{id:length(24)}")]
        public async Task<ActionResult<Book>> Get(string id)
        {
            var book = await _booksService.GetAsync(id);

            if (book is null)
            {
                return NotFound();
            }

            return book;
        }

        [HttpPost]
        public async Task<IActionResult> Post(Book newBook)
        {
            await _booksService.CreateAsync(newBook);

            return CreatedAtAction(nameof(Get), new { id = newBook.Id }, newBook);
        }

        [HttpPut("{id:length(24)}")]
        public async Task<IActionResult> Update(string id, Book updatedBook)
        {
            var book = await _booksService.GetAsync(id);

            if (book is null)
            {
                return NotFound();
            }

            updatedBook.Id = book.Id;

            await _booksService.UpdateAsync(id, updatedBook);

            return NoContent();
        }

        [HttpDelete("{id:length(24)}")]
        public async Task<IActionResult> Delete(string id)
        {
            var book = await _booksService.GetAsync(id);

            if (book is null)
            {
                return NotFound();
            }

            await _booksService.RemoveAsync(id);

            return NoContent();
        }

        // Them Collection khac
        [HttpPost("add-user")]
        public async Task<IActionResult> PostUser(User input)
        {
            await _booksService.CreateUserAsync(input);

            return CreatedAtAction(nameof(Get), new { id = input.Id }, input);
        }

        [HttpGet("get-user-list")]
        public async Task<List<object>> GetUserList() =>
            await _booksService.GetUserAsync();
    }
}
