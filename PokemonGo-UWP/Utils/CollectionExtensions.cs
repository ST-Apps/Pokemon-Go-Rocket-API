using System;
using System.Collections.Generic;
using System.Linq;
using PokemonGo_UWP.Entities;
using Template10.Utils;

namespace PokemonGo_UWP.Utils
{
    public static class CollectionExtensions
    {
        public static void UpdateWith<T, T1>(this ICollection<T> destination, IReadOnlyCollection<T1> updates, Func<T1, T> createFunc, Func<T, T1, bool> compareFunc)
            where T : IUpdatable<T1>
        {
            // delete items that no longer exist in the destination collection
            destination
                .Where(x => updates.All(y => !compareFunc(x, y)))
                .ToArray() // create a copy because we're modifying the existing collection
                .ForEach(x => destination.Remove(x));

            foreach (var update in updates)
            {
                var existingItem = destination.FirstOrDefault(x => compareFunc(x, update));
                if (existingItem == null)
                {
                    // add the update into the collection
                    destination.Add(createFunc(update));
                }
                else
                {
                    // update item in destination
                    existingItem.Update(update);
                }
            }
        }

        public static void UpdateByIndexWith<T, T1>(this IList<T> destination, IReadOnlyList<T1> updates, Func<T1, T> createFunc)
            where T : IUpdatable<T1>
        {
            // we update the existing collection by index
            var i = 0;
            for (; i < updates.Count; i++)
            {
                if (destination.Count > i)
                    destination[i].Update(updates[i]);
                else
                    destination.Add(createFunc(updates[i]));
            }
            // cull the rest of the destination collection
            for (; i < destination.Count; i++)
            {
                destination.RemoveAt(i);
            }
        }
    }
}