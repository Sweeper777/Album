using System.Collections.Generic;

namespace Album {
    static class AsSingleEnumerableExtension {
        public static IEnumerable<T> AsSingleEnumerable<T>(this T t) {
            yield return t;
        }
    }
}