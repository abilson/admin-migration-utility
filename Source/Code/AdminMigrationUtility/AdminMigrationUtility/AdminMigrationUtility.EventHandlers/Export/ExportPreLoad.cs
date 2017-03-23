using AdminMigrationUtility.Helpers;
using AdminMigrationUtility.Helpers.SQL;
using kCura.EventHandler;
using System;

namespace AdminMigrationUtility.EventHandlers.Export
{
	[kCura.EventHandler.CustomAttributes.Description("Updates Export Job Progress.")]
	[System.Runtime.InteropServices.Guid("63C3F970-E63C-4912-BBC1-D73370A7B9E1")]
	public class ExportPreLoad : PreLoadEventHandler
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
				var statusField = ActiveArtifact.Fields[Constant.Guids.Field.ExportUtilityJob.Status.ToString()].Value;
				var expectedField = ActiveArtifact.Fields[Constant.Guids.Field.ExportUtilityJob.Expected.ToString()].Value;

				if (statusField != null && statusField.Value != null & expectedField != null)
				{
					String currentStatus = (String)statusField.Value;
					Int32 expectedNumberOfImports = ((Int32?)expectedField.Value).GetValueOrDefault();

					if (currentStatus == Constant.Status.Job.IN_PROGRESS_WORKER && expectedNumberOfImports > 0)
					{
						ISqlQueryHelper sqlQueryHelper = new SqlQueryHelper();
						Int32 numberOfWorkerRecords = sqlQueryHelper.CountNumberOfExportWorkerRecordsAsync(Helper.GetDBContext(-1), Helper.GetActiveCaseID(), ActiveArtifact.ArtifactID).Result;
						ActiveArtifact.Fields[Constant.Guids.Field.ExportUtilityJob.RecordsThatHaveBeenProcessed.ToString()].Value.Value = expectedNumberOfImports - numberOfWorkerRecords;
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
					new kCura.EventHandler.Field(Constant.Guids.Field.ExportUtilityJob.Status),
					new kCura.EventHandler.Field(Constant.Guids.Field.ExportUtilityJob.Expected),
					new kCura.EventHandler.Field(Constant.Guids.Field.ExportUtilityJob.RecordsThatHaveBeenProcessed)
				};

				return fieldCollection;
			}
		}
	}
}