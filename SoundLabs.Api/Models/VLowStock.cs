using System;
using System.Collections.Generic;

namespace SoundLabs.Models;

public partial class VLowStock
{
    public string? ItemType { get; set; }

    public string? Article { get; set; }

    public string? ItemName { get; set; }

    public int? StockQuantity { get; set; }

    public int? MinQuantity { get; set; }
}
