using AlienRace;
using Pawnmorph.Hediffs;
using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace Pawnmorph.User_Interface.Preview
{
    internal class PawnPreview : Preview
    {
        Pawn _pawn;

        Vector2 _headOffset;
        DrawOptions _drawOptions;

        Graphic _bodyGraphics;
        Graphic _headGraphics;
        Graphic _hairGraphics;
        IList<ApparelGraphicRecord> _apparelGraphics;
        IList<Apparel> _apparel;


        public PawnPreview(int height, int width, Pawn pawn)
            : base(height, width)
        {
            _pawn = pawn;
            _drawOptions = DrawOptions.All;
        }

        public PawnPreview(int height, int width, ThingDef_AlienRace race)
            : base(height, width)
        {
            _drawOptions = DrawOptions.All;

            _pawn = new Pawn();
            _pawn.def = race;
            _pawn.apparel = new Pawn_ApparelTracker(_pawn);
            _pawn.health = new Pawn_HealthTracker(_pawn);
            _pawn.gender = Gender.Male;
            _pawn.story = new Pawn_StoryTracker(_pawn);
            _pawn.story.bodyType = BodyTypeDefOf.Male;
            _pawn.story.crownType = CrownType.Average;
            _pawn.story.hairDef = HairDefOf.Shaved;
            _pawn.Drawer.renderer.graphics.ResolveAllGraphics();
        }

        public override void OnRefresh()
        {
            if (_pawn == null)
                return;

            RenderPawn();
            //RenderAddons();
        }



        public void AddMutation(MutationDef mutation)
        {

        }

        private void RenderPawn()
        {
            Vector3 headOffset = _up * _pawn.Drawer.renderer.BaseHeadOffsetAt(_rotation); // Can be cached.

            _bodyGraphics = _pawn.Drawer.renderer.graphics.nakedGraphic;
            _headGraphics = _pawn.Drawer.renderer.graphics.headGraphic;
            _hairGraphics = _pawn.Drawer.renderer.graphics.hairGraphic;
            _apparelGraphics = _pawn.Drawer.renderer.graphics.apparelGraphics;
            _apparel = _pawn.apparel.WornApparel;

            // Draw body
            Vector3 bodyOffset = new Vector3(_previewOffsetX, 0.007575758f, 0f);
            Mesh bodyMesh = _bodyGraphics.MeshAt(_rotation);
            if (_drawOptions == DrawOptions.All || (_drawOptions & DrawOptions.Body) == DrawOptions.Body)
            {
                Material bodyMaterial = _bodyGraphics.MatAt(_rotation);
                GenDraw.DrawMeshNowOrLater(bodyMesh, bodyOffset, _up, bodyMaterial, false);
            }


            Vector3 clothesLayer = new Vector3(_previewOffsetX, (_rotation == Rot4.North ? 0.026515152f : 0.022727273f), 0f);
            Vector3 headLayer = new Vector3(_previewOffsetX, (_rotation == Rot4.North ? 0.022727273f : 0.026515152f), 0f);


            if (_drawOptions == DrawOptions.All || (_drawOptions & DrawOptions.Head) == DrawOptions.Head)
            {
                Mesh headMesh = _headGraphics.MeshAt(_rotation);
                Material material = _headGraphics.MatAt(_rotation);
                GenDraw.DrawMeshNowOrLater(headMesh, headLayer + headOffset, _up, material, false);
            }


            // Headwear
            Mesh hairMesh = _hairGraphics.MeshAt(_rotation);
            Vector3 hairOffset = new Vector3(_previewOffsetX + headOffset.x, 0.030303031f, headOffset.z);
            bool isWearingHat = false;
            if (_drawOptions == DrawOptions.All || (_drawOptions & DrawOptions.Clothes) == DrawOptions.Clothes)
            {
                for (int i = 0; i < _apparelGraphics.Count; i++)
                {
                    ApparelGraphicRecord apparel = _apparelGraphics[i];
                    if (apparel.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead)
                    {
                        Material hatMat = apparel.graphic.MatAt(_rotation);
                        if (apparel.sourceApparel.def.apparel.hatRenderedFrontOfFace)
                        {
                            isWearingHat = true;
                            hairOffset.y += 0.03f;
                            GenDraw.DrawMeshNowOrLater(hairMesh, hairOffset, _up, hatMat, false);
                        }
                        else
                        {
                            Vector3 hatOffset = new Vector3(_previewOffsetX + headOffset.x, _rotation == Rot4.North ? 0.003787879f : 0.03409091f, headOffset.z);
                            GenDraw.DrawMeshNowOrLater(hairMesh, hatOffset, _up, hatMat, false);
                        }
                    }
                }
            }

            // Hair
            if (!isWearingHat && _drawOptions == DrawOptions.All || (_drawOptions & DrawOptions.Clothes) == DrawOptions.Clothes)
            {
                // Draw hair
                Material hairMat = _hairGraphics.MatAt(_rotation);
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
                        Material clothingMat = apparel.graphic.MatAt(_rotation);
                        GenDraw.DrawMeshNowOrLater(bodyMesh, clothesLayer, _up, clothingMat, false);
                    }
                }
            }

            if (_drawOptions == DrawOptions.All || (_drawOptions & DrawOptions.BodyAddons) == DrawOptions.BodyAddons)
            {
                HarmonyPatches.DrawAddons(PawnRenderFlags.Clothes, clothesLayer, headOffset, _pawn, _up, _rotation);
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

        //private void RenderAddons()
        //{
        //    //string bodytype;
        //    //string headtype;

        //    //List<AlienPartGenerator.BodyAddon> bodyAddons;


        //    //for (int i = 0; i < bodyAddons.Count; i++)
        //    //{
        //    //    AlienPartGenerator.BodyAddon bodyAddon = bodyAddons[i];
        //    //    Vector3 offsetVector = (bodyAddon.defaultOffsets.GetOffset(_rotation)?.GetOffset(false, bodytype, headtype) ?? Vector3.zero) + (bodyAddon.offsets.GetOffset(_rotation)?.GetOffset(false, bodytype, headtype) ?? Vector3.zero);
        //    //    offsetVector.y = (bodyAddon.inFrontOfBody ? (0.3f + offsetVector.y) : (-0.3f - offsetVector.y));
        //    //    float angle = bodyAddon.angle;
        //    //    if (_rotation == Rot4.North)
        //    //    {
        //    //        if (bodyAddon.layerInvert)
        //    //        {
        //    //            offsetVector.y = 0f - offsetVector.y;
        //    //        }
        //    //        angle = 0f;
        //    //    }
        //    //    if (_rotation == Rot4.East)
        //    //    {
        //    //        angle = 0f - angle;
        //    //        offsetVector.x = 0f - offsetVector.x;
        //    //    }

        //    //    Graphic path = bodyAddon.GetPath( alienPartGenerator.bodyAddons[i].GetPath(pawn, ref sharedIndex, (comp.addonVariants.Count > i) ? new int?(comp.addonVariants[i]) : null);
        //    //    comp.addonGraphics.Add(path);

        //    //    Graphic graphic = comp.addonGraphics[i];
        //    //    graphic.drawSize = bodyAddon.drawSize * 1.5f;
        //    //    Material mat = graphic.MatAt(_rotation);


        //    //    GenDraw.DrawMeshNowOrLater(graphic.MeshAt(_rotation), vector + (bodyAddon.alignWithHead ? headOffset : Vector3.zero) + offsetVector.RotatedBy(Mathf.Acos(Quaternion.Dot(Quaternion.identity, quat)) * 2f * 57.29578f), Quaternion.AngleAxis(angle, Vector3.up) * quat, mat, renderFlags.FlagSet(PawnRenderFlags.DrawNow));
        //    //}
        //}








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
