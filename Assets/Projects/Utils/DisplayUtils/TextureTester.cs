using System;
using System.Linq;
using PrototypePackages.PrototypeUtils;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Utils.JobUtils;
namespace Utils.DisplayUtils
{
	[RequireComponent(typeof(Renderer), typeof(Collider))]
	public class TextureTester : MonoBehaviour
	{

	#region Event

		public event Action<(int2 texture_size, Texture2D texture)> OnTextureInited;
		public event Action<(Vector3 world_pos, int2 pixel_pos)> OnHoverTexture;

	#endregion

	#region Reference

		Renderer mRenderer;
		Collider mCollider;
		Camera mouse_camera;

	#endregion

	#region Config

		public int2 TextureSize;
		public int PixelCount => TextureSize.area();

	#endregion

	#region Data

		Texture2D mTexture;
		Index2D texture_i;

	#endregion

	#region Interface

		public void InitTexture(int2 textureSize)
		{
			TextureSize = textureSize;
			mTexture = new(TextureSize.x, TextureSize.y, TextureFormat.RGBAFloat, false) { filterMode = FilterMode.Point };
			mRenderer.material.mainTexture = mTexture;
			texture_i = new(textureSize);

			OnTextureInited?.Invoke((TextureSize, mTexture));
		}

		public void GetTextureSlice<TSliceStride>(out NativeSlice<TSliceStride> Slice, int float_offset_count) where TSliceStride : struct
		{
			Slice = mTexture.GetRawTextureData<float4>().Slice().
				SliceWithStride<TSliceStride>(sizeof(float) * float_offset_count);
		}

		public void SetTextureSlice<TSliceStride>(NativeArray<TSliceStride> Result, int float_offset_count) where TSliceStride : struct
		{
			var slice = mTexture.GetRawTextureData<float4>().Slice().
				SliceWithStride<TSliceStride>(sizeof(float) * float_offset_count);
			slice.CopyFrom(Result);
		}


		public void ApplyTexture()
		{
			mTexture.Apply();
		}
		
		void ApplyRGBResult(int2 TextureResultSize, NativeArray<float3> ResultRGB, float Alpha)
		{
			InitTexture(TextureResultSize);

			GetTextureSlice<float3>(out var rgb_slice, 0);
			rgb_slice.CopyFrom(ResultRGB);

			GetTextureSlice<float>(out var alpha_slice, 3);
			alpha_slice.CopyFrom(Enumerable.Repeat(Alpha, alpha_slice.Length).ToArray());

			ApplyTexture();
		}

	#endregion

	#region UnityEntry

		void Awake()
		{
			mRenderer = GetComponent<Renderer>();
			mCollider = GetComponent<Collider>();
			mouse_camera = Camera.main;
		}

		void Update()
		{
			var mouse_position = Input.mousePosition;
			if (Physics.Raycast(
				mouse_camera.ScreenPointToRay(mouse_position),
				out var hit_info, 1000000f, LayerMask.GetMask("TestTexture")))
			{
				if (hit_info.collider == mCollider)
				{
					var uv = hit_info.textureCoord;
					var pixel_pos = texture_i.SampleUV(uv.x, uv.y);
					OnHoverTexture?.Invoke((hit_info.point, pixel_pos));
				}
			}
		}

	#endregion

	}
}