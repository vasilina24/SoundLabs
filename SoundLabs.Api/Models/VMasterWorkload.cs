using System;
using System.Collections.Generic;

namespace SoundLabs.Models;

public partial class VMasterWorkload
{
    public int? IdEmployee { get; set; }

    public string? FullName { get; set; }

    public long? ActiveRepairs { get; set; }

    public long? FinishedRepairs { get; set; }
}
