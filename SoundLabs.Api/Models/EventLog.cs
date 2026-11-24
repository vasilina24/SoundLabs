using System;
using System.Collections.Generic;

namespace SoundLabs.Models;

public partial class EventLog
{
    public int IdLog { get; set; }

    public DateTime EventDate { get; set; }

    public string EventType { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int? IdUser { get; set; }

    public virtual User? IdUserNavigation { get; set; }
}
