using System;
using System.Collections.Generic;

namespace SoundLabs.Models;

public partial class SystemSetting
{
    public int IdSetting { get; set; }

    public string ParamName { get; set; } = null!;

    public string ParamValue { get; set; } = null!;

    public string? Description { get; set; }
}
