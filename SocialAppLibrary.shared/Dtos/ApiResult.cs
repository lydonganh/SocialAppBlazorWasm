using System.Text.Json.Serialization;

namespace SocialAppLibrary.GotShared.Dtos
{
    public record ApiResult(bool IsSuccess, string? Error)
    {
        public static ApiResult Success() => new(true, null);
        public static ApiResult Fail(string errorMessage) => new(false, errorMessage);
    }
    public record ApiResult<TData>([property: JsonPropertyName("isSuccess")] bool IsSuccess,
    [property: JsonPropertyName("data")] TData? Data,
    [property: JsonPropertyName("error")] string? Error)
    {

        public static ApiResult<TData> Success(TData data) => new(true, data, null);
        public static ApiResult<TData> Fail(string errorMessage) => new(false, default!, errorMessage);
    }
}
