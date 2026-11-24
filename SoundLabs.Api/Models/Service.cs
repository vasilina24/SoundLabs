using System;
using System.Collections.Generic;

namespace SoundLabs.Models;

public partial class Service
{
    public int IdService { get; set; }

    public string ServiceName { get; set; } = null!;

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public int DurationDays { get; set; }

    public virtual ICollection<Repair> Repairs { get; set; } = new List<Repair>();
}
