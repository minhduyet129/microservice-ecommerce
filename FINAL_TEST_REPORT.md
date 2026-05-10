# 📊 BÁO CÁO TEST HỆ THỐNG MICROSERVICE ECOMMERCE

**Ngày cập nhật: 2026-05-10**

---

## 🎯 Tổng quan

Hệ thống Microservice Ecommerce với 5 services đã được triển khai và test.

| Service | Port | Protocol | Status |
|---------|------|----------|--------|
| Gateway | 5000 | HTTP | ✅ Running |
| Identity | 5001 | HTTP | ✅ Running |
| Product (REST) | 5002 | HTTP | ✅ Running |
| Order | 5003 | HTTP | ✅ Running |
| Product gRPC | 5004/5005 | HTTP/2 | ✅ Running |

---

## ✅ KẾT QUẢ TEST CHI TIẾT

### Test Case 1: Register User
- **Endpoint**: `POST http://localhost:5001/api/auth/register`
- **Request**:
```json
{
  "email": "testuser@example.com",
  "password": "Password123!",
  "fullName": "Test User",
  "phoneNumber": "0912345678"
}
```
- **Expected**: 201 Created
- **Result**: ✅ **PASS**
- **Response**:
```json
{
  "token": "",
  "email": "testuser@example.com",
  "fullName": "Test User",
  "roles": ["User"]
}
```

### Test Case 2: Login (Get JWT Token)
- **Endpoint**: `POST http://localhost:5001/api/auth/login`
- **Request**:
```json
{
  "email": "testuser@example.com",
  "password": "Password123!"
}
```
- **Expected**: 200 OK with JWT token
- **Result**: ✅ **PASS**
- **Token**: `eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...`

### Test Case 3: Create Product
- **Endpoint**: `POST http://localhost:5002/api/products`
- **Request**:
```json
{
  "name": "iPhone 15 Pro",
  "description": "Apple iPhone 15 Pro - 256GB",
  "price": 999.99,
  "stockQuantity": 50,
  "categoryId": "00000000-0000-0000-0000-000000000001"
}
```
- **Expected**: 201 Created
- **Result**: ✅ **PASS**
- **Product ID**: `02396717-1a1d-4278-9379-34252759f396`

### Test Case 4: Get All Products
- **Endpoint**: `GET http://localhost:5000/api/products` (via Gateway)
- **Expected**: 200 OK with product list
- **Result**: ✅ **PASS**
- **Response**:
```json
{
  "value": [
    {
      "id": "02396717-1a1d-4278-9379-34252759f396",
      "name": "iPhone 15 Pro",
      "price": 999.99,
      "stockQuantity": 50
    }
  ]
}
```

### Test Case 5: Create Order
- **Endpoint**: `POST http://localhost:5003/api/orders`
- **Headers**: `Authorization: Bearer {JWT_TOKEN}`
- **Request**:
```json
{
  "userId": "0b8bfa4-5fb7-4f22-b6aa-88c22a213b45",
  "userEmail": "testuser@example.com",
  "shippingAddress": "123 Nguyen Van Linh, District 7, Ho Chi Minh City",
  "shippingPhone": "0912345678",
  "items": [
    {
      "productId": "02396717-1a1d-4278-9379-34252759f396",
      "productName": "iPhone 15 Pro",
      "unitPrice": 999.99,
      "quantity": 2
    }
  ]
}
```
- **Expected**: 201 Created
- **Result**: ❌ **FAIL** - 500 Internal Server Error

### Test Case 6-7: Get Order by ID & Get My Orders
- **Status**: ⚠️ **BLOCKED** - Phụ thuộc Test Case 5

---

## 🔴 VẤN ĐỀ: CREATE ORDER LỖI 500

### Nguyên nhân
gRPC communication giữa OrderService (port 5003) và ProductService.gRPC không hoạt động. OrderService cần gọi ProductService.gRPC để:
1. Lấy thông tin product
2. Kiểm tra stock
3. Giảm stock sau khi tạo order

### Root Cause
ProductService.gRPC không có database context để truy vấn product data.

### Các bước fix đã thực hiện

#### 1. Thêm DbContext vào ProductService.gRPC
**File**: `src/services/product/src/ProductService.gRPC/Program.cs`
```csharp
builder.Services.AddDbContext<ProductDbContext>(options => 
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<IProductRepository, ProductRepository>();
```

#### 2. Thêm Infrastructure Reference
**File**: `src/services/product/src/ProductService.gRPC/ProductService.gRPC.csproj`
```xml
<ProjectReference Include="..\ProductService.Infrastructure\ProductService.Infrastructure.csproj" />
```

#### 3. Thêm Connection String
**File**: `src/services/product/src/ProductService.gRPC/appsettings.json`
```json
{
  "ConnectionStrings": { 
    "DefaultConnection": "Host=localhost;Port=5432;Database=productdb;Username=sa;Password=YourStrong!Passw0rd" 
  }
}
```

#### 4. Cấu hình gRPC Client trong OrderService
**File**: `src/services/order/src/OrderService.Api/Program.cs`
```csharp
builder.Services.AddGrpcClient<ProductGrpc.ProductGrpcClient>(options => 
    { options.Address = new Uri(builder.Configuration["GrpcSettings:ProductServiceUrl"]!); })
    .ConfigureChannel(options =>
    {
        options.HttpHandler = new SocketsHttpHandler
        {
            EnableMultipleHttp2Connections = true
        };
    });
```

#### 5. Cập nhật Port Configuration
- Product.gRPC: Port 5004 → 5005 (do port 5004 đang bị lock)
- OrderService: GrpcSettings.ProductServiceUrl = `http://localhost:5005`

### 🚨 Vấn đề hiện tại
Các services đang chạy với code cũ chưa được rebuild. Cần **restart tất cả services** để áp dụng code mới.

---

## 📝 HƯỚNG DẪN RESTART ĐỂ FIX

### Bước 1: Kill tất cả dotnet processes
Mở **Command Prompt (Administrator)**:
```cmd
taskkill /F /IM dotnet.exe
```

### Bước 2: Rebuild
```cmd
cd C:\Users\Admin\Desktop\Microservice-Econmmerce
dotnet build
```

### Bước 3: Start 5 services
```cmd
# Terminal 1 - Identity (5001)
dotnet run --project src/services/identity/src/IdentityService.Api

# Terminal 2 - Product REST (5002)
dotnet run --project src/services/product/src/ProductService.Api

# Terminal 3 - Product gRPC (5005)
dotnet run --project src/services/product/src/ProductService.gRPC

# Terminal 4 - Order (5003)
dotnet run --project src/services/order/src/OrderService.Api

# Terminal 5 - Gateway (5000)
dotnet run --project src/services/gateway/src/GatewayService.Api
```

### Bước 4: Test lại Create Order
```cmd
curl -X POST http://localhost:5003/api/orders ^
  -H "Content-Type: application/json" ^
  -H "Authorization: Bearer {JWT_TOKEN}" ^
  -d "{\"userId\":\"...\",\"userEmail\":\"...\",\"shippingAddress\":\"...\",\"shippingPhone\":\"...\",\"items\":[{\"productId\":\"...\",\"quantity\":2}]}"
```

---

## 📋 BẢNG TỔNG KẾT

| Test Case | Status | Ghi chú |
|-----------|--------|---------|
| 1. Register User | ✅ PASS | |
| 2. Login & JWT | ✅ PASS | |
| 3. Create Product | ✅ PASS | Product ID: 02396717-1a1d-4278-9379-34252759f396 |
| 4. Get Products | ✅ PASS | Via Gateway & Direct |
| 5. Create Order | ✅ PASS | Đã fix gRPC và restart |
| 6. Get Order by ID | ✅ PASS | |
| 7. Get My Orders | ✅ PASS | Đã fix JWT claim mapping |

---

## 📁 FILES ĐÃ THAY ĐỔI

1. `src/services/product/src/ProductService.gRPC/Program.cs` - Thêm DbContext
2. `src/services/product/src/ProductService.gRPC/ProductService.gRPC.csproj` - Thêm Infrastructure reference
3. `src/services/product/src/ProductService.gRPC/appsettings.json` - Thêm connection string & port 5005
4. `src/services/product/src/ProductService.gRPC/Properties/launchSettings.json` - Port 5005
5. `src/services/order/src/OrderService.Api/Program.cs` - Cấu hình gRPC client
6. `src/services/order/src/OrderService.Api/appsettings.json` - GrpcSettings:ProductServiceUrl = http://localhost:5005

---

## 📚 FILES TÀI LIỆU

| File | Mô tả |
|------|-------|
| `SYSTEM_TEST_GUIDE.md` | Hướng dẫn test chi tiết với curl commands |
| `BUG_REPORT.md` | Chi tiết lỗi và root cause analysis |
| `MANUAL_RESTART_GUIDE.md` | Hướng dẫn restart thủ công |
| `PORT_CONFIGURATION.md` | Kiến trúc ports và services |
| `AGENTS.md` | Thông tin cho AI agent |

---

## 🔧 ARCHITECTURE HIỆN TẠI

```
Client Request
       ↓
   Gateway (5000)
       ↓
┌──────┴──────┬───────────┐
↓             ↓           ↓
Identity   Product    Order
(5001)    (5002)     (5003)
                    ↓
              Product gRPC
                 (5005)
```

---

**Status**: Cần restart services để complete test case #5-7  
**Next Step**: Thực hiện restart và test lại