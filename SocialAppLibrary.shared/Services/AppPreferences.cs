using System.Diagnostics;
using System.Text.Json;
using SocialAppLibrary.GotShared.Dtos;

namespace SocialAppLibrary.GotShared.Services
{
    public class AppPreferences
    {
        private const string UserInfoKey = "UserInfo";
        private const string LoginResponseKey = "LoginResponse";

        public void SetBool(string key, bool value)
        {
            Preferences.Set(key, value);
            Debug.WriteLine($"✅ [AppPreferences.SetBool] Lưu {key}={value}");
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            var value = Preferences.Get(key, defaultValue);
            Debug.WriteLine($"🔍 [AppPreferences.GetBool] Lấy {key}={value}");
            return value;
        }

        public async Task SaveUserInfoAsync(LoggedInUser loggedInUser)
        {
            if (loggedInUser == null)
            {
                Debug.WriteLine("⚠️ [AppPreferences.SaveUserInfoAsync] loggedInUser là null, không lưu.");
                return;
            }

            try
            {
                var json = JsonSerializer.Serialize(loggedInUser);
                Preferences.Set(UserInfoKey, json);
                Debug.WriteLine($"✅ [AppPreferences.SaveUserInfoAsync] Lưu UserInfo: {json}");
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"🚨 [AppPreferences.SaveUserInfoAsync] Lỗi serialize: {ex.Message}");
            }
        }

        public async Task<LoggedInUser?> GetUserInfoAsync()
        {
            var json = Preferences.Get(UserInfoKey, null);
            Debug.WriteLine($"🔍 [AppPreferences.GetUserInfoAsync] Đọc UserInfo: {json}");

            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.WriteLine("ℹ️ [AppPreferences.GetUserInfoAsync] Không tìm thấy UserInfo.");
                return null;
            }

            try
            {
                var loggedInUser = JsonSerializer.Deserialize<LoggedInUser>(json);
                return loggedInUser;
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"🚨 [AppPreferences.GetUserInfoAsync] Lỗi deserialize: {ex.Message}");
                return null;
            }
        }

        public async Task SaveLoginResponseAsync(LoginResponse loginResponse)
        {
            if (loginResponse == null)
            {
                Debug.WriteLine("⚠️ [AppPreferences.SaveLoginResponseAsync] loginResponse là null, không lưu.");
                return;
            }

            try
            {
                var json = JsonSerializer.Serialize(loginResponse);
                Preferences.Set(LoginResponseKey, json);
                Debug.WriteLine($"✅ [AppPreferences.SaveLoginResponseAsync] Lưu LoginResponse: {json}");
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"🚨 [AppPreferences.SaveLoginResponseAsync] Lỗi serialize: {ex.Message}");
            }
        }

        public async Task<LoginResponse?> GetLoginResponseAsync()
        {
            var json = Preferences.Get(LoginResponseKey, null);
            Debug.WriteLine($"🔍 [AppPreferences.GetLoginResponseAsync] Đọc LoginResponse: {json}");

            if (string.IsNullOrWhiteSpace(json))
            {
                Debug.WriteLine("ℹ️ [AppPreferences.GetLoginResponseAsync] Không tìm thấy LoginResponse.");
                return null;
            }

            try
            {
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(json);
                return loginResponse;
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"🚨 [AppPreferences.GetLoginResponseAsync] Lỗi deserialize: {ex.Message}");
                return null;
            }
        }

        public void Remove(string key)
        {
            Preferences.Remove(key);
            Debug.WriteLine($"🗑️ [AppPreferences.Remove] Đã xóa key: {key}");
        }
    }
}