using System;
using UnityEngine;
using Verse;

namespace Pawnmorph.UserInterface.Preview
{
	internal abstract class Preview
	{
		protected int _previewOffsetX = -10; // Render pawn away from the map to avoid capturing parts of the map.

		Camera _camera;
		RenderTexture _previewTexture;
		protected int _height;
		protected int _width;
		protected Rot4 _rotation;
		protected Quaternion _up;
		protected float _scale;

		/// <summary>
		/// Gets or sets the height of the preview texture.
		/// </summary>
		public int Height
		{
			get => _height;
			set => _height = value;
		}

		/// <summary>
		/// Gets or sets the index of the preview. Identical indexes may cause preview overlap.
		/// </summary>
		public int PreviewIndex
		{
			get => _previewOffsetX / -10;
			set => _previewOffsetX = -10 * value;
		}

		/// <summary>
		/// Gets or sets the width of the preview texture.
		/// </summary>
		public int Width
		{
			get => _width;
			set => _width = value;
		}

		/// <summary>
		/// Gets or sets the preview rotation.
		/// </summary>
		public Rot4 Rotation
		{
			get => _rotation;
			set => _rotation = value;
		}

		protected Preview(int height, int width)
		{
			_scale = 1f;
			Height = height;
			Width = width;
			_rotation = Rot4.South;
			_up = Quaternion.AngleAxis(0f, Vector3.up);
			InitCamera();
		}

		/// <summary>
		/// Triggers an invalidate and refresh of displayed preview.
		/// </summary>
		public void Refresh()
		{
			OnRefresh();
			OnDraw(new Vector3(_previewOffsetX, 0f, 0f));

			_camera.gameObject.SetActive(true);
			_camera.transform.position = new Vector3(_previewOffsetX, 1f, 0f);
			_camera.orthographicSize = 1f / _scale;
			_camera.Render();
			_camera.gameObject.SetActive(false);
		}

		/// <summary>
		/// Called when preview is refreshed.
		/// </summary>
		protected abstract void OnRefresh();

		/// <summary>
		/// Called when preview should draw.
		/// </summary>
		protected abstract void OnDraw(Vector3 drawPosition);

		/// <summary>
		/// Draws preview to the specified bounding box.
		/// </summary>
		/// <param name="boundingBox">The bounding box.</param>
		public void Draw(Rect boundingBox)
		{
			if (_previewTexture != null)
				GUI.DrawTexture(boundingBox, _previewTexture);
		}

		private void InitCamera()
		{
			if (_camera == null)
			{
				var cameraObject = new GameObject("PreviewCamera", new Type[]
				{
					typeof(Camera)
				});
				cameraObject.SetActive(false);

				_camera = cameraObject.GetComponent<Camera>();
				_camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
				_camera.orthographic = true;
				_camera.orthographicSize = 1f;
				_camera.clearFlags = CameraClearFlags.Color;
				_camera.backgroundColor = new Color(0f, 0f, 0f, 0f);
				_camera.renderingPath = RenderingPath.Forward;
				_camera.nearClipPlane = Current.Camera.nearClipPlane;
				_camera.farClipPlane = Current.Camera.farClipPlane;
				_camera.forceIntoRenderTexture = false;
				_previewTexture = new RenderTexture(_width, _height, 24);
				_camera.targetTexture = _previewTexture;
			}
		}
	}
}
