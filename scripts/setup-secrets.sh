#!/usr/bin/env bash
set -euo pipefail
cat <<'HELP'
v2: AppHost tự cấp mật khẩu PostgreSQL và truyền connection strings bằng WithReference.
Không cần JWT ở chương này. Không ghi connection strings có mật khẩu vào Git.
Nếu muốn đặt mật khẩu PostgreSQL cố định, dùng user-secrets của AppHost:
  dotnet user-secrets set 'Parameters:postgres-password' '<mật-khẩu-local>' --project src/SimpleStore.AppHost
Nếu chạy riêng: Catalog API nhận ConnectionStrings__catalogdb; Web/Admin nhận
ConnectionStrings__orderdb và ConnectionStrings__identitydb. Web/Admin còn cần
services:catalog-api:http:0 trỏ tới URL Catalog API (qua cấu hình hoặc biến môi trường).
EF CLI design-time đọc các biến môi trường trên; fallback chỉ để sinh schema, không có mật khẩu.
HELP
