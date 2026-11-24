using System;
using System.Collections.Generic;

namespace SoundLabs.Models;

public partial class User
{
    public int IdUser { get; set; }

    public string Login { get; set; } = null!;

    /// <summary>
    /// Хэш пароля (не хранить пароль в открытом виде)
    /// </summary>
    public string Password { get; set; } = null!;

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Обновляется при каждой авторизации, может быть пустым
    /// </summary>
    public DateTime? LastLogin { get; set; }

    public int IdRole { get; set; }

    public virtual Client? Client { get; set; }

    public virtual Employee? Employee { get; set; }

    public virtual ICollection<EventLog> EventLogs { get; set; } = new List<EventLog>();

    public virtual Role IdRoleNavigation { get; set; } = null!;
}
