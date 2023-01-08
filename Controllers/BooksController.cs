using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using book_app_api.Models;
using book_app_api.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using book_app_api.Infrastructure.Extensions;


namespace book_app_api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/books")]
public class BooksController : ControllerBase
{
    private readonly IBooksService _booksService;
    private readonly IValidator<Book> _validator;

    public BooksController(IBooksService booksService, IValidator<Book> validator)
    {
        _booksService = booksService;
        _validator = validator;
    }

    [HttpGet("list")]
    [ProducesResponseType(typeof(List<Book>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllBooksAsync()
    {
        List<Book> allBooks = await _booksService.GetAllBooksAsync();
        return Ok(allBooks);
    }

    /// <summary>
    /// For API Versioning test
    /// </summary>
    [ApiVersion("2.0")]
    [HttpGet("list")]
    public Task<IActionResult> GetAllBooksAsync2()
    {
        return Task.FromResult<IActionResult>(Ok("lol kek"));
    }

    [HttpGet("{isbn}")]
    [ProducesResponseType(typeof(Book), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(void), StatusCodes.Status204NoContent)]
    public async Task<IActionResult> GetBookAsync(string isbn)
    {
        Book book = await _booksService.GetBookAsync(isbn);
        return Ok(book);
    }

    [HttpPost]
    [ProducesResponseType(typeof(Book), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddBookAsync([FromBody] Book book)
    {
        ValidationResult result = await _validator.ValidateAsync(book);
        if (!result.IsValid)
        {
            // TODO: добавить исключение которое выкидывает ValidationREsult из сервиса
            // а тут его читать и добавить в ModelState
            this.ModelState
            var dict = result.AddModelError();
            return BadRequest(dict);
        }

        await _booksService.AddBookAsync(book);
        return CreatedAtAction(nameof(GetBookAsync), new { isbn = book.Isbn }, book);
    }

    [HttpDelete("{isbn}")]
    [ProducesResponseType(typeof(void), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteBookAsync(string isbn)
    {
        var resp = _booksService.DeleteBookAsync(isbn);
        return Ok();
    }

    [HttpPatch("{isbn}")]
    public async Task<IActionResult> ToggleFavoriteAsync(string isbn)
    {
        try
        {
            var bookInBase = await _booksService.ToggleFavoriteAsync(isbn);
            return Ok(new { IsFavorite = bookInBase.IsFavorite });
        }
        catch (Exception)
        {
            return NotFound();
        }
    }

    [HttpGet("search/{title}")]
    public async Task<IActionResult> Search(string title)
    {
        List<Book> response = await _booksService.GetBooksByTitleAsync(title);
        return Ok(response);
    }

    [HttpPut("{isbn}")]
    public async Task<IActionResult> UpdateBookAsync([FromBody] Book book, string isbn)
    {
        try
        {
            await _booksService.UpdateBookAsync(isbn, book);
            return Ok(book);
        }
        catch (Exception e)
        {
            return NotFound();
        }
    }
}