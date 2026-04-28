# 🎯 GIAI ĐOẠN 1: FOUNDATION
## Thời gian: Day 1-3
## Mục tiêu: Tạo solution structure và infrastructure cơ bản

---

## 📝 TASK 1.1: TẠO SOLUTION FILE

### Bước 1.1.1: Mở Terminal
Mở PowerShell hoặc CMD tại thư mục:
```
C:\Users\Admin\Desktop\Microservice-Econmmerce
```

### Bước 1.1.2: Tạo Solution
```bash
dotnet new sln -n MicroserviceEcommerce
```

### Bước 1.1.3: Verify
Kiểm tra file `MicroserviceEcommerce.sln` đã được tạo:
```bash
ls
```

**Kết quả mong đợi:** Thấy file `MicroserviceEcommerce.sln`

---

## 📝 TASK 1.2: TẠO CẤU TRÚC THƯ MỤC

### Bước 1.2.1: Tạo thư mục gốc

Chạy từng lệnh sau (copy paste vào terminal):

```bash
mkdir src
mkdir src\buildingblocks
mkdir src\buildingblocks\Core
mkdir src\buildingblocks\Core\Abstractions
mkdir src\buildingblocks\Core\Events
mkdir src\buildingblocks\Core\Exceptions
mkdir src\buildingblocks\Core\Extensions
mkdir src\buildingblocks\Shared
mkdir src\buildingblocks\Shared\DTOs
mkdir src\buildingblocks\Shared\Enums
```

### Bước 1.2.2: Tạo thư mục services

```bash
mkdir src\services
mkdir src\services\identity
mkdir src\services\identity\src
mkdir src\services\product
mkdir src\services\product\src
mkdir src\services\order
mkdir src\services\order\src
mkdir src\services\gateway
mkdir src\services\gateway\src
```

### Bước 1.2.3: Tạo thư mục infrastructure và docs

```bash
mkdir infrastructure
mkdir infrastructure\databases
mkdir infrastructure\rabbitmq
mkdir docs
mkdir docs\architecture
mkdir docs\architecture\api-contracts
mkdir docs\runbook
mkdir scripts
```

### Bước 1.2.4: Verify cấu trúc
```bash
tree /F src
```

**Kết quả mong đợi:** Thấy đầy đủ cây thư mục như đã lên kế hoạch

---

## 📝 TASK 1.3: TẠO DOCKER COMPOSE

### Bước 1.3.1: Tạo file docker-compose.yml

Tạo file tại: `infrastructure/docker-compose.yml`

```yaml
version: '3.8'

services:
  # SQL Server
  sqlserver:
    image: mcr.microsoft.com/mssql/server:2022-latest
    container_name: ecommerce_sqlserver
    environment:
      - ACCEPT_EULA=Y
      - SA_PASSWORD=YourStrong!Passw0rd
      - MSSQL_PID=Developer
    ports:
      - "1433:1433"
    volumes:
      - sqlserver_data:/var/opt/mssql
    healthcheck:
      test: /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P YourStrong!Passw0rd -Q "SELECT 1" -C
      interval: 10s
      timeout: 3s
      retries: 5
    networks:
      - ecommerce-network

  # RabbitMQ
  rabbitmq:
    image: rabbitmq:3-management
    container_name: ecommerce_rabbitmq
    environment:
      - RABBITMQ_DEFAULT_USER=guest
      - RABBITMQ_DEFAULT_PASS=guest
    ports:
      - "5672:5672"
      - "15672:15672"
    volumes:
      - rabbitmq_data:/var/lib/rabbitmq
    healthcheck:
      test: rabbitmq-diagnostics -q ping
      interval: 10s
      timeout: 5s
      retries: 5
    networks:
      - ecommerce-network

volumes:
  sqlserver_data:
  rabbitmq_data:

networks:
  ecommerce-network:
    driver: bridge
```

### Bước 1.3.2: Chạy Docker Compose

```bash
cd infrastructure
docker-compose up -d
```

### Bước 1.3.3: Verify services đang chạy

```bash
docker ps
```

**Kết quả mong đợi:** Thấy 2 containers: `ecommerce_sqlserver` và `ecommerce_rabbitmq`

### Bước 1.3.4: Kiểm tra RabbitMQ Management UI

Mở trình duyệt: http://localhost:15672
- Username: `guest`
- Password: `guest`

---

## ✅ CHECKLIST GIAI ĐOẠN 1

| Task | Status | Ghi chú |
|------|--------|---------|
| 1.1.1 | ⬜ | Mở terminal |
| 1.1.2 | ⬜ | Tạo solution file |
| 1.1.3 | ⬜ | Verify file tồn tại |
| 1.2.1 | ⬜ | Tạo thư mục buildingblocks |
| 1.2.2 | ⬜ | Tạo thư mục services |
| 1.2.3 | ⬜ | Tạo thư mục infrastructure/docs |
| 1.2.4 | ⬜ | Verify cấu trúc |
| 1.3.1 | ⬜ | Tạo docker-compose.yml |
| 1.3.2 | ⬜ | Chạy docker-compose |
| 1.3.3 | ⬜ | Verify docker containers |
| 1.3.4 | ⬜ | Test RabbitMQ UI |

---

## ❓ KHI HOÀN THÀNH

Khi tất cả tasks hoàn thành, reply:
> **"Done Phase 1"**

Tôi sẽ hướng dẫn tiếp **Giai đoạn 2: Building Blocks**