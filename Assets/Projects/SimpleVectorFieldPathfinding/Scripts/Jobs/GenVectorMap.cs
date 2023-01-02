using Unity.Collections;
using Unity.Mathematics;
using Utils.JobUtils;
using Utils.JobUtils.Template;
namespace SimpleVectorFieldPathfinding.Jobs
{
	public struct GenVectorMap : IJobForRunner
	{
		public (int ExecuteLen, int InnerLoopBatchCount) ScheduleParam => (distance_map.Length, 64);

		Index2D map_i;
		NativeArray<int> distance_map;
		NativeArray<float2> vector_map;

		void GetDistanceAvailable(int2 pos, int default_value, out int available_distance)
		{
			if (!map_i.OutOfRange(pos))
			{
				available_distance = distance_map[map_i[pos]];
				if (available_distance == -1)
				{
					available_distance = default_value;
				}
			}
			else
			{
				available_distance = default_value;
			}
		}

		public void Execute(int i)
		{
			var cur_d = distance_map[i];
			if (cur_d == -1) { return; }
			var cur_pos = map_i[i];
			var left_pos = cur_pos + new int2(-1, 0);
			var right_pos = cur_pos + new int2(+1, 0);
			var down_pos = cur_pos + new int2(0, -1);
			var up_pos = cur_pos + new int2(0, +1);
			GetDistanceAvailable(left_pos, cur_d, out var left_d);
			GetDistanceAvailable(right_pos, cur_d, out var right_d);
			GetDistanceAvailable(down_pos, cur_d, out var down_d);
			GetDistanceAvailable(up_pos, cur_d, out var up_d);
			var vector = new float2(left_d - right_d, down_d - up_d);
			vector_map[i] = math.normalize(vector);
			// vector_map[i] = vector;
		}

		public GenVectorMap(Index2D MapI, NativeArray<int> DistanceMap, out NativeArray<float2> VectorMap)
		{
			map_i = MapI;
			distance_map = DistanceMap;
			vector_map = new(distance_map.Length, Allocator.TempJob);
			VectorMap = vector_map;
		}
	}
}