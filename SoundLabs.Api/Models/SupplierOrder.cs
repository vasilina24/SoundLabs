using System;
using System.Collections.Generic;

namespace SoundLabs.Models;

public partial class SupplierOrder
{
    public int IdSupplierOrder { get; set; }

    public DateTime OrderDate { get; set; }

    public string Status { get; set; } = null!;

    public decimal TotalAmount { get; set; }

    public int IdSupplier { get; set; }

    public int IdEmployee { get; set; }

    public virtual ICollection<Delivery> Deliveries { get; set; } = new List<Delivery>();

    public virtual Employee IdEmployeeNavigation { get; set; } = null!;

    public virtual Supplier IdSupplierNavigation { get; set; } = null!;

    public virtual ICollection<SupplierOrderItem> SupplierOrderItems { get; set; } = new List<SupplierOrderItem>();
}
