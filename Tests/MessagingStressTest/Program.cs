using System;
using System.Diagnostics;
using CrossStitch.Core.Messaging;
using CrossStitch.Core.Messaging.RequestResponse;
using CrossStitch.Core.Messaging.Threading;

namespace MessagingStressTest
{
    public class Request : IRequest<Response>
    {
        public int Id { get; set; }
    }

    public class Response
    {
        public int Id { get; set; }
        public int ThreadId { get; set; }
    }

    public class PubEvent
    {
        public int Id { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {
            const int maxRequests = 10;
            var bus = new LocalMessageBus(4);
            bus.Subscribe<Request, Response>("Test", Respond, new PublishOptions {
                DispatchType = DispatchThreadType.AnyWorkerThread
            });
            bus.StartWorkers();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            for (int i = 0; i < maxRequests; i++)
            {
                var response = bus.Request<Request, Response>("Test", new Request { Id = i });
                //var r = response.Responses.First();
                //Console.WriteLine("{0}: {1}", r.ThreadId, r.Id);
            }
            stopwatch.Stop();
            Console.WriteLine("{0} messages in {1}ms", maxRequests, stopwatch.ElapsedMilliseconds);

            bus.Subscribe<PubEvent>("Test", Subscriber, new PublishOptions {
                DispatchType = DispatchThreadType.AnyWorkerThread
            });
            for (int i = 0; i < maxRequests; i++)
            {
                bus.Publish("Test", new PubEvent {
                    Id = i
                });
            }

            Console.ReadKey();
        }

        public static void Subscriber(PubEvent e)
        {
            Console.WriteLine("{0}: {1}", System.Threading.Thread.CurrentThread.ManagedThreadId, e.Id);
        }

        public static Response Respond(Request request)
        {
            return new Response {
                Id = request.Id,
                ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId
            };
        }

    }
}
