using Verse;

namespace Pawnmorph
{
	/// <summary>
	/// comp property for the hediff comp add severity 
	/// </summary>
	public class HediffCompProperties_AddSeverity : HediffCompProperties
	{
		/// <summary>
		/// the hediff 
		/// </summary>
		public HediffDef hediff = null;
		/// <summary>
		/// the amount of severity to add 
		/// </summary>
		public float severity = 0;
		/// <summary>
		/// how often to add the severity 
		/// </summary>
		public float mtbDays = 0;

		/// <summary>
		/// create a new instance of this type 
		/// </summary>
		public HediffCompProperties_AddSeverity()
		{
			compClass = typeof(HediffComp_AddSeverity);
		}
	}
}
