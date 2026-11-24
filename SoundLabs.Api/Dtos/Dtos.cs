using System.ComponentModel.DataAnnotations;

namespace SoundLabs.Dtos;

// ---- Авторизация ----
public record LoginDto(string Login, string Password);

public class RegisterDto
{
    [StringLength(25, MinimumLength = 3)]
    public string Login { get; set; } = string.Empty;

    [MinLength(6)]
    public string Password { get; set; } = string.Empty;

    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [RegularExpression(@"^\d{11}$", ErrorMessage = "Телефон – 11 цифр")]
    public string Phone { get; set; } = string.Empty;

    [EmailAddress]
    [StringLength(100)]
    public string Email { get; set; } = string.Empty;
}

public record AuthResponse(string Token, string Role, string Login, DateTime ExpiresAt);

// ---- Каталог ----
public record ProductDto(int Id, string Article, string Name, string? Description,
    decimal Price, int Available, int CategoryId, string Category, string? ImageUrl);

public record ProductSaveDto(
    [param: StringLength(30)] string Article,
    [param: StringLength(100)] string Name,
    [param: StringLength(500)] string? Description,
    [param: Range(0, 100000000)] decimal Price,
    [param: Range(0, int.MaxValue)] int StockQuantity,
    [param: Range(0, int.MaxValue)] int MinQuantity,
    int CategoryId);

// ---- Заказы клиентов ----
public class OrderItemInDto
{
    public int ProductId { get; set; }

    [Range(1, 10000)]
    public int Quantity { get; set; }
}

public record CreateOrderDto(
    string ReceiveMethod,
    string? DeliveryAddress,
    List<OrderItemInDto> Items);

public record OrderItemDto(int ProductId, string ProductName, int Quantity, decimal UnitPrice);

public record OrderDto(int Id, DateTime OrderDate, string Status, string ReceiveMethod,string DeliveryAddress,
    decimal TotalAmount, string? RejectReason, DateTime StatusDate,
    int ClientId, string ClientName, List<OrderItemDto> Items);

public record ChangeOrderStatusDto(string Status, string? RejectReason);

// ---- Ремонт ----
public record CreateRepairDto(int? ClientId, int ServiceId,
    [param: StringLength(50)] string InstrumentType,
    [param: StringLength(100)] string InstrumentModel,
    [param: StringLength(500)] string FaultDescription);

public record AssignRepairDto(int MasterId, DateOnly? PlannedDate);

public record RepairMasterDto(int Id, string FullName, string JobTitle);

public record ChangeRepairStatusDto(string Status);

public record AddPartDto(int PartId, [property: Range(1, 10000)] int Quantity);

public record UsedPartDto(int PartId, string PartName, int Quantity, decimal UnitPrice);

public record RepairDto(int Id, DateTime CreatedDate, string InstrumentType, string InstrumentModel,
    string FaultDescription, string Status, DateTime StatusDate, DateOnly? PlannedDate,
    DateTime? CompletedDate, decimal TotalCost, int ClientId, string ClientName,
    int ServiceId, string ServiceName, int? MasterId, string? MasterName,
    List<UsedPartDto> Parts);


// ---- Управление сотрудниками (только администратор) ----
public class CreateEmployeeDto
{
    [Required, StringLength(25, MinimumLength = 3)] public string Login { get; set; } = string.Empty;
    [Required, MinLength(6), StringLength(100)] public string Password { get; set; } = string.Empty;
    [Required, StringLength(100)] public string FullName { get; set; } = string.Empty;
    [Required, RegularExpression(@"^\d{11}$", ErrorMessage = "Телефон должен содержать 11 цифр")] public string Phone { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(100)] public string Email { get; set; } = string.Empty;
    [Required] public string Role { get; set; } = string.Empty;
}
public class UpdateEmployeeDto
{
    [Required, StringLength(25, MinimumLength = 3)] public string Login { get; set; } = string.Empty;
    [StringLength(100, MinimumLength = 6)] public string? Password { get; set; }
    [Required, StringLength(100)] public string FullName { get; set; } = string.Empty;
    [Required, RegularExpression(@"^\d{11}$", ErrorMessage = "Телефон должен содержать 11 цифр")] public string Phone { get; set; } = string.Empty;
    [Required, EmailAddress, StringLength(100)] public string Email { get; set; } = string.Empty;
    [Required] public string Role { get; set; } = string.Empty;
}
public record EmployeeDto(int Id, string Login, string FullName, string Phone, string Email, string Role, string JobTitle, DateOnly HireDate, bool IsActive);

// ---- Личный профиль ----
public record ProfileDto(string Login, string Role, string FullName, string Phone, string Email, string? JobTitle);
public class UpdateProfileDto
{
    [Required, StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{11}$", ErrorMessage = "Телефон должен содержать 11 цифр")]
    public string Phone { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(100)]
    public string Email { get; set; } = string.Empty;
}
