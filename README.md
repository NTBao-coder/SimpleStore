# SimpleStore (bản dựng lại để học)

Bản dựng lại **từng chương một** của kiến trúc microservices tham khảo
[`daohainam/simple-store`](https://github.com/daohainam/simple-store) (MIT):
.NET 10, .NET Aspire, PostgreSQL, Redis, RabbitMQ + MassTransit, KurrentDB, YARP.

Mục tiêu không phải là sao chép, mà là **hiểu từng quyết định kiến trúc**: mỗi chương
(v0 → v12) là đúng **một commit + một tag**, kèm ghi chú học tập trong `docs/chapters/`.

## Trạng thái

Mốc hiện tại: **v3 — Identity API và JWT**. Identity API sở hữu `identitydb`, phát access token
và refresh token. Web/Admin đăng nhập qua HTTP; trình duyệt giữ cookie phiên, còn token được lưu
trong cache phía server. Catalog API xác thực JWT cho thao tác ghi. Order vẫn dùng DbContext trực tiếp.
Xem [ghi chú v3](docs/chapters/v3.md) và [lộ trình](docs/ROADMAP.md).

## Yêu cầu

- .NET 10 SDK **10.0.401** (hoặc patch mới hơn trong cùng feature band, xem `global.json`)
- Docker Desktop từ **v0**; `v-skeleton` chưa có container nên không cần Docker.

## Chạy thử v3

Bật Docker và tạo JWT key cục bộ trước:

```bash
dotnet user-secrets set 'Parameters:jwt-key' "$(openssl rand -base64 32)" --project src/SimpleStore.AppHost
```

Sau đó chạy:

```bash
dotnet build SimpleStore.slnx -c Release
dotnet run --project src/SimpleStore.AppHost --launch-profile http
```

AppHost tạo PostgreSQL với ba database, PgWeb, Identity API, Catalog API, Web và Admin. Mở URL Web/Admin
trong dashboard Aspire (`http://localhost:15235`, token đăng nhập được in ở console).
Catalog API migrate/seed catalogdb; Identity API migrate/seed identitydb, role và tài khoản demo.
Web chỉ migrate orderdb. Web/Admin gọi Identity API qua HTTP.
Chờ Web khởi tạo xong trước khi mở Admin.

Tài khoản học thử: `demo@simplestore.local` / `Demo123!` (Customer) và
`admin@simplestore.local` / `Admin123!` (Admin), chỉ là seed data, không dùng production.
`/health` và `/alive` chỉ bật ở Development. Aspire cấp riêng catalogdb/identitydb cho service sở hữu dữ liệu
và địa chỉ Identity/Catalog API cho Web/Admin;
không ghi mật khẩu PostgreSQL vào Git. Xem `scripts/setup-secrets.sh`.

v1 tạo schema mới, **không tự chuyển dữ liệu từ `storedb`**. Database cũ không bị xóa.
Nếu cần giữ dữ liệu v0, phải lập kế hoạch chuyển dữ liệu riêng trước khi đổi môi trường.

## Cấu trúc

```
src/
├── SimpleStore.AppHost            # Aspire orchestration
├── SimpleStore.ServiceDefaults    # service discovery, resilience, health, OpenTelemetry
├── SimpleStore.Contracts          # integration events dùng chung (từ v6)
├── SimpleStore.Data               # TẠM THỜI: OrderDbContext dùng chung (v0–v3), xoá ở v4
├── SimpleStore.Gateway            # YARP (v5)
├── SimpleStore.{Identity,Catalog,Order,Cart,Inventory,Payment}.API (+ .API.Client)
├── SimpleStore.Checkout.API       # saga orchestrator (v8)
├── SimpleStore.Web                # storefront MVC (v0)
└── SimpleStore.Admin              # Blazor Server (v0)
```

## Ghi công

Thiết kế, tên chương và nhiều chi tiết kỹ thuật đến từ
[`daohainam/simple-store`](https://github.com/daohainam/simple-store) của Dao Hai Nam, giấy phép MIT.
Thông báo bản quyền gốc được giữ nguyên trong [`LICENSE`](LICENSE).

## Quy trình học

Mỗi chương là một commit và một annotated tag. Đọc `AGENTS.md` cùng ROADMAP trước khi sửa mã.
Sau khi hoàn tất v3, dừng chờ yêu cầu “next” để bắt đầu v4.
Chỉ push tới remote của chủ dự án; repository của tác giả là nguồn tham khảo.

## Môi trường kiểm tra hiện tại

.NET SDK 10.0.401 đã có trong PATH. Build Release đạt 0 lỗi; còn cảnh báo NuGet
NU1902/NU1903 cho dependency MessagePack 2.5.192. Chưa có Docker, nên chưa kiểm tra
AppHost/container. PostgreSQL và HTTP smoke checks cho v3 chưa chạy được trong môi trường hiện tại.
Remote `origin` hiện trỏ tới repository cục bộ của chủ dự án; remote `github` là địa chỉ khác.
