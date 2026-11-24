namespace SoundLabs.Infrastructure;

/// <summary>Названия ролей — как в таблице roles.</summary>
public static class Roles
{
    public const string Admin = "Администратор";
    public const string Manager = "Менеджер";
    public const string Master = "Мастер";
    public const string Client = "Покупатель";

    public const string AdminOrManager = Admin + "," + Manager;
    public const string EmployeeRoles = Manager + "," + Master;
}
