# v-skeleton — Khung chạy được trước khi học nghiệp vụ

## Mục tiêu

Tạo nơi thực hành có cấu trúc ổn định: build một solution và khởi động tất cả placeholder bằng Aspire.
Mốc này giúp phân biệt lỗi môi trường với lỗi nghiệp vụ sẽ được thêm vào từ v0.

## Trước / Sau

Trước: repository rỗng, chưa có SDK/project hay lịch sử chương.
Sau: AppHost quản lý mười tiến trình độc lập; mỗi tiến trình dùng ServiceDefaults.

```text
AppHost
  ├─ Identity.API, Catalog.API, Order.API, Cart.API
  ├─ Inventory.API, Payment.API, Checkout.API
  └─ Gateway, Web, Admin
        mỗi project → ServiceDefaults

Chỉ có thư viện rỗng: Data, Contracts, sáu *.API.Client
```

Chưa có luồng gọi giữa các service. Có mười process không có nghĩa là đã giải quyết kiến trúc
microservices: quyền sở hữu dữ liệu, hợp đồng và xử lý lỗi còn phải được học từng bước.

## Những gì thay đổi

- 20 project cùng target `net10.0`, nullable và implicit usings bật trong `Directory.Build.props`.
- Mười web project có `GET /` trả tên cùng nhãn `v-skeleton placeholder`.
- AppHost khai báo mười resource bằng tên kebab-case. Cổng project được cấp động.
- ServiceDefaults gom OpenTelemetry log/metric/trace, service discovery, HTTP resilience và health checks.
- Data, Contracts và API.Client chưa chứa domain model hay dependency nghiệp vụ.
- ROADMAP, AGENTS, mẫu ghi chú và MIT LICENSE giữ ghi công tác giả gốc.

## Quyết định thiết kế và đánh đổi (trade-off)

**Khai báo trước project.** Người học có thể nhìn vị trí tương lai của mỗi phần, đổi lại solution nhiều
project chưa làm việc hữu ích. Tạo project đúng lúc cần sẽ gọn hơn nhưng làm diff cấu trúc lớn ở mỗi chương.
Web và Admin tạm dùng Minimal API; v0 mới đăng ký MVC/Blazor và giao diện thật.

**Dùng chung ServiceDefaults.** Đồng nhất hạ tầng quan sát, giảm cấu hình lặp. Đổi lại thay đổi thư viện
này tác động mọi service. Chỉ chia sẻ hành vi kỹ thuật; domain model không đi vào đây.
HTTP resilience được đăng ký theo baseline Aspire; ứng dụng hiện chưa gọi HTTP ra ngoài.

**Chưa nối dependency.** Chưa có lời gọi thực tế nên chưa thêm `WithReference` hay `WaitFor` giữa service.
Khi có dependency thật, `WithReference` cung cấp địa chỉ/cấu hình còn `WaitFor` điều khiển chờ sẵn sàng.
Khai báo các quan hệ đó sớm sẽ tạo cảm giác sai rằng tính năng đã hoạt động.

**Health chỉ ở Development.** `/alive` chạy self-check, `/health` chạy mọi check đã đăng ký;
hiện chỉ có self-check nên kết quả giống nhau. Đây chưa phải kiểm tra database hoặc khả năng phục vụ nghiệp vụ.
V9 mới mở health mọi môi trường; v10 mới thêm `/ready`.

**HTTP local và không container.** Không cần chứng chỉ tin cậy hoặc Docker để học skeleton.
Dashboard vẫn dùng token của Aspire. Production cần cấu hình transport riêng; các container được đưa vào đúng chương.

**Phiên bản cố định.** SDK trong `global.json`; Aspire 13.5.3, extensions 10.10.0 và OpenTelemetry 1.18.0
bám mốc upstream `1b1ef8cbce79ba9ce16764c0501135e871e66b91`. Tránh floating version để chương có thể tái hiện.
Phần ServiceDefaults được rút gọn từ upstream, giữ MIT LICENSE; không mang instrumentation của các chương sau.

## Cách kiểm tra

```bash
dotnet build SimpleStore.slnx -c Release
dotnet run --project src/SimpleStore.AppHost
```

1. Mở URL dashboard console in ra (kèm token), xác nhận đúng 10 project resources đang chạy.
2. Mở URL từng resource, kiểm tra `/` trả đúng tên project và nhãn placeholder.
3. Trong Development, `/health` và `/alive` trả HTTP 200 với nội dung `Healthy`.
4. Dừng AppHost. Chạy riêng Catalog ở Production:

   ```bash
   dotnet run --project src/SimpleStore.Catalog.API --no-launch-profile -- --urls http://localhost:5100
   curl -i http://localhost:5100/
   curl -i http://localhost:5100/health
   curl -i http://localhost:5100/alive
   ```

   `/` phải trả 200; hai health endpoint phải trả 404.

Kết quả thực thi ngày 08/10/2026 trên macOS arm64:

- Build Release: thành công, **0 lỗi, 1 cảnh báo ASPIRE010**. SDK dùng bộ DCP/dashboard qua NuGet;
  một số tính năng CLI bundle tùy chọn chưa bật. Không suppress cảnh báo.
- AppHost khởi động dashboard và đủ 10 tiến trình project; gọi trực tiếp cả 10 resource:
  `/`, `/health`, `/alive` đều trả 200 và đúng nội dung (30 yêu cầu).
- Catalog chạy riêng ở Production: `/` trả 200; `/health`, `/alive` trả 404.
- Đã dừng AppHost sau kiểm tra. Chưa kiểm tra giao diện dashboard thủ công trong trình duyệt.
- Máy chưa có Docker; skeleton không khai báo container nên vẫn chạy được. Chưa kiểm tra hạ tầng v0 trở đi.
- Runtime báo chưa có chứng chỉ Aspire tin cậy và Data Protection key chưa mã hóa tại chỗ;
  lần chạy này dùng HTTP loopback như cấu hình local, không phải cấu hình triển khai production.
- SDK 10.0.401 cài phục vụ kiểm tra tại `/tmp/simplestore-dotnet`; chưa cài vào PATH toàn máy.
  NuGet cache của lần kiểm tra ở `/tmp/simplestore-nuget`. Các thư mục tạm có thể bị hệ điều hành xóa.
- Repository chưa có `origin`; chưa push. Commit/tag local được tạo sau khi các bước trên đạt.

## Câu hỏi tự kiểm tra

1. Vì sao 20 project chỉ tạo 10 resource ứng dụng trong Aspire?
2. Vì sao một process báo `Healthy` chưa chứng minh rằng đặt hàng hoạt động?
3. `WithReference` khác `WaitFor` ở điểm nào, và vì sao skeleton chưa cần chúng?
4. Vì sao Data chỉ là thư viện tạm, còn Contracts cũng không được chứa mọi domain model?
5. Thêm sẵn YARP hoặc RabbitMQ ở mốc này sẽ làm mất bài học nào của các chương sau?
