using Unity.Mathematics;
using static Unity.Mathematics.math;
namespace Utils.JobUtils
{

	public static class IndexUtil {}

	public readonly struct Index2D
	{
		public readonly int2 Size;

		public Index2D(int2 size)
		{
			Size = size;
		}

	#region Property

		public int Count => Size.x * Size.y;
		public int2 CenterPoint => Size / 2;

	#endregion

	#region Indexer

		public int this[int x, int y] => x + y * Size.x;
		public int this[int2 pos] => pos.x + pos.y * Size.x;
		public int2 this[int i]
		{
			get
			{
				int y = i / Size.x;
				i -= y * Size.x;
				int x = i;

				return new(x, y);
			}
		}

	#endregion

	#region Util

		public bool OutOfRange(int x, int y)
		{
			return
				x < 0 || x > Size.x - 1 ||
				y < 0 || y > Size.y - 1;
		}

		public bool OutOfRange(int2 p)
		{
			return
				p.x < 0 || p.x > Size.x - 1 ||
				p.y < 0 || p.y > Size.y - 1;
		}

		public bool IsEdge(int2 p)
		{
			return
				p.x == 0 || p.x == Size.x - 1 ||
				p.y == 0 || p.y == Size.y - 1;
		}

		public int2 SampleUV(float u, float v)
		{
			return (int2)round(new float2(Size.x * u - 0.5f, Size.y * v - 0.5f));
		}

		public int2 RepeatWrap(int2 p)
		{
			var result = p;
			if (p.x >= Size.x) { result.x -= Size.x; }
			if (p.y >= Size.y) { result.y -= Size.y; }
			return result;
		}

	#endregion
	}

	public readonly struct Index3D
	{
		public readonly int3 Size;

		public Index3D(int3 size)
		{
			Size = size;
		}

	#region Property

		public int Count => Size.x * Size.y * Size.z;
		public int3 CenterPoint => Size / 2;

	#endregion

	#region Indexer

		public int this[int x, int y, int z] => x + y * Size.x + z * Size.y * Size.x;
		public int3 this[int i]
		{
			get
			{
				int z = i / (Size.x * Size.y);
				i -= z * Size.x * Size.y;
				int y = i / Size.x;
				i -= y * Size.x;
				int x = i;

				return new(x, y, z);
			}
		}

	#endregion

	#region Util

		public bool OutOfRange(int x, int y, int z)
		{
			return
				x < 0 || x > Size.x - 1 ||
				y < 0 || y > Size.y - 1 ||
				z < 0 || z > Size.z - 1;
		}

	#endregion
	}
}