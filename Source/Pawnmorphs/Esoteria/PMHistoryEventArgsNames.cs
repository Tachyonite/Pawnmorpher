﻿// PMHistoryEventArgsNames.cs created by Iron Wolf for Pawnmorph on 07/21/2021 4:42 PM
// last updated 07/21/2021  4:42 PM


namespace Pawnmorph
{
    /// <summary>
    /// static class containing pawnmorpher specific history event argument names 
    /// </summary>
    public static class PMHistoryEventArgsNames
    {
        /// <summary>
        /// the argument name for a mutation 
        /// </summary>
        public const string MUTATION = "Mutation";


        /// <summary>
        /// the argument for the old race of a pawn 
        /// </summary>
        public const string OLD_RACE = "OldRace";

        /// <summary>
        /// the argument for a new race 
        /// </summary>
        public const string NEW_RACE = "NewRace";


        /// <summary>
        /// label for a morph def
        /// </summary>
        public const string MORPH = "Morph"; 

        /// <summary>
        /// label for an animal pawnkindDef 
        /// </summary>
        public const string ANIMAL = "Animal"; 

        /// <summary>
        /// label for the old sapience level
        /// </summary>
        public const string OLD_SAPIENCE_LEVEL = "OldSapienceLevel";

        /// <summary>
        /// label for the new sapience level
        /// </summary>
        public const string NEW_SAPIENCE_LEVEL = "NewSapienceLevel";

        /// <summary>
        /// label for the faction responsible for some event 
        /// </summary>
        public const string FACTION_RESPONSIBLE = "FactionResponsible"; 
    }
}