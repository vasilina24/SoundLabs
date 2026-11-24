using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace SoundLabs.Models;

public partial class SoundLabsContext : DbContext
{
    public SoundLabsContext(DbContextOptions<SoundLabsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<Delivery> Deliveries { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<EventLog> EventLogs { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderItem> OrderItems { get; set; }

    public virtual DbSet<Part> Parts { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    public virtual DbSet<Repair> Repairs { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<SupplierOrder> SupplierOrders { get; set; }

    public virtual DbSet<SupplierOrderItem> SupplierOrderItems { get; set; }

    public virtual DbSet<SystemSetting> SystemSettings { get; set; }

    public virtual DbSet<UsedPart> UsedParts { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<VCatalog> VCatalogs { get; set; }

    public virtual DbSet<VLowStock> VLowStocks { get; set; }

    public virtual DbSet<VMasterWorkload> VMasterWorkloads { get; set; }

    public virtual DbSet<VSalesByCategory> VSalesByCategories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.IdCategory).HasName("categories_pkey");

            entity.ToTable("categories");

            entity.HasIndex(e => e.CategoryName, "categories_category_name_key").IsUnique();

            entity.Property(e => e.IdCategory)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_category");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(50)
                .HasColumnName("category_name");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
        });

        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.IdClient).HasName("clients_pkey");

            entity.ToTable("clients");

            entity.HasIndex(e => e.Email, "clients_email_key").IsUnique();

            entity.HasIndex(e => e.IdUser, "clients_id_user_key").IsUnique();

            entity.Property(e => e.IdClient)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_client");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.Phone)
                .HasMaxLength(11)
                .IsFixedLength()
                .HasColumnName("phone");
            entity.Property(e => e.RegistrationDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("registration_date");

            entity.HasOne(d => d.IdUserNavigation).WithOne(p => p.Client)
                .HasForeignKey<Client>(d => d.IdUser)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("clients_id_user_fkey");
        });

        modelBuilder.Entity<Delivery>(entity =>
        {
            entity.HasKey(e => e.IdDelivery).HasName("deliveries_pkey");

            entity.ToTable("deliveries");

            entity.HasIndex(e => e.IdSupplierOrder, "idx_deliveries_order");

            entity.Property(e => e.IdDelivery)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_delivery");
            entity.Property(e => e.DeliveryDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("delivery_date");
            entity.Property(e => e.IdSupplierOrder).HasColumnName("id_supplier_order");
            entity.Property(e => e.InvoiceNumber)
                .HasMaxLength(30)
                .HasColumnName("invoice_number");

            entity.HasOne(d => d.IdSupplierOrderNavigation).WithMany(p => p.Deliveries)
                .HasForeignKey(d => d.IdSupplierOrder)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("deliveries_id_supplier_order_fkey");
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.IdEmployee).HasName("employees_pkey");

            entity.ToTable("employees");

            entity.HasIndex(e => e.Email, "employees_email_key").IsUnique();

            entity.HasIndex(e => e.IdUser, "employees_id_user_key").IsUnique();

            entity.Property(e => e.IdEmployee)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_employee");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.HireDate).HasColumnName("hire_date");
            entity.Property(e => e.IdUser).HasColumnName("id_user");
            entity.Property(e => e.JobTitle)
                .HasMaxLength(50)
                .HasColumnName("job_title");
            entity.Property(e => e.Phone)
                .HasMaxLength(11)
                .IsFixedLength()
                .HasColumnName("phone");

            entity.HasOne(d => d.IdUserNavigation).WithOne(p => p.Employee)
                .HasForeignKey<Employee>(d => d.IdUser)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("employees_id_user_fkey");
        });

        modelBuilder.Entity<EventLog>(entity =>
        {
            entity.HasKey(e => e.IdLog).HasName("event_log_pkey");

            entity.ToTable("event_log");

            entity.HasIndex(e => e.EventDate, "idx_event_log_date");

            entity.Property(e => e.IdLog)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_log");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.EventDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("event_date");
            entity.Property(e => e.EventType)
                .HasMaxLength(50)
                .HasColumnName("event_type");
            entity.Property(e => e.IdUser).HasColumnName("id_user");

            entity.HasOne(d => d.IdUserNavigation).WithMany(p => p.EventLogs)
                .HasForeignKey(d => d.IdUser)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("event_log_id_user_fkey");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.IdOrder).HasName("orders_pkey");

            entity.ToTable("orders");

            entity.HasIndex(e => e.IdClient, "idx_orders_client");

            entity.HasIndex(e => e.IdEmployee, "idx_orders_employee");

            entity.HasIndex(e => e.Status, "idx_orders_status");

            entity.Property(e => e.IdOrder)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_order");
            entity.Property(e => e.IdClient).HasColumnName("id_client");
            entity.Property(e => e.IdEmployee).HasColumnName("id_employee");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("order_date");
            entity.Property(e => e.ReceiveMethod)
                .HasMaxLength(20)
                .HasColumnName("receive_method");
            entity.Property(e => e.RejectReason)
                .HasMaxLength(255)
                .HasColumnName("reject_reason");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Новый'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.StatusDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("status_date");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(12, 2)
                .HasColumnName("total_amount");

            entity.HasOne(d => d.IdClientNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.IdClient)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("orders_id_client_fkey");

            entity.HasOne(d => d.IdEmployeeNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.IdEmployee)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("orders_id_employee_fkey");
            entity.Property(e => e.DeliveryAddress)
                .HasColumnName("delivery_address")
                .HasMaxLength(300);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.IdOrderItem).HasName("order_items_pkey");

            entity.ToTable("order_items");

            entity.HasIndex(e => e.IdOrder, "idx_order_items_order");

            entity.HasIndex(e => e.IdProduct, "idx_order_items_product");

            entity.HasIndex(e => new { e.IdOrder, e.IdProduct }, "uq_order_product").IsUnique();

            entity.Property(e => e.IdOrderItem)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_order_item");
            entity.Property(e => e.IdOrder).HasColumnName("id_order");
            entity.Property(e => e.IdProduct).HasColumnName("id_product");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.UnitPrice)
                .HasPrecision(10, 2)
                .HasColumnName("unit_price");

            entity.HasOne(d => d.IdOrderNavigation).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.IdOrder)
                .HasConstraintName("order_items_id_order_fkey");

            entity.HasOne(d => d.IdProductNavigation).WithMany(p => p.OrderItems)
                .HasForeignKey(d => d.IdProduct)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("order_items_id_product_fkey");
        });

        modelBuilder.Entity<Part>(entity =>
        {
            entity.HasKey(e => e.IdPart).HasName("parts_pkey");

            entity.ToTable("parts");

            entity.HasIndex(e => e.Article, "parts_article_key").IsUnique();

            entity.Property(e => e.IdPart)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_part");
            entity.Property(e => e.Article)
                .HasMaxLength(30)
                .HasColumnName("article");
            entity.Property(e => e.MinQuantity)
                .HasDefaultValue(0)
                .HasColumnName("min_quantity");
            entity.Property(e => e.PartName)
                .HasMaxLength(100)
                .HasColumnName("part_name");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.StockQuantity)
                .HasDefaultValue(0)
                .HasColumnName("stock_quantity");
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.IdProduct).HasName("products_pkey");

            entity.ToTable("products");

            entity.HasIndex(e => e.IdCategory, "idx_products_category");

            entity.HasIndex(e => e.Article, "products_article_key").IsUnique();

            entity.Property(e => e.IdProduct)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_product");
            entity.Property(e => e.Article)
                .HasMaxLength(30)
                .HasColumnName("article");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.IdCategory).HasColumnName("id_category");
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
            entity.Property(e => e.MinQuantity)
                .HasDefaultValue(0)
                .HasColumnName("min_quantity");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.ProductName)
                .HasMaxLength(100)
                .HasColumnName("product_name");
            entity.Property(e => e.ReservedQuantity)
                .HasDefaultValue(0)
                .HasColumnName("reserved_quantity");
            entity.Property(e => e.StockQuantity)
                .HasDefaultValue(0)
                .HasColumnName("stock_quantity");

            entity.HasOne(d => d.IdCategoryNavigation).WithMany(p => p.Products)
                .HasForeignKey(d => d.IdCategory)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("products_id_category_fkey");
        });

        modelBuilder.Entity<Repair>(entity =>
        {
            entity.HasKey(e => e.IdRepair).HasName("repairs_pkey");

            entity.ToTable("repairs");

            entity.HasIndex(e => e.IdClient, "idx_repairs_client");

            entity.HasIndex(e => e.IdMaster, "idx_repairs_master");

            entity.HasIndex(e => e.Status, "idx_repairs_status");

            entity.Property(e => e.IdRepair)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_repair");
            entity.Property(e => e.CompletedDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("completed_date");
            entity.Property(e => e.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_date");
            entity.Property(e => e.FaultDescription)
                .HasMaxLength(500)
                .HasColumnName("fault_description");
            entity.Property(e => e.IdClient).HasColumnName("id_client");
            entity.Property(e => e.IdMaster).HasColumnName("id_master");
            entity.Property(e => e.IdService).HasColumnName("id_service");
            entity.Property(e => e.InstrumentModel)
                .HasMaxLength(100)
                .HasColumnName("instrument_model");
            entity.Property(e => e.InstrumentType)
                .HasMaxLength(50)
                .HasColumnName("instrument_type");
            entity.Property(e => e.PlannedDate).HasColumnName("planned_date");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Новая'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.StatusDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("status_date");
            entity.Property(e => e.TotalCost)
                .HasPrecision(12, 2)
                .HasColumnName("total_cost");

            entity.HasOne(d => d.IdClientNavigation).WithMany(p => p.Repairs)
                .HasForeignKey(d => d.IdClient)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("repairs_id_client_fkey");

            entity.HasOne(d => d.IdMasterNavigation).WithMany(p => p.Repairs)
                .HasForeignKey(d => d.IdMaster)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("repairs_id_master_fkey");

            entity.HasOne(d => d.IdServiceNavigation).WithMany(p => p.Repairs)
                .HasForeignKey(d => d.IdService)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("repairs_id_service_fkey");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.IdRole).HasName("roles_pkey");

            entity.ToTable("roles", tb => tb.HasComment("Роли пользователей: Администратор, Менеджер, Мастер, Покупатель"));

            entity.HasIndex(e => e.RoleName, "roles_role_name_key").IsUnique();

            entity.Property(e => e.IdRole)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_role");
            entity.Property(e => e.RoleName)
                .HasMaxLength(30)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.IdService).HasName("services_pkey");

            entity.ToTable("services");

            entity.HasIndex(e => e.ServiceName, "services_service_name_key").IsUnique();

            entity.Property(e => e.IdService)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_service");
            entity.Property(e => e.Description)
                .HasMaxLength(500)
                .HasColumnName("description");
            entity.Property(e => e.DurationDays).HasColumnName("duration_days");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.ServiceName)
                .HasMaxLength(100)
                .HasColumnName("service_name");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.IdSupplier).HasName("suppliers_pkey");

            entity.ToTable("suppliers");

            entity.HasIndex(e => e.SupplierName, "suppliers_supplier_name_key").IsUnique();

            entity.Property(e => e.IdSupplier)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_supplier");
            entity.Property(e => e.Address)
                .HasMaxLength(200)
                .HasColumnName("address");
            entity.Property(e => e.ContactPerson)
                .HasMaxLength(100)
                .HasColumnName("contact_person");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.Phone)
                .HasMaxLength(11)
                .IsFixedLength()
                .HasColumnName("phone");
            entity.Property(e => e.SupplierName)
                .HasMaxLength(100)
                .HasColumnName("supplier_name");
        });

        modelBuilder.Entity<SupplierOrder>(entity =>
        {
            entity.HasKey(e => e.IdSupplierOrder).HasName("supplier_orders_pkey");

            entity.ToTable("supplier_orders");

            entity.HasIndex(e => e.IdSupplier, "idx_sorders_supplier");

            entity.Property(e => e.IdSupplierOrder)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_supplier_order");
            entity.Property(e => e.IdEmployee).HasColumnName("id_employee");
            entity.Property(e => e.IdSupplier).HasColumnName("id_supplier");
            entity.Property(e => e.OrderDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("order_date");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValueSql("'Оформлен'::character varying")
                .HasColumnName("status");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(12, 2)
                .HasColumnName("total_amount");

            entity.HasOne(d => d.IdEmployeeNavigation).WithMany(p => p.SupplierOrders)
                .HasForeignKey(d => d.IdEmployee)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("supplier_orders_id_employee_fkey");

            entity.HasOne(d => d.IdSupplierNavigation).WithMany(p => p.SupplierOrders)
                .HasForeignKey(d => d.IdSupplier)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("supplier_orders_id_supplier_fkey");
        });

        modelBuilder.Entity<SupplierOrderItem>(entity =>
        {
            entity.HasKey(e => e.IdSupplierItem).HasName("supplier_order_items_pkey");

            entity.ToTable("supplier_order_items");

            entity.HasIndex(e => e.IdSupplierOrder, "idx_sitems_order");

            entity.Property(e => e.IdSupplierItem)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_supplier_item");
            entity.Property(e => e.IdPart).HasColumnName("id_part");
            entity.Property(e => e.IdProduct).HasColumnName("id_product");
            entity.Property(e => e.IdSupplierOrder).HasColumnName("id_supplier_order");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.ReceivedQuantity)
                .HasDefaultValue(0)
                .HasColumnName("received_quantity");
            entity.Property(e => e.UnitPrice)
                .HasPrecision(10, 2)
                .HasColumnName("unit_price");

            entity.HasOne(d => d.IdPartNavigation).WithMany(p => p.SupplierOrderItems)
                .HasForeignKey(d => d.IdPart)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("supplier_order_items_id_part_fkey");

            entity.HasOne(d => d.IdProductNavigation).WithMany(p => p.SupplierOrderItems)
                .HasForeignKey(d => d.IdProduct)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("supplier_order_items_id_product_fkey");

            entity.HasOne(d => d.IdSupplierOrderNavigation).WithMany(p => p.SupplierOrderItems)
                .HasForeignKey(d => d.IdSupplierOrder)
                .HasConstraintName("supplier_order_items_id_supplier_order_fkey");
        });

        modelBuilder.Entity<SystemSetting>(entity =>
        {
            entity.HasKey(e => e.IdSetting).HasName("system_settings_pkey");

            entity.ToTable("system_settings");

            entity.HasIndex(e => e.ParamName, "system_settings_param_name_key").IsUnique();

            entity.Property(e => e.IdSetting)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_setting");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .HasColumnName("description");
            entity.Property(e => e.ParamName)
                .HasMaxLength(50)
                .HasColumnName("param_name");
            entity.Property(e => e.ParamValue)
                .HasMaxLength(255)
                .HasColumnName("param_value");
        });

        modelBuilder.Entity<UsedPart>(entity =>
        {
            entity.HasKey(e => e.IdUsedPart).HasName("used_parts_pkey");

            entity.ToTable("used_parts");

            entity.HasIndex(e => e.IdRepair, "idx_used_parts_repair");

            entity.Property(e => e.IdUsedPart)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_used_part");
            entity.Property(e => e.IdPart).HasColumnName("id_part");
            entity.Property(e => e.IdRepair).HasColumnName("id_repair");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.UnitPrice)
                .HasPrecision(10, 2)
                .HasColumnName("unit_price");

            entity.HasOne(d => d.IdPartNavigation).WithMany(p => p.UsedParts)
                .HasForeignKey(d => d.IdPart)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("used_parts_id_part_fkey");

            entity.HasOne(d => d.IdRepairNavigation).WithMany(p => p.UsedParts)
                .HasForeignKey(d => d.IdRepair)
                .HasConstraintName("used_parts_id_repair_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.IdUser).HasName("users_pkey");

            entity.ToTable("users");

            entity.HasIndex(e => e.IdRole, "idx_users_role");

            entity.HasIndex(e => e.Login, "users_login_key").IsUnique();

            entity.Property(e => e.IdUser)
                .UseIdentityAlwaysColumn()
                .HasColumnName("id_user");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IdRole).HasColumnName("id_role");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LastLogin)
                .HasComment("Обновляется при каждой авторизации, может быть пустым")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_login");
            entity.Property(e => e.Login)
                .HasMaxLength(25)
                .HasColumnName("login");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .HasComment("Хэш пароля (не хранить пароль в открытом виде)")
                .HasColumnName("password");

            entity.HasOne(d => d.IdRoleNavigation).WithMany(p => p.Users)
                .HasForeignKey(d => d.IdRole)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("users_id_role_fkey");
        });

        modelBuilder.Entity<VCatalog>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_catalog");

            entity.Property(e => e.Article)
                .HasMaxLength(30)
                .HasColumnName("article");
            entity.Property(e => e.AvailableQuantity).HasColumnName("available_quantity");
            entity.Property(e => e.CategoryName)
                .HasMaxLength(50)
                .HasColumnName("category_name");
            entity.Property(e => e.IdProduct).HasColumnName("id_product");
            entity.Property(e => e.Price)
                .HasPrecision(10, 2)
                .HasColumnName("price");
            entity.Property(e => e.ProductName)
                .HasMaxLength(100)
                .HasColumnName("product_name");
        });

        modelBuilder.Entity<VLowStock>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_low_stock");

            entity.Property(e => e.Article)
                .HasMaxLength(30)
                .HasColumnName("article");
            entity.Property(e => e.ItemName)
                .HasMaxLength(100)
                .HasColumnName("item_name");
            entity.Property(e => e.ItemType).HasColumnName("item_type");
            entity.Property(e => e.MinQuantity).HasColumnName("min_quantity");
            entity.Property(e => e.StockQuantity).HasColumnName("stock_quantity");
        });

        modelBuilder.Entity<VMasterWorkload>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_master_workload");

            entity.Property(e => e.ActiveRepairs).HasColumnName("active_repairs");
            entity.Property(e => e.FinishedRepairs).HasColumnName("finished_repairs");
            entity.Property(e => e.FullName)
                .HasMaxLength(100)
                .HasColumnName("full_name");
            entity.Property(e => e.IdEmployee).HasColumnName("id_employee");
        });

        modelBuilder.Entity<VSalesByCategory>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("v_sales_by_category");

            entity.Property(e => e.CategoryName)
                .HasMaxLength(50)
                .HasColumnName("category_name");
            entity.Property(e => e.ItemsSold).HasColumnName("items_sold");
            entity.Property(e => e.Revenue).HasColumnName("revenue");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
