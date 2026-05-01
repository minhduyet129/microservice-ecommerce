# 🎯 GIAI ĐOẠN 1: FOUNDATION (SETUP MÁY & INFRASTRUCTURE)

---

## Bước 1.1: Mở Terminal tại thư mục dự án

```bash
cd C:\Users\Admin\Desktop\Microservice-Econmmerce
```

> **Giải thích:** Di chuyển đến thư mục gốc của dự án nơi bạn sẽ làm việc suốt quá trình.

---

## Bước 1.2: Tạo Solution file

```bash
dotnet new sln -n MicroserviceEcommerce
```

> **Giải thích:** 
> - Solution file là file quản lý tất cả projects trong solution
> - File tạo ra: `MicroserviceEcommerce.sln`
> - Khi bạn thêm project mới, sẽ reference vào solution này

---

## Bước 1.3: Tạo cấu trúc thư mục

```bash
mkdir -p src/buildingblocks/Core/Abstractions
mkdir -p src/buildingblocks/Core/Events
mkdir -p src/buildingblocks/Core/Exceptions
mkdir -p src/buildingblocks/Core/Extensions
mkdir -p src/buildingblocks/Shared/DTOs
mkdir -p src/services/identity/src
mkdir -p src/services/product/src
mkdir -p src/services/order/src
mkdir -p src/services/gateway/src
mkdir -p infrastructure
mkdir -p docs/architecture
mkdir -p scripts
```

> **Giải thích:**
> - `src/buildingblocks/` - Thư mục chứa code dùng chung cho tất cả services (Shared kernel)
> - `src/services/` - Thư mục chứa các microservices (Identity, Product, Order, Gateway)
> - `infrastructure/` - Docker configs, database scripts
> - `docs/` - Tài liệu kiến trúc, API contracts

---

## Bước 1.4: Tạo file docker-compose.yml

Tạo file `infrastructure/docker-compose.yml`:

```yaml
services:
  postgres:
    image: postgres:16-alpine
    container_name: ecommerce_postgres
    environment:
      - POSTGRES_USER=sa
      - POSTGRES_PASSWORD=YourStrong!Passw0rd
      - POSTGRES_DB=postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

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

volumes:
  postgres_data:
  rabbitmq_data:
```

> **Giải thích:**
> - **PostgreSQL**: Database cho tất cả services (đã chuyển từ SQL Server sang vì nhẹ hơn, open source)
> - **RabbitMQ**: Message broker để services giao tiếp với nhau (event-driven)
> - **Volumes**: Để dữ liệu không bị mất khi stop/start container
> - **Ports**: 
>   - 5432: PostgreSQL (để kết nối từ app)
>   - 5672: RabbitMQ AMQP
>   - 15672: RabbitMQ Management UI (web)

---

## Bước 1.5: Chạy Docker Compose

```bash
cd infrastructure
docker-compose up -d
```

> **Giải thích:**
> - `-d` = detached mode (chạy background)
> - Docker sẽ tải images và chạy 2 containers: postgres và rabbitmq
> - Kiểm tra: `docker ps` để xem containers đang chạy

---

## ✅ CHECKLIST GIAI ĐOẠN 1

| Task | Commands | Status |
|------|----------|--------|
| 1.1 | Mở Terminal | ⬜ |
| 1.2 | Tạo Solution | ⬜ |
| 1.3 | Tạo Folders | ⬜ |
| 1.4 | Tạo docker-compose.yml | ⬜ |
| 1.5 | Chạy Docker | ⬜ |

---

## ❓ KHI HOÀN THÀNH

Khi tất cả tasks hoàn thành, reply:
> **"Done Phase 1"**

Tôi sẽ hướng dẫn tiếp **Giai đoạn 2: Building Blocks**