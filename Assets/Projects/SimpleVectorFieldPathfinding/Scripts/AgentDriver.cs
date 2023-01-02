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
			if (obstacle_map[map_i[(int2)floor(to)]] == 1)
			{
				to = sample_location + new float2(0.5f, 0.5f);
			}
		}
		public AgentDriver(int2 size, NativeArray<int> ObstacleMap, List<float2> Agents)
		{
			map_i = new(size);
			obstacle_map = ObstacleMap;
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