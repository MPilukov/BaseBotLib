using System.Threading.Tasks;

namespace BaseBotLib.Interfaces.Storage
{
    public interface IStorage
    {
        Task SetValue(string key, string value);
        Task<string> GetValue(string key);
    }
}
