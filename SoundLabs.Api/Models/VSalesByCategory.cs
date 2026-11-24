using System;
using System.Collections.Generic;

namespace SoundLabs.Models;

public partial class VSalesByCategory
{
    public string? CategoryName { get; set; }

    public long? ItemsSold { get; set; }

    public decimal? Revenue { get; set; }
}
