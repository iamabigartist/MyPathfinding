using System.Threading.Tasks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Utils.JobUtils;
namespace SimpleVectorFieldPathfinding
{
	public static class Generate
	{
		public static int[] GenerateMap(int2 size, float obstacle_ratio, uint seed)
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
					rgb[i] = new(0, 1, 0);
				}
			});
			return rgb;
		}
	}
}