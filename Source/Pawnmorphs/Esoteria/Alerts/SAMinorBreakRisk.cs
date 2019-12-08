// SAMinorBreakRisk.cs modified by Iron Wolf for Pawnmorph on 12/08/2019 8:47 AM
// last updated 12/08/2019  8:47 AM

using System.Linq;
using RimWorld;

namespace Pawnmorph.Alerts
{
    /// <summary>
    ///     alert for sapient animals that are at risk of a minor mental break
    /// </summary>
    /// <seealso cref="RimWorld.Alert" />
    /// note, rimworld creates instances of this via reflection
    public class SAMinorBreakRisk : Alert
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SAMinorBreakRisk"/> class.
        /// </summary>
        public SAMinorBreakRisk()
        {
            defaultPriority = AlertPriority.High;
        }

        /// <summary>
        ///     Gets the label.
        /// </summary>
        /// <returns></returns>
        public override string GetLabel()
        {
            return FormerHumanUtilities.BreakAlertLabel;
        }

        /// <summary>
        /// Gets the explanation.
        /// </summary>
        /// <returns></returns>
        public override string GetExplanation()
        {
            return FormerHumanUtilities.BreakAlertExplanation; 
        }

        /// <summary>
        ///     Gets the report.
        /// </summary>
        /// <returns></returns>
        public override AlertReport GetReport()
        {
            if (FormerHumanUtilities.AllSapientAnimalsExtremeBreakRisk.Any()
             || FormerHumanUtilities.AllSapientAnimalsMajorBreakRisk.Any())
                return false;
            return AlertReport.CulpritsAre(FormerHumanUtilities.AllSapientAnimalsMinorBreakRisk);
        }
    }

    /// <summary>
    ///     alert for sapient animals that are at risk of a major or extreme break
    /// </summary>
    /// <seealso cref="RimWorld.Alert" />
    public class SAMajorOrExtremeBreakRisk : Alert_Critical
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SAMajorOrExtremeBreakRisk"/> class.
        /// </summary>
        public SAMajorOrExtremeBreakRisk()
        {
        }
        /// <summary>
        /// Gets the explanation.
        /// </summary>
        /// <returns></returns>
        public override string GetExplanation()
        {
            return FormerHumanUtilities.BreakAlertExplanation;
        }


        /// <summary>
        ///     Gets the label.
        /// </summary>
        /// <returns></returns>
        public override string GetLabel()
        {
            return FormerHumanUtilities.BreakAlertLabel;
        }

        /// <summary>
        ///     Gets the report.
        /// </summary>
        /// <returns></returns>
        public override AlertReport GetReport()
        {
            return AlertReport.CulpritsAre(FormerHumanUtilities.AllSapientAnimalsMajorBreakRisk.Concat(FormerHumanUtilities
                                                                                                          .AllSapientAnimalsExtremeBreakRisk));
        }
    }
}