using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker.Http;
using System.Net;

namespace Shortener.Core.Redirect
{
    public interface IUserIpLocationService
    {
        IPAddress GetClientIpn(HttpRequest request);
        IPAddress? GetClientIpn(HttpRequestData request);
        Task<UserIpResponse> GetUserIpAsync(string ip, CancellationToken cancellationToken);
    }
}