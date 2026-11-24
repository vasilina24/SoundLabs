using System;
using System.Collections.Generic;

namespace SoundLabs.Models;

public partial class Employee
{
    public int IdEmployee { get; set; }

    public string FullName { get; set; } = null!;

    public string JobTitle { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateOnly HireDate { get; set; }

    public int IdUser { get; set; }

    public virtual User IdUserNavigation { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Repair> Repairs { get; set; } = new List<Repair>();

    public virtual ICollection<SupplierOrder> SupplierOrders { get; set; } = new List<SupplierOrder>();
}
