// FillabeBarDrawerProps.cs created by Iron Wolf for Pawnmorph on 10/26/2020 4:14 PM
// last updated 10/26/2020  4:14 PM

using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace Pawnmorph.ThingComps
{
    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Verse.CompProperties" />
    public class FillableBarDrawerProps : CompProperties
    {

        /// <summary>
        /// The bar size
        /// </summary>
        public Vector2 barSize;
        /// <summary>
        /// The bar offset
        /// </summary>
        public Vector3 barOffset;
        /// <summary>
        /// The bar color
        /// </summary>
        public Color barColor;
        /// <summary>
        /// The period
        /// </summary>
        public float period; 


        /// <summary>
        /// Initializes a new instance of the <see cref="FillableBarDrawerProps"/> class.
        /// </summary>
        public FillableBarDrawerProps()
        {
            compClass = typeof(FillableBarDrawer);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <seealso cref="Verse.ThingComp" />
    public class FillableBarDrawer : ThingComp
    {
        private bool _filling;
        private float _counter;
        
        [NotNull]
        private FillableBarDrawerProps Props => (FillableBarDrawerProps) props;


        private bool _full;



        private float PercentFilled => _counter / Props.period.SecondsToTicks(); 

        /// <summary>
        /// Triggers this instance to fill the bar.
        /// </summary>
        public void Trigger()
        {
            _full = false;
            _counter = 0;
            _filling = true; 

        }


        /// <summary>
        /// Posts the draw.
        /// </summary>
        public void PreDraw()
        {
            if (!_filling && !_full) return;
            float fillP = _full ? 1 : PercentFilled;

            GenDraw.FillableBarRequest req = new GenDraw.FillableBarRequest()
            {
                center = parent.DrawPos + Props.barOffset,
                size = Props.barSize,
                fillPercent = fillP,
                rotation = parent.Rotation.Rotated(RotationDirection.Clockwise),
                filledMat = SolidColorMaterials.SimpleSolidColorMaterial(Props.barColor),
                unfilledMat = SolidColorMaterials.SimpleSolidColorMaterial(Color.clear)
            };
            GenDraw.DrawFillableBar(req);
        }


        public override void CompTick()
        {
            base.CompTick();

            if (_filling)
            {
                _counter += 1;
                if (_counter >= Props.period.SecondsToTicks())
                {
                    _full = true; 
                }
            }

        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            _full = false;
            _filling = false;
            _counter = 0; 
        }


        /// <summary>
        /// serializes/deserializes data 
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref _filling, "filling");
            Scribe_Values.Look(ref _full, "full");
            Scribe_Values.Look(ref _counter, "counter"); 
        }
    }
}