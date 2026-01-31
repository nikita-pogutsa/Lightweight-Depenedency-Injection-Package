using System;

namespace Runtime
{

	/// <summary>
	/// Useful when providing a concrete implementation
	/// </summary>
	public class InjectAttributeSpecific : Attribute
	{
		/// <summary>
		/// The type to inject, e.g. A:IInterface 
		/// </summary>
		public Type type;
	}
	public class InjectAttribute : Attribute
	{

	}
}
