using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Mathematics;
using Utils.JobUtils;
using static Unity.Mathematics.math;
namespace SimpleVectorFieldPathfinding
{
	public class AgentDriver
	{
		public Index2D map_i;
		public int[] obstacle_map;
		public float2[] vector_map;
		public List<float2> agents;

		void CheckEdge(float2 location, out float2 adjusted)
		{
			var sample_location = (int2)floor(location);
			var cur_loc = sample_location;
			if (cur_loc.x < 0)
			{
				cur_loc.x += 1;
			}
			if (cur_loc.y < 0)
			{
				cur_loc.y += 1;
			}
			if (cur_loc.x >= map_i.Size.x)
			{
				cur_loc.x -= 1;
			}
			if (cur_loc.y >= map_i.Size.y)
			{
				cur_loc.y -= 1;
			}
			var changed = cur_loc != sample_location;
			adjusted = changed.x || changed.y ? cur_loc : location;
		}

		void CheckPenetration(float2 location, out float2 adjusted)
		{
			var sample_location = (int2)floor(location);
			var cur_loc = sample_location;
			if (obstacle_map[map_i[sample_location]] == 1)
			{
				var round_location = (int2)round(location);
				cur_loc += 2 * (sample_location - round_location) - 1;
			}
			var changed = cur_loc != sample_location;
			adjusted = changed.x || changed.y ? cur_loc : location;
		}
		
		void Move(float2 from, float distance, out float2 to)
		{
			var sample_location = (int2)floor(from);
			var vector = vector_map[map_i[sample_location]];
			to = from + vector * distance;
			CheckEdge(to, out to);
			CheckPenetration(to, out to);
		}
		public AgentDriver(int2 size, int[] ObstacleMap, float2[] VectorMap, List<float2> Agents)
		{
			map_i = new(size);
			obstacle_map = ObstacleMap;
			vector_map = VectorMap;
			agents = Agents;
		}
		public void Step(float distance)
		{
			Parallel.For(0, agents.Count, i =>
			{
				var location = agents[i];
				Move(location, distance, out var new_location);
				agents[i] = new_location;
			});
		}
	}
}