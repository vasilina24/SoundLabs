using System;
using System.Collections.Generic;

namespace SoundLabs.Models;

public partial class Product
{
    public int IdProduct { get; set; }

    public string Article { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public int ReservedQuantity { get; set; }

    public int MinQuantity { get; set; }

    public int IdCategory { get; set; }

    public string? ImageUrl { get; set; }

    public virtual Category IdCategoryNavigation { get; set; } = null!;

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

    public virtual ICollection<SupplierOrderItem> SupplierOrderItems { get; set; } = new List<SupplierOrderItem>();
}
