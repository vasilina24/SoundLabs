using System;
using System.Collections.Generic;

namespace SoundLabs.Models;

public partial class UsedPart
{
    public int IdUsedPart { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public int IdRepair { get; set; }

    public int IdPart { get; set; }

    public virtual Part IdPartNavigation { get; set; } = null!;

    public virtual Repair IdRepairNavigation { get; set; } = null!;
}
