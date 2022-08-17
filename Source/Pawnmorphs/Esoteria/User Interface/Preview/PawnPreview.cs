using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph.User_Interface.Preview
{
    internal class PawnPreview : PreviewBase
    {

        Pawn _pawn;
        bool _renderClothes;

        public bool RenderClothes
        {
            get => _renderClothes;
            set => _renderClothes = value;
        }


        public PawnPreview(int height, int width, Pawn pawn)
            : base(height, width)
        {
            _renderClothes = true;
            _pawn = pawn;
        }

        public override void OnRefresh()
        {
            
        }

        /*
         * _pawn.RaceProps.Humanlike ? MeshPool.humanlikeBodySet.MeshAt(_rotation) : graphics.nakedGraphic.MeshAt(_rotation)
         * 
         * 
            
            ////_race
            //PawnGraphicSet graphics = _pawn.Drawer.renderer.graphics;
            //graphics.ResolveAllGraphics();

            //Quaternion quaternion = Quaternion.AngleAxis(0f, Vector3.up);

            //Mesh bodyMesh = _pawn.RaceProps.Humanlike ? MeshPool.humanlikeBodySet.MeshAt(_rotation) : graphics.nakedGraphic.MeshAt(_rotation);
            //Vector3 bodyOffset = new Vector3(PREVIEW_POSITION_X, _pawn.Position.y + 0.007575758f, 0f);

            //foreach (Material mat in graphics.MatsBodyBaseAt(_rotation))
            //{
            //    Material damagedMat = graphics.flasher.GetDamagedMat(mat);
            //    GenDraw.DrawMeshNowOrLater(bodyMesh, bodyOffset, quaternion, damagedMat, false);
            //    bodyOffset.y += 0.00390625f;
            //    if (!_renderClothes)
            //    {
            //        break;
            //    }
            //}
            //Vector3 vector3 = new Vector3(PREVIEW_POSITION_X, _pawn.Position.y + (_rotation == Rot4.North ? 0.026515152f : 0.022727273f), 0f);
            //Vector3 vector4 = new Vector3(PREVIEW_POSITION_X, _pawn.Position.y + (_rotation == Rot4.North ? 0.022727273f : 0.026515152f), 0f);
            //if (graphics.headGraphic != null)
            //{
            //    Mesh mesh2 = MeshPool.humanlikeHeadSet.MeshAt(_rotation);
            //    Vector3 headOffset = quaternion * _pawn.Drawer.renderer.BaseHeadOffsetAt(_rotation);
            //    Material material = graphics.HeadMatAt(_rotation);

            //    GenDraw.DrawMeshNowOrLater(mesh2, vector4 + headOffset, quaternion, material, false);

            //    Mesh hairMesh = graphics.HairMeshSet.MeshAt(_rotation);
            //    Vector3 hairOffset = new Vector3(PREVIEW_POSITION_X + headOffset.x, _pawn.Position.y + 0.030303031f, headOffset.z);
            //    bool isWearingHat = false;
            //    if (_renderClothes)
            //    {
            //        foreach (ApparelGraphicRecord apparel in graphics.apparelGraphics)
            //        {
            //            if (apparel.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead)
            //            {
            //                Material hatMat = graphics.flasher.GetDamagedMat(apparel.graphic.MatAt(_rotation));
            //                if (apparel.sourceApparel.def.apparel.hatRenderedFrontOfFace)
            //                {
            //                    isWearingHat = true;
            //                    hairOffset.y += 0.03f;
            //                    GenDraw.DrawMeshNowOrLater(hairMesh, hairOffset, quaternion, hatMat, false);
            //                }
            //                else
            //                {
            //                    Vector3 hatOffset = new Vector3(PREVIEW_POSITION_X + headOffset.x, _pawn.Position.y + (_rotation == Rot4.North ? 0.003787879f : 0.03409091f), headOffset.z);
            //                    GenDraw.DrawMeshNowOrLater(hairMesh, hatOffset, quaternion, hatMat, false);
            //                }
            //            }
            //        }
            //    }
            //    if (!isWearingHat)
            //    {
            //        if (addedMutations.Parts.Any(x => x.IsInGroup(BodyPartGroupDefOf.UpperHead)) == false)
            //        {
            //            // Draw hair
            //            Material hairMat = graphics.HairMatAt(_rotation);
            //            GenDraw.DrawMeshNowOrLater(hairMesh, hairOffset, quaternion, hairMat, false);
            //        }
            //    }
            //}
            //if (_renderClothes)
            //{
            //    foreach (ApparelGraphicRecord graphicsSet in graphics.apparelGraphics)
            //    {
            //        if (graphicsSet.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Shell)
            //        {
            //            Material clothingMat = graphics.flasher.GetDamagedMat(graphicsSet.graphic.MatAt(_rotation));
            //            GenDraw.DrawMeshNowOrLater(bodyMesh, vector3, quaternion, clothingMat, false);
            //        }
            //    }
            //}

            //Vector3 hOffset = quaternion * _pawn.Drawer.renderer.BaseHeadOffsetAt(_rotation);
            //HarmonyPatches.DrawAddons(PawnRenderFlags.Clothes, vector3, hOffset, _pawn, quaternion, _rotation);
            //if (_renderClothes)
            //{
            //    if (_pawn.apparel != null)
            //    {
            //        foreach (Apparel apparel in _pawn.apparel.WornApparel)
            //        {
            //            apparel.DrawWornExtras();
            //        }
            //    }
            //}
         * 
         * 
         */
    }
}
