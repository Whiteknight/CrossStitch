using Acquaintance;
using CrossStitch.Core.MessageBus;
using CrossStitch.Core.Messages;
using CrossStitch.Core.Messages.CoordinatedRequests;
using CrossStitch.Core.Models;
using System;

namespace CrossStitch.Core.Modules.RequestCoordinator
{
    public class RequestCoordinatorModule : IModule
    {
        private readonly CrossStitchCore _core;
        private readonly IMessageBus _messageBus;
        private readonly CoordinatorService _service;

        private SubscriptionCollection _subscriptions;

        public RequestCoordinatorModule(CrossStitchCore core)
        {
            if (core == null)
                throw new ArgumentNullException(nameof(core));

            _messageBus = core.MessageBus;
            _core = core;

            var log = new ModuleLog(_messageBus, Name);
            var data = new DataHelperClient(_messageBus);
            _service = new CoordinatorService(core, data, log);
        }

        public RequestCoordinatorModule(CrossStitchCore core, CoordinatorService service)
        {
            if (core == null)
                throw new ArgumentNullException(nameof(core));
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            _messageBus = core.MessageBus;
            _core = core;
            _service = service;
        }

        public string Name => ModuleNames.RequestCoordinator;

        public void Start()
        {
            _subscriptions = new SubscriptionCollection(_messageBus);

            // TODO: Move all these methods into the Master module. For each request, determine
            // if the instance is local or remote, and dispatch to the Data module or Backplane
            // as appropriate.

            // CRUD requests for Application records
            _subscriptions.Listen<ApplicationChangeRequest, Application>(l => l
                .WithChannelName(ApplicationChangeRequest.Insert)
                .Invoke(arg => _service.CreateApplication(arg.Name)));
            _subscriptions.Listen<ApplicationChangeRequest, Application>(l => l
                .WithChannelName(ApplicationChangeRequest.Update)
                .Invoke(m => _service.UpdateApplication(m.Id, m.Name)));
            _subscriptions.Listen<ApplicationChangeRequest, GenericResponse>(l => l
                .WithChannelName(ApplicationChangeRequest.Delete)
                .Invoke(DeleteApplication));

            // CRUD requests for Components, which are all updates on Application records
            _subscriptions.Listen<ComponentChangeRequest, GenericResponse>(l => l
                .WithChannelName(ComponentChangeRequest.Insert)
                .Invoke(InsertComponent));
            _subscriptions.Listen<ComponentChangeRequest, GenericResponse>(l => l
                .WithChannelName(ComponentChangeRequest.Update)
                .Invoke(UpdateComponent));
            _subscriptions.Listen<ComponentChangeRequest, GenericResponse>(l => l
                .WithChannelName(ComponentChangeRequest.Delete)
                .Invoke(DeleteComponent));
        }

        public System.Collections.Generic.IReadOnlyDictionary<string, string> GetStatusDetails()
        {
            return new System.Collections.Generic.Dictionary<string, string>();
        }

        public void Stop()
        {
            _subscriptions?.Dispose();
            _subscriptions = null;
        }

        public void Dispose()
        {
            Stop();
        }

        private GenericResponse DeleteComponent(ComponentChangeRequest arg)
        {
            bool ok = _service.DeleteComponent(arg.Application, arg.Name);
            return new GenericResponse(ok);
        }

        // TODO: This method doesn't make sense. We wouldn't change the name of a Component.
        // Remove this if there is no other use-case.
        private GenericResponse UpdateComponent(ComponentChangeRequest arg)
        {
            bool ok = _service.UpdateComponent(arg.Application, arg.Name);
            return new GenericResponse(ok);
        }

        private GenericResponse InsertComponent(ComponentChangeRequest arg)
        {
            bool ok = _service.InsertComponent(arg.Application, arg.Name);
            return new GenericResponse(ok);
        }

        private GenericResponse DeleteApplication(ApplicationChangeRequest arg)
        {
            bool ok = _service.DeleteApplication(arg.Id);
            return new GenericResponse(ok);
        }
    }
}
