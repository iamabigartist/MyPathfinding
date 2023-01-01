using Unity.Mathematics;
using UnityEngine;
namespace Utils.DisplayUtils
{
	public class PlanePixelRuler
	{
		public int2 size;
		public float cur_height;
		public Vector3 cur_scale;

		public PlanePixelRuler(int2 Size)
		{
			size = Size;
		}

		public Vector3 TextureLocationToWorldPosition(float2 location, float h_offset)
		{
			var local_scale = cur_scale;
			var pos_x = (location.x / size.x - 0.5f) * local_scale.x;
			var pos_z = (location.y / size.y - 0.5f) * local_scale.z;
			return new(pos_x, cur_height + h_offset, pos_z);
		}

		public Vector3 TextureVectorToWorldVector(float2 texture_vector)
		{
			var v_x = texture_vector.x / size.x * cur_scale.x;
			var v_z = texture_vector.y / size.y * cur_scale.z;
			return new(v_x, 0, v_z);
		}
	}
}