using Unity.Mathematics;
using UnityEngine;
using Utils.DisplayUtils;
using Utils.JobUtils;
namespace SimpleVectorFieldPathfinding
{
	public class MapDrawer
	{
		Index2D i;
		public PlanePixelRuler ruler;

	#region Interface

		public MapDrawer(PlanePixelRuler Ruler)
		{
			ruler = Ruler;
			i = new(ruler.size);
		}

		public delegate void DrawGrid(int index, int2 location);
		public void Draw(int2 center_location, float radius, float h_offset, DrawGrid DrawGridFunction)
		{
			int2 start_point = math.clamp(center_location - new int2((int)radius, (int)radius), new(0, 0), i.Size);
			for (int y = 0; y < radius * 2; y++)
			{
				for (int x = 0; x < radius * 2; x++)
				{
					int2 location = start_point + new int2(x, y);
					if (!i.OutOfRange(location))
					{
						int j = i[location];
						if (math.distance(center_location, location) < radius)
						{
							DrawGridFunction(j, location);
						}
					}
				}
			}
		}

	#endregion

	}
}