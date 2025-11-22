# Hệ thống Phân cấp, Phân quyền và Đăng nhập/Đăng ký

## Tổng quan

Hệ thống đã được triển khai với các tính năng:
- Đăng ký tài khoản cho Customer (khách hàng)
- Đăng nhập cho cả Customer và Admin/Staff
- Phân quyền theo Role (Admin, Moderator, Customer)
- Authorization Policies linh hoạt

## Cấu trúc Database

### Bảng TbAccount (Admin/Staff)
- `AccountId`: ID tài khoản
- `Username`: Tên đăng nhập
- `Email`: Email
- `Password`: Mật khẩu (đã hash MD5)
- `FullName`: Họ tên
- `Phone`: Số điện thoại
- `RoleId`: ID vai trò (FK đến TbRole)
- `IsActive`: Trạng thái hoạt động
- `LastLogin`: Lần đăng nhập cuối

### Bảng TbCustomer (Khách hàng)
- `CustomerId`: ID khách hàng
- `Username`: Tên đăng nhập
- `Email`: Email
- `Password`: Mật khẩu (đã hash MD5)
- `Phone`: Số điện thoại
- `Birthday`: Ngày sinh
- `Avatar`: Ảnh đại diện
- `IsActive`: Trạng thái hoạt động
- `LastLogin`: Lần đăng nhập cuối

### Bảng TbRole (Vai trò)
- `RoleId`: ID vai trò
- `RoleName`: Tên vai trò (Admin, Moderator, User, Customer)
- `Description`: Mô tả

## Authentication & Authorization

### Authentication Scheme
- Sử dụng Cookie Authentication với scheme mặc định
- Cookie tồn tại 30 ngày
- Sliding expiration: có
- HttpOnly: có
- Secure: SameAsRequest

### Authorization Policies

1. **AdminOnly**: Chỉ Admin
   ```csharp
   [Authorize(Policy = "AdminOnly")]
   ```

2. **ModeratorOnly**: Chỉ Moderator
   ```csharp
   [Authorize(Policy = "ModeratorOnly")]
   ```

3. **AdminOrModerator**: Admin hoặc Moderator
   ```csharp
   [Authorize(Policy = "AdminOrModerator")]
   ```

4. **CustomerOnly**: Chỉ Customer
   ```csharp
   [Authorize(Policy = "CustomerOnly")]
   ```

5. **AuthenticatedUser**: Bất kỳ user đã đăng nhập
   ```csharp
   [Authorize(Policy = "AuthenticatedUser")]
   ```

6. **RequireLogin**: Yêu cầu đăng nhập (bất kỳ role)
   ```csharp
   [Authorize(Policy = "RequireLogin")]
   ```

### Sử dụng Role trực tiếp
```csharp
[Authorize(Roles = "Admin")]
[Authorize(Roles = "Admin,Moderator")]
[Authorize(Roles = "Customer")]
```

## Routes

### Customer Routes
- `/Account/Login` - Đăng nhập
- `/Account/Register` - Đăng ký
- `/Account/Logout` - Đăng xuất
- `/Account/AccessDenied` - Trang từ chối truy cập

### Admin Routes
- `/Admin/Account/Login` - Đăng nhập Admin
- `/Admin/Dashboard` - Trang quản trị (yêu cầu Admin/Moderator)

## ViewModels

### LoginViewModel
```csharp
- Email: string (required, email format)
- Password: string (required)
- RememberMe: bool
```

### RegisterViewModel
```csharp
- Username: string (required, 3-50 chars)
- Email: string (required, email format)
- Password: string (required, 6-100 chars)
- ConfirmPassword: string (required, must match Password)
- Phone: string (optional, phone format)
- Birthday: DateTime? (optional)
- FullName: string (optional, max 50 chars)
```

## Helper Classes

### UserHelper (Ultilities/UserHelper.cs)
Các phương thức tiện ích để kiểm tra quyền:

```csharp
// Kiểm tra role
UserHelper.IsAdmin(User)
UserHelper.IsModerator(User)
UserHelper.IsCustomer(User)
UserHelper.IsAdminOrModerator(User)

// Lấy thông tin user
UserHelper.GetUserId(User)
UserHelper.GetUserEmail(User)
UserHelper.GetUserFullName(User)
UserHelper.GetUserType(User) // "Account" hoặc "Customer"
UserHelper.GetRoleId(User) // Chỉ cho Account
```

## Claims được tạo khi đăng nhập

### Cho Customer:
- `ClaimTypes.NameIdentifier`: CustomerId
- `ClaimTypes.Name`: Username
- `ClaimTypes.Email`: Email
- `ClaimTypes.Role`: "Customer"
- `UserType`: "Customer"
- `FullName`: Username
- `Phone`: Phone

### Cho Admin/Staff:
- `ClaimTypes.NameIdentifier`: AccountId
- `ClaimTypes.Name`: FullName hoặc Email
- `ClaimTypes.Email`: Email
- `ClaimTypes.Role`: RoleName (Admin, Moderator, User)
- `UserType`: "Account"
- `FullName`: FullName
- `RoleId`: RoleId

## Views

### Customer Views
- `Views/Account/Index.cshtml` - Trang đăng nhập
- `Views/Account/Register.cshtml` - Trang đăng ký
- `Views/Account/AccessDenied.cshtml` - Trang từ chối truy cập

### Shared Views
- `Views/Shared/_UserMenu.cshtml` - Menu người dùng (hiển thị trong header)

## Cách sử dụng

### 1. Bảo vệ Controller/Action với Role
```csharp
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    // Chỉ Admin mới truy cập được
}
```

### 2. Bảo vệ với Policy
```csharp
[Authorize(Policy = "AdminOrModerator")]
public IActionResult ManageProducts()
{
    // Admin hoặc Moderator mới truy cập được
}
```

### 3. Kiểm tra quyền trong View
```razor
@if (User.IsInRole("Admin"))
{
    <a href="/Admin">Quản trị</a>
}

@using Harmic.Ultilities
@if (UserHelper.IsAdminOrModerator(User))
{
    <a href="/Admin">Quản trị</a>
}
```

### 4. Kiểm tra quyền trong Controller
```csharp
using Harmic.Ultilities;

if (UserHelper.IsAdmin(User))
{
    // Logic cho Admin
}
```

## Lưu ý

1. **Mật khẩu**: Được hash bằng MD5 (có thể nâng cấp lên BCrypt hoặc Identity sau)
2. **Email/Username**: Customer có thể đăng nhập bằng Email hoặc Username
3. **Tự động đăng nhập**: Sau khi đăng ký thành công, user sẽ tự động được đăng nhập
4. **Validation**: Sử dụng Data Annotations cho validation phía server và client
5. **Anti-Forgery Token**: Tất cả form POST đều có CSRF protection

## Cải tiến có thể thêm

1. Quên mật khẩu / Reset mật khẩu
2. Xác thực email khi đăng ký
3. Two-factor authentication
4. Nâng cấp hash password từ MD5 sang BCrypt
5. Session management tốt hơn
6. Logging và audit trail

