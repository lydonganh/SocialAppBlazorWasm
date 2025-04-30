


AuthService xử lý logic xác thực (login, logout, refresh token).
AppPreferences quản lý lưu trữ dữ liệu cục bộ.
AuthHeaderHandler xử lý việc thêm header xác thực vào các request HTTP.
BaseViewModel cung cấp các phương thức tiện ích chung (navigation, error handling, API call wrapper).
LoginViewModel, RegisterViewModel, SavePostViewModel, HomeViewModels mỗi cái xử lý logic giao diện riêng.
Tái sử dụng tốt: BaseViewModel cung cấp các phương thức như MakeApiCall, ShowErrorAlertAsync, NavigationAsync, được các ViewModel khác kế thừa và sử dụng lại.
Dependency Injection: Sử dụng DI để inject các service (HttpClient, IAuthApi, ILogger), giúp dễ dàng thay đổi hoặc mock trong unit test.