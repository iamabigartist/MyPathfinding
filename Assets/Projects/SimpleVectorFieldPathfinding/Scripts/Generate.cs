using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using Utils.JobUtils;
using static Unity.Mathematics.math;
namespace SimpleVectorFieldPathfinding
{
	public static class Generate
	{
		public static int[] GenerateObstacleMap(int2 size, float obstacle_ratio, uint seed)
		{
			var rand_gen = new IndexRandGenerator(seed);
			var map = new int[size.area()];
			Parallel.For(0, map.Length, i =>
			{
				rand_gen.Gen(i, out var rand);
				map[i] = rand.NextFloat(0, 1) < obstacle_ratio ? 1 : 0;
			});
			return map;
		}

		public static NativeArray<float3> GenMapDisplay(int2 size, int[] map)
		{
			var rgb = new NativeArray<float3>(size.area(), Allocator.Temp);
			Parallel.For(0, map.Length, i =>
			{
				if (map[i] == 1) { rgb[i] = new(0, 0, 0); }
				else if (map[i] == 0)
				{
					rgb[i] = new(0f, 0.7f, 0f);
				}
			});
			return rgb;
		}

		public static int[] GenerateDistanceMap(int2 size, int[] obstacle_map)
		{
			var rand_gen = new IndexRandGenerator(100);
			var map = new int[size.area()];
			Parallel.For(0, map.Length, i =>
			{
				rand_gen.Gen(i, out var rand);
				map[i] = rand.NextInt(0, 100);
			});
			return map;
		}

		public static float2[] GenerateVectorMap(int2 size, int[] obstacle_map, int[] distance_map)
		{
			var map = new float2[size.area()];
			Parallel.For(0, map.Length, i =>
			{
				var distance = distance_map[i];
				if (obstacle_map[i] == 0)
				{
					map[i] = normalize(new float2(sin(distance), cos(distance)));
				}
				else
				{
					map[i] = new(0, 0);
				}
			});
			return map;
		}
	}
}