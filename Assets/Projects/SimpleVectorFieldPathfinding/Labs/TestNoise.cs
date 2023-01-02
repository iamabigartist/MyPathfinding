using Unity.Mathematics;
using UnityEngine;
namespace SimpleVectorFieldPathfinding.Labs
{
	[ExecuteAlways]
	public class TestNoise : MonoBehaviour
	{
		public float2 f2;
		public float result;
		void Update()
		{
			result = noise.cnoise(f2);
		}
	}
}