using System;
using System.Collections.Generic;

namespace SoundLabs.Models;

public partial class SupplierOrderItem
{
    public int IdSupplierItem { get; set; }

    public int Quantity { get; set; }

    public int ReceivedQuantity { get; set; }

    public decimal UnitPrice { get; set; }

    public int IdSupplierOrder { get; set; }

    public int? IdProduct { get; set; }

    public int? IdPart { get; set; }

    public virtual Part? IdPartNavigation { get; set; }

    public virtual Product? IdProductNavigation { get; set; }

    public virtual SupplierOrder IdSupplierOrderNavigation { get; set; } = null!;
}
