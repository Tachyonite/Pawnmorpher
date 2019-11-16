// InitialGraphics.cs modified by Iron Wolf for Pawnmorph on 09/10/2019 6:43 PM
// last updated 09/10/2019  6:43 PM

using System.Text;
using AlienRace;
using RimWorld;
using UnityEngine;
using Verse;
using static Pawnmorph.DebugUtils.DebugLogUtils;

namespace Pawnmorph.GraphicSys
{
    /// <summary>
    /// comp for storing the initial graphics settings of a pawn for use latter
    /// </summary>
    /// <seealso cref="Verse.ThingComp" />
    public class InitialGraphicsComp : ThingComp
    {
        private bool _scanned;

        private Vector2 _customDrawSize = Vector2.one;
        private Vector2 _customPortraitDrawSize = Vector2.one;
        private bool _fixedGenderPostSpawn;
        private Color _skinColor;
        private Color _skinColorSecond;
        private Color _hairColorSecond;
        private Color _hairColor;
        private string _crownType;

        /// <summary>Gets the draw size.</summary>
        /// <value>The size of the custom draw.</value>
        public Vector2 CustomDrawSize
        {
            get
            {
                if (!_scanned)
                    ScanGraphics();
                return _customDrawSize;
            }
        }

        /// <summary>Gets the draw size of the custom portrait </summary>
        /// <value>The size of the custom portrait draw.</value>
        public Vector2 CustomPortraitDrawSize
        {
            get
            {
                if (!_scanned)
                    ScanGraphics();
                return _customPortraitDrawSize;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [fix gender post spawn].
        /// </summary>
        /// <value><c>true</c> if [fix gender post spawn]; otherwise, <c>false</c>.</value>
        public bool FixGenderPostSpawn
        {
            get
            {
                if (!_scanned)
                    ScanGraphics();
                return _fixedGenderPostSpawn;
            }
        }

        /// <summary>Gets the color of the skin.</summary>
        /// <value>The color of the skin.</value>
        public Color SkinColor
        {
            get
            {
                if (!_scanned) ScanGraphics();
                return _skinColor;
            }
        }

        /// <summary>Gets the color of the hair.</summary>
        /// <value>The color of the hair.</value>
        public Color HairColor
        {
            get
            {
                if (!_scanned) ScanGraphics();
                if (_hairColor == default)
                    _hairColor = Pawn.story.hairColor; //fix for hair color not being saved in previous saves 

                return _hairColor;
            }
        }
        /// <summary>Gets the skin color second.</summary>
        /// <value>The skin color second.</value>
        public Color SkinColorSecond
        {
            get
            {
                if (!_scanned)
                    ScanGraphics();
                return _skinColorSecond;
            }
        }

        /// <summary>Gets the hair color second.</summary>
        /// <value>The hair color second.</value>
        public Color HairColorSecond
        {
            get
            {
                if (!_scanned) ScanGraphics();
                return _hairColorSecond;
            }
        }

        /// <summary>Gets the type of the crown.</summary>
        /// <value>The type of the crown.</value>
        public string CrownType
        {
            get
            {
                if (!_scanned) ScanGraphics();

                return _crownType;
            }
        }

        private Pawn Pawn => (Pawn) parent;

        /// <summary>Gets the debug string.</summary>
        /// <returns></returns>
        public string GetDebugStr()
        {
            var builder = new StringBuilder();

            builder.AppendLine($"{nameof(SkinColor)} {SkinColor}");
            builder.AppendLine($"{nameof(HairColor)} {HairColor}");
            builder.AppendLine($"{nameof(CrownType)} {CrownType}");
            return builder.ToString();
        }

        /// <summary>Initializes this instance with the specified properties.</summary>
        /// <param name="props">The properties.</param>
        public override void Initialize(CompProperties props)
        {
            base.Initialize(props);
            Assert(parent is Pawn, "parent is Pawn");
        }


        /// <summary>expose data.</summary>
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref _customDrawSize, "customDrawSize");
            Scribe_Values.Look(ref _customPortraitDrawSize, "customPortraitDrawSize");
            Scribe_Values.Look(ref _fixedGenderPostSpawn, nameof(FixGenderPostSpawn));
            Scribe_Values.Look(ref _skinColor, "skinColor");
            Scribe_Values.Look(ref _skinColorSecond, "skinColorSecond");
            Scribe_Values.Look(ref _hairColorSecond, "hairColorSecond");
            Scribe_Values.Look(ref _crownType, "crownType");
            Scribe_Values.Look(ref _hairColor, nameof(HairColor));
            Scribe_Values.Look(ref _scanned, nameof(_scanned));


            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                if (_skinColor == Color.clear)
                {
                    _skinColor = PawnSkinColors.GetSkinColor(Pawn.story.melanin); 
                }
            }
        }

        /// <summary>called after the pawn is spawned</summary>
        /// <param name="respawningAfterLoad">if set to <c>true</c> [respawning after load].</param>
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!_scanned) ScanGraphics();
        }

        /// <summary>
        ///     Restores the alien Comp attached to the parent from the ones stored earlier
        ///     this does not resolve the graphics, that is the job of the caller
        /// </summary>
        public void RestoreGraphics()
        {
            Assert(_scanned, "_scanned");

            var comp = parent.GetComp<AlienPartGenerator.AlienComp>();

            comp.skinColor = SkinColor;
            comp.customDrawSize = CustomDrawSize;
            comp.customPortraitDrawSize = CustomPortraitDrawSize;
            comp.fixGenderPostSpawn = FixGenderPostSpawn;
            comp.skinColorSecond = SkinColorSecond;
            comp.hairColorSecond = HairColorSecond;
            comp.crownType = CrownType;
            comp.hairColorSecond = HairColorSecond;
            ((Pawn) parent).story.hairColor = HairColor;
        }

        private void ScanGraphics()
        {
            _scanned = true;
            var comp = parent.GetComp<AlienPartGenerator.AlienComp>();
            if (comp == null) return;

            _customDrawSize = comp.customDrawSize;
            _customPortraitDrawSize = comp.customPortraitDrawSize;
            _fixedGenderPostSpawn = comp.fixGenderPostSpawn;
            _skinColor = comp.skinColor;
            _skinColorSecond = comp.skinColorSecond;
            _hairColorSecond = comp.hairColorSecond;
            _crownType = comp.crownType;
            _hairColorSecond = Pawn.story.hairColor;
        }
    }
}