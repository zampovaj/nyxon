using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Nyxon.Client.Interfaces;

namespace Nyxon.Client.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _http;

        public ApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<TResponse?> PostAsync<TResponse, TRequest>(string uri, TRequest request)
        {
            try
            {
                var response = await _http.PostAsJsonAsync(uri, request);
                response.EnsureSuccessStatusCode();
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    return default;
                return await response.Content.ReadFromJsonAsync<TResponse>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error [POST {uri}]: {ex.Message}");
                throw; // throw again for viewmodel
            }
        }
        public async Task<TResponse?> DeleteAsync<TResponse>(string uri)
        {
            try
            {
                var response = await _http.DeleteAsync(uri);
                response.EnsureSuccessStatusCode();

                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    return default;

                return await response.Content.ReadFromJsonAsync<TResponse>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error [DELETE {uri}]: {ex.Message}");
                throw; // rethrow for viewmodel
            }
        }

        public async Task<TResponse?> GetAsync<TResponse>(string uri)
        {
            try
            {
                var response = await _http.GetAsync(uri);
                response.EnsureSuccessStatusCode();
                return await response.Content.ReadFromJsonAsync<TResponse>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error [GET {uri}]: {ex.Message}");
                throw; // rethrow for viewmodel
            }
        }

        public async Task PostAsync<TRequest>(string uri, TRequest data)
        {
            try
            {
                var response = await _http.PostAsJsonAsync(uri, data);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException("{response.StatusCode}: {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error [POST {uri}]: {ex.Message}");
                throw; // throw again for viewmodel
            }
        }

        public async Task PostAsync(string uri)
        {
            try
            {
                var response = await _http.PostAsJsonAsync(uri, new {});

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException("{response.StatusCode}: {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error [POST {uri}]: {ex.Message}");
                throw; // throw again for viewmodel
            }
        }
        
        public async Task<TResponse?> PatchAsync<TResponse, TRequest>(string uri, TRequest request)
        {
            try
            {
                var response = await _http.PatchAsJsonAsync(uri, request);
                response.EnsureSuccessStatusCode();
                if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                    return default;
                return await response.Content.ReadFromJsonAsync<TResponse>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error [PATCH {uri}]: {ex.Message}");
                throw; // throw again for viewmodel
            }
        }

        public async Task DeleteAsync(string uri)
        {
            try
            {
                var response = await _http.DeleteAsync(uri);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    throw new HttpRequestException($"{response.StatusCode}: {error}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error [DELETE {uri}]: {ex.Message}");
                throw; // rethrow for viewmodel
            }
        }
    }
}