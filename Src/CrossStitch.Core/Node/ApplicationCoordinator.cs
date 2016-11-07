﻿using Acquaintance;
using CrossStitch.Core.Apps.Messages;
using CrossStitch.Core.Data.Entities;
using CrossStitch.Core.Messages;
using CrossStitch.Core.Node.Messages;
using System.Linq;

namespace CrossStitch.Core.Node
{
    public class ApplicationCoordinator : IModule
    {
        private SubscriptionCollection _subscriptions;
        private RunningNode _node;
        private IMessageBus _messageBus;
        private DataHelperClient _data;

        public string Name => "ApplicationCoordinator";

        public void Start(RunningNode context)
        {
            _node = context;
            _subscriptions = new SubscriptionCollection(context.MessageBus);
            _messageBus = context.MessageBus;
            _data = new DataHelperClient(_messageBus);

            _subscriptions.Listen<ApplicationChangeRequest, Application>(ApplicationChangeRequest.Insert, CreateApplication);
            _subscriptions.Listen<ApplicationChangeRequest, Application>(ApplicationChangeRequest.Update, UpdateApplication);
            _subscriptions.Listen<ApplicationChangeRequest, GenericResponse>(ApplicationChangeRequest.Delete, DeleteApplication);

            _subscriptions.Listen<ComponentChangeRequest, GenericResponse>(ComponentChangeRequest.Insert, InsertComponent);
            _subscriptions.Listen<ComponentChangeRequest, GenericResponse>(ComponentChangeRequest.Update, UpdateComponent);
            _subscriptions.Listen<ComponentChangeRequest, GenericResponse>(ComponentChangeRequest.Delete, DeleteComponent);

            _subscriptions.Listen<Instance, Instance>(Instance.CreateEvent, CreateInstance);
            _subscriptions.Listen<InstanceRequest, InstanceResponse>(InstanceRequest.Delete, DeleteInstance);
            _subscriptions.Listen<InstanceRequest, InstanceResponse>(InstanceRequest.Clone, CloneInstance);

        }

        private GenericResponse DeleteComponent(ComponentChangeRequest arg)
        {
            Application application = _data.Update<Application>(arg.Application, a =>
            {
                a.Components = a.Components.Where(c => c.Name != arg.Name).ToList();
            });
            return new GenericResponse(application != null);
        }

        private GenericResponse UpdateComponent(ComponentChangeRequest arg)
        {
            bool updated = false;
            Application application = _data.Update<Application>(arg.Application, a =>
            {
                updated = false;
                var component = a.Components.FirstOrDefault(c => c.Name == arg.Name);
                if (component != null)
                {
                    updated = true;
                    component.Name = arg.Name;
                }
            });
            return new GenericResponse(application != null && updated);
        }

        private GenericResponse InsertComponent(ComponentChangeRequest arg)
        {
            bool updated = false;
            Application application = _data.Update<Application>(arg.Application, a =>
            {
                updated = false;
                var component = a.Components.FirstOrDefault(c => c.Name == arg.Name);
                if (component == null)
                {
                    a.Components.Add(new ApplicationComponent
                    {
                        Name = arg.Name
                    });
                }
            });
            return new GenericResponse(application != null && updated);
        }

        private GenericResponse DeleteApplication(ApplicationChangeRequest arg)
        {
            bool ok = _data.Delete<Application>(arg.Id);
            return new GenericResponse(ok);
        }

        private Application UpdateApplication(ApplicationChangeRequest arg)
        {
            return _data.Update<Application>(arg.Id, a => a.Name = arg.Name);
        }

        // TODO: We also need to broadcast these messages out over the backplane so other nodes keep
        // track of applications.

        private Application CreateApplication(ApplicationChangeRequest arg)
        {
            // TODO: Check that an application with the same name doesn't already exist
            return _data.Insert(new Application
            {
                Name = arg.Name,
                NodeId = _node.NodeId
            });
        }

        private Instance CreateInstance(Instance instance)
        {
            instance = _data.Insert<Instance>(instance);
            return instance;
        }

        private InstanceResponse DeleteInstance(InstanceRequest request)
        {
            var stopResponse = _messageBus.Request<InstanceRequest, InstanceResponse>(InstanceRequest.Stop, request);
            bool deleted = _data.Delete<Instance>(request.Id);
            return new InstanceResponse
            {
                Success = deleted
            };
        }

        private InstanceResponse CloneInstance(InstanceRequest request)
        {
            var instance = _data.Get<Instance>(request.Id);
            instance.Id = null;
            instance.StoreVersion = 0;
            instance = _data.Insert(instance);
            return new InstanceResponse
            {
                Success = true,
                Id = instance.Id
            };
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
    }
}