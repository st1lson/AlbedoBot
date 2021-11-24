using AlbedoBot.Core;
using System.Threading.Tasks;

namespace AlbedoBot
{
    public class Program
    {
        protected static Task Main(string[] args) => new AlbedoBotClient().InitializeAsync();
    }
}