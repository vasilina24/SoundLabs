using System;
using System.Collections.Generic;

namespace SoundLabs.Models;

public partial class Order
{
    public int IdOrder { get; set; }

    public DateTime OrderDate { get; set; }

    public string Status { get; set; } = null!;

    public string ReceiveMethod { get; set; } = null!;
    public string? DeliveryAddress { get; set; }

    public decimal TotalAmount { get; set; }

    public DateTime StatusDate { get; set; }

    public string? RejectReason { get; set; }

    public int IdClient { get; set; }

    public int? IdEmployee { get; set; }

    public virtual Client IdClientNavigation { get; set; } = null!;

    public virtual Employee? IdEmployeeNavigation { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
