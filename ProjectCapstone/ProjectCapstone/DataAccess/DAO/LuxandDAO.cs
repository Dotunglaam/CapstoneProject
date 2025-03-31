using BusinessObject.DTOS;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.DAO
{
    public class LuxandDAO
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiToken;

        public LuxandDAO(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _apiToken = configuration["Luxand:ApiKey"];
        }
        public async Task<List<Person>> ListPersonsAsync()
        {
            var url = "https://api.luxand.cloud/v2/person";

            var request = new HttpRequestMessage(HttpMethod.Get, url);

            request.Headers.Add("token", _apiToken);

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API call failed: {response.StatusCode} - {errorContent}");
            }

            var content = await response.Content.ReadAsStringAsync();
            var persons = JsonConvert.DeserializeObject<List<Person>>(content);

            return persons;
        }

        public async Task<string> AddPersonAsync(byte[] photoData, string name, string collections)
        {
            using var client = new HttpClient();

            client.DefaultRequestHeaders.Add("token", _apiToken);

            var content = new MultipartFormDataContent();

            content.Add(new StringContent(name), "name");

            var imageContent = new ByteArrayContent(photoData);
            imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            content.Add(imageContent, "photos", "photo.jpg");

            content.Add(new StringContent("1"), "store");  // store luôn là true
            content.Add(new StringContent(collections ?? ""), "collections");
            content.Add(new StringContent("0"), "unique");  // unique luôn là false

            var response = await client.PostAsync("https://api.luxand.cloud/v2/person", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"API call failed: {response.StatusCode} - {errorContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> RecognizePeopleAsync(byte[] photoData, string collections, string fileExtension)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("token", _apiToken);

            var content = new MultipartFormDataContent();

            var contentType = GetContentType(fileExtension);

            var imageContent = new ByteArrayContent(photoData);
            imageContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            content.Add(imageContent, "photo", $"image{fileExtension}");

            if (!string.IsNullOrEmpty(collections))
            {
                content.Add(new StringContent(collections), "collections");
            }

            var response = await client.PostAsync("https://api.luxand.cloud/photo/search/v2", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Luxand API call failed: {response.StatusCode} - {errorContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> VerifyPersonInPhotoAsync(byte[] photoData, string uuid, string fileExtension)
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("token", _apiToken);
            client.Timeout = TimeSpan.FromSeconds(30);

            var content = new MultipartFormDataContent();

            var contentType = GetContentType(fileExtension);

            var imageContent = new ByteArrayContent(photoData);
            imageContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);

            content.Add(imageContent, "photo", $"photo{fileExtension}");

            var endpoint = $"https://api.luxand.cloud/photo/verify/{uuid}";
            var response = await client.PostAsync(endpoint, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new Exception($"Luxand API call failed: {response.StatusCode} - {errorContent}");
            }

            return await response.Content.ReadAsStringAsync();
        }

        public async Task DeletePersonAsync(string uuid)
        {
            var url = $"https://api.luxand.cloud/person/{uuid}";

            var request = new HttpRequestMessage(HttpMethod.Delete, url);

            request.Headers.Add("token", _apiToken);

            try
            {
                var response = await _httpClient.SendAsync(request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    throw new Exception($"API call failed: {response.StatusCode} - {errorContent}");
                }

                Console.WriteLine($"Person with UUID {uuid} has been successfully deleted.");
            }
            catch (HttpRequestException ex)
            {
                throw new Exception($"Request failed: {ex.Message}", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unexpected error: {ex.Message}", ex);
            }
        }

        private string GetContentType(string fileExtension)
        {
            return fileExtension.ToLower() switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".bmp" => "image/bmp",
                _ => throw new Exception("Unsupported file format.")
            };
        }
    }
}
