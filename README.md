# SimpleStore (bản dựng lại để học)

Bản dựng lại **từng chương một** của kiến trúc microservices tham khảo
[`daohainam/simple-store`](https://github.com/daohainam/simple-store) (MIT):
.NET 10, .NET Aspire, PostgreSQL, Redis, RabbitMQ + MassTransit, KurrentDB, YARP.

Mục tiêu không phải là sao chép, mà là **hiểu từng quyết định kiến trúc**: mỗi chương
(v0 → v12) là đúng **một commit + một tag**, kèm ghi chú học tập trong `docs/chapters/`.

## Trạng thái

Mốc hiện tại: **v2 — Catalog API**. Web MVC và Admin Blazor gọi catalog qua
`ICatalogApiClient`; chỉ Catalog API sở hữu `catalogdb`. Identity và Order vẫn dùng
DbContext trực tiếp. Các API/Client khác còn là placeholder.
Xem [ghi chú v2](docs/chapters/v2.md) và [lộ trình](docs/ROADMAP.md).

## Yêu cầu

- .NET 10 SDK **10.0.401** (hoặc patch mới hơn trong cùng feature band, xem `global.json`)
- Docker Desktop từ **v0**; `v-skeleton` chưa có container nên không cần Docker.

## Chạy thử v2

Bật Docker, sau đó chạy:

```bash
dotnet build SimpleStore.slnx -c Release
dotnet run --project src/SimpleStore.AppHost --launch-profile http
```

AppHost tạo PostgreSQL với ba database, PgWeb, Catalog API, Web và Admin. Mở URL Web/Admin
trong dashboard Aspire (`http://localhost:15235`, token đăng nhập được in ở console).
Catalog API migrate/seed catalogdb; Web migrate Order/Identity và seed tài khoản demo.
Chờ Web khởi tạo xong trước khi mở Admin.

Tài khoản học thử: `demo@simplestore.local` / `Demo123!` (seed data, không dùng production).
`/health` và `/alive` chỉ bật ở Development. Aspire truyền catalogdb cho Catalog API và địa chỉ API cho Web/Admin;
không ghi mật khẩu PostgreSQL vào Git. Xem `scripts/setup-secrets.sh`.

v1 tạo schema mới, **không tự chuyển dữ liệu từ `storedb`**. Database cũ không bị xóa.
Nếu cần giữ dữ liệu v0, phải lập kế hoạch chuyển dữ liệu riêng trước khi đổi môi trường.

## Cấu trúc

```
src/
├── SimpleStore.AppHost            # Aspire orchestration
├── SimpleStore.ServiceDefaults    # service discovery, resilience, health, OpenTelemetry
├── SimpleStore.Contracts          # integration events dùng chung (từ v6)
├── SimpleStore.Data               # TẠM THỜI: EF Core dùng chung (v0–v3), xoá ở v4
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
Sau khi hoàn tất v2, dừng chờ yêu cầu “next” để bắt đầu v3.
Chỉ push tới remote của chủ dự án; repository của tác giả là nguồn tham khảo.

## Môi trường kiểm tra hiện tại

.NET SDK 10.0.401 đã có trong PATH. Build Release đạt 0 lỗi; còn cảnh báo NuGet
NU1902/NU1903 cho dependency MessagePack 2.5.192. Chưa có Docker, nên chưa kiểm tra
AppHost/container. Các kiểm tra PostgreSQL và HTTP trực tiếp được ghi ở [chương v2](docs/chapters/v2.md).
Remote `origin` hiện trỏ tới repository cục bộ của chủ dự án; remote `github` là địa chỉ khác.
