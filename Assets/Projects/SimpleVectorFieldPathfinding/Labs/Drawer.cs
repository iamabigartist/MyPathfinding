using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Utils.DisplayUtils;
using Utils.JobUtils;
using static Unity.Mathematics.math;
namespace SimpleVectorFieldPathfinding.Labs
{
	public class Drawer : MonoBehaviour
	{
		public TextureTester tester;
		public int2 size;
		public float obstacle_ratio;
		public uint seed;
		public float agent_speed;
		NativeArray<int> obstacle_map;
		NativeArray<int> distance_map;
		float2[] vector_map;
		AgentDriver agent_driver;

		PlanePixelRuler ruler;

		MapDrawer map_drawer;
		int2 map_draw_center;
		AgentDrawer agent_drawer;


		void Start()
		{
			obstacle_map = Generate.GenerateObstacleMap(size, obstacle_ratio, seed);
			distance_map = Generate.GenerateDistanceMap(size, obstacle_map, new(10, 10));
			vector_map = Generate.GenerateVectorMap(size, obstacle_map, distance_map);
			var rand_gen = new IndexRandGenerator(100);
			agent_driver = new(size, obstacle_map, vector_map,
				Enumerable.Range(0, 100).Select(i =>
				{
					rand_gen.Gen(i, out var rand);
					return rand.NextFloat2(new(0, 0), size - new int2(1, 1));
				}).ToList());
			var map_display = Generate.GenMapDisplay(size, obstacle_map);
			tester.InitTexture(size);
			tester.SetTextureSlice(map_display, 0);
			tester.ApplyTexture();
			tester.OnHoverTexture += Tuple =>
			{
				(Vector3 world_pos, int2 pixel_pos) = Tuple;
				map_draw_center = pixel_pos;
			};
			ruler = new(size);
			map_drawer = new(ruler);
			agent_drawer = new(agent_driver.agents);
		}

		void Update()
		{
			agent_driver.Step(Time.deltaTime * agent_speed);
		}

		void OnGUI()
		{
			var cur_transform = transform;
			ruler.cur_height = cur_transform.position.y;
			ruler.cur_scale = cur_transform.localScale;
			map_drawer.Draw(map_draw_center, 10, 0, (index, location) =>
			{
				var label_pos = ruler.TextureLocationToWorldPosition(location + new float2(0, 1), 0);
				var vector_pos = ruler.TextureLocationToWorldPosition(location + new float2(0.5f, 0.5f), 0);
				Handles.color = Color.black;
				int distance = distance_map[index];
				Handles.Label(label_pos, obstacle_map[index] == 0 ?
					$"<color=black>{distance}</color>" : "<color=red>inf</color>");
				var vector = ruler.TextureVectorToWorldVector(new(sin(distance), cos(distance))) * 0.3f;
				Handles.DrawLine(vector_pos, vector_pos + vector);

			});

			Handles.color = Color.blue;
			agent_drawer.Draw((index, location) =>
			{
				var agent_pos = ruler.TextureLocationToWorldPosition(location, 0);
				var corner0 = ruler.TextureLocationToWorldPosition(location + new float2(0.5f, 0.5f), 0);
				var corner1 = corner0;
				var corner2 = corner0;
				corner1.x = agent_pos.x;
				corner2.z = agent_pos.z;
				Handles.DrawAAPolyLine(3, agent_pos, corner1, corner0, corner2, agent_pos);
			});

		}

	}
}