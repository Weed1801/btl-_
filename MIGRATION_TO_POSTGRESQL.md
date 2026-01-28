# Thay Đổi từ SQL Server sang PostgreSQL

## Tóm Tắt Các Thay Đổi

Dự án đã được cấu hình để sử dụng **PostgreSQL** thay vì SQL Server.

### 1. Các File Đã Cập Nhật

#### QuanLyChoThuePhongTro.csproj
```xml
<!-- Cũ -->
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.0" />

<!-- Mới -->
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.0" />
```

#### appsettings.json
```json
<!-- Cũ -->
"DefaultConnection": "Server=.;Database=QuanLyChoThuePhongTro;Trusted_Connection=true;TrustServerCertificate=true;"

<!-- Mới -->
"DefaultConnection": "Server=localhost;Port=5432;Database=quan_ly_cho_thue_phong_tro;User Id=postgres;Password=password;"
```

#### Program.cs
```csharp
<!-- Cũ -->
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

<!-- Mới -->
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(connectionString));
```

### 2. Các File Tài Liệu Mới

| File | Nội dung |
|------|---------|
| **SETUP_GUIDE.md** | Hướng dẫn chi tiết cài đặt .NET, PostgreSQL, VS Code |
| **init_database.sql** | Script tạo database PostgreSQL |
| **setup.bat** | Script cài đặt nhanh (Windows) |
| **setup.sh** | Script cài đặt nhanh (Linux/macOS) |
| **.env.example** | Ví dụ cấu hình biến môi trường |

### 3. Ưu Điểm của PostgreSQL

✅ **Open Source**: Miễn phí, mã nguồn mở  
✅ **Hiệu suất cao**: Xử lý tốt với dữ liệu lớn  
✅ **Hỗ trợ tốt**: Cộng đồng lớn, tài liệu đầy đủ  
✅ **Khả năng mở rộng**: Hỗ trợ JSON, Array, Full-text search  
✅ **An toàn**: ACID compliant, transaction support tốt  
✅ **Đa nền tảng**: Windows, macOS, Linux  

---

## Bước Tiếp Theo

### 1. Cài Đặt Công Nghệ

Theo dõi **SETUP_GUIDE.md** để cài đặt:
- .NET 8.0 SDK
- PostgreSQL
- VS Code (tùy chọn)

### 2. Cấu Hình Database

```bash
# Chạy script khởi tạo database
psql -U postgres -f init_database.sql

# Hoặc tạo thủ công
psql -U postgres
CREATE DATABASE quan_ly_cho_thue_phong_tro;
\q
```

### 3. Cập Nhật Connection String

Chỉnh sửa `appsettings.json`:
```json
"DefaultConnection": "Server=localhost;Port=5432;Database=quan_ly_cho_thue_phong_tro;User Id=postgres;Password=YOUR_POSTGRES_PASSWORD;"
```

### 4. Tạo Database Schema

```bash
# Restore dependencies
dotnet restore

# Tạo migration
dotnet ef migrations add InitialCreate

# Áp dụng migration
dotnet ef database update
```

### 5. Chạy Ứng Dụng

```bash
dotnet run
# hoặc
dotnet watch
```

---

## Tài Liệu Tham Khảo

📚 **Tài liệu chính:**
- [SETUP_GUIDE.md](SETUP_GUIDE.md) - Hướng dẫn chi tiết
- [README.md](README.md) - Tổng quan dự án
- [init_database.sql](init_database.sql) - Script database

🔗 **Liên kết bên ngoài:**
- [.NET 8.0 Documentation](https://learn.microsoft.com/en-us/dotnet/)
- [Entity Framework Core - PostgreSQL](https://www.npgsql.org/efcore/)
- [PostgreSQL Official](https://www.postgresql.org/)
- [Npgsql Documentation](https://www.npgsql.org/)

---

## Hỗ Trợ

Nếu gặp vấn đề, vui lòng:

1. **Kiểm tra SETUP_GUIDE.md** - phần Troubleshooting
2. **Kiểm tra kết nối PostgreSQL**: `psql -U postgres -h localhost`
3. **Kiểm tra appsettings.json** - xác nhận connection string
4. **Clean & rebuild**: `dotnet clean && dotnet build`

---

**Cập nhật**: Tháng 1 năm 2026  
**Phiên bản**: 1.0  
**Trạng thái**: ✅ Sẵn sàng sử dụng PostgreSQL
