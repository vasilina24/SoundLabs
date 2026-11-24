using System;
using System.Collections.Generic;

namespace SoundLabs.Models;

public partial class Delivery
{
    public int IdDelivery { get; set; }

    public DateTime DeliveryDate { get; set; }

    public string InvoiceNumber { get; set; } = null!;

    public int IdSupplierOrder { get; set; }

    public virtual SupplierOrder IdSupplierOrderNavigation { get; set; } = null!;
}
