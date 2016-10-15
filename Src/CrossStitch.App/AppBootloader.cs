using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using CrossStitch.App.Networking;
using NetMQ;
using NetMQ.Sockets;

namespace CrossStitch.App
{
    public class AppContext : IDisposable
    {
        public int CommunicationPort { get; private set; }
        private RequestSocket _clientSocket;
        private ReceiveChannel _receiver;
        private readonly NetMqMessageMapper _mapper;

        public AppContext(int communicationPort)
        {
            CommunicationPort = communicationPort;
            _mapper = new NetMqMessageMapper(new JsonSerializer());
        }

        internal bool Initialize()
        {
            _clientSocket = new RequestSocket();
            _clientSocket.Connect("tcp://127.0.0.1:" + CommunicationPort);
            _receiver = new ReceiveChannel();
            _receiver.MessageReceived += MessageReceived;
            _receiver.StartListening("127.0.0.1");

            var envelope = new MessageEnvelope {
                Header = new MessageHeader {
                    FromType = TargetType.Local,
                    ToType = TargetType.Local,
                    PayloadType = MessagePayloadType.CommandString
                },
                CommandStrings = new List<string> {
                    "App Instance Initialize",
                    "ReceivePort=" + _receiver.Port
                }
            };
            NetMQMessage message = _mapper.Map(envelope);
            _clientSocket.SendMultipartMessage(message);
            NetMQMessage response = new NetMQMessage();
            bool ok = _clientSocket.TryReceiveMultipartMessage(TimeSpan.FromMilliseconds(1000), ref response, 1);
            return ok;
        }

        private void MessageReceived(object sender, MessageReceivedEventArgs args)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            _clientSocket.Close();
            _clientSocket.Dispose();
            _clientSocket = null;
            _receiver.StopListening();
            _receiver.Dispose();
        }
    }

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

                _context = new AppContext(communicationPort);
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
