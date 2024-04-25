// InitializationException.cs modified by Iron Wolf for Pawnmorph on 01/20/2020 1:35 PM
// last updated 01/20/2020  1:35 PM

using System;
using JetBrains.Annotations;

namespace Pawnmorph
{
	/// <summary>
	/// exception for when something goes very wrong during mod initialization 
	/// </summary>
	/// <seealso cref="System.Exception" />
	[Serializable] //Resharper wants this for some reason 
	public class ModInitializationException : System.Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ModInitializationException"/> class.
		/// </summary>
		/// <param name="message">The message that describes the error.</param>
		public ModInitializationException(string message) : base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ModInitializationException"/> class.
		/// </summary>
		/// <param name="message">The error message that explains the reason for the exception.</param>
		/// <param name="innerException">The exception that is the cause of the current exception, or a null reference (Nothing in Visual Basic) if no inner exception is specified.</param>
		public ModInitializationException(string message, System.Exception innerException) : base(message, innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ModInitializationException"/> class.
		/// </summary>
		/// <param name="serializationInfo">The serialization information.</param>
		/// <param name="streamingContext">The streaming context.</param>
		protected ModInitializationException([NotNull] System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext) :
			base(serializationInfo, streamingContext)
		{

		}
	}
}