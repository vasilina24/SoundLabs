namespace SoundLabs.Blazor.Models;

public record Product(int Id, string Article, string Name, string? Description, decimal Price, int Available, int CategoryId, string Category, string? ImageUrl = null);
public record Category(int id, string name, string? description);
public record ShopService(int id, string name, string? description, decimal price, int durationDays);
public record AuthResponse(string Token, string Role, string Login, DateTime ExpiresAt);
public record LoginRequest(string Login, string Password);
public record RegisterRequest(string Login, string Password, string FullName, string Phone, string Email);
public record OrderItemIn(int ProductId, int Quantity);
public record CreateOrderRequest(
    string ReceiveMethod,
    string? DeliveryAddress,
    List<OrderItemIn> Items
);
public record OrderLine(int ProductId, string ProductName, int Quantity, decimal UnitPrice);
public record ShopOrder(int Id, DateTime OrderDate, string Status, string ReceiveMethod, decimal TotalAmount, string? RejectReason, DateTime StatusDate, int ClientId, string ClientName, List<OrderLine> Items);
public record ChangeOrderStatus(string Status, string? RejectReason);
public record OrderNotification(int Id, DateTime CreatedAt, string Message);
public record CreateRepairRequest(int? ClientId, int ServiceId, string InstrumentType, string InstrumentModel, string FaultDescription);
public record UsedPart(int PartId, string PartName, int Quantity, decimal UnitPrice);
public record Repair(int Id, DateTime CreatedDate, string InstrumentType, string InstrumentModel, string FaultDescription, string Status, DateTime StatusDate, DateOnly? PlannedDate, DateTime? CompletedDate, decimal TotalCost, int ClientId, string ClientName, int ServiceId, string ServiceName, int? MasterId, string? MasterName, List<UsedPart> Parts);
public record RepairMaster(int Id, string FullName, string JobTitle);
public record AssignRepairRequest(int MasterId, DateOnly? PlannedDate);
public record ProductSave(string Article, string Name, string? Description, decimal Price, int StockQuantity, int MinQuantity, int CategoryId);

public record ProfileDto(string Login, string Role, string FullName, string Phone, string Email, string? JobTitle);
public record UpdateProfileRequest(string FullName, string Phone, string Email);

public record Employee(int Id, string Login, string FullName, string Phone, string Email, string Role, string JobTitle, DateOnly HireDate, bool IsActive);
public record EmployeeSave(string Login, string? Password, string FullName, string Phone, string Email, string Role);
public class SystemSettingView { public int Id { get; set; } public string Name { get; set; } = ""; public string Value { get; set; } = ""; public string? Description { get; set; } }
public record AdminLog(int Id, DateTime Date, string Type, string Description, string Login);
public record SparePartView(int Id, string Article, string Name, decimal Price, int Stock, int Minimum);
public record SupplierView(int Id, string Name, string Contact, string Phone, string Email, string? Address);
public record SupplierPurchaseView(int Id, string Supplier, DateTime Date, string Status, decimal Total, List<SupplierPurchaseLine> Items);
public record SupplierPurchaseLine(int? PartId, string PartName, int Quantity, int Received, decimal UnitPrice);
public record SparePartSave(string Article, string Name, decimal Price, int StockQuantity, int MinQuantity);
public record SupplierSave(string Name, string ContactPerson, string Phone, string Email, string? Address);
public record PurchaseLineSave(int PartId, int Quantity, decimal UnitPrice);
public record PurchaseSave(int SupplierId, List<PurchaseLineSave> Items);
