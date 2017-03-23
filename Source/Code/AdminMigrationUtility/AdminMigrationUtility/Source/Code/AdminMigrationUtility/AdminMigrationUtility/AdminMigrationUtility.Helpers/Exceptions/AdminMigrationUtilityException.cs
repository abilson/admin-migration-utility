namespace AdminMigrationUtility.Helpers.Exceptions
{
	[System.Serializable]
	public class AdminMigrationUtilityException : System.Exception
	{
		public AdminMigrationUtilityException()
		{
		}

		public AdminMigrationUtilityException(string message)
			: base(message)
		{
		}

		public AdminMigrationUtilityException(string message, System.Exception inner)
			: base(message, inner)
		{
		}

		// A constructor is needed for serialization when an
		// exception propagates from a remoting server to the client.
		protected AdminMigrationUtilityException(System.Runtime.Serialization.SerializationInfo info, System.Runtime.Serialization.StreamingContext context)
		{
		}
	}
}