using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Verse;

namespace Pawnmorph.Hediffs
{
    public class MutationStagePatch
    {
        string identity;
        string function = "modify";
        MutationStage values = null;

        public void Apply(MutationDef mutation)
        {
            if (values == null)
                return;

            switch (function)
            {
                case "add":
                    Add(mutation);
                    break;

                case "modify":
                    Modify(mutation);
                    break;

                case "remove":
                    Remove(mutation);
                    break;
            }

        }

        private void Add(MutationDef mutation)
        {
            mutation.stages.Add(values);
        }

        private void Remove(MutationDef mutation)
        {
            MutationStage stage = mutation.stages.FirstOrDefault(x => (x.overrideLabel ?? x.label) == identity) as MutationStage;
            if (stage != null)
                mutation.stages.Remove(stage);
        }



        private void Modify(MutationDef mutation)
        {
            MutationStage stage = mutation.stages.FirstOrDefault(x => (x.overrideLabel ?? x.label) == identity) as MutationStage;
            if (stage != null)
            {
                if (values.graphics != null)
                    stage.graphics = values.graphics;

                if (values.capMods != null)
                    stage.capMods = values.capMods;

                if (values.description != null)
                    stage.description = values.description;

                if (values.minSeverity > 0)
                    stage.minSeverity = values.minSeverity;


                if (values.healthOffset > 0)
                    stage.healthOffset = values.healthOffset;

                if (values.labelOverride != null)
                    stage.labelOverride = values.labelOverride;

                if (values.stopChance > 0)
                    stage.stopChance = values.stopChance;
            }
        }
    }
}
