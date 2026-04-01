using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nyxon.Client.Interfaces
{
    public interface IApiService
    {
        Task<TResponse?> PostAsync<TResponse, TRequest>(string uri, TRequest request);
        Task<TResponse?> GetAsync<TResponse>(string uri);
        Task<TResponse?> DeleteAsync<TResponse>(string uri);
        Task DeleteAsync(string uri);
        Task PostAsync<TRequest>(string uri, TRequest data);
        Task PostAsync(string uri);
        Task<TResponse?> PatchAsync<TResponse, TRequest>(string uri, TRequest request);
    }
}