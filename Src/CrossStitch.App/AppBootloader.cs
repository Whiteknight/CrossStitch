using System;
using System.IO;
using System.Linq;
using System.Reflection;
using CrossStitch.App.Networking;
using CrossStitch.App.Networking.NetMq;

namespace CrossStitch.App
{
    public class AppBootloader : MarshalByRefObject
    {
        private string _assemblyFileUri;
        private object _appInstance;
        private AppContext _context;

        public bool StartApp(string assemblyFile, int communicationPort)
        {
            return StartApp(assemblyFile, communicationPort, IsServiceEntry);
        }

        public bool StartApp(string assemblyFile, string serviceEntryClassName, int communicationPort)
        {
            return StartApp(assemblyFile, communicationPort, t => IsServiceEntry(t, serviceEntryClassName));
        }

        private bool StartApp(string assemblyFile, int communicationPort, Func<Type, bool> classSelector)
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            try
            {
                _assemblyFileUri = new Uri(assemblyFile).ToString();
                var assembly = Assembly.LoadFrom(_assemblyFileUri);
                var type = assembly.GetExportedTypes().FirstOrDefault(classSelector);
                if (type == null)
                    return false;

                var network = new NetMqNetwork();
                _context = new AppContext(network, communicationPort);
                _appInstance = Activator.CreateInstance(type);
                var method = _appInstance.GetType().GetMethod("Start", new Type[] { typeof (AppContext) });
                if (method != null)
                {
                    method.Invoke(_appInstance, new object[] { _context });
                    if (!_context.Initialize())
                    {
                        Stop();
                        return false;
                    }
                    return true;
                }

                method = _appInstance.GetType().GetMethod("Start", Type.EmptyTypes);
                if (method != null)
                {
                    method.Invoke(_appInstance, null);
                    if (!_context.Initialize())
                    {
                        Stop();
                        return false;
                    }
                    return true;
                }

                return false;
            }
            catch
            {
                // TODO: Log error
                return false;
            }
            finally
            {
                AppDomain.CurrentDomain.AssemblyResolve -= OnAssemblyResolve;
            }
        }

        private bool IsServiceEntry(Type t, string callInClassName)
        {
            return !t.IsAbstract &&
                   t.Name == callInClassName &&
                   t.GetInterfaces().Any(i => i.FullName == "CrossStitch.App.ICallIn");
        }

        private bool IsServiceEntry(Type t)
        {
            return !t.IsAbstract && t.GetInterfaces().Any(i => i.FullName == "CrossStitch.App.ICallIn");
        }

        public void Stop()
        {
            if (_appInstance == null)
                return;
            _context.Dispose();
            var method = _appInstance.GetType().GetMethod("Stop");
            if (method == null)
                return;
            try
            {
                method.Invoke(_appInstance, null);
            }
            catch
            {
                // TODO: Log?
            }
        }

        private Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var directory = new FileInfo(_assemblyFileUri).Directory;

            var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(asm => string.Equals(asm.FullName, args.Name, StringComparison.OrdinalIgnoreCase));

            if (loadedAssembly != null)
            {
                return loadedAssembly;
            }

            if (directory != null)
            {
                var assemblyName = new AssemblyName(args.Name);
                var dependentAssemblyFilename = Path.Combine(directory.FullName, assemblyName.Name + ".dll");

                if (File.Exists(dependentAssemblyFilename))
                {
                    return Assembly.LoadFrom(dependentAssemblyFilename);
                }
            }
            return Assembly.Load(args.Name);
        }
    }
}
