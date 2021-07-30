using shortenerTools.Models;
using System.Threading;
using System.Threading.Tasks;

namespace shortenerTools.Abstractions
{
    public interface IUserIpLocationService
    {
        Task<UserIpResponse> GetUserIpAsync(string ip, CancellationToken cancellationToken);
    }
}