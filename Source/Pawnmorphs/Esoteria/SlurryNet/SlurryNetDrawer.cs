// SlurryNetDrawer.cs created by Iron Wolf for Pawnmorph on 11/27/2020 9:56 AM
// last updated 11/27/2020  9:56 AM

using System;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace Pawnmorph.SlurryNet
{
    /// <summary>
    /// </summary>
    /// <seealso cref="Pawnmorph.SlurryNet.SlurryNetComp" />
    /// <seealso cref="Pawnmorph.SlurryNet.ISlurryNetTrader" />
    public class SlurryNetDrawer : SlurryNetComp, ISlurryNetTrader
    {
        /// <summary>
        ///     delegate for the SlurryDrawnFromNetEvent
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="amount">The amount.</param>
        public delegate void SlurryDrawnFromNetHandle([NotNull] SlurryNetDrawer sender, float amount);



        /// <summary>
        /// The draw speed multiplier
        /// </summary>
        public float drawSpeedMultiplier = 1.0f; 

        /// <summary>
        ///     Occurs when slurry is drawn from the net.
        /// </summary>
        public event SlurryDrawnFromNetHandle SlurryDrawnFromNet;

        /// <summary>
        ///     if this comp is enabled or not
        /// </summary>
        public bool enabled = true;

        private SlurryNetDrawerProps _props;

        /// <summary>
        ///     Gets the parent.
        /// </summary>
        /// <value>
        ///     The parent.
        /// </value>
        Thing ISlurryNetTrader.Parent => parent;

        /// <summary>
        ///     Gets the slurry used in units of slurry per day
        /// </summary>
        /// gets the amount of slurry used by this trader. positive values are used while negative values are production
        /// <value>
        ///     The slurry used.
        /// </value>
        public float SlurryUsed => enabled ? Mathf.Max(Props.slurryDrawPerDay * drawSpeedMultiplier, 0) : 0;

        /// <summary>
        ///     Tries to receive some amount of slurry.
        /// </summary>
        /// <param name="slurryReceived">The slurry received.</param>
        /// <returns>the amount of slurry used</returns>
        float ISlurryNetTrader.TryReceiveSlurry(float slurryReceived)
        {
            if (!enabled) return 0;
            SlurryDrawnFromNet?.Invoke(this, slurryReceived);
            return slurryReceived;
        }

        /// <summary>
        ///     Gets or sets the network.
        /// </summary>
        /// <value>
        ///     The network.
        /// </value>
        public override SlurryNet Network { get; set; }

        /// <summary>
        ///     Gets a value indicating whether this instance can transmit slurry.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance transmits slurry; otherwise, <c>false</c>.
        /// </value>
        public override bool TransmitsNow => true;


        [NotNull]
        private SlurryNetDrawerProps Props
        {
            get
            {
                if (_props == null)
                    try
                    {
                        _props = (SlurryNetDrawerProps) props;
                        if (_props == null)
                            Log.Error($"unable to find props for {nameof(SlurryNetDrawer)} comp on {parent.Label}");
                    }
                    catch (InvalidCastException e)
                    {
                        Log.Error($"unable to cast props {props?.GetType().Name ?? "[NULL]"} to {nameof(SlurryNetDrawerProps)}!\n{e}");
                        throw;
                    }

                return _props;
            }
        }

        /// <summary>
        ///     Posts the expose data.
        /// </summary>
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref enabled, nameof(enabled), true);
            Scribe_Values.Look(ref drawSpeedMultiplier, nameof(drawSpeedMultiplier),1); 
        }
    }

    /// <summary>
    /// </summary>
    /// <seealso cref="Verse.CompProperties" />
    public class SlurryNetDrawerProps : CompProperties
    {
        /// <summary>
        ///     the amount of slurry to draw per day from the net
        /// </summary>
        public float slurryDrawPerDay;

        /// <summary>
        ///     Initializes a new instance of the <see cref="SlurryNetDrawerProps" /> class.
        /// </summary>
        public SlurryNetDrawerProps()
        {
            compClass = typeof(SlurryNetDrawer);
        }
    }
}