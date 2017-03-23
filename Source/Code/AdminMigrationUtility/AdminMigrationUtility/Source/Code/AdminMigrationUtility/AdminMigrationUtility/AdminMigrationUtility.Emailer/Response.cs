using System;

namespace AdminMigrationUtility.Emailer
{
	public class Response
	{
		public Boolean Success { get; set; }
		public String Message { get; set; }

		public Response(Boolean success, String message)
		{
			Success = success;
			Message = message;
		}
	}
}