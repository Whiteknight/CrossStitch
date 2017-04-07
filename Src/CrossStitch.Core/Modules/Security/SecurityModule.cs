using System.Collections.Generic;
using Acquaintance;
using CrossStitch.Core.Messages.Security;

namespace CrossStitch.Core.Modules.Security
{
    public interface ISecurityPolicy
    {
        SecurityResponse RequestAccess(SecurityRequest request);
    }

    public class PermissiveSecurityPolicy : ISecurityPolicy
    {
        public SecurityResponse RequestAccess(SecurityRequest request)
        {
            return new SecurityResponse
            {
                Allowed = true
            };
        }
    }

    public class SecurityModule : IModule
    {
        private readonly IMessageBus _messageBus;
        private readonly ISecurityPolicy _policy;
        private SubscriptionCollection _subscriptions;

        public SecurityModule(IMessageBus messageBus, ISecurityPolicy policy = null)
        {
            _messageBus = messageBus;
            _policy = policy;
        }

        public string Name => ModuleNames.Security;

        public void Start()
        {
            _subscriptions = new SubscriptionCollection(_messageBus);
            _subscriptions.Listen<SecurityRequest, SecurityResponse>(b => b
                .OnDefaultChannel()
                .Invoke(_policy.RequestAccess));
        }

        public void Stop()
        {
            _subscriptions?.Dispose();
        }

        public IReadOnlyDictionary<string, string> GetStatusDetails()
        {
            return new Dictionary<string, string>
            {
            };
        }

        public void Dispose()
        {
            Stop();
            _subscriptions = null;
        }
    }
}
