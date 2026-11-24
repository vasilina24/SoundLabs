using System;
using System.Collections.Generic;

namespace SoundLabs.Models;

public partial class Part
{
    public int IdPart { get; set; }

    public string Article { get; set; } = null!;

    public string PartName { get; set; } = null!;

    public decimal Price { get; set; }

    public int StockQuantity { get; set; }

    public int MinQuantity { get; set; }

    public virtual ICollection<SupplierOrderItem> SupplierOrderItems { get; set; } = new List<SupplierOrderItem>();

    public virtual ICollection<UsedPart> UsedParts { get; set; } = new List<UsedPart>();
}
