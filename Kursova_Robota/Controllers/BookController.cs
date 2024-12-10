using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class BookController : Controller
{
    private readonly ApplicationDbContext _context;

    public BookController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Загальний метод для відображення книг за жанром
    [HttpGet]
    public IActionResult Genre(string genreName)
    {
        if (string.IsNullOrEmpty(genreName))
        {
            return NotFound("Жанр не знайдено.");
        }

        var books = _context.Books
            .Where(b => b.Genre.Name == genreName)
            .ToList();

        if (!books.Any())
        {
            ViewBag.Message = "Книги цього жанру відсутні.";
        }

        ViewBag.GenreName = genreName;
        return View(books);
    }

    // Окремі методи для кожного жанру
    [HttpGet]
    public IActionResult Fiction()
    {
        return Genre("Художня література");
    }

    [HttpGet]
    public IActionResult Science()
    {
        return Genre("Наукова література");
    }

    [HttpGet]
    public IActionResult Fantasy()
    {
        return Genre("Фентезі");
    }

    [HttpGet]
    public IActionResult SelfHelp()
    {
        return Genre("Саморозвиток");
    }

    [HttpGet]
    public IActionResult Romance()
    {
        return Genre("Романтика");
    }

    [HttpGet]
    public IActionResult History()
    {
        return Genre("Історія");
    }

    // Деталі книги
    [HttpGet("Book/Details/{id}")]
    public IActionResult Details(int id)
    {
        var book = _context.Books
                   .Include(b => b.Genre) // Ensure related genre data is loaded
                   .FirstOrDefault(b => b.Id == id);

        if (book == null)
        {
            return NotFound("Книга не знайдена.");
        }

        return View(book); // Make sure you are passing the 'book' model here.
    }

}
