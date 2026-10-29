# SimpleStore.Identity.API.Client

Typed HTTP client và DTO cho Identity API: register, login, refresh, logout, profile và thao tác user của Admin.
Browser app nhận cookie có session ID; token nằm trong cache phía server. `ApiBearerTokenProvider` lấy và refresh
token trước các lời gọi được bảo vệ. Không tham chiếu DbContext hoặc EF Core.
