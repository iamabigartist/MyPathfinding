using System.Runtime.CompilerServices;
using Unity.Collections;
using Unity.Mathematics;
using Utils.JobUtils;
using Utils.JobUtils.Template;
namespace SimpleVectorFieldPathfinding.Jobs
{
	public struct ScanDistance_Half : IJobForRunner
	{
		public (int ExecuteLen, int InnerLoopBatchCount) ScheduleParam => (cur_visit_list.Length, 1);

		int cur_distance;
		int visit_direction;

		[ReadOnly] NativeList<int2> cur_visit_list;
		[WriteOnly] NativeList<int2>.ParallelWriter next_visit_list;

		Index2D map_i;
		NativeArray<int> obstacle_map;
		NativeArray<bool> added_map;
		NativeArray<int> distance_map;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		bool JudgeCanAdd(int2 pos)
		{
			return !map_i.OutOfRange(pos) &&
				obstacle_map[map_i[pos]] == 0 &&
				!added_map[map_i[pos]];
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		void AddToList(int2 pos)
		{
			next_visit_list.AddNoResize(pos);
			added_map[map_i[pos]] = true;
		}

		public void Execute(int i)
		{
			var cur_pos = cur_visit_list[i];

			//assign distance
			distance_map[map_i[cur_pos]] = cur_distance;

			//gather neighbors
			var x_neighbor_pos = cur_pos + new int2(visit_direction, 0);
			var y_neighbor_pos = cur_pos + new int2(0, visit_direction);
			if (JudgeCanAdd(x_neighbor_pos)) { AddToList(x_neighbor_pos); }
			if (JudgeCanAdd(y_neighbor_pos)) { AddToList(y_neighbor_pos); }
		}

		public ScanDistance_Half(int CurDistance, bool PositiveDirection, NativeList<int2> CurVisitList, NativeList<int2> NextVisitList, Index2D MapI, NativeArray<int> ObstacleMap, NativeArray<bool> AddedMap, NativeArray<int> DistanceMap)
		{
			cur_distance = CurDistance;
			visit_direction = PositiveDirection ? 1 : -1;
			cur_visit_list = CurVisitList;
			next_visit_list = NextVisitList.AsParallelWriter();
			map_i = MapI;
			obstacle_map = ObstacleMap;
			added_map = AddedMap;
			distance_map = DistanceMap;
		}
	}
}