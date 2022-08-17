using AlienRace;
using Pawnmorph.Hediffs;
using Pawnmorph.Hediffs.MutationRetrievers;
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

        DrawOptions _drawOptions;

        Graphic _bodyGraphics;
        Graphic _headGraphics;
        Graphic _hairGraphics;
        IList<ApparelGraphicRecord> _apparelGraphics;
        IList<Apparel> _apparel;


        //public PawnPreview(int height, int width, Pawn pawn)
        //    : base(height, width)
        //{
        //    _pawn = pawn;
        //    _drawOptions = DrawOptions.All;
        //}

        public PawnPreview(int height, int width, ThingDef_AlienRace race)
            : base(height, width)
        {
            _drawOptions = DrawOptions.All;

            _pawn = new Pawn();
            _pawn.def = race;
            _pawn.gender = Gender.Male;
            _pawn.kindDef = PawnKindDefOf.Colonist;
            //_pawn.apparel = new Pawn_ApparelTracker(_pawn);
            //_pawn.health = new Pawn_HealthTracker(_pawn);
            //_pawn.story = new Pawn_StoryTracker(_pawn);
            PawnComponentsUtility.CreateInitialComponents(_pawn);


            _pawn.story.bodyType = BodyTypeDefOf.Male;
            _pawn.story.crownType = CrownType.Average;
            _pawn.story.hairDef = HairDefOf.Shaved;
            _pawn.PostMake();

            _pawn.GetComp<AlienPartGenerator.AlienComp>().OverwriteColorChannel("skin", Color.white, Color.white);
            _pawn.Drawer.renderer.graphics.ResolveAllGraphics();
        }

        public override void OnRefresh()
        {
            if (_pawn == null)
                return;

            _pawn.Drawer.renderer.graphics.ResolveAllGraphics();
            RenderPawn();
            //RenderAddons();
        }

        public void AddMutation(MutationDef hediff, BodyPartDef bodyPart)
        {
            IEnumerable<BodyPartRecord> parts = _pawn.RaceProps.body.GetPartsWithDef(bodyPart);

            foreach (BodyPartRecord part in parts)
            {
                Hediff mutation = _pawn.health.AddHediff(hediff, part);
                
                //Hediff mutation = HediffMaker.MakeHediff(hediff, _pawn, part) as Hediff_AddedMutation;
                //_pawn.health.hediffSet.AddDirect(mutation);
                mutation.Severity = hediff.maxSeverity;
                mutation.Tick();
            }
            //_pawn.Tick();
        }

        public void AddMutation(MutationDef hediff)
        {
            foreach (BodyPartDef part in hediff.parts)
                AddMutation(hediff, part);
        }

        public void ClearMutations()
        {
            _pawn.health.RemoveAllHediffs();
        }

        private void RenderPawn()
        {
            _pawn.Rotation = _rotation;
            _pawn.DrawAt(new UnityEngine.Vector3(_previewOffsetX, 0, 0));

            //Vector3 headOffset = _up * _pawn.Drawer.renderer.BaseHeadOffsetAt(_rotation); // Can be cached.

            //_bodyGraphics = _pawn.Drawer.renderer.graphics.nakedGraphic;
            //_headGraphics = _pawn.Drawer.renderer.graphics.headGraphic;
            //_hairGraphics = _pawn.Drawer.renderer.graphics.hairGraphic;
            //_apparelGraphics = _pawn.Drawer.renderer.graphics.apparelGraphics;
            //_apparel = _pawn.apparel.WornApparel;

            //// Draw body
            //Vector3 bodyOffset = new Vector3(_previewOffsetX, 0.007575758f, 0f);
            //Mesh bodyMesh = _bodyGraphics.MeshAt(_rotation);
            //if (_drawOptions == DrawOptions.All || (_drawOptions & DrawOptions.Body) == DrawOptions.Body)
            //{
            //    Material bodyMaterial = _bodyGraphics.MatAt(_rotation);
            //    GenDraw.DrawMeshNowOrLater(bodyMesh, bodyOffset, _up, bodyMaterial, false);
            //}


            //Vector3 clothesLayer = new Vector3(_previewOffsetX, (_rotation == Rot4.North ? 0.026515152f : 0.022727273f), 0f);
            //Vector3 headLayer = new Vector3(_previewOffsetX, (_rotation == Rot4.North ? 0.022727273f : 0.026515152f), 0f);


            //if (_drawOptions == DrawOptions.All || (_drawOptions & DrawOptions.Head) == DrawOptions.Head)
            //{
            //    Mesh headMesh = _headGraphics.MeshAt(_rotation);
            //    Material material = _headGraphics.MatAt(_rotation);
            //    GenDraw.DrawMeshNowOrLater(headMesh, headLayer + headOffset, _up, material, false);
            //}


            //// Headwear
            //Mesh hairMesh = _hairGraphics.MeshAt(_rotation);
            //Vector3 hairOffset = new Vector3(_previewOffsetX + headOffset.x, 0.030303031f, headOffset.z);
            //bool isWearingHat = false;
            //if (_drawOptions == DrawOptions.All || (_drawOptions & DrawOptions.Clothes) == DrawOptions.Clothes)
            //{
            //    for (int i = 0; i < _apparelGraphics.Count; i++)
            //    {
            //        ApparelGraphicRecord apparel = _apparelGraphics[i];
            //        if (apparel.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Overhead)
            //        {
            //            Material hatMat = apparel.graphic.MatAt(_rotation);
            //            if (apparel.sourceApparel.def.apparel.hatRenderedFrontOfFace)
            //            {
            //                isWearingHat = true;
            //                hairOffset.y += 0.03f;
            //                GenDraw.DrawMeshNowOrLater(hairMesh, hairOffset, _up, hatMat, false);
            //            }
            //            else
            //            {
            //                Vector3 hatOffset = new Vector3(_previewOffsetX + headOffset.x, _rotation == Rot4.North ? 0.003787879f : 0.03409091f, headOffset.z);
            //                GenDraw.DrawMeshNowOrLater(hairMesh, hatOffset, _up, hatMat, false);
            //            }
            //        }
            //    }
            //}

            //// Hair
            //if (!isWearingHat && _drawOptions == DrawOptions.All || (_drawOptions & DrawOptions.Clothes) == DrawOptions.Clothes)
            //{
            //    // Draw hair
            //    Material hairMat = _hairGraphics.MatAt(_rotation);
            //    GenDraw.DrawMeshNowOrLater(hairMesh, hairOffset, _up, hairMat, false);
            //}


            //// Body clothes
            //bool drawClothes = _drawOptions == DrawOptions.All || (_drawOptions & DrawOptions.Clothes) == DrawOptions.Clothes;
            //if (drawClothes)
            //{
            //    for (int i = 0; i < _apparelGraphics.Count; i++)
            //    {
            //        ApparelGraphicRecord apparel = _apparelGraphics[i];
            //        if (apparel.sourceApparel.def.apparel.LastLayer == ApparelLayerDefOf.Shell)
            //        {
            //            Material clothingMat = apparel.graphic.MatAt(_rotation);
            //            GenDraw.DrawMeshNowOrLater(bodyMesh, clothesLayer, _up, clothingMat, false);
            //        }
            //    }
            //}

            //if (_drawOptions == DrawOptions.All || (_drawOptions & DrawOptions.BodyAddons) == DrawOptions.BodyAddons)
            //{
            //    HarmonyPatches.DrawAddons(PawnRenderFlags.Clothes, clothesLayer, headOffset, _pawn, _up, _rotation);
            //}


            //if (drawClothes)
            //{
            //    if (_apparel != null)
            //    {
            //        for (int i = 0; i < _apparel.Count; i++)
            //        {
            //            _apparel[i].DrawWornExtras();
            //        }
            //    }
            //}
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





    }
}
