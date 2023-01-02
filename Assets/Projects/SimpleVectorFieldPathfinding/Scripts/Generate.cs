using System.Threading.Tasks;
using SimpleVectorFieldPathfinding.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Utils.JobUtils;
using Utils.JobUtils.Template;
using static Unity.Mathematics.math;
namespace SimpleVectorFieldPathfinding
{
	public static class Generate
	{
		public static NativeArray<int> GenerateObstacleMap(int2 size, float obstacle_ratio, uint seed)
		{
			var rand_gen = new IndexRandGenerator(seed);
			var map = new int[size.area()];
			Parallel.For(0, map.Length, i =>
			{
				rand_gen.Gen(i, out var rand);
				map[i] = rand.NextFloat(0, 1) < obstacle_ratio ? 1 : 0;
			});
			return new(map, Allocator.Persistent);
		}

		public static NativeArray<float3> GenMapDisplay(int2 size, NativeArray<int> map)
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


		public static NativeArray<int> GenerateDistanceMap(int2 size, NativeArray<int> obstacle_map, int2 start_pos)
		{
			//Declare and allocate memories
			var map_i = new Index2D(size);
			var distance_map = new NativeArray<int>(size.area(), Allocator.TempJob);
			var added_map = new NativeArray<bool>(size.area(), Allocator.TempJob);
			var visit_list_0 = new NativeList<int2>(1, Allocator.TempJob);
			var visit_list_1 = new NativeList<int2>(4, Allocator.TempJob);
			var cur_visit_list = visit_list_0;
			var next_visit_list = visit_list_1;
			var cur_distance = 0;
			var deps = new JobHandle();

			//Build initial situation
			cur_visit_list.Add(start_pos);
			added_map[map_i[start_pos]] = true;

			//Round scan
			while (cur_visit_list.Length > 0)
			{
				//Scan twice, one for positive directions, another for negative ones
				JobFor<ScanDistance_Half>.PlanByRef(new(cur_distance, true, cur_visit_list, next_visit_list, map_i, obstacle_map, added_map, distance_map), ref deps, out var job_a);
				JobFor<ScanDistance_Half>.PlanByRef(new(cur_distance, false, cur_visit_list, next_visit_list, map_i, obstacle_map, added_map, distance_map), ref deps, out var job_b);
				deps.Complete();

				//Next round prepare
				(cur_visit_list, next_visit_list) = (next_visit_list, cur_visit_list); //Swap 2 list
				cur_distance++;
				//Resize write buffer according to current neighbor count.
				next_visit_list.Clear();
				next_visit_list.SetCapacity(cur_visit_list.Length * 3); //at most 3 neighbor collected
			}

			var return_map = new NativeArray<int>(distance_map, Allocator.Persistent);
			distance_map.Dispose();
			added_map.Dispose();
			visit_list_0.Dispose();
			visit_list_1.Dispose();

			return return_map;
		}

		public static float2[] GenerateVectorMap(int2 size, NativeArray<int> obstacle_map, NativeArray<int> distance_map)
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