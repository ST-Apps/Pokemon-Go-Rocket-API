using System.Collections.Generic;
using System.Linq;
using Google.Common.Geometry;

namespace PokemonGo.RocketAPI.Helpers
{
    public class S2Helper
    {
        public static List<ulong> GetNearbyCellIds(double longitude, double latitude)
        {
            var nearbyCellIds = new List<S2CellId>();

            var cellId = S2CellId.FromLatLng(S2LatLng.FromDegrees(latitude, longitude)).ParentForLevel(15);

            nearbyCellIds.Add(cellId);

            var neighbours = new List<S2CellId>();
            cellId.GetAllNeighbors(15, neighbours);

            foreach (var neighbour in neighbours)
            {
                nearbyCellIds.Add(neighbour);
                nearbyCellIds.AddRange(neighbour.GetEdgeNeighbors());
            }

            return nearbyCellIds.Select(c => c.Id).Distinct().OrderBy(c => c).ToList();
        }

        private static S2CellId GetPrevious(S2CellId cellId, int depth)
        {
            if (depth < 0)
                return cellId;

            depth--;

            return GetPrevious(cellId.Previous, depth);
        }

        private static S2CellId GetNext(S2CellId cellId, int depth)
        {
            if (depth < 0)
                return cellId;

            depth--;

            return GetNext(cellId.Next, depth);
        }
    }
}