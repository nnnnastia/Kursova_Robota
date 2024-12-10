using System.ComponentModel.DataAnnotations;

namespace Kursova_Robota.Models
{
    public class Book
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [StringLength(5000)]
        public string Description { get; set; }
        public int GenreId { get; set; }  // Ідентифікатор жанру
        public Genre Genre { get; set; }
        public string Author { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Введіть коректне значення")]
        public int Price { get; set; }

        [StringLength(255)]
        public string ImagePath { get; set; }
    }
}
