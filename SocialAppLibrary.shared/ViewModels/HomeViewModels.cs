using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Refit;
using SocialAppLibrary.GotShared.Apis;
using SocialAppLibrary.GotShared.Services;
using SocialAppLibrary.GotShared.Dtos;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace SocialAppLibrary.GotShared.ViewModels
{
    /// <summary>
    /// ViewModel cho trang chính (Home), quản lý danh sách bài viết và điều hướng.
    /// Kế thừa BaseViewModel để sử dụng các phương thức tiện ích như điều hướng và xử lý API.
    /// </summary>
    public partial class HomeViewModels : BaseViewModel
    {
        #region Fields

        private readonly IPostApi _postApi;
        private static readonly AuthService authService;
        private int _startIndex = 0;
        private const int PageSize = 10;

        #endregion

        #region Properties

        /// <summary>
        /// Danh sách bài viết hiển thị trên giao diện.
        /// </summary>
        //[ObservableProperty]
        //private ObservableCollection<PostDto> _posts = []; Sử dụng thư viện:
        [ObservableProperty] //Tự động hóa:
        private ObservableCollection<PostDto> _posts = new ObservableCollection<PostDto>();


        #endregion

        #region Constructor

        /// <summary>
        /// Khởi tạo HomeViewModel với dịch vụ API bài viết.
        /// Tự động gọi phương thức lấy danh sách bài viết khi khởi tạo.
        /// </summary>
        /// <param name="postApi">Dịch vụ API để gọi các phương thức liên quan đến bài viết.</param>
        public HomeViewModels
            (IPostApi postApi, AuthService authService, ILogger<BaseViewModel> logger)
            : base(authService, logger)
        {
            _postApi = postApi ?? throw new ArgumentNullException(nameof(postApi));
            Debug.WriteLine("✅ [HomeViewModel] Khởi tạo HomeViewModel");

            // Tải danh sách bài viết ban đầu
            FetchPostsAsync().GetAwaiter().GetResult();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Lấy danh sách bài viết từ API và cập nhật vào danh sách hiển thị.
        /// Hỗ trợ phân trang và làm mới (pull-to-refresh).
        /// </summary>
        /// <returns>Task hoàn thành khi danh sách bài viết được cập nhật.</returns>
        [RelayCommand]
        private async Task FetchPostsAsync()
        {
            await MakeApiCall(async () =>
            {
                try
                {
                    Debug.WriteLine($"ℹ️ [FetchPostsAsync] Bắt đầu lấy bài viết: startIndex={_startIndex}, pageSize={PageSize}");
                    var posts = await _postApi.GetPostsAsync(_startIndex, PageSize);

                    if (posts.Length > 0)
                    {
                        if (_startIndex == 0 && Posts.Count > 0)
                        {
                            // Làm mới danh sách (pull-to-refresh)
                            Posts.Clear();
                            Debug.WriteLine("ℹ️ [FetchPostsAsync] Làm mới danh sách bài viết");
                        }

                        _startIndex += posts.Length; // Cập nhật chỉ số cho lần gọi tiếp theo
                        foreach (var post in posts)
                        {
                            Posts.Add(post);
                        }
                        Debug.WriteLine($"✅ [FetchPostsAsync] Đã thêm {posts.Length} bài viết, startIndex mới={_startIndex}");
                    }
                    else
                    {
                        Debug.WriteLine("ℹ️ [FetchPostsAsync] Không có bài viết mới để tải");
                    }
                }
                catch (ApiException ex)
                {
                    Debug.WriteLine($"🚨 [FetchPostsAsync] Lỗi API: {ex.StatusCode} - {ex.Message}");
                    throw; // Để MakeApiCall xử lý lỗi
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"🚨 [FetchPostsAsync] Lỗi chung: {ex.Message}");
                    throw; // Để MakeApiCall xử lý lỗi
                }
            });
        }

        /// <summary>
        /// Điều hướng đến trang thông báo.
        /// </summary>
        /// <returns>Task hoàn thành khi điều hướng xong.</returns>
        [RelayCommand]
        private async Task NavigateToNotificationAsync()
        {
            try
            {
                await NavigationAsync("//NotificationPage");
                Debug.WriteLine("➡️ [NavigateToNotificationAsync] Điều hướng đến NotificationPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🚨 [NavigateToNotificationAsync] Lỗi khi điều hướng: {ex.Message}");
                await ShowErrorAlertAsync("Không thể điều hướng đến trang thông báo.");
            }
        }

        /// <summary>
        /// Điều hướng đến trang tạo bài viết.
        /// </summary>
        /// <returns>Task hoàn thành khi điều hướng xong.</returns>
        [RelayCommand]
        private async Task NavigateToCreatePostAsync()
        {
            try
            {
                await NavigationAsync("///CreatePostPage");
                Debug.WriteLine("➡️ [NavigateToCreatePostAsync] Điều hướng đến CreatePostPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🚨 [NavigateToCreatePostAsync] Lỗi khi điều hướng: {ex.Message}");
                await ShowErrorAlertAsync("Không thể điều hướng đến trang tạo bài viết.");
            }
        }

        /// <summary>
        /// Điều hướng đến trang cá nhân và hiển thị thông báo.
        /// </summary>
        /// <returns>Task hoàn thành khi điều hướng xong.</returns>
        [RelayCommand]
        private async Task NavigateToProfileAsync()
        {
            try
            {
                await NavigationAsync("//Profile");
                Debug.WriteLine("➡️ [NavigateToProfileAsync] Điều hướng đến Profile");
                await ToastAsync("Chuyển đến trang cá nhân!");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🚨 [NavigateToProfileAsync] Lỗi khi điều hướng: {ex.Message}");
                await ShowErrorAlertAsync("Không thể điều hướng đến trang cá nhân.");
            }
        }

        /// <summary>
        /// Điều hướng đến trang chi tiết bài viết.
        /// </summary>
        /// <returns>Task hoàn thành khi điều hướng xong.</returns>
        [RelayCommand]
        private async Task NavigateToPostDetailAsync()
        {
            try
            {
                await NavigationAsync("///PostDetailsPage");
                Debug.WriteLine("➡️ [NavigateToPostDetailAsync] Điều hướng đến PostDetailsPage");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"🚨 [NavigateToPostDetailAsync] Lỗi khi điều hướng: {ex.Message}");
                await ShowErrorAlertAsync("Không thể điều hướng đến trang chi tiết bài viết.");
            }
        }

        #endregion
    }
}