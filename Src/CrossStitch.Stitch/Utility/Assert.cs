using System;

namespace CrossStitch.Stitch.Utility
{
    public static class Assert
    {
        public static void ArgNotNull<T>(T argument, string name)
        {
            if (argument == null)
                throw new ArgumentNullException(name);
        }
    }
}
