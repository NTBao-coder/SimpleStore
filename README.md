# SimpleStore (bản dựng lại để học)

Bản dựng lại **từng chương một** của kiến trúc microservices tham khảo
[`daohainam/simple-store`](https://github.com/daohainam/simple-store) (MIT):
.NET 10, .NET Aspire, PostgreSQL, Redis, RabbitMQ + MassTransit, KurrentDB, YARP.

Mục tiêu không phải là sao chép, mà là **hiểu từng quyết định kiến trúc**: mỗi chương
(v0 → v12) là đúng **một commit + một tag**, kèm ghi chú học tập trong `docs/chapters/`.

## Trạng thái

Mốc `v-skeleton` chứa **khung solution** (chương `v-skeleton`): toàn bộ project đã có mặt
nhưng đa số chỉ là placeholder. Mỗi chương sau đó "đánh thức" một phần của hệ thống.
Xem lộ trình chi tiết ở [`docs/ROADMAP.md`](docs/ROADMAP.md).

## Yêu cầu

- .NET 10 SDK **10.0.401** (hoặc patch mới hơn trong cùng feature band, xem `global.json`)
- Docker Desktop từ **v0**; `v-skeleton` chưa có container nên không cần Docker.

## Chạy thử khung

```bash
dotnet build SimpleStore.slnx
dotnet run --project src/SimpleStore.AppHost
```

Dashboard Aspire tại URL console in ra sẽ hiện 10 tài nguyên, mỗi cái chỉ trả về chuỗi placeholder ở `/`.
Endpoint `/health` và `/alive` chỉ bật ở môi trường Development (đến chương v9 mới bật mọi môi trường).

AppHost mặc định dùng HTTP loopback để chạy local, không cần trust chứng chỉ HTTPS.
Giữ token đăng nhập dashboard do Aspire tạo; không dùng cấu hình HTTP này để triển khai production.
Cổng dashboard là `15235`; cổng của 10 ứng dụng được Aspire cấp tự động.
Mở từng resource trong dashboard để lấy URL và kiểm tra `/`, `/health`, `/alive`.
Chạy riêng một placeholder (cổng cố định do bạn chọn):

```bash
dotnet run --project src/SimpleStore.Catalog.API --no-launch-profile -- --urls http://localhost:5100
```

Lệnh trên mặc định ở Production: `/` trả placeholder, `/health` và `/alive` trả 404.
Để bật health endpoints khi chạy riêng, đặt `ASPNETCORE_ENVIRONMENT=Development`.
Mốc này chưa cần secrets; `bash scripts/setup-secrets.sh` chỉ giải thích điều đó.
Xem [ghi chú v-skeleton](docs/chapters/v-skeleton.md) và
[khóa học Microservices Journey](https://courses.microservices.vn/microservices/simple-store/).

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
Sau khi hoàn tất `v-skeleton`, dừng chờ yêu cầu “next” để bắt đầu v0.
Chỉ push khi đã cấu hình `origin` là repository của bạn; repository của tác giả là nguồn tham khảo.


## Môi trường kiểm tra hiện tại

Build Release và smoke checks đã đạt; chi tiết/cảnh báo trong [ghi chú chương](docs/chapters/v-skeleton.md).
Máy hiện dùng SDK tạm tại `/tmp/simplestore-dotnet`, chưa có `dotnet` trong PATH toàn máy.
Trong terminal hiện tại có thể dùng:

```bash
export DOTNET_ROOT=/tmp/simplestore-dotnet
export PATH="$DOTNET_ROOT:$PATH"
export DOTNET_CLI_HOME=/tmp/simplestore-cli
export NUGET_PACKAGES=/tmp/simplestore-nuget
dotnet run --project src/SimpleStore.AppHost -c Release --no-build
```

Nếu thư mục tạm bị xóa, cài .NET SDK theo `global.json` rồi chạy các lệnh build/run ở đầu README.
Chưa có Docker hoặc Git remote `origin` trên máy tại thời điểm khởi tạo.
