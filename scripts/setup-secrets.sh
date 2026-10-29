#!/usr/bin/env bash
set -euo pipefail
cat <<'HELP'
v3: AppHost tự cấp mật khẩu PostgreSQL, truyền connection strings và cấu hình JWT.
Không ghi secret hoặc connection strings có mật khẩu vào Git.
Nếu muốn đặt mật khẩu PostgreSQL cố định, dùng user-secrets của AppHost:
  dotnet user-secrets set 'Parameters:postgres-password' '<mật-khẩu-local>' --project src/SimpleStore.AppHost
  dotnet user-secrets set 'Parameters:jwt-key' "$(openssl rand -base64 32)" --project src/SimpleStore.AppHost
Nếu chạy riêng: Catalog API nhận ConnectionStrings__catalogdb cùng Jwt__Key, Jwt__Issuer,
Jwt__Audience; Identity API nhận ConnectionStrings__identitydb và cùng bộ Jwt__*; Web/Admin
nhận ConnectionStrings__orderdb, services:catalog-api:http:0, services:identity-api:http:0
và cùng bộ Jwt__*. Không dùng key minh họa ở production.
EF CLI design-time đọc các biến môi trường trên; fallback chỉ để sinh schema, không có mật khẩu.
HELP
