using AlienRace;
using Pawnmorph.GraphicSys;
using Pawnmorph.Hediffs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;

namespace Pawnmorph.User_Interface.Preview
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


        public int Height
        {
            get => _height;
            set => _height = value;
        }

        public int PreviewIndex
        {
            get => _previewOffsetX / -10;
            set => _previewOffsetX = -10 * value;
        }

        public int Width
        {
            get => _width;
            set => _width = value;
        }

        public Rot4 Rotation
        {
            get => _rotation;
            set => _rotation = value;
        }

        protected Preview(int height, int width)
        {
            Height = height;
            Width = width;
            _rotation = Rot4.South;
            _up = Quaternion.AngleAxis(0f, Vector3.up);
            InitCamera();
        }

        public void Refresh()
        {
            OnRefresh();

            _camera.gameObject.SetActive(true);
            _camera.transform.position = new Vector3(_previewOffsetX, 1f, 0f);
            _camera.Render();
            _camera.gameObject.SetActive(false);
        }

        public abstract void OnRefresh();


        public void Draw(Rect boundingBox)
        {
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
                _camera.transform.position = new Vector3(0f, 1f, 0f);
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
