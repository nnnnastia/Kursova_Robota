using System.ComponentModel.DataAnnotations;

namespace Kursova_Robota.Models
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Введіть ім'я користувача.")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Введіть номер телефону.")]
        public string PhoneNumber { get; set; }

        public string? Email { get; set; }

        [Required(ErrorMessage = "Підтвердіть пароль.")]
        public string CurrentPassword { get; set; }
    }
}
