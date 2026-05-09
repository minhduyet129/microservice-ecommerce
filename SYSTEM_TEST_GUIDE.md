# 🧪 SYSTEM TESTING GUIDE

## Mục đích
Test toàn bộ flow của hệ thống từ đầu đến cuối để verify mọi thứ hoạt động đúng.

---

## ⚠️ PREREQUISITES

Trước khi test, đảm bảo:
1. ✅ .NET 8 SDK đã cài đặt
2. ✅ Docker Desktop đã chạy

---

## 🚀 BƯỚC 0: SETUP TỪ ĐẦU

### Bước 0.1: Khởi động Docker Infrastructure

**Start PostgreSQL và RabbitMQ:**
```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce\infrastructure
docker-compose up -d
```

**Kiểm tra Docker đang chạy:**
```bash
docker ps
```
**Expected:** Hiển thị 2 containers: `ecommerce_postgres` và `ecommerce_rabbitmq`

---

### Bước 0.2: Tạo Databases

**Tạo 3 databases cho 3 services:**
```bash
# Tạo identitydb
docker exec ecommerce_postgres psql -U sa -d postgres -c "CREATE DATABASE identitydb;"

# Tạo productdb
docker exec ecommerce_postgres psql -U sa -d postgres -c "CREATE DATABASE productdb;"

# Tạo orderdb
docker exec ecommerce_postgres psql -U sa -d postgres -c "CREATE DATABASE orderdb;"
```

**Kiểm tra databases đã tạo:**
```bash
docker exec ecommerce_postgres psql -U sa -d postgres -c "\l"
```
**Expected:** Hiển thị 3 databases: `identitydb`, `productdb`, `orderdb`

---

### Bước 0.3: Chạy EF Core Migrations

**Mở 3 terminal riêng và chạy migration cho từng service:**

**Terminal 1 - Identity Service (tạo tables):**
```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce\src\services\identity\src\IdentityService.Api
dotnet ef database update
```

**Terminal 2 - Product Service (tạo tables):**
```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce\src\services\product\src\ProductService.Api
dotnet ef database update
```

**Terminal 3 - Order Service (tạo tables):**
```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce\src\services\order\src\OrderService.Api
dotnet ef database update
```

**Kiểm tra tables đã tạo:**
```bash
docker exec ecommerce_postgres psql -U sa -d identitydb -c "\dt"
docker exec ecommerce_postgres psql -U sa -d productdb -c "\dt"
docker exec ecommerce_postgres psql -U sa -d orderdb -c "\dt"
```

---

### Bước 0.4: Build tất cả Services

**Build toàn bộ solution:**
```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce
dotnet build
```

---

### Bước 0.5: Start các Services

**Mở 4 terminal riêng (hoặc chạy background):**

**Terminal 1 - Identity Service (Port 5001):**
```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce
dotnet run --project src/services/identity/src/IdentityService.Api
```

**Terminal 2 - Product Service (Port 5002):**
```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce
dotnet run --project src/services/product/src/ProductService.Api
```

**Terminal 3 - Order Service (Port 5003):**
```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce
dotnet run --project src/services/order/src/OrderService.Api
```

**Terminal 4 - Gateway (Port 5000):**
```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce
dotnet run --project src/services/gateway/src/GatewayService.Api
```

**Kiểm tra các ports đang listen:**
```bash
netstat -ano | Select-String "LISTENING" | Select-String "500"
```
**Expected:** 4 ports: 5000, 5001, 5002, 5003

---

### Bước 0.6: Verify Services Health

**Test từng service:**
```bash
# Test Gateway (5000)
curl http://localhost:5000/api/products

# Test Identity (5001)
curl http://localhost:5001/api/auth/login

# Test Product (5002)
curl http://localhost:5002/api/products

# Test Order (5003)
curl http://localhost:5003/api/orders
```
**Expected:** HTTP 200/400/401 (không phải lỗi kết nối)

---

## ⚠️ LƯU Ý QUAN TRỌNG: PORTS

| Service | Port | URL |
|---------|------|-----|
| Gateway | 5000 | http://localhost:5000 |
| Identity | 5001 | http://localhost:5001 |
| Product | 5002 | http://localhost:5002 |
| Order | 5003 | http://localhost:5003 |

**Lưu ý:** Identity Service chạy trên port **5001** (không phải 5007) theo mặc định trong launchSettings.json

---

## 📝 FULL CURL COMMANDS (COPY VÀO POSTMAN)

### STEP 1: Register User (Tạo tài khoản)

**Bash curl:**
```bash
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "testuser@example.com",
    "password": "Password123!",
    "confirmPassword": "Password123!"
  }'
```

**Expected Response (201):**
```json
{
  "id": "guid",
  "email": "testuser@example.com",
  "message": "User registered successfully"
}
```

---

### STEP 2: Login (Đăng nhập lấy JWT Token)

**Bash curl:**
```bash
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "testuser@example.com",
    "password": "Password123!"
  }'
```

**Expected Response (200):**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresIn": 3600
}
```

**⚠️ QUAN TRỌNG: Copy giá trị "token" để dùng cho các bước sau!**

---

### STEP 3: Create Product (Tạo sản phẩm - Admin)

**Bash curl:**
```bash
curl -X POST http://localhost:5002/api/products \
  -H "Content-Type: application/json" \
  -d '{
    "name": "iPhone 15 Pro",
    "description": "Apple iPhone 15 Pro - 256GB",
    "price": 999.99,
    "stockQuantity": 50,
    "categoryId": "00000000-0000-0000-0000-000000000001"
  }'
```

**Expected Response (201):**
```json
{
  "id": "guid-cua-product",
  "name": "iPhone 15 Pro",
  "description": "Apple iPhone 15 Pro - 256GB",
  "price": 999.99,
  "discountPrice": null,
  "stockQuantity": 50,
  "imageUrl": null,
  "categoryId": "00000000-0000-0000-0000-000000000001",
  "sku": "SKU-ABC12345"
}
```

**⚠️ QUAN TRỌNG: Copy "id" của product để tạo order!**

---

### STEP 4: Get All Products (Lấy danh sách sản phẩm)

**Bash curl:**
```bash
curl -X GET http://localhost:5002/api/products
```

**Expected Response (200):**
```json
[
  {
    "id": "product-id-1",
    "name": "iPhone 15 Pro",
    "description": "Apple iPhone 15 Pro - 256GB",
    "price": 999.99,
    "discountPrice": null,
    "stockQuantity": 50,
    "categoryId": "guid",
    "sku": "SKU-XXX"
  }
]
```

---

### STEP 5: Create Order (Tạo đơn hàng - Cần JWT)

**Thay thế:**
- `{YOUR_JWT_TOKEN}` = token đã copy ở Step 2
- `{PRODUCT_ID}` = product id đã copy ở Step 3

**Bash curl:**
```bash
curl -X POST http://localhost:5003/api/orders \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {YOUR_JWT_TOKEN}" \
  -d '{
    "shippingAddress": "123 Nguyen Van Linh, District 7, Ho Chi Minh City",
    "shippingPhone": "0912345678",
    "items": [
      {
        "productId": "{PRODUCT_ID}",
        "quantity": 2
      }
    ]
  }'
```

**Expected Response (201):**
```json
{
  "id": "order-guid",
  "userId": "user-id",
  "userEmail": "testuser@example.com",
  "status": "Pending",
  "paymentStatus": "Pending",
  "totalAmount": 1999.98,
  "shippingAddress": "123 Nguyen Van Linh, District 7, Ho Chi Minh City",
  "items": [
    {
      "productId": "product-id",
      "productName": "iPhone 15 Pro",
      "unitPrice": 999.99,
      "quantity": 2
    }
  ],
  "createdAt": "2024-01-01T12:00:00Z"
}
```

**⚠️ QUAN TRỌNG: Copy "id" của order để kiểm tra!**

---

### STEP 6: Get Order By ID (Lấy thông tin đơn hàng)

**Thay thế:**
- `{YOUR_JWT_TOKEN}` = token đã copy ở Step 2
- `{ORDER_ID}` = order id đã copy ở Step 5

**Bash curl:**
```bash
curl -X GET http://localhost:5003/api/orders/{ORDER_ID} \
  -H "Authorization: Bearer {YOUR_JWT_TOKEN}"
```

**Expected Response (200):**
```json
{
  "id": "order-guid",
  "userId": "user-id",
  "userEmail": "testuser@example.com",
  "status": "Pending",
  "paymentStatus": "Pending",
  "totalAmount": 1999.98,
  "shippingAddress": "123 Nguyen Van Linh, District 7, Ho Chi Minh City",
  "items": [...],
  "createdAt": "2024-01-01T12:00:00Z"
}
```

---

### STEP 7: Get My Orders (Lấy danh sách đơn hàng của user)

**Thay thế:**
- `{YOUR_JWT_TOKEN}` = token đã copy ở Step 2

**Bash curl:**
```bash
curl -X GET http://localhost:5003/api/orders/my-orders \
  -H "Authorization: Bearer {YOUR_JWT_TOKEN}"
```

**Expected Response (200):**
```json
[
  {
    "id": "order-guid",
    "userId": "user-id",
    "userEmail": "testuser@example.com",
    "status": "Pending",
    "paymentStatus": "Pending",
    "totalAmount": 1999.98,
    "shippingAddress": "123 Nguyen Van Linh, District 7, Ho Chi Minh City",
    "items": [...],
    "createdAt": "2024-01-01T12:00:00Z"
  }
]
```

---

## 🔄 TEST QUA GATEWAY (PORT 5000)

Tất cả các request ở trên cũng có thể test qua Gateway:

### Gateway - Register:
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user2@example.com",
    "password": "Password123!",
    "confirmPassword": "Password123!"
  }'
```

### Gateway - Login:
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user2@example.com",
    "password": "Password123!"
  }'
```

### Gateway - Get Products:
```bash
curl -X GET http://localhost:5000/api/products
```

### Gateway - Create Order (Cần JWT):
```bash
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {YOUR_JWT_TOKEN}" \
  -d '{
    "shippingAddress": "456 Test Street",
    "shippingPhone": "0987654321",
    "items": [
      {
        "productId": "{PRODUCT_ID}",
        "quantity": 1
      }
    ]
  }'
```

---

## 🧪 ADDITIONAL TEST CASES

### Test 1: Login sai password
```bash
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "testuser@example.com",
    "password": "wrongpassword"
  }'
```
**Expected:** 401 Unauthorized

### Test 2: Tạo order với product không tồn tại
```bash
curl -X POST http://localhost:5003/api/orders \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {YOUR_JWT_TOKEN}" \
  -d '{
    "shippingAddress": "Test Address",
    "shippingPhone": "1234567890",
    "items": [
      {
        "productId": "00000000-0000-0000-0000-000000000999",
        "quantity": 1
      }
    ]
  }'
```
**Expected:** 500 Error - "Product not found"

### Test 3: Tạo order không có token
```bash
curl -X POST http://localhost:5003/api/orders \
  -H "Content-Type: application/json" \
  -d '{
    "shippingAddress": "Test Address",
    "shippingPhone": "1234567890",
    "items": [
      {
        "productId": "{PRODUCT_ID}",
        "quantity": 1
      }
    ]
  }'
```
**Expected:** 401 Unauthorized

### Test 4: Get products không tồn tại
```bash
curl -X GET http://localhost:5002/api/products/00000000-0000-0000-0000-000000000999
```
**Expected:** 404 Not Found

---

## 📋 BASH CURL COMMANDS - QUICK REFERENCE

```bash
# 1. Register
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"testuser@example.com","password":"Password123!","confirmPassword":"Password123!"}'

# 2. Login
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"testuser@example.com","password":"Password123!"}'

# 3. Create Product
curl -X POST http://localhost:5002/api/products \
  -H "Content-Type: application/json" \
  -d '{"name":"iPhone 15 Pro","description":"Apple iPhone 15 Pro - 256GB","price":999.99,"stockQuantity":50,"categoryId":"00000000-0000-0000-0000-000000000001"}'

# 4. Get All Products
curl -X GET http://localhost:5002/api/products

# 5. Create Order (thay thế {JWT_TOKEN} và {PRODUCT_ID})
curl -X POST http://localhost:5003/api/orders \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {JWT_TOKEN}" \
  -d '{"shippingAddress":"123 Nguyen Van Linh, District 7, Ho Chi Minh City","shippingPhone":"0912345678","items":[{"productId":"{PRODUCT_ID}","quantity":2}]}'

# 6. Get Order By ID (thay thế {JWT_TOKEN} và {ORDER_ID})
curl -X GET http://localhost:5003/api/orders/{ORDER_ID} \
  -H "Authorization: Bearer {JWT_TOKEN}"

# 7. Get My Orders (thay thế {JWT_TOKEN})
curl -X GET http://localhost:5003/api/orders/my-orders \
  -H "Authorization: Bearer {JWT_TOKEN}"

# Gateway - Get Products
curl -X GET http://localhost:5000/api/products
```

---

## 📊 EXPECTED FLOW DIAGRAM

```
1. Register → 2. Login (get JWT) → 3. Create Product → 4. Get Products
                                                              ↓
                                              5. Create Order (with JWT)
                                                              ↓
                                              6. Get Order Details
                                                              ↓
                                              7. Get My Orders
```

---

## 🔧 TROUBLESHOOTING

### Lỗi: Connection refused
```bash
# Kiểm tra services đang chạy
docker ps
netstat -an | findstr "5000 5001 5002 5003"
```

### Lỗi: 401 Unauthorized mặc dù đã login
- Kiểm tra JWT token có expired không
- Kiểm tra token format: `Bearer {token}`

### Lỗi: Database connection error
```bash
# Kiểm tra PostgreSQL
docker logs ecommerce_postgres
docker exec ecommerce_postgres pg_isready -U sa
```

### Lỗi: EF Core Migration failed
```bash
# Nếu migration không chạy được, tạo migration mới
cd C:\Users\Admin\Desktop\Microservice-Econmmerce\src\services\identity\src\IdentityService.Api
dotnet ef migrations add InitialCreate
dotnet ef database update
```

### Lỗi: gRPC service lỗi "AddGrpc not found"
```bash
# Kiểm tra Program.cs có dòng này chưa:
builder.Services.AddGrpc();
```

### Lỗi: 500 Internal Server Error khi gọi API
```bash
# Kiểm tra logs của service
docker logs ecommerce_identity
docker logs ecommerce_product
docker logs ecommerce_order

# Kiểm tra database tables
docker exec ecommerce_postgres psql -U sa -d identitydb -c "\dt"
docker exec ecommerce_postgres psql -U sa -d productdb -c "\dt"
docker exec ecommerce_postgres psql -U sa -d orderdb -c "\dt"
```

### Lỗi: Service không start được (port đã sử dụng)
```bash
# Kill dotnet processes đang chạy
Get-Process | Where-Object { $_.ProcessName -like "*dotnet*" } | Stop-Process -Force

# Hoặc kill process cụ thể
netstat -ano | Select-String "5002"
# Tìm PID và kill
taskkill /PID <PID> /F
```

---

## ✅ CHECKLIST SETUP + TEST RESULT

### Setup Phase
| Step | Task | Status |
|------|------|--------|
| 0.1 | Start Docker (PostgreSQL + RabbitMQ) | ⬜ |
| 0.2 | Tạo 3 databases | ⬜ |
| 0.3 | Chạy EF Core migrations | ⬜ |
| 0.4 | Build all services | ⬜ |
| 0.5 | Start 4 services | ⬜ |
| 0.6 | Verify services health | ⬜ |

### Test Phase
| Step | Test Case | Status |
|------|-----------|--------|
| 1 | Register user | ⬜ |
| 2 | Login & get JWT | ⬜ |
| 3 | Create product | ⬜ |
| 4 | Get all products | ⬜ |
| 5 | Create order | ⬜ |
| 6 | Get order by ID | ⬜ |
| 7 | Get my orders | ⬜ |
| 8 | Test via Gateway | ⬜ |
| 9 | Error cases | ⬜ |

---

## 📝 POSTMAN COLLECTION (MANUAL)

Nếu muốn tạo Postman Collection:

1. **New Collection**: "Microservice Ecommerce"
2. **Add Request**: Theo các step ở trên
3. **Environment**: Tạo biến `baseUrl` = `http://localhost:5000`
4. **Save Response**: Lưu token vào biến `jwtToken` sau login

---

**Sẵn sàng test! Copy các curl commands ở trên vào Postman và chạy thôi!** 🚀