using System.Runtime.CompilerServices;
using Project.JobTerrainGen.Utils.JobUtil.Template;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Utils.JobUtils;
namespace SimpleVectorFieldPathfinding.Jobs
{
	public struct ScanDistance_Half : IJobForRunner
	{
		public (int ExecuteLen, int InnerLoopBatchCount) ScheduleParam => (cur_visit_list.Length, 1);

		int cur_distance;
		int visit_direction;

		NativeList<int2> cur_visit_list;
		NativeList<int2>.ParallelWriter next_visit_list;

		Index2D map_i;
		NativeArray<bool> obstacle_map;
		NativeArray<bool> added_map;
		NativeArray<int> distance_map;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool JudgeCanAdd(int2 pos)
		{
			return !map_i.OutOfRange(pos) &&
				!obstacle_map[map_i[pos]] &&
				!added_map[map_i[pos]];
		}

		public void Execute(int i)
		{
			var cur_pos = cur_visit_list[i];

			//assign distance
			distance_map[map_i[cur_pos]] = cur_distance;

			//gather neighbors
			var x_neighbor_pos = cur_pos + new int2(visit_direction, 0);
			var y_neighbor_pos = cur_pos + new int2(0, visit_direction);
			if (JudgeCanAdd(x_neighbor_pos)) { next_visit_list.AddNoResize(x_neighbor_pos); }
			if (JudgeCanAdd(y_neighbor_pos)) { next_visit_list.AddNoResize(y_neighbor_pos); }
		}

		public ScanDistance_Half(int CurDistance, bool PositiveDirection, NativeList<int2> CurVisitList, NativeList<int2> NextVisitList, int2 map_size, NativeArray<bool> ObstacleMap, NativeArray<bool> AddedMap, NativeArray<int> DistanceMap)
		{
			cur_distance = CurDistance;
			visit_direction = PositiveDirection ? 1 : -1;
			cur_visit_list = CurVisitList;
			next_visit_list = NextVisitList.AsParallelWriter();
			map_i = new(map_size);
			obstacle_map = ObstacleMap;
			added_map = AddedMap;
			distance_map = DistanceMap;
		}
	}
}