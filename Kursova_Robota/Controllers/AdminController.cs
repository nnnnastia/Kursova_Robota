using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Kursova_Robota.Models;
using Microsoft.EntityFrameworkCore;
using System;

[Route("Admin")]
public class AdminController : Controller
{
    private bool IsAdmin()
    {
        var isAdmin = HttpContext.Session.GetString("IsAdmin");
        return isAdmin != null && bool.Parse(isAdmin);
    }

    private readonly ApplicationDbContext _context;

    public AdminController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Головна сторінка адмін-панелі
    [HttpGet("AdminPanel")]
    public IActionResult AdminPanel()
    {
        var books = _context.Books.ToList(); // Отримуємо всі книги
        return View(books);
    }

    [HttpGet("Orders")]
    public IActionResult Orders()
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        var orders = _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Book)
            .Include(o => o.User)
            .ToList();

        ViewBag.StatusOptions = new List<SelectListItem>
{
    new SelectListItem { Value = "Pending", Text = "Обробляється" },
    new SelectListItem { Value = "InProgess", Text = "В процесі" },
    new SelectListItem { Value = "Completed", Text = "Завершено" },
    new SelectListItem { Value = "Cancelled", Text = "Відмінено" }
};
        return View(orders);
    }

    [HttpPost]
    public IActionResult UpdateOrderStatus(int orderId, string status)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        var order = _context.Orders.Find(orderId);
        if (order != null)
        {
            order.Status = status;
            _context.SaveChanges();
        }

        return RedirectToAction("Orders");
    }


    // Підтвердження видалення книги
    [HttpGet("Delete")]
    public IActionResult Delete(int id)
    {
        var book = _context.Books.FirstOrDefault(b => b.Id == id);
        if (book == null)
        {
            return NotFound("Книга не знайдена.");
        }

        return RedirectToAction("Delete");
    }


    // Обробка видалення (POST)
    [HttpPost("Delete")]
    public IActionResult DeleteConfirmed(int id)
    {
        var book = _context.Books.FirstOrDefault(b => b.Id == id);
        if (book != null)
        {
            _context.Books.Remove(book);
            _context.SaveChanges();
        }

        return RedirectToAction("AdminPanel");
    }



    // Редагування книги (форма)
    [HttpGet("Edit/{id}")]
    public IActionResult Edit(int id)
    {
        // Отримуємо список жанрів
        var genres = _context.Genres.Select(g => new SelectListItem
        {
            Value = g.Id.ToString(), // Значення - це ID жанру
            Text = g.Name,          // Текст - це назва жанру
            Selected = false        // За замовчуванням - не вибраний
        }).ToList();

        var book = _context.Books.FirstOrDefault(b => b.Id == id);
        if (book == null)
        {
            return NotFound("Книга не знайдена.");
        }

        // Встановлюємо обраний жанр для випадаючого списку
        genres.FirstOrDefault(g => g.Value == book.GenreId.ToString()).Selected = true;
        ViewBag.Genres = genres; // Передаємо список жанрів

        return View(book); // Перенаправляємо на форму редагування
    }

    // Редагування книги (обробка)
    [HttpPost("Edit/{id}")]
    public IActionResult Edit(int id, Book updatedBook)
    {
        var book = _context.Books.FirstOrDefault(b => b.Id == id);
        if (book != null)
        {
            book.Title = updatedBook.Title;
            book.Author = updatedBook.Author;
            book.Description = updatedBook.Description;
            book.GenreId = updatedBook.GenreId; // Зберігаємо обраний жанр
            book.ImagePath = updatedBook.ImagePath;

            _context.SaveChanges();
        }

        return RedirectToAction("AdminPanel");
    }


    [HttpGet("Create")]
    public IActionResult Create()
    {
        // Отримуємо список жанрів
        var genres = _context.Genres.Select(g => new SelectListItem
        {
            Value = g.Id.ToString(),
            Text = g.Name
        }).ToList();

        ViewBag.Genres = genres; // Передаємо список жанрів
        return View();
    }

    [HttpPost("Create")]
    public async Task<IActionResult> Create(string Title, string Author, int Price, string Description, int GenreId, IFormFile file)
    {
        // Перевіряємо, чи книга з такою назвою вже існує
        if (_context.Books.Any(b => b.Title == Title))
        {
            ModelState.AddModelError("", "Книга з такою назвою вже існує.");
            // Знову завантажуємо список жанрів для випадаючого списку
            var genres = _context.Genres.Select(g => new SelectListItem
            {
                Value = g.Id.ToString(),
                Text = g.Name
            }).ToList();

            ViewBag.Genres = genres;
            return View();
        }

        if (ModelState.IsValid)
        {
            string imagePath = "default-image.jpg"; // Значення за замовчуванням
            if (file != null && file.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Images", fileName);

                // Зберігаємо файл
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                imagePath = fileName;
            }

            // Створення нової книги
            var book = new Book
            {
                Title = Title,
                Author = Author,
                Price = Price,
                Description = Description,
                GenreId = GenreId,
                ImagePath = imagePath
            };

            // Додавання книги до бази даних
            _context.Books.Add(book);
            await _context.SaveChangesAsync();

            return RedirectToAction("AdminPanel");
        }

        // Якщо модель не валідна, знову завантажуємо список жанрів
        var genresList = _context.Genres.Select(g => new SelectListItem
        {
            Value = g.Id.ToString(),
            Text = g.Name
        }).ToList();

        ViewBag.Genres = genresList;
        return View();
    }


    // GET: Create
    public IActionResult CreateList()
    {
        // Get all genres from the database
        var genres = _context.Genres.Select(g => new SelectListItem
        {
            Value = g.Id.ToString(),   // Value should be the Id of the genre
            Text = g.Name              // Text should be the name of the genre
        }).ToList();

        // Pass the genres list to the view via ViewBag
        ViewBag.Genres = genres;

        return View();
    }
    // Перегляд деталей замовлення
    [HttpGet("OrderDetails/{orderId}")]
    public IActionResult OrderDetails(int orderId)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login", "Account");
        }

        var order = _context.Orders
            .Include(o => o.Items)
            .ThenInclude(i => i.Book)
            .FirstOrDefault(o => o.Id == orderId);

        if (order == null)
        {
            return NotFound("Замовлення не знайдено.");
        }

        return View(order);
    }

}