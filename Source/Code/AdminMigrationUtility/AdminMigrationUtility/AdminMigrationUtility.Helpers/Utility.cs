using AdminMigrationUtility.Helpers.Exceptions;
using AdminMigrationUtility.Helpers.Models;
using AdminMigrationUtility.Helpers.Models.AdminObject;
using AdminMigrationUtility.Helpers.Models.Interfaces;
using AdminMigrationUtility.Helpers.Rsapi;
using kCura.Relativity.Client;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using AdminMigrationUtility.Helpers.Rsapi.Interfaces;

namespace AdminMigrationUtility.Helpers
{
	public class Utility
	{
		public static void ValidateEmailAddresses(String delimitedEmailAddresses, String delimiter, kCura.EventHandler.Response response)
		{
			if (!String.IsNullOrWhiteSpace(delimitedEmailAddresses))
			{
				var invalidEmailAddresses = new List<String>();
				var individualEmailAddress = delimitedEmailAddresses.Split(delimiter.ToCharArray());
				if (individualEmailAddress.Any())
				{
					foreach (var emailAddress in individualEmailAddress)
					{
						var match = System.Text.RegularExpressions.Regex.Match(emailAddress, Constant.RegExPatterns.EmailAddress);
						if (!match.Success)
						{
							invalidEmailAddresses.Add(emailAddress);
						}
					}
				}

				if (invalidEmailAddresses.Any())
				{
					response.Success = false;
					response.Message = response.Message ?? String.Empty;
					response.Message = response.Message + String.Format(Constant.ErrorMessages.InvalidEmailAddresses, String.Join(",", invalidEmailAddresses));
				}
			}
		}

		public static String FormatOrderByColumns(IEnumerable<String> columns)
		{
			var retVal = String.Empty;
			if(columns!= null && columns.Any())
			{
				var formattedColumns = new List<String>();
				foreach (var column in columns)
				{
					if(column != null)
					{
						var temp = column.Trim();
						if(column.IndexOf("[") < 0)
						{
							temp = "[" + temp;
						}
						if (column.IndexOf("]") < 0)
						{
							temp = temp + "]";
						}
						formattedColumns.Add(temp);
					}
				}

				retVal = "ORDER BY " + String.Join(",", formattedColumns);
			}
			return retVal;
		}

		public static async Task<IAdminObject> GetImportObjectSelectionAsync(String objectType)
		{
			return await Task.Run(() =>
			{
				IAdminObject migrationObject;
				var supportedObjects = String.Join(Constant.CommaSeparator, Enum.GetNames(typeof(Constant.Enums.SupportedObjects)));
				var unsupportedObjectMsg = String.Format(Constant.ErrorMessages.UnsupportedObjectError, supportedObjects);
				try
				{
					Constant.Enums.SupportedObjects parsedObjectType;
					if (Enum.TryParse<Constant.Enums.SupportedObjects>(objectType, out parsedObjectType))
					{
						switch (parsedObjectType)
						{
							case Constant.Enums.SupportedObjects.User:
								migrationObject = new UserAdminObject();
								break;

							default:
								throw new AdminMigrationUtilityException();
						}
					}
					else
					{
						throw new AdminMigrationUtilityException();
					}
				}
				catch (Exception ex)
				{
					throw new AdminMigrationUtilityException(unsupportedObjectMsg, ex);
					//violations.Add(new ImportJobError() { Message = unsupportedObjectMsg, Type = Constant.ImportUtilityJob.ErrorType.JobLevel, LineNumber = null, Details = ex.ToString()});
				}
				return migrationObject;
			});
		}

		public static async Task<String> FormatCsvLineAsync(IEnumerable<String> dataCells)
		{
			var processedCells = new List<String>();
			foreach (var cell in dataCells)
			{
				processedCells.Add(await FormatCsvCellAsync(cell));
			}
			return String.Join(",", processedCells);
		}

		public static async Task<String> FormatCsvCellAsync(String cell)
		{
			return await Task.Run(() =>
			{
				// Implements RCF4180 for csv format https://tools.ietf.org/html/rfc4180
				// Escape Quotes with double quote
				cell = cell.Replace("\"", "\"\"");

				// Encase Cell with Quotes if it contains and reserved characters
				var quoteRequirments = new String[] { ",", "\"", System.Environment.NewLine };
				var cnt = 0;
				var foundReservedchar = false;
				while (cnt < quoteRequirments.Length && foundReservedchar == false)
				{
					if (cell.Contains(quoteRequirments[cnt]))
					{
						cell = "\"" + cell + "\"";
						foundReservedchar = true;
					}
					cnt++;
				}
				return cell;
			});
		}

		public static Version GetRelativityVersion(Type type)
		{
			//Version userSessionInfoMaxRelativityVersion = new Version(9, 3, 0, 0);
			Version currentRelativityVersion = Assembly.GetAssembly(type).GetName().Version;
			return currentRelativityVersion;
		}

		public static Int32 CalculateImportJobImports(IEnumerable<ImportJobError> jobErrors, Int32 expectedNumberOfImports, Int32 numberOfWorkerRecords)
		{
			var retVal = 0;
			var fileLevelErrors = jobErrors.Where(x => x.Type == Constant.ImportUtilityJob.ErrorType.FileLevel);
			var distinctDataLevelErrors = jobErrors.Where(x => x.Type == Constant.ImportUtilityJob.ErrorType.DataLevel).Where(x => x.LineNumber != null).GroupBy(x => x.LineNumber);
			if (fileLevelErrors.Any())
			{
				retVal = 0;
			}
			else
			{
				retVal = expectedNumberOfImports - numberOfWorkerRecords - distinctDataLevelErrors.Count();
			}

			return retVal;
		}

        public static Int32 CalculateImportJobObjectsThatWereNotImported(IEnumerable<ImportJobError> jobErrors, Int32 expectedNumberOfImports, Int32 numberOfWorkerRecords)
        {
            var retVal = 0;
            var fileLevelErrors = jobErrors.Where(x => x.Type == Constant.ImportUtilityJob.ErrorType.FileLevel);
            var distinctDataLevelErrors = jobErrors.Where(x => x.Type == Constant.ImportUtilityJob.ErrorType.DataLevel).Where(x => x.LineNumber != null).GroupBy(x => x.LineNumber);
            if (fileLevelErrors.Any())
            {
                retVal = expectedNumberOfImports;
            }
            else if (distinctDataLevelErrors.Any())
            {
                retVal = numberOfWorkerRecords + distinctDataLevelErrors.Count();
            }
            return retVal;
        }

        public static Boolean UserIsAdmin(IHelper helper, Int32 userArtifactID, IArtifactQueries artifactQueries)
        {
            using (var proxy = helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System))
            {
                var apiOptions = new APIOptions() { WorkspaceID = -1 };
                proxy.APIOptions = apiOptions;
                return artifactQueries.UserIsInAdministratorGroup(apiOptions, proxy.Repositories.User, proxy.Repositories.Group, userArtifactID).Result;
            }
        }

    }
}