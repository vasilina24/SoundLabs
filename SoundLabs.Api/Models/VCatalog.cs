using System;
using System.Collections.Generic;

namespace SoundLabs.Models;

public partial class VCatalog
{
    public int? IdProduct { get; set; }

    public string? Article { get; set; }

    public string? ProductName { get; set; }

    public string? CategoryName { get; set; }

    public decimal? Price { get; set; }

    public int? AvailableQuantity { get; set; }
}
