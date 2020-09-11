using System.Linq;
using System.Collections.Generic;

namespace BedrockServerConfigurator.Library
{
    public static class ExtensionMethods
    {
        public static T RandomElement<T>(this IList<T> list) =>
            list[Utilities.RandomGenerator.Next(list.Count)];

        public static KeyValuePair<T, U> RandomElement<T, U>(this IDictionary<T, U> dict) =>
            dict.ElementAt(Utilities.RandomGenerator.Next(dict.Count));
    }
}
