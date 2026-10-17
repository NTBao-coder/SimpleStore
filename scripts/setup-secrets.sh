#!/usr/bin/env bash
set -euo pipefail
cat <<'HELP'
v1: AppHost tự cấp mật khẩu PostgreSQL và truyền connection strings bằng WithReference.
Không cần JWT ở chương này. Không ghi connection strings có mật khẩu vào Git.
Nếu muốn đặt mật khẩu PostgreSQL cố định, dùng user-secrets của AppHost:
  dotnet user-secrets set 'Parameters:postgres-password' '<mật-khẩu-local>' --project src/SimpleStore.AppHost
Nếu chạy Web/Admin riêng, cấu hình ConnectionStrings:catalogdb, :orderdb, :identitydb
qua user-secrets hoặc biến môi trường ConnectionStrings__<tên-db>.
EF CLI design-time đọc các biến môi trường trên; fallback chỉ để sinh schema, không có mật khẩu.
HELP
