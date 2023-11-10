using Il2CppInterop.Runtime.InteropTypes.Arrays;
using Il2CppSystem.Collections.Generic;

namespace Dozing.Utils {
    internal static class Il2CppArrayUtils {
        public static T[] Append<T>(this Il2CppArrayBase<T> array, params T[] values) {
            T[] result = new T[array.Length + values.Length];
            array.CopyTo(result, 0);
            values.CopyTo(result, array.Length);
            return result;
        }

        public static List<T> Add<T>(this List<T> list, params T[] values) {
            foreach(T v in values)
                list.Add(v);
            return list;
        }

        public static T[] Insert<T>(this Il2CppArrayBase<T> array, int index, params T[] values) {
            T[] added = new T[array.Length + values.Length];
            for (int i = 0; i < index; i++)
                added[i] = array[i];
            values.CopyTo(added, index);
            for (int i = index; i < array.Length; i++)
                added[i + values.Length] = array[i];
            return added;
        }

        public static void AddIfNotPresent<K, V>(this Dictionary<K, V> dict, K key, V value) {
            if (!dict.ContainsKey(key))
                dict.Add(key, value);
        }
    }
}
