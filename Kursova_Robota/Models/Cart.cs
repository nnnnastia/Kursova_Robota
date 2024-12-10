﻿using Kursova_Robota.Models;

public class Cart
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; }
    public List<CartItem> Items { get; set; }
    public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
}
