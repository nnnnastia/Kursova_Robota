using Kursova_Robota.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Kursova_Robota.Controllers
{
    public class AccountController : Controller
    {

        private readonly ApplicationDbContext _context;

        public AccountController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(string Username, string Email, string Password, string? PhoneNumber)
        {
            if (_context.Users.Any(u => u.Username == Username))
            {
                ModelState.AddModelError("", "Користувач с таким ім'ям вже існує.");
                return View();
            }

            if (ModelState.IsValid)
            {
                // Створення нового користувача
                var user = new User
                {
                    Username = Username,
                    PhoneNumber = PhoneNumber,
                    Password = Password,
                    Email = Email,
                    IsAdmin = false,
                    // Генерація токену для скидання пароля
                    ResetPasswordToken = Guid.NewGuid().ToString()  // Генерація унікального токену
                };

                _context.Users.Add(user);
                _context.SaveChanges();

                return RedirectToAction("Login");
            }

            return View();
        }

        // Страница входа (GET)
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Обработка входа (POST)
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users.SingleOrDefault(u => u.Username == username && u.Password == password);
            if (user != null)
            {
                HttpContext.Session.SetInt32("UserId", user.Id);
                HttpContext.Session.SetString("Username", user.Username);
                HttpContext.Session.SetString("IsAdmin", user.IsAdmin.ToString());

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Неправильно введене ім'я або пароль.");
            return View();
        }

        // GET: Logout
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Очистка сесії
            return RedirectToAction("Login");
        }

        // Страница профілю (GET)
        [HttpGet]
        public IActionResult Profile()
        {
            // Отримуємо поточного користувача з сесії
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = _context.Users.SingleOrDefault(u => u.Id == userId);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            return View(user); // Переходимо на сторінку профілю з даними користувача
        }

        [HttpGet]
        public IActionResult OrderHistory()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null) return RedirectToAction("Login");

            var orders = _context.Orders
                .Where(o => o.UserId == userId)
                .Include(o => o.Items)
                .ThenInclude(oi => oi.Book)
                .ToList();

            return View(orders);
        }

        [HttpGet]
        public IActionResult ProfileEdit()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = _context.Users.Find(userId);
            if (user == null)
            {
                return NotFound();
            }

            var model = new EditProfileViewModel
            {
                Username = user.Username,
                PhoneNumber = user.PhoneNumber,
                Email = user.Email
            };

            return View(model);
        }

        [HttpPost]
        public IActionResult ProfileEdit(EditProfileViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user = _context.Users.Find(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Проверка текущего пароля
            if (user.Password != model.CurrentPassword)
            {
                ModelState.AddModelError("CurrentPassword", "Неправильний пароль.");
                return View(model);
            }

            // Проверка уникальности имени пользователя
            var existingUser = _context.Users.FirstOrDefault(u => u.Username == model.Username && u.Id != userId);
            if (existingUser != null)
            {
                ModelState.AddModelError("Username", "Це ім'я користувача вже зайнято.");
                return View(model);
            }

            // Обновление данных
            user.Username = model.Username;
            user.PhoneNumber = model.PhoneNumber;
            user.Email = model.Email;

            _context.SaveChanges();

            return RedirectToAction("Profile");
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            return View();
        }


        [HttpPost]
        public IActionResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login");
            }

            var user = _context.Users.Find(userId);
            if (user == null)
            {
                return NotFound();
            }

            if (user.Password != oldPassword)
            {
                ModelState.AddModelError("", "Старий пароль неправильний.");
                return View();
            }

            if (newPassword != confirmPassword)
            {
                ModelState.AddModelError("", "Паролі не співпадають.");
                return View();
            }

            user.Password = newPassword;
            _context.SaveChanges();

            return RedirectToAction("Profile");
        }
    }
}
