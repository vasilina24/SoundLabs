using System;
using System.Collections.Generic;

namespace SoundLabs.Models;

public partial class Supplier
{
    public int IdSupplier { get; set; }

    public string SupplierName { get; set; } = null!;

    public string ContactPerson { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string? Address { get; set; }

    public virtual ICollection<SupplierOrder> SupplierOrders { get; set; } = new List<SupplierOrder>();
}
