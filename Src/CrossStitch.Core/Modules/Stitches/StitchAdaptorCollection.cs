using System;
using System.Collections.Concurrent;
using CrossStitch.Core.Modules.Stitches.Adaptors;

namespace CrossStitch.Core.Modules.Stitches
{
    public class StitchAdaptorCollection
    {
        private readonly ConcurrentDictionary<string, IStitchAdaptor> _adaptors;

        public StitchAdaptorCollection()
        {
            _adaptors = new ConcurrentDictionary<string, IStitchAdaptor>();
        }

        public IStitchAdaptor Get(string id)
        {
            bool found = _adaptors.TryGetValue(id, out IStitchAdaptor adaptor);
            return found ? adaptor : null;
        }

        public IStitchAdaptor Remove(string id)
        {
            bool removed = _adaptors.TryRemove(id, out IStitchAdaptor adaptor);
            return removed ? adaptor : null;
        }

        public void ForEach(Action<string, IStitchAdaptor> act)
        {
            foreach (var kvp in _adaptors)
                act(kvp.Key, kvp.Value);
        }

        public int Count => _adaptors.Count;

        public void Clear()
        {
            _adaptors.Clear();
        }

        public bool Add(string id, IStitchAdaptor adaptor)
        {
            bool added = _adaptors.TryAdd(id, adaptor);
            return added;
        }
    }
}