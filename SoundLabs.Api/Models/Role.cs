using System;
using System.Collections.Generic;

namespace SoundLabs.Models;

/// <summary>
/// Роли пользователей: Администратор, Менеджер, Мастер, Покупатель
/// </summary>
public partial class Role
{
    public int IdRole { get; set; }

    public string RoleName { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
