using System;

namespace AdminMigrationUtility.Helpers.Exceptions
{
	public class ObjectColumnDataConversionException : Exception
	{
		public ObjectColumnDataConversionException()
		{
		}

		public ObjectColumnDataConversionException(string message)
			: base(message)
		{
		}

		public ObjectColumnDataConversionException(string message, Exception inner)
			: base(message, inner)
		{
		}

		// A constructor is needed for serialization when an
		// exception propagates from a remoting server to the client.
		protected ObjectColumnDataConversionException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
		}
	}
}