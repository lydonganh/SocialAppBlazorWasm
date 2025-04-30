using Microsoft.Extensions.Logging;
using SocialAppLibrary.GotShared.Services;
using System.Net.Http.Headers;

namespace SocialAppLibrary.GotShared.Services
{
    public class AuthHeaderHandler : DelegatingHandler
    {
        private readonly AuthService _authService;
        private readonly ILogger<AuthHeaderHandler> _logger;

        public AuthHeaderHandler(AuthService authService, ILogger<AuthHeaderHandler> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var token = _authService.Token;

                if (!string.IsNullOrEmpty(token))
                {
                    _logger.LogDebug("Đang thêm Bearer token vào yêu cầu.");
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                }
                else
                {
                    _logger.LogWarning("Không có token cho yêu cầu.");
                }

                var response = await base.SendAsync(request, cancellationToken);

                if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    _logger.LogWarning("Nhận lỗi 401 Unauthorized. Đang thử làm mới token.");

                    var success = await _authService.RefreshTokenAsync();
                    if (success && !string.IsNullOrEmpty(_authService.Token))
                    {
                        _logger.LogDebug("Đã làm mới token thành công, thử lại yêu cầu.");
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _authService.Token);
                        response = await base.SendAsync(request, cancellationToken);
                    }
                    else
                    {
                        _logger.LogError("Không thể làm mới token.");
                    }
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi trong AuthHeaderHandler khi xử lý yêu cầu.");
                throw;
            }
        }
    }
}