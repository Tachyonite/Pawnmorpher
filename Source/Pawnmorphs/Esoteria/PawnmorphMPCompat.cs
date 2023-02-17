using System.Reflection;
using HarmonyLib;
using Multiplayer.API;
using Pawnmorph;
using Pawnmorph.Hediffs;
using Verse;

namespace PawnMorpher
{
    [StaticConstructorOnStartup]
    static class PawnmorphMPCompat
    {
        static int lastTick;
        static int lastSeed;

        static PawnmorphMPCompat()
        {
            if (!MP.enabled) return;

            PatchRand();
            SyncMethods();

            Log.Message("Pawnmorpher :: Multiplayer Compatibility enabled");
        }

        static void PatchRand()
        {
            var rngMethods = new MethodBase[] {
                // Motes
                AccessTools.Method(typeof(IntermittentMagicSprayer), nameof(IntermittentMagicSprayer.ThrowMagicPuffUp)),
                AccessTools.Method(typeof(IntermittentMagicSprayer), nameof(IntermittentMagicSprayer.ThrowMagicPuffDown)),

                // Fixes a very rare desync called from AI, I can't find the logic here, possibly a core MP issue.
                AccessTools.Method(typeof(SpreadingMutationComp), nameof(SpreadingMutationComp.CompPostTick)),
            };

            var harmony = new Harmony("com.pawnmorpher.mpcompat");

            foreach(var method in rngMethods) {
                harmony.Patch(method,
                    prefix: new HarmonyMethod(typeof(PawnmorphMPCompat), nameof(PushState)),
                    postfix: new HarmonyMethod(typeof(PawnmorphMPCompat), nameof(PopState))
                );
            }
        }

        static void SyncMethods()
        {
#pragma warning disable 612
            var syncMethods = new MethodInfo[] {
                // Gizmos
#pragma warning restore 612
            };

            foreach(var method in syncMethods) {
                MP.RegisterSyncMethod(method);
            }
        }

        static void PushState()
        {
            if (MP.IsInMultiplayer) {
                Rand.PushState(MPSafeSeed);
            }
        }
        static void PopState()
        {
            if (MP.IsInMultiplayer) {
                Rand.PopState();
            }
        }

        static int MPSafeSeed
        {
            get
            {
                var ticks = Find.TickManager.TicksAbs;
                if (ticks != lastTick)
                {
                    lastTick = ticks;
                    lastSeed = ticks;
                    return lastTick;
                }

                lastSeed = ZorShift(lastSeed);
                return lastSeed;
            }
        }

        static int ZorShift(int val)
        {
            uint uVal = unchecked((uint) val);
            uVal ^= uVal << 13;
            uVal ^= uVal >> 17;
            uVal ^= uVal << 5;
            return unchecked((int) uVal);
        }
    }
}
