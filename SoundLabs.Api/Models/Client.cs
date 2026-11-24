using System;
using System.Collections.Generic;

namespace SoundLabs.Models;

public partial class Client
{
    public int IdClient { get; set; }

    public string FullName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string Email { get; set; } = null!;

    public DateTime RegistrationDate { get; set; }

    public int IdUser { get; set; }

    public virtual User IdUserNavigation { get; set; } = null!;

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

    public virtual ICollection<Repair> Repairs { get; set; } = new List<Repair>();
}
