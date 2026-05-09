# HRM System — Hệ thống Quản lý Nhân sự

## Tech Stack
- **Backend:** ASP.NET Core 8 Web API, Entity Framework Core, SQL Server
- **Frontend:** Vue 3, Vuex, Axios, Element Plus
- **Realtime:** SignalR
- **Cache:** Redis
- **DevOps:** Docker, GitHub Actions

## Tính năng
- Quản lý nhân viên, phòng ban, chức vụ
- Chấm công Check In/Out theo ngày
- Tính lương tự động cuối tháng, xuất Excel
- Thông báo real-time khi có cập nhật lương
- Phân quyền: Admin / Manager / Employee

## Chạy với Docker
```bash
docker-compose up -d
```

## Chạy thủ công

### Backend
```bash
cd backend/HrmSystem.API
dotnet run
```

### Frontend
```bash
cd frontend
npm install
npm run dev
```

## Tài khoản mặc định
| Username | Password  | Role  |
|----------|-----------|-------|
| admin    | Admin@123 | Admin |