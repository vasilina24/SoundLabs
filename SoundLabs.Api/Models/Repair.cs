using System;
using System.Collections.Generic;

namespace SoundLabs.Models;

public partial class Repair
{
    public int IdRepair { get; set; }

    public DateTime CreatedDate { get; set; }

    public string InstrumentType { get; set; } = null!;

    public string InstrumentModel { get; set; } = null!;

    public string FaultDescription { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime StatusDate { get; set; }

    public DateOnly? PlannedDate { get; set; }

    public DateTime? CompletedDate { get; set; }

    public decimal TotalCost { get; set; }

    public int IdClient { get; set; }

    public int IdService { get; set; }

    public int? IdMaster { get; set; }

    public virtual Client IdClientNavigation { get; set; } = null!;

    public virtual Employee? IdMasterNavigation { get; set; }

    public virtual Service IdServiceNavigation { get; set; } = null!;

    public virtual ICollection<UsedPart> UsedParts { get; set; } = new List<UsedPart>();
}
