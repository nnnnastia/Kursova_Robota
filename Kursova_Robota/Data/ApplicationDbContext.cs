using Kursova_Robota.Models;
using Microsoft.EntityFrameworkCore;


public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Book> Books { get; set; }
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Зв'язок між Book та Genre (один до багатьох)
        modelBuilder.Entity<Book>()
            .HasOne(b => b.Genre)
            .WithMany(g => g.Books)
            .HasForeignKey(b => b.GenreId);

        // Seed data for Genre (unchanged)
        modelBuilder.Entity<Genre>().HasData(
            new Genre { Id = 1, Name = "Художня література" },
            new Genre { Id = 2, Name = "Наукова література" },
            new Genre { Id = 3, Name = "Фентезі" },
            new Genre { Id = 4, Name = "Саморозвиток" },
            new Genre { Id = 5, Name = "Романтика" },
            new Genre { Id = 6, Name = "Історія" }
        );

        // Зв'язок між Cart та CartItem (один до багатьох)
        modelBuilder.Entity<Cart>()
            .HasMany(c => c.CartItems)
            .WithOne(ci => ci.Cart)
            .HasForeignKey(ci => ci.CartId)
            .OnDelete(DeleteBehavior.Cascade);


    }
}
