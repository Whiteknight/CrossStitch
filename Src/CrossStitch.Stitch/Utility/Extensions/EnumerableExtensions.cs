﻿using System.Collections.Generic;
using System.Linq;

namespace CrossStitch.Stitch.Utility.Extensions
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<T> OrEmptyIfNull<T>(this IEnumerable<T> source)
        {
            return source ?? Enumerable.Empty<T>();
        }
    }
}
