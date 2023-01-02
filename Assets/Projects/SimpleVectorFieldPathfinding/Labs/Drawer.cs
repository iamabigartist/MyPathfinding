using System.Linq;
using Unity.Collections;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using Utils.DisplayUtils;
using Utils.JobUtils;
using static SimpleVectorFieldPathfinding.Generate;
using Random = Unity.Mathematics.Random;
namespace SimpleVectorFieldPathfinding.Labs
{
	public class Drawer : MonoBehaviour
	{
		public TextureTester tester;
		public uint seed;
		public int2 size;
		public CnoiseSampler sampler;
		public float obstacle_ratio;
		public Gradient gradient;
		public float agent_speed;
		public int2 destination;
		NativeArray<int> obstacle_map;
		NativeArray<int> distance_map;
		NativeArray<int2> parent_map;
		Index2D map_i;
		PlanePixelRuler ruler;
		MapDrawer map_drawer;
		AgentDriver agent_driver;
		int2 mouse_hover_location;
		AgentDrawer agent_drawer;

	#region Process

		void ClearMem()
		{
			ClearPathMem();
			obstacle_map.Dispose();
		}

		void ClearPathMem()
		{
			distance_map.Dispose();
			parent_map.Dispose();
		}

		int2 ChooseRandomAvailableLocation(Random rand)
		{
			while (true)
			{
				var pos = rand.NextInt2(new(0, 0), size);
				if (obstacle_map[map_i[pos]] == 0)
				{
					return pos;
				}
			}
		}

		void GeneratePath()
		{
			Generate.GeneratePath(size, obstacle_map, destination, out var max_distance, out distance_map, out parent_map);
			var map_display = GenMapDisplay(size, obstacle_map, distance_map, gradient, max_distance);
			tester.InitTexture(size);
			tester.SetTextureSlice(map_display, 0);
			tester.ApplyTexture();
			agent_driver.parent_map = parent_map;
			agent_driver.destination = destination;
		}

	#endregion

	#region UnityInterfcae

		void OnDestroy()
		{
			ClearMem();
		}

		void Start()
		{
			obstacle_map = GenerateObstacleMap(size, obstacle_ratio, sampler);
			map_i = new(size);
			ruler = new(size);
			map_drawer = new(ruler);

			var rand_gen = new IndexRandGenerator(100);
			agent_driver = new(size, obstacle_map,
				Enumerable.Range(0, 1000).Select(i =>
				{
					rand_gen.Gen(i, out var rand);
					return ChooseRandomAvailableLocation(rand) + new float2(0.5f, 0.5f);
				}).ToList());
			agent_drawer = new(agent_driver.agents);

			destination = ChooseRandomAvailableLocation(new(seed));
			GeneratePath();

			tester.OnHoverTexture += Tuple =>
			{
				(Vector3 world_pos, int2 pixel_pos) = Tuple;
				mouse_hover_location = pixel_pos;
			};
		}

		void Update()
		{
			if (Input.GetKeyDown(KeyCode.Mouse0))
			{
				if (obstacle_map[map_i[mouse_hover_location]] == 0)
				{
					destination = mouse_hover_location;
					ClearPathMem();
					GeneratePath();
				}
			}

			agent_driver.Step(Time.deltaTime * agent_speed);
		}

		void OnGUI()
		{
			var cur_transform = transform;
			ruler.cur_height = cur_transform.position.y;
			ruler.cur_scale = cur_transform.localScale;

			Handles.color = Color.black;
			map_drawer.Draw(mouse_hover_location, 10, 0, (i, location) =>
			{
				if (obstacle_map[i] == 0 && !parent_map[i].Equals(new(-1, -1)))
				{
					var label_pos = ruler.TextureLocationToWorldPosition(location + new float2(0, 1), 0);
					var vector_pos = ruler.TextureLocationToWorldPosition(location + new float2(0.5f, 0.5f), 0);
					var cube_size = ruler.TextureLengthToWorldLength_X(0.5f) / 20f;
					int distance = distance_map[i];
					var next_pos = ruler.TextureLocationToWorldPosition(parent_map[i] + new float2(0.5f, 0.5f), 0);
					var vector = (next_pos - vector_pos) * 0.5f;
					Handles.DrawSolidDisc(vector_pos, Vector3.up, cube_size);
					Handles.DrawLine(vector_pos, vector_pos + vector);
					
					Handles.Label(label_pos, $"<color=black>{distance}</color>");

				}
			});
			
			Handles.color = Color.blue;
			agent_drawer.Draw((index, location) =>
			{
				var corner_0 = ruler.TextureLocationToWorldPosition(location + new float2(0.1f, 0.1f), 0);
				var corner_1 = ruler.TextureLocationToWorldPosition(location - new float2(0.1f, 0.1f), 0);
				var corner_2 = ruler.TextureLocationToWorldPosition(location + new float2(-0.1f, 0.1f), 0);
				var corner_3 = ruler.TextureLocationToWorldPosition(location - new float2(-0.1f, 0.1f), 0);
				// Handles.Draw(agent_pos, Vector3.up, 0.1f);
				Handles.DrawAAPolyLine(3, corner_0, corner_1);
				Handles.DrawAAPolyLine(3, corner_2, corner_3);
			});

			Handles.color = Color.red;
			Handles.DrawWireDisc(
				ruler.TextureLocationToWorldPosition(destination + new float2(0.5f, 0.5f), 0), Vector3.up,
				ruler.TextureLengthToWorldLength_X(0.5f) / 5f);

		}

	#endregion


	}
}