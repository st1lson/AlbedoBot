using System.Threading.Tasks;
using AlbedoBot.Core;

namespace AlbedoBot
{
    public class Program
    {
        protected static Task Main(string[] args) => new AlbedoBotClient().InitializeAsync();
    }
}