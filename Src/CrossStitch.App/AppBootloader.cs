using System;
using System.IO;
using System.Linq;
using System.Reflection;

namespace CrossStitch.App
{
    public class AppBootloader : MarshalByRefObject
    {
        private string _assemblyFileUri;
        private object _appInstance;

        public bool StartApp(string assemblyFile)
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            try
            {
                _assemblyFileUri = new Uri(assemblyFile).ToString();
                var assembly = Assembly.LoadFrom(_assemblyFileUri);
                var type = assembly.GetExportedTypes()
                    .FirstOrDefault(t => !t.IsAbstract && t.GetInterfaces().Any(i => i.FullName == "CrossStitch.App.ICallIn"));
                if (type == null)
                    return false;

                _appInstance = Activator.CreateInstance(type);
                _appInstance.GetType().GetMethod("Start").Invoke(_appInstance, null);
                return true;
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

        public bool StartApp(string assemblyFile, string callInClassName)
        {
            AppDomain.CurrentDomain.AssemblyResolve += OnAssemblyResolve;

            try
            {
                _assemblyFileUri = new Uri(assemblyFile).ToString();
                var assembly = Assembly.LoadFrom(_assemblyFileUri);
                var type = assembly.GetExportedTypes()
                    .FirstOrDefault(t => !t.IsAbstract && 
                        t.Name == callInClassName && 
                        t.GetInterfaces().Any(i => i.FullName == "CrossStitch.App.ICallIn"));
                if (type == null)
                    return false;

                _appInstance = Activator.CreateInstance(type);
                _appInstance.GetType().GetMethod("Start").Invoke(_appInstance, null);
                return true;
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

        public void Stop()
        {
            _appInstance.GetType().GetMethod("Stop").Invoke(_appInstance, null);
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
