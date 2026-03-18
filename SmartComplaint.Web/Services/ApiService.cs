using Newtonsoft.Json;
using System.Text;
using System.Text.Json.Serialization;

namespace SmartComplaint.Web.Services;

public class ApiService
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _ctx;
    private readonly string _baseUrl;

    public ApiService(HttpClient http, IHttpContextAccessor ctx, IConfiguration config)
    {
        _http = http;
        _ctx = ctx;
        _baseUrl = config["ApiSettings:BaseUrl"] ?? "https://localhost:7182";
    }

    // ─── Attach JWT token from cookie ────────────────────────
    private void AttachToken()
    {
        var token = _ctx.HttpContext?.Request.Cookies["AccessToken"];
        if (!string.IsNullOrEmpty(token))
        {
            _http.DefaultRequestHeaders.Remove("Authorization");
            _http.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }
    }

    // ─── GET ──────────────────────────────────────────────────
    public async Task<T?> GetAsync<T>(string endpoint)
    {
        AttachToken();
        var response = await _http.GetAsync($"{_baseUrl}/{endpoint}");
        if (!response.IsSuccessStatusCode) return default;
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<T>(json);
    }

    // ─── POST ─────────────────────────────────────────────────
    public async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object data)
    {
        AttachToken();
        var content = new StringContent(
            JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        var response = await _http.PostAsync($"{_baseUrl}/{endpoint}", content);
        var json = await response.Content.ReadAsStringAsync();

        return new ApiResponse<T>
        {
            IsSuccess = response.IsSuccessStatusCode,
            StatusCode = (int)response.StatusCode,
            Data = response.IsSuccessStatusCode
                         ? JsonConvert.DeserializeObject<T>(json)
                         : default,
            ErrorMessage = !response.IsSuccessStatusCode
                           ? TryGetMessage(json)
                           : null
        };
    }

    // ─── PUT ──────────────────────────────────────────────────
    public async Task<ApiResponse<T>> PutAsync<T>(string endpoint, object data)
    {
        AttachToken();
        var content = new StringContent(
            JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        var response = await _http.PutAsync($"{_baseUrl}/{endpoint}", content);
        var json = await response.Content.ReadAsStringAsync();

        return new ApiResponse<T>
        {
            IsSuccess = response.IsSuccessStatusCode,
            StatusCode = (int)response.StatusCode,
            Data = response.IsSuccessStatusCode
                           ? JsonConvert.DeserializeObject<T>(json)
                           : default,
            ErrorMessage = !response.IsSuccessStatusCode
                           ? TryGetMessage(json)
                           : null
        };
    }

    // ─── PATCH ────────────────────────────────────────────────
    public async Task<ApiResponse<T>> PatchAsync<T>(string endpoint, object data)
    {
        AttachToken();
        var content = new StringContent(
            JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
        var request = new HttpRequestMessage(HttpMethod.Patch,
            $"{_baseUrl}/{endpoint}")
        { Content = content };
        var response = await _http.SendAsync(request);
        var json = await response.Content.ReadAsStringAsync();

        return new ApiResponse<T>
        {
            IsSuccess = response.IsSuccessStatusCode,
            StatusCode = (int)response.StatusCode,
            Data = response.IsSuccessStatusCode
                           ? JsonConvert.DeserializeObject<T>(json)
                           : default,
            ErrorMessage = !response.IsSuccessStatusCode
                           ? TryGetMessage(json)
                           : null
        };
    }

    // ─── DELETE ───────────────────────────────────────────────
    public async Task<ApiResponse<T>> DeleteAsync<T>(string endpoint)
    {
        AttachToken();
        var response = await _http.DeleteAsync($"{_baseUrl}/{endpoint}");
        var json = await response.Content.ReadAsStringAsync();

        return new ApiResponse<T>
        {
            IsSuccess = response.IsSuccessStatusCode,
            StatusCode = (int)response.StatusCode,
            Data = response.IsSuccessStatusCode
                           ? JsonConvert.DeserializeObject<T>(json)
                           : default,
            ErrorMessage = !response.IsSuccessStatusCode
                           ? TryGetMessage(json)
                           : null
        };
    }

    // ─── Multipart File Upload ────────────────────────────────
    public async Task<ApiResponse<T>> UploadAsync<T>(string endpoint, IFormFile file)
    {
        AttachToken();
        using var form = new MultipartFormDataContent();
        using var stream = file.OpenReadStream();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType =
            new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
        form.Add(fileContent, "file", file.FileName);

        var response = await _http.PostAsync($"{_baseUrl}/{endpoint}", form);
        var json = await response.Content.ReadAsStringAsync();

        return new ApiResponse<T>
        {
            IsSuccess = response.IsSuccessStatusCode,
            StatusCode = (int)response.StatusCode,
            Data = response.IsSuccessStatusCode
                           ? JsonConvert.DeserializeObject<T>(json)
                           : default,
            ErrorMessage = !response.IsSuccessStatusCode
                           ? TryGetMessage(json)
                           : null
        };
    }

    // ─── Helper ───────────────────────────────────────────────
    private string TryGetMessage(string json)
    {
        try
        {
            dynamic? obj = JsonConvert.DeserializeObject(json);
            return obj?.message?.ToString() ?? "Something went wrong.";
        }
        catch { return "Something went wrong."; }
    }
}

// ─── Generic API Response Wrapper ────────────────────────────
public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public int StatusCode { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
}