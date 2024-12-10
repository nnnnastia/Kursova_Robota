using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Kursova_Robota.Models;
using System.Linq;

namespace Kursova_Robota.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Book)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                ViewBag.Message = "Ваша корзина пуста!";
                return View(new List<CartItem>());
            }

            return View(cart.CartItems);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int bookId, int quantity)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Перевірка, чи існує книга з таким ID
            var book = _context.Books.FirstOrDefault(b => b.Id == bookId);
            if (book == null)
            {
                ModelState.AddModelError("", "Книга не знайдена.");
                return RedirectToAction("Index", "Home");
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId.Value, CartItems = new List<CartItem>() };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();  // Потрібно зберегти картку перед додаванням товару
            }

            // Перевірка, чи є вже такий товар у корзині
            var cartItem = cart.CartItems.FirstOrDefault(i => i.BookId == bookId);
            if (cartItem == null)
            {
                cartItem = new CartItem
                {
                    BookId = bookId,
                    Quantity = quantity,
                    Price = book.Price // Використовуємо ціну книги
                };
                cart.CartItems.Add(cartItem);  // Додаємо товар у список картки
            }
            else
            {
                cartItem.Quantity += quantity;  // Якщо товар є, збільшуємо кількість
            }

            // Оновлення кількості товарів у сесії
            HttpContext.Session.SetInt32("CartItemCount", cart.CartItems.Sum(ci => ci.Quantity));

            await _context.SaveChangesAsync(); // Збереження змін
            return RedirectToAction("Index");
        }


        [HttpPost]
        public IActionResult RemoveFromCart(int cartItemId)
        {
            var cartItem = _context.CartItems.Find(cartItemId);
            if (cartItem != null)
            {
                _context.CartItems.Remove(cartItem);
                _context.SaveChanges();

                // Оновлюємо кількість товарів у сесії після видалення
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId != null)
                {
                    var cart = _context.Carts
                        .Include(c => c.CartItems)
                        .FirstOrDefault(c => c.UserId == userId);

                    // Якщо корзина не порожня, то оновлюємо індикатор кількості
                    if (cart != null)
                    {
                        HttpContext.Session.SetInt32("CartItemCount", cart.CartItems.Sum(ci => ci.Quantity));
                    }
                    else
                    {
                        HttpContext.Session.SetInt32("CartItemCount", 0); // Якщо корзина порожня, кількість товарів 0
                    }
                }
            }

            return RedirectToAction("Index");
        }


        [HttpGet]
        public IActionResult Checkout()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login", "Account");

            var cart = _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Book)
                .FirstOrDefault(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                return RedirectToAction("Index");
            }

            return View(new CheckoutViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("Checkout", model);
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Book)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.CartItems.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            // Створюємо замовлення
            var order = new Order
            {
                UserId = userId.Value,
                OrderDate = DateTime.Now,
                Status = "Pending",
                Items = cart.CartItems.Select(i => new OrderItem
                {
                    BookId = i.BookId,
                    Quantity = i.Quantity
                }).ToList(),
                Address = model.Address,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                Notes = model.Notes ?? string.Empty
            };

            // Додаємо замовлення в базу даних
            _context.Orders.Add(order);

            // Видаляємо товар з корзини
            _context.Carts.Remove(cart); // Це очистить корзину

            // Оновлюємо кількість товарів у сесії
            HttpContext.Session.SetInt32("CartItemCount", 0);

            // Зберігаємо зміни
            await _context.SaveChangesAsync();

            return RedirectToAction("OrderConfirmation", new { orderId = order.Id });
        }


        [HttpGet]
        public IActionResult OrderConfirmation(int orderId)
        {
            var order = _context.Orders
                .Include(o => o.Items)
                .ThenInclude(i => i.Book)
                .FirstOrDefault(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound(); // Якщо замовлення не знайдено
            }

            return View(order); // Відображає замовлення
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int cartItemId, string action)
        {
            var cartItem = _context.CartItems.Find(cartItemId);
            if (cartItem != null)
            {
                if (action == "increase")
                {
                    cartItem.Quantity++;
                }
                else if (action == "decrease" && cartItem.Quantity > 1)
                {
                    cartItem.Quantity--;
                }

                _context.SaveChanges();

                // Оновлюємо кількість товарів у сесії
                var userId = HttpContext.Session.GetInt32("UserId");
                if (userId != null)
                {
                    var cart = _context.Carts
                        .Include(c => c.CartItems)
                        .FirstOrDefault(c => c.UserId == userId);

                    // Оновлюємо індикатор кількості товарів у кошику
                    if (cart != null)
                    {
                        HttpContext.Session.SetInt32("CartItemCount", cart.CartItems.Sum(ci => ci.Quantity));
                    }
                }
            }

            return RedirectToAction("Index");
        }

    }
}
