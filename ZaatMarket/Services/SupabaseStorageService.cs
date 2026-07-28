using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;

namespace ZaatMarket.Services;

public class SupabaseStorageService(HttpClient httpClient, IConfiguration config)
{
    private readonly string _supabaseUrl = config["Supabase:Url"] ?? "https://bqmimiufdzevlpienpjt.supabase.co";
    private readonly string _supabaseKey = config["Supabase:Key"] ?? "";
    private readonly string _bucketName = config["Supabase:Bucket"] ?? "product-images";

    public async Task<string?> UploadImageAsync(Stream fileStream, string fileName, string contentType)
    {
        if (string.IsNullOrWhiteSpace(_supabaseKey))
        {
            Console.WriteLine("Supabase Storage Error: Missing API Key in configuration.");
            return null;
        }

        var uploadUrl = $"{_supabaseUrl}/storage/v1/object/{_bucketName}/{fileName}";

        using var request = new HttpRequestMessage(HttpMethod.Post, uploadUrl);
        request.Headers.Add("Authorization", $"Bearer {_supabaseKey}");
        request.Headers.Add("apikey", _supabaseKey);

        using var content = new StreamContent(fileStream);
        content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        request.Content = content;

        var response = await httpClient.SendAsync(request);

        if (response.IsSuccessStatusCode)
        {
            return $"{_supabaseUrl}/storage/v1/object/public/{_bucketName}/{fileName}";
        }

        var error = await response.Content.ReadAsStringAsync();
        Console.WriteLine($"Supabase Storage Upload Error ({response.StatusCode}): {error}");
        return null;
    }
}