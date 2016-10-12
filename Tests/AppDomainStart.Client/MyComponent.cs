using System.IO;
using CrossStitch.App;

namespace AppDomainStart.Client
{
    public class MyComponent : ICallIn
    {
        public void Start()
        {
            File.WriteAllText("C:/Test/MyComponent.txt", "Started");
        }

        public void Stop()
        {
            File.WriteAllText("C:/Test/MyComponent.txt", "Stopped");
        }
    }
}
