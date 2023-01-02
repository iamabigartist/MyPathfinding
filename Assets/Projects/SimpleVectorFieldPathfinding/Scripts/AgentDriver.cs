using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using Utils.JobUtils;
using static Unity.Mathematics.math;
namespace SimpleVectorFieldPathfinding
{
	public class AgentDriver
	{
		public Index2D map_i;
		public NativeArray<int> obstacle_map;
		public NativeArray<int2> parent_map;
		public List<float2> agents;
		public int2 destination;

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
			var next_location = parent_map[map_i[sample_location]];
			if (sample_location.Equals(destination) || next_location.Equals(new(-1, -1)))
			{
				to = from;
				return;
			}
			var target = next_location + new float2(0.5f, 0.5f);
			var vector = normalize(target - from);
			to = from + vector * distance;
			CheckEdge(to, out to);
			CheckPenetration(to, out to);
		}
		public AgentDriver(int2 size, NativeArray<int> ObstacleMap, NativeArray<int2> ParentMap, List<float2> Agents, int2 Destination)
		{
			map_i = new(size);
			obstacle_map = ObstacleMap;
			parent_map = ParentMap;
			agents = Agents;
			destination = Destination;
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