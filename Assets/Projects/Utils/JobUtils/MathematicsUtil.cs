using Unity.Mathematics;
using UnityEngine;
namespace Utils.JobUtils
{
	public static class MathematicsUtil
	{
		public static int volume(this int3 i3) { return i3.x * i3.y * i3.z; }
		public static int area(this int2 i2) { return i2.x * i2.y; }
		public static float3 f3(this Color color) { return new(color.r, color.g, color.b); }
	}
}