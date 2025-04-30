using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Refit;
using SocialAppLibrary.GotShared.Apis;
using SocialAppLibrary.GotShared.Dtos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using SkiaSharp;
using SocialAppLibrary.GotShared.Services;
using System.Diagnostics;
using Microsoft.Extensions.Logging;
namespace SocialAppLibrary.GotShared.ViewModels
{
    public partial class SavePostViewModel : BaseViewModel
    {
        private readonly IAuthApi _authApi;
        private readonly AuthService _authService;
        private readonly IPostApi _postApi;
        private static ILogger<SavePostViewModel> _logger;
        public SavePostViewModel(
       IPostApi postApi,
       IAuthApi authApi,
       AuthService authService,
       ILogger<SavePostViewModel> logger)
       : base(authService, logger)
        {
            _postApi = postApi;
            _authApi = authApi;
            _authService = authService;
            _logger = logger;
        }
        [ObservableProperty]
        public string _content = string.Empty;
        [ObservableProperty]
        public string _photoPath = string.Empty;
        

        [RelayCommand]
        private async Task SelectPhotoAsync()
        {
            if (!MediaPicker.Default.IsCaptureSupported)
            {
                await ShowErrorAlertAsync( "Thiết bị không hỗ trợ chọn ảnh.");
                return;
            }

            const string PickFromDevice = "Chọn từ thiết bị";
            const string CapturePhoto = "Chụp ảnh";

            var result = await Shell.Current.DisplayActionSheet(
                "Chọn ảnh",
                "Hủy",
                null,
                PickFromDevice,
                CapturePhoto);

            if (string.IsNullOrWhiteSpace(result))
                return;

            switch (result)
            {
                case PickFromDevice:
                    await PickPhotoFromDeviceAsync();
                    break;

                case CapturePhoto:
                    await CapturePhotoAsync();
                    break;

                default:
                    // Người dùng chọn "Hủy" hoặc đóng prompt
                    break;
            }


            async Task PickPhotoFromDeviceAsync()
            {
                try
                {
                    FileResult? fileResult = await MediaPicker.Default.PickPhotoAsync(new MediaPickerOptions
                    {
                        Title = "Select photos"
                    });

                    if (fileResult is null)
                    {
                        await ToastAsync("no photo selected");
                        return;
                    }// Lưu đường dẫn của ảnh đã chọn
                    PhotoPath = fileResult.FullPath;
                }
                catch (Exception ex)
                {
                    await ShowErrorAlertAsync( $"Không thể chọn ảnh: {ex.Message}");
                }

            }

            async Task CapturePhotoAsync()
            {
                try
                {
                    // Yêu cầu quyền camera và lưu trữ
                    var cameraPermissionStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
                    if (cameraPermissionStatus != PermissionStatus.Granted)
                    {
                        cameraPermissionStatus = await Permissions.RequestAsync<Permissions.Camera>();
                    }

                    var storagePermissionStatus = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
                    if (storagePermissionStatus != PermissionStatus.Granted)
                    {
                        storagePermissionStatus = await Permissions.RequestAsync<Permissions.StorageRead>();
                    }

                    // Nếu quyền bị từ chối, thông báo cho người dùng
                    if (cameraPermissionStatus == PermissionStatus.Denied || storagePermissionStatus == PermissionStatus.Denied)
                    {
                        bool openSettings = await Shell.Current.DisplayAlert(
        "Quyền bị từ chối",
        "Bạn có thể bật quyền trong cài đặt ứng dụng.",
        "Mở cài đặt",
        "Hủy");

                        if (openSettings)
                        {
                            // Mở cài đặt ứng dụng (nếu cần)
                           // OpenAppSettings();
                        }
                        return;
                    }

                    // Chụp ảnh
                    FileResult? photo = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
                    {
                        Title = "Chụp ảnh"
                    });

                    if (photo is null)
                    {
                        await ShowErrorAlertAsync("Không có ảnh nào được chụp");
                        return;
                    }

                    // Lưu đường dẫn của ảnh đã chụp
                    PhotoPath = photo.FullPath;
                }
                catch (Exception ex)
                {
                    await ShowErrorAlertAsync($"Không thể chụp ảnh: {ex.Message}");
                }
            }

        }




        [RelayCommand]
        private void RemovePhotoAsync()
        {
            // Logic xóa ảnh (ví dụ: xóa đường dẫn ảnh)
            PhotoPath = "";
            // Thông báo cho người dùng
            ToastAsync("Ảnh đã được xóa.");

        }
        
        

        // Phương thức nén ảnh bằng SkiaSharp
        private async Task<string> CompressImageAsync(string imagePath, int quality = 80)
        {
            try
            {
                // Đọc file ảnh gốc
                using var inputStream = File.OpenRead(imagePath);
                using var originalBitmap = SKBitmap.Decode(inputStream);

                // Tạo thông tin ảnh và surface để vẽ
                var imageInfo = new SKImageInfo(originalBitmap.Width, originalBitmap.Height);
                using var surface = SKSurface.Create(imageInfo);
                using var canvas = surface.Canvas;
                canvas.DrawBitmap(originalBitmap, 0, 0);

                // Nén ảnh với chất lượng được chỉ định
                using var image = surface.Snapshot();
                using var data = image.Encode(SKEncodedImageFormat.Jpeg, quality);

                // Lưu ảnh đã nén vào file tạm
                var compressedFilePath = Path.Combine(FileSystem.CacheDirectory, $"{Guid.NewGuid()}.jpg");
                await File.WriteAllBytesAsync(compressedFilePath, data.ToArray());

                return compressedFilePath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error compressing image: {ex.Message}");
            }
        }

      


        [RelayCommand]
        private async Task SavePostAsync()
        {
            if (string.IsNullOrWhiteSpace(Content) && string.IsNullOrWhiteSpace(PhotoPath))
            {
                await ToastAsync("Either content or photo is required");
                return;
            }

            await MakeApiCall(async () =>
            {
                StreamPart? photoStreamPart = null;

                // Nếu có ảnh, mở và tạo StreamPart
                if (!string.IsNullOrWhiteSpace(PhotoPath))
                {
                    var compressedPath = await CompressImageAsync(PhotoPath);
                    var fileName = Path.GetFileName(compressedPath); // Sử dụng compressedPath thay vì PhotoPath
                    var fileStream = File.OpenRead(compressedPath); // Đọc từ file đã nén
                    photoStreamPart = new StreamPart(fileStream, fileName);
                }

                // Tạo đối tượng SavePost và serialize nó
                var serializedSavePostDto = JsonSerializer.Serialize(new SavePost
                {
                    Content = Content,
                });

                // Kiểm tra xem serializedSavePostDto có hợp lệ hay không
                if (string.IsNullOrWhiteSpace(serializedSavePostDto))
                {
                    await ShowErrorAlertAsync("Lỗi khi tạo nội dung bài viết.");
                    return;
                }


                // Gọi API SavePostAsync để lưu bài viết
                Debug.WriteLine($"📤 Gửi bài viết với token: {_authService.Token}");
                var result = await _postApi.SavePostAsync(photoStreamPart, serializedSavePostDto);

                if (!result.IsSuccess)
                {
                    await ShowErrorAlertAsync(result.Error);
                    return;
                }

                await ToastAsync("🎉 Bài viết đã được đăng thành công!");

                Content = null;
                PhotoPath = "";
                //await NavigationAsync($"//{nameof(HomePage)}");
            });
        }


    }

}
    
