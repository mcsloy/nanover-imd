// Copyright (c) Intangible Realities Lab. All rights reserved.
// Licensed under the GPL. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Narupa.Utility
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<(T First, T Second)> GetPairs<T>(this IEnumerable<T> enumerable)
        {
            var started = false;
            var prev = default(T);
            foreach (var e in enumerable)
            {
                if (!started)
                {
                    started = true;
                    prev = e;
                }
                else
                {
                    yield return (prev, e);
                    prev = e;
                }
            }
        }

        public static int IndexOf<T>(this IEnumerable<T> list, T item)
        {
            var i = 0;
            foreach (var thing in list)
            {
                if (thing.Equals(item))
                    return i;
                i++;
            }

            return -1;
        }
    }
}