# E-Commerce System Design & Testing Project
## Giới thiệu

Dự án này tập trung vào việc phân tích, thiết kế và kiểm thử hệ thống thương mại điện tử thông qua các bài lab thực hành.

Project kết hợp:
- System Design (UML, Architecture, ERD)
- UI Automation Testing (Selenium)
- API Testing (WooCommerce REST API)

Mục tiêu là xây dựng tư duy Full-Stack Tester, hiểu hệ thống từ:

Business Flow → System Design → Testing Implementation

## Mục tiêu
- Thiết kế hệ thống E-commerce ở mức tổng thể
- Hiểu và mô phỏng luồng nghiệp vụ thực tế
Thực hành:
  - UI Automation Testing
  - API Testing
  - Kết nối giữa system design và testing
🏗️ Thiết kế hệ thống
📊 Tài liệu bao gồm:
- **Use Case Diagram**: Xác định các actor (Customer, Admin) và các chức năng chính như login, search, checkout → làm cơ sở xây dựng test scenarios  
- **Sequence Diagram**: Mô tả luồng tương tác giữa User – Backend – Database → hỗ trợ phân tích integration points  
- **System Architecture**: Thiết kế kiến trúc gồm Frontend, Backend, Database → xác định phạm vi kiểm thử (UI, API, DB)  
- **Database Design (ERD)**: Xây dựng các entity (User, Product, Order) và quan hệ → phục vụ kiểm thử dữ liệu  
- **Business Workflow**: Mô tả luồng nghiệp vụ end-to-end → dùng để thiết kế test case và kiểm thử hệ thống  

Các tài liệu này giúp:
- Hiểu luồng hệ thống
- Xác định test points
- Phân tích dependency giữa các module

## Testing Implementation
### UI Automation Testing (Lab 4)

Công nghệ sử dụng:
- Selenium WebDriver
- C# (.NET)
- MSTest

Kịch bản test:
- Mở trang Hasaki
- Tìm kiếm sản phẩm theo SKU
- Thêm sản phẩm vào giỏ hàng
- Xử lý login popup tự động
- Lặp lại với nhiều sản phẩm

Điểm nổi bật:

- Xử lý dynamic UI (Login Modal)
- Tự động hóa workflow mua hàng
- Mô phỏng hành vi người dùng thực tế
   
### API Testing (Lab 5)

Sử dụng WooCommerce REST API

Các API đã test:

- GET: Lấy danh sách sản phẩm
- POST: Tạo sản phẩm mới
- PUT: Cập nhật sản phẩm
- DELETE: Xoá sản phẩm
- GET Orders: Lấy danh sách đơn hàng

Nội dung kiểm thử:

- Status code validation
- JSON response validation
- CRUD operations
  
* QA Perspective

Từ system design, project thực hiện:

Xác định test scenarios cho các luồng:
- Login
- Search
- Checkout
Phân tích risk points:
- Login interruption
- API failure
- Data inconsistency

👉 Kết hợp:

Design → Identify test points
Testing → Validate system behavior

* Công nghệ sử dụng
💻 Development & Testing
C# (.NET)
Selenium WebDriver
MSTest
🌐 API
WooCommerce REST API
JSON
🛠 Tools
ChromeDriver
Git

💡 Kỹ năng thể hiện
- System Design (E-commerce)
- UI Automation Testing
- API Testing (RESTful)
- Phân tích test scenarios
- Hiểu workflow hệ thống

🚀 Hướng phát triển
- Mở rộng automation test coverage
- Tạo Postman collection
- Tích hợp CI/CD testing
- Thực hiện performance testing
