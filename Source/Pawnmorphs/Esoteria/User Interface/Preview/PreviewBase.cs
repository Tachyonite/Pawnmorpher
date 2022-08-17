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
    internal abstract class PreviewBase
    {
        static int PREVIEW_POSITION_X = -10; // Render pawn away from the map to avoid capturing parts of the map.

        GameObject _cameraObject;
        Camera _camera;
        RenderTexture _previewTexture;
        int _height;
        int _width;
        Rot4 __rotation;
        Quaternion _up;
        Vector2 _headOffset;

        Graphic _bodyGraphics;
        Graphic _headGraphics;
        Graphic _hairGraphics;

        DrawOptions _drawOptions;

        IList<ApparelGraphicRecord> _apparelGraphics;
        IList<Apparel> _apparel;

        

        public int Height
        {
            get => _height;
            set => _height = value;
        }
        public int Width
        {
            get => _width;
            set => _width = value;
        }

        public Rot4 _rotation
        {
            get => __rotation;
            set => __rotation = value;
        }

        protected PreviewBase(int height, int width)
        {
            Height = height;
            Width = width;
            __rotation = Rot4.South;
            _up = Quaternion.AngleAxis(0f, Vector3.up);
            _drawOptions = DrawOptions.All;
        }

        public void Refresh()
        {
            int num2;
            //return GraphicDatabase.Get<Graphic_Multi_RotationFromData>(empty += (((num2 = (savedIndex.HasValue ? (sharedIndex = savedIndex.Value % num) : (linkVariantIndexWithPrevious ? (sharedIndex % num) : (sharedIndex = Rand.Range(0, num))))) == 0) ? "" : num2.ToString()), (ContentFinder<Texture2D>.Get(empty + "_northm", reportFailure: false) == null) ? ShaderType.Shader : ShaderDatabase.CutoutComplex, drawSize * 1.5f, channel.first, channel.second, new GraphicData
            //{
            //    drawRotated = !drawRotated
            //}
        }

        public void AddMutation(MutationDef mutation)
        {

        }



        public abstract void OnRefresh();
        public void Draw(Rect boundingBox)
        {
            if (_bodyGraphics == null || _headGraphics == null)
                return;
            Vector3 headOffset = _up * _headOffset; // Can be cached.


            // Draw body
            Vector3 bodyOffset = new Vector3(PREVIEW_POSITION_X, 0.007575758f, 0f);
            Mesh bodyMesh = _bodyGraphics.MeshAt(__rotation);
            if (_drawOptions == DrawOptions.All || (_drawOptions & DrawOptions.Body) == DrawOptions.Body)
            {
                Material bodyMaterial = _bodyGraphics.MatAt(__rotation);
                GenDraw.DrawMeshNowOrLater(bodyMesh, bodyOffset, _up, bodyMaterial, false);
            }


            Vector3 clothesLayer = new Vector3(PREVIEW_POSITION_X, (__rotation == Rot4.North ? 0.026515152f : 0.022727273f), 0f);
            Vector3 headLayer = new Vector3(PREVIEW_POSITION_X, (__rotation == Rot4.North ? 0.022727273f : 0.026515152f), 0f);


            if (_drawOptions == DrawOptions.All || (_drawOptions & DrawOptions.Head) == DrawOptions.Head)
            {
                Mesh headMesh = _headGraphics.MeshAt(__rotation);
                Material material = _headGraphics.MatAt(__rotation);
                GenDraw.DrawMeshNowOrLater(headMesh, headLayer + headOffset, _up, material, false);
            }


            // Headwear
            Mesh hairMesh = _hairGraphics.MeshAt(__rotation);
            Vector3 hairOffset = new Vector3(PREVIEW_POSITION_X + headOffset.x, 0.030303031f, headOffset.z);
            bool isWearingHat = false;
            if (_drawOptions == DrawOptions.All || (_drawOptions & DrawOptions.Clothes) == DrawOptions.Clothes)
            {
                for (int i = 0; i < _apparelGraphics.Count; i++)
                {
                    ApparelGraphicRecord apparel = _apparelGraphics[i];
                    if (apparel.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead)
                    {
                        Material hatMat = apparel.graphic.MatAt(__rotation);
                        if (apparel.sourceApparel.def.apparel.hatRenderedFrontOfFace)
                        {
                            isWearingHat = true;
                            hairOffset.y += 0.03f;
                            GenDraw.DrawMeshNowOrLater(hairMesh, hairOffset, _up, hatMat, false);
                        }
                        else
                        {
                            Vector3 hatOffset = new Vector3(PREVIEW_POSITION_X + headOffset.x, __rotation == Rot4.North ? 0.003787879f : 0.03409091f, headOffset.z);
                            GenDraw.DrawMeshNowOrLater(hairMesh, hatOffset, _up, hatMat, false);
                        }
                    }
                }
            }

            // Hair
            if (!isWearingHat && _drawOptions == DrawOptions.All || (_drawOptions & DrawOptions.Clothes) == DrawOptions.Clothes)
            {
                // Draw hair
                Material hairMat = _hairGraphics.MatAt(__rotation);
                GenDraw.DrawMeshNowOrLater(hairMesh, hairOffset, _up, hairMat, false);
            }


            // Body clothes
            bool drawClothes = _drawOptions == DrawOptions.All || (_drawOptions & DrawOptions.Clothes) == DrawOptions.Clothes;
            if (drawClothes)
            {
                for (int i = 0; i < _apparelGraphics.Count; i++)
                {
                    ApparelGraphicRecord apparel = _apparelGraphics[i];
                    if (apparel.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Shell)
                    {
                        Material clothingMat = apparel.graphic.MatAt(__rotation);
                        GenDraw.DrawMeshNowOrLater(bodyMesh, clothesLayer, _up, clothingMat, false);
                    }
                }
            }

            if (_drawOptions == DrawOptions.All || (_drawOptions & DrawOptions.BodyAddons) == DrawOptions.BodyAddons)
            {
                // HarmonyPatches.DrawAddons(PawnRenderFlags.Clothes, clothesLayer, headOffset, _pawn, _up, __rotation);
            }


            if (drawClothes)
            {
                if (_apparel != null)
                {
                    for (int i = 0; i < _apparel.Count; i++)
                    {
                        _apparel[i].DrawWornExtras();
                    }
                }
            }
        }

        private void DrawAddons()
        {
            //string bodytype;
            //string headtype;

            //List<AlienPartGenerator.BodyAddon> bodyAddons;


            //for (int i = 0; i < bodyAddons.Count; i++)
            //{
            //    AlienPartGenerator.BodyAddon bodyAddon = bodyAddons[i];
            //    Vector3 offsetVector = (bodyAddon.defaultOffsets.GetOffset(_rotation)?.GetOffset(false, bodytype, headtype) ?? Vector3.zero) + (bodyAddon.offsets.GetOffset(_rotation)?.GetOffset(false, bodytype, headtype) ?? Vector3.zero);
            //    offsetVector.y = (bodyAddon.inFrontOfBody ? (0.3f + offsetVector.y) : (-0.3f - offsetVector.y));
            //    float angle = bodyAddon.angle;
            //    if (_rotation == Rot4.North)
            //    {
            //        if (bodyAddon.layerInvert)
            //        {
            //            offsetVector.y = 0f - offsetVector.y;
            //        }
            //        angle = 0f;
            //    }
            //    if (_rotation == Rot4.East)
            //    {
            //        angle = 0f - angle;
            //        offsetVector.x = 0f - offsetVector.x;
            //    }

            //    Graphic path = bodyAddon.GetPath( alienPartGenerator.bodyAddons[i].GetPath(pawn, ref sharedIndex, (comp.addonVariants.Count > i) ? new int?(comp.addonVariants[i]) : null);
            //    comp.addonGraphics.Add(path);

            //    Graphic graphic = comp.addonGraphics[i];
            //    graphic.drawSize = bodyAddon.drawSize * 1.5f;
            //    Material mat = graphic.MatAt(_rotation);


            //    GenDraw.DrawMeshNowOrLater(graphic.MeshAt(_rotation), vector + (bodyAddon.alignWithHead ? headOffset : Vector3.zero) + offsetVector.RotatedBy(Mathf.Acos(Quaternion.Dot(Quaternion.identity, quat)) * 2f * 57.29578f), Quaternion.AngleAxis(angle, Vector3.up) * quat, mat, renderFlags.FlagSet(PawnRenderFlags.DrawNow));
            //}
        }


        internal void InitCamera()
        {
            if (_cameraObject == null)
            {
                _cameraObject = new GameObject("PreviewCamera", new Type[]
                {
                    typeof(Camera)
                });
                _cameraObject.SetActive(false);
            }
            if (_camera == null)
            {
                _camera = _cameraObject.GetComponent<Camera>();
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
