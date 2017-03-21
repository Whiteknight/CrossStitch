using System;
using Topshelf;

namespace CrossStitch.Service
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                HostFactory.Run(x =>
                {
                    x.RunAsLocalService();
                    x.SetDescription("CrossStitch node");
                    x.SetDisplayName("CrossStitch");
                    x.SetServiceName("CrossStitch");
                    x.Service(() => new CrossStitchServiceControl());
                    x.OnException(e =>
                    {
                        Console.WriteLine(e.ToString());
                    });
                });
            }
            catch (Exception ex)
            {

            }
        }
    }
}
