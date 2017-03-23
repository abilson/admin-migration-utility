using System;
using System.Text;

namespace AdminMigrationUtility.Helpers.Exceptions
{
	public class ExceptionMessageFormatter
	{
		public static String GetInnerMostExceptionMessage(Exception exception)
		{
			String retVal;

			if (exception == null)
			{
				retVal = String.Empty;
			}
			else
			{
				Exception currentException = exception;
				while (currentException.InnerException != null)
				{
					currentException = currentException.InnerException;
				}

				retVal = currentException.Message;
			}
			return retVal;
		}

		public static String GetExceptionMessageIncludingAllInnerExceptions(Exception exception)
		{
			String retVal;

			if (exception == null)
			{
				retVal = String.Empty;
			}
			else
			{
				StringBuilder stringBuilder = new StringBuilder();
				Exception currentException = exception;
				stringBuilder.Append(currentException.Message);

				while (currentException.InnerException != null)
				{
					currentException = currentException.InnerException;
					stringBuilder.Append(" --> " + currentException.Message);
				}

				retVal = stringBuilder.ToString();
			}
			return retVal;
		}
	}
}