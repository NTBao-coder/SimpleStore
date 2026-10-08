# Lộ trình dựng lại SimpleStore

## Cách sử dụng

Đọc toàn bộ file trước khi sửa mã. Chỉ làm một chương, rồi dừng chờ chủ dự án nói “next”.
Mỗi chương hoàn tất có đúng một commit `chapter(vN): <short title>` và một annotated tag.
`v-skeleton` là mốc chuẩn bị; chưa triển khai nghiệp vụ v0. Các tag học tập này thuộc bản dựng lại,
không cam kết trùng commit/tag upstream. Không rewrite lịch sử đã push.

Nguồn thiết kế: https://github.com/daohainam/simple-store (MIT, Dao Hai Nam).
Mốc tham khảo ban đầu: `1b1ef8cbce79ba9ce16764c0501135e871e66b91`.
Khóa học bổ trợ: https://courses.microservices.vn/microservices/simple-store/ .
Khóa học tổ chức theo vấn đề kiến trúc; các mốc vN dưới đây theo ghi chú thay đổi upstream.
Đây là lộ trình dựng lại được đề xuất từ các nguồn đó, không phải ROADMAP có sẵn của upstream.

## Điều kiện chung để hoàn tất một chương

- `dotnet build SimpleStore.slnx -c Release` thành công, 0 lỗi.
- Khi có Docker, chạy AppHost và smoke checks tương ứng. Nếu không kiểm tra được, ghi rõ giới hạn.
- Ghi chú tiếng Việt theo `docs/chapters/_TEMPLATE.md`, giải thích lựa chọn và đánh đổi, 3–5 câu tự kiểm tra.
- Cập nhật bảng trạng thái cuối file, commit và annotated tag; push khi có remote `origin` của chủ dự án.
- Không đưa secrets vào Git; không triển khai trước chức năng của chương sau.

## v-skeleton — Khung solution

Phạm vi: 20 project .NET 10: AppHost, ServiceDefaults, Contracts, Data, sáu cặp API/Client,
Checkout.API, Gateway, Web và Admin. Mười executable web trả chuỗi placeholder tại `/`.
ServiceDefaults cung cấp discovery, HTTP resilience, OpenTelemetry cơ bản; `/health`, `/alive`
chỉ ở Development. AppHost chạy đúng 10 project resources, chưa khai báo container hoặc JWT.
Web/Admin cũng là placeholder; MVC/Blazor thực sự bắt đầu ở v0.

Chấp nhận: build Release toàn solution; AppHost khởi động 10 resource; GET `/` trả đúng tên;
health Development trả 200, Production trả 404. Không có API nghiệp vụ, EF, YARP hoặc message broker.
Tham khảo: `docs/guide/vi/01-architecture-and-aspire.md`; skeleton là mốc riêng của bản học.

## v0 — Monolith ban đầu

Phạm vi: Web MVC + Admin Blazor Server dùng StoreDbContext chung trong Data; PostgreSQL storedb, Identity, catalog, giỏ hàng và đặt hàng cơ bản.

Chấp nhận: Đăng nhập, xem sản phẩm, đặt đơn, quản trị dữ liệu; khởi động lại vẫn đọc được dữ liệu.

Đọc upstream: `docs/v1-changes.md (phần Before)`.

## v1 — Tách quyền sở hữu dữ liệu

Phạm vi: CatalogDbContext, OrderDbContext, IdentityDbContext và ba database; Web/Admin còn truy cập trực tiếp.

Chấp nhận: Migration độc lập; không có foreign key hoặc SQL join xuyên database.

Đọc upstream: `docs/v1-changes.md`.

## v2 — Tách Catalog API

Phạm vi: Minimal API và typed HTTP client cho Catalog; Web/Admin gọi API.

Chấp nhận: CRUD catalog qua HTTP; Web/Admin không đọc CatalogDbContext.

Đọc upstream: `docs/v2-changes.md`.

## v3 — Tách Identity API

Phạm vi: JWT và luồng xác thực cho Web/Admin; chuyển quyền sở hữu identity sang service.

Chấp nhận: Đăng nhập, refresh/logout; API phân biệt truy cập hợp lệ và trái phép.

Đọc upstream: `docs/v3-changes.md`.

## v4 — Tách Order và Cart

Phạm vi: Order sở hữu orderdb; Cart sở hữu Redis; typed clients; xóa SimpleStore.Data.

Chấp nhận: Đặt/xem đơn và giỏ hàng qua API; frontend không truy cập DbContext.

Đọc upstream: `docs/v4-changes.md`.

## v5 — Cổng vào YARP

Phạm vi: Gateway định tuyến và service discovery; frontend chỉ biết gateway.

Chấp nhận: Các lời gọi frontend đi qua gateway và đến đúng backend.

Đọc upstream: `docs/v5-changes.md`.

## v6 — Giao tiếp bằng sự kiện

Phạm vi: RabbitMQ, MassTransit, integration events trong Contracts; outbox/inbox theo thiết kế upstream.

Chấp nhận: Đặt đơn phát sự kiện, consumer cập nhật trạng thái; thử giao lại message.

Đọc upstream: `docs/v6-changes.md`.

## v7 — Inventory với CQRS/Event Sourcing

Phạm vi: KurrentDB ghi domain events; PostgreSQL làm read model; projector và API tồn kho.

Chấp nhận: Ghi sự kiện, quan sát projection; kiểm tra chống đặt vượt tồn kho.

Đọc upstream: `docs/v7-changes.md`.

## v8 — Checkout saga

Phạm vi: Checkout điều phối đơn hàng và giữ hàng; saga state, timeout và nhánh thất bại.

Chấp nhận: Đơn đủ tồn kho hoàn tất; thiếu hàng hoặc timeout kết thúc đúng trạng thái.

Đọc upstream: `docs/v8-changes.md; checkout-saga.md`.

## v8a — Cải thiện chất lượng

Phạm vi: Áp dụng các cải thiện hiệu năng, validation, type safety và quan sát trong ghi chú upstream.

Chấp nhận: Kiểm tra dữ liệu sai, luồng thành công/thất bại và schema thay đổi của chương.

Đọc upstream: `docs/v8a-changes.md`.

## v8b — Timeout bền vững

Phạm vi: Quartz persistent store cho saga timeout; cấu hình phù hợp số replica.

Chấp nhận: Khởi động lại Checkout khi đang chờ; timeout vẫn được xử lý.

Đọc upstream: `docs/v8b-durable-store-for-saga-timeouts.md`.

## v9 — Khả năng chịu lỗi

Phạm vi: Retry DB/bus, projector reconnect, migration retry, Redis hardening, single-flight refresh; health mọi môi trường.

Chấp nhận: Thử dependency gián đoạn rồi phục hồi; /health và /alive hoạt động ngoài Development.

Đọc upstream: `docs/v9-changes.md`.

## v10 — Quan sát hệ thống

Phạm vi: Bổ sung instrumentation, business metrics, tracing, /ready và YARP active probes.

Chấp nhận: Theo dõi trace xuyên service; phân biệt liveness/readiness khi dependency lỗi.

Đọc upstream: `docs/v10-changes.md`.

## v11 — Phiên bản hóa hợp đồng

Phạm vi: HTTP /api/v1, OpenAPI theo phiên bản, event V1/URN ổn định và domain-event upcaster scaffold.

Chấp nhận: Định tuyến phiên bản đúng; các event đã lưu vẫn deserialize được.

Đọc upstream: `docs/v11-changes.md; versioning.md`.

## v12 — Payment và bù trừ

Phạm vi: Payment sở hữu paymentdb, số dư và ledger; saga thu tiền, giải phóng giữ hàng khi thất bại.

Chấp nhận: Đủ tiền hoàn tất; thiếu tiền hủy đơn và trả tồn kho; giao lại không trừ tiền hai lần.

Đọc upstream: `docs/v12-changes.md; payment-service.md`.

## Trạng thái

| Mốc | Trạng thái | Ghi chú |
|---|---|---|
| v-skeleton | Hoàn tất | Release 0 lỗi; 10 resource và health đã smoke-test; xem [ghi chú](chapters/v-skeleton.md). Chưa push vì thiếu origin. |
| v0 | Chưa bắt đầu | |
| v1 | Chưa bắt đầu | |
| v2 | Chưa bắt đầu | |
| v3 | Chưa bắt đầu | |
| v4 | Chưa bắt đầu | |
| v5 | Chưa bắt đầu | |
| v6 | Chưa bắt đầu | |
| v7 | Chưa bắt đầu | |
| v8 | Chưa bắt đầu | |
| v8a | Chưa bắt đầu | |
| v8b | Chưa bắt đầu | |
| v9 | Chưa bắt đầu | |
| v10 | Chưa bắt đầu | |
| v11 | Chưa bắt đầu | |
| v12 | Chưa bắt đầu | |
