using Unity.Mathematics;
using UnityEngine;
using Utils.DisplayUtils;
namespace SimpleVectorFieldPathfinding
{
	public class DrawMap : MonoBehaviour
	{
		public TextureTester tester;
		int2 size;
		int[] map;
		void Start()
		{
			size = new(100, 100);
			map = Generate.GenerateMap(size, 0.05f, 10);
			var map_display = Generate.GenMapDisplay(size, map);
			tester.InitTexture(size);
			tester.SetTextureSlice(map_display, 0);
			tester.ApplyTexture();
		}

	}
}