using AdminMigrationUtility.Helpers;
using AdminMigrationUtility.Helpers.Models;
using AdminMigrationUtility.Helpers.Rsapi;
using AdminMigrationUtility.Helpers.Rsapi.Interfaces;
using AdminMigrationUtility.Helpers.SQL;
using kCura.EventHandler;
using kCura.Relativity.Client;
using Relativity.API;
using System;
using System.Collections.Generic;

namespace AdminMigrationUtility.EventHandlers.Import
{
	[kCura.EventHandler.CustomAttributes.Description("Updates Import Job Progress.")]
	[System.Runtime.InteropServices.Guid("44CC2194-422F-492A-9A66-C4B34E8C6E83")]
	public class ImportPreLoad : PreLoadEventHandler
	{
		public override Response Execute()
		{
			Response response = new Response()
			{
				Success = true,
				Message = string.Empty
			};

			if (!ActiveArtifact.IsNew)
			{
				var statusField = ActiveArtifact.Fields[Constant.Guids.Field.ImportUtilityJob.Status.ToString()].Value;
				var expectedField = ActiveArtifact.Fields[Constant.Guids.Field.ImportUtilityJob.Expected.ToString()].Value;
				var submittedForMigrationField = ActiveArtifact.Fields[Constant.Guids.Field.ImportUtilityJob.SubmittedForMigration.ToString()].Value;
				if (statusField != null && statusField.Value != null & expectedField != null && submittedForMigrationField != null && submittedForMigrationField.Value != null)
				{
					String currentStatus = (String)statusField.Value;
					Int32 expectedNumberOfImports = ((Int32?)expectedField.Value).GetValueOrDefault();
					Boolean submittedForMigration = ((Boolean?)submittedForMigrationField.Value).GetValueOrDefault();

					if (currentStatus == Constant.Status.Job.IN_PROGRESS_WORKER && submittedForMigration && expectedNumberOfImports > 0)
					{
						using (var proxy = Helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.System))
						{
							var apiOptions = new APIOptions();
							proxy.APIOptions = apiOptions;
							ISqlQueryHelper sqlQueryHelper = new SqlQueryHelper();
							IArtifactQueries artifactQueries = new ArtifactQueries();
							Int32 numberOfWorkerRecords = sqlQueryHelper.CountImportWorkerRecordsAsync(Helper.GetDBContext(-1), Helper.GetActiveCaseID(), ActiveArtifact.ArtifactID).Result;
							IEnumerable<ImportJobError> jobErrors = artifactQueries.GetImportJobErrorsAsync(apiOptions, Helper.GetActiveCaseID(), proxy.Repositories.RDO, ActiveArtifact.ArtifactID).Result;
							ActiveArtifact.Fields[Constant.Guids.Field.ImportUtilityJob.Imported.ToString()].Value.Value = Utility.CalculateImportJobImports(jobErrors, expectedNumberOfImports, numberOfWorkerRecords);
							ActiveArtifact.Fields[Constant.Guids.Field.ImportUtilityJob.NotImported.ToString()].Value.Value = Utility.CalculateImportJobObjectsThatWereNotImported(jobErrors, expectedNumberOfImports, numberOfWorkerRecords);
						}
					}
				}
			}

			return response;
		}

		public override FieldCollection RequiredFields
		{
			get
			{
				FieldCollection fieldCollection = new FieldCollection
				{
					new kCura.EventHandler.Field(Constant.Guids.Field.ImportUtilityJob.Status),
					new kCura.EventHandler.Field(Constant.Guids.Field.ImportUtilityJob.Expected),
					new kCura.EventHandler.Field(Constant.Guids.Field.ImportUtilityJob.Imported),
					new kCura.EventHandler.Field(Constant.Guids.Field.ImportUtilityJob.NotImported),
					new kCura.EventHandler.Field(Constant.Guids.Field.ImportUtilityJob.SubmittedForMigration)
				};

				return fieldCollection;
			}
		}
	}
}