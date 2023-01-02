using System;
using System.Linq;
using System.Threading.Tasks;
using SimpleVectorFieldPathfinding.Jobs;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Utils.JobUtils;
using Utils.JobUtils.Template;
using static Unity.Mathematics.noise;
namespace SimpleVectorFieldPathfinding
{
	public static class Generate
	{
		[Serializable]
		public class CnoiseSampler : INoiseSampler<float2, float>
		{
			public float2 offset;
			public float scale;
			public CnoiseSampler(float2 Offset, float Scale)
			{
				offset = Offset;
				scale = Scale;
			}
			public float2 AdjustInput(float2 ori_input) { return (ori_input + offset) * scale; }
			public float AdjustOutput(float ori_result) { return (ori_result + 1) / 2f; }
		}
		public static NativeArray<int> GenerateObstacleMap(int2 size, float obstacle_ratio, CnoiseSampler sampler)
		{
			var map_i = new Index2D(size);
			var obstacle_map = new int[size.area()];
			Parallel.For(0, obstacle_map.Length, i =>
			{
				float2 pos = map_i[i];
				obstacle_map[i] = sampler.AdjustOutput(cnoise(sampler.AdjustInput(pos))) < obstacle_ratio ? 1 : 0;
			});
			return new(obstacle_map, Allocator.Persistent);
		}
		public static NativeArray<float3> GenMapDisplay(int2 size, NativeArray<int> obstacle_map, NativeArray<int> distance_map, Gradient gradient, float max_distance)
		{
			var rgb = new NativeArray<float3>(size.area(), Allocator.Temp);
			Parallel.For(0, size.area(), i =>
			{
				if (obstacle_map[i] == 1) { rgb[i] = new(0, 0, 0); }
				else if (obstacle_map[i] == 0)
				{
					rgb[i] = gradient.Evaluate(distance_map[i] / max_distance).f3();
				}
			});
			return rgb;
		}

		public static NativeArray<int> GenerateDistanceMap(int2 size, NativeArray<int> obstacle_map, int2 start_pos, out int max_distance)
		{
			//Declare and allocate memories
			var map_i = new Index2D(size);
			var distance_map = new NativeArray<int>(Enumerable.Repeat(-1, size.area()).ToArray(), Allocator.TempJob); //Initialise with -1
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

			max_distance = cur_distance;

			return return_map;
		}

		public static NativeArray<float2> GenerateVectorMap(int2 size, NativeArray<int> distance_map)
		{
			var deps = new JobHandle();
			JobFor<GenVectorMap>.Plan(new(new(size), distance_map, out var vector_map), ref deps);
			deps.Complete();
			return new(vector_map, Allocator.Persistent);
		}
	}
}