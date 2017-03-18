using CrossStitch.Core.Models;
using CrossStitch.Core.Utility;
using System.Linq;

namespace CrossStitch.Core.Modules.RequestCoordinator
{
    public class CoordinatorService
    {
        private readonly IDataRepository _data;
        private readonly IModuleLog _log;
        private readonly CrossStitchCore _core;

        public CoordinatorService(CrossStitchCore core, IDataRepository data, IModuleLog log)
        {
            _data = data;
            _log = log;
            _core = core;
        }

        public Application CreateApplication(string name)
        {
            // TODO: Check that an application with the same name doesn't already exist
            var application = _data.Insert(new Application
            {
                Name = name
            });
            if (application != null)
                _log.LogInformation("Created application {0}:{1}", application.Id, application.Name);
            return application;
        }

        public Application UpdateApplication(string applicationId, string newName)
        {
            return _data.Update<Application>(applicationId, a => a.Name = newName);
        }

        public bool DeleteApplication(string applicationId)
        {
            // TODO: Should we delete stitches from this application? If so, we'll need to get a 
            // list of all stitches from the Data module and stop all of them, delete them,
            // and then delete the application.
            return _data.Delete<Application>(applicationId);
        }

        public bool DeleteComponent(string applicationId, string component)
        {
            // TODO: Should we delete all stitches of this component? If so, we need to get a list
            // of all stitches in this component, stop and delete each, and then update our record
            // here.
            Application application = _data.Update<Application>(applicationId, a =>
            {
                a.RemoveComponent(component);
            });
            return application != null;
        }

        public bool UpdateComponent(string applicationId, string component)
        {
            bool updated = false;
            Application application = _data.Update<Application>(applicationId, a =>
            {
                updated = false;
                var comp = a.Components.FirstOrDefault(c => c.Name == component);
                if (comp != null)
                {
                    updated = true;
                    comp.Name = component;
                }
            });
            return application != null && updated;
        }

        public bool InsertComponent(string applicationId, string component)
        {
            bool updated = false;
            Application application = _data.Update<Application>(applicationId, a =>
            {
                updated = a.AddComponent(component);
            });
            return application != null && updated;
        }
    }
}
