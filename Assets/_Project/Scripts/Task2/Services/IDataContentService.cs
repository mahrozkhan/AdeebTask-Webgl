using Cysharp.Threading.Tasks;

namespace ContentDiscovery.Services
{
    public interface IDataContentService
    {
        UniTask<bool> FetchDataAsync();
    }
}
