using AdminMigrationUtility.Helpers;
using AdminMigrationUtility.Helpers.Rsapi;
using AdminMigrationUtility.Helpers.Rsapi.Interfaces;
using kCura.EventHandler;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Console = kCura.EventHandler.Console;

namespace AdminMigrationUtility.EventHandlers
{
	[kCura.EventHandler.CustomAttributes.Description("Allows a user to submit a job to the Import Utility Job Manager queue")]
	[System.Runtime.InteropServices.Guid("3A5D193B-B91F-4D1C-B131-DA3B664373D4")]
	public class ImportManagerJobConsole : ConsoleEventHandler
	{
		public ISqlQueryHelper SqlQueryHelper = new SqlQueryHelper();
		public IArtifactQueries ArtifactQueries = new ArtifactQueries();

		public override Console GetConsole(ConsoleEventHandler.PageEvent pageEvent)
		{
			var console = GetConsoleAsync(pageEvent, GetLogger().Result).Result;
			return console;
		}

		public async Task<Console> GetConsoleAsync(PageEvent pageEvent, IAPILog logger)
		{
			var console = new Console { Items = new List<IConsoleItem>(), Title = "Manage Import Job" };

			var validateButton = new ConsoleButton
			{
				Name = Constant.Buttons.VALIDATE,
				DisplayText = "Validate",
				ToolTip = "Click here to validate this job",
				RaisesPostBack = true
			};

			var submitButton = new ConsoleButton
			{
				Name = Constant.Buttons.SUBMIT,
				DisplayText = "Submit",
				ToolTip = "Click here to add this job to the queue",
				RaisesPostBack = true
			};

			var cancelButton = new ConsoleButton
			{
				Name = Constant.Buttons.CANCEL,
				DisplayText = "Cancel",
				ToolTip = "Click here to cancel this job",
				RaisesPostBack = true
			};

			if (pageEvent == PageEvent.PreRender)
			{
				var recordExists = await DoesRecordExistAsync();
				if (recordExists)
				{
					validateButton.Enabled = false;
					submitButton.Enabled = false;
					cancelButton.Enabled = true;
				}
				else
				{
					validateButton.Enabled = true;
					submitButton.Enabled = true;
					cancelButton.Enabled = false;
				}
			}

			console.Items.Add(validateButton);
			console.Items.Add(submitButton);
			console.Items.Add(cancelButton);

			return console;
		}

		public override void OnButtonClick(ConsoleButton consoleButton)
		{
			OnButtonClickAsync(consoleButton, GetLogger().Result).Wait();
		}

		public async Task OnButtonClickAsync(ConsoleButton consoleButton, IAPILog logger)
		{
			bool recordExists;
			var selectedObjectType = ActiveArtifact.Fields[GetArtifactIdByGuid(Constant.Guids.Field.ExportUtilityJob.ObjectType)].Value.Value.ToString();

			switch (consoleButton.Name)
			{
				case Constant.Buttons.VALIDATE:
					logger.LogDebug($"{Constant.Names.ApplicationName} - {Constant.Buttons.VALIDATE} button clicked.");
					recordExists = await DoesRecordExistAsync();
					if (recordExists == false)
					{
						Int32 resourceGroupId = await ArtifactQueries.GetResourcePoolArtifactId(Helper.GetServicesManager(), ExecutionIdentity.System, Helper.GetActiveCaseID());
						await SqlQueryHelper.InsertRowIntoImportManagerQueueAsync(Helper.GetDBContext(-1), Helper.GetActiveCaseID(), 1, Helper.GetAuthenticationManager().UserInfo.ArtifactID, ActiveArtifact.ArtifactID, resourceGroupId, selectedObjectType, Constant.ExportUtilityJob.JobType.VALIDATE);
					}
					break;
				case Constant.Buttons.SUBMIT:
					logger.LogDebug($"{Constant.Names.ApplicationName} - {Constant.Buttons.SUBMIT} button clicked.");
					recordExists = await DoesRecordExistAsync();
					if (recordExists == false)
					{
						Int32 resourceGroupId = await ArtifactQueries.GetResourcePoolArtifactId(Helper.GetServicesManager(), ExecutionIdentity.System, Helper.GetActiveCaseID());
						await SqlQueryHelper.InsertRowIntoImportManagerQueueAsync(Helper.GetDBContext(-1), Helper.GetActiveCaseID(), 1, Helper.GetAuthenticationManager().UserInfo.ArtifactID, ActiveArtifact.ArtifactID, resourceGroupId, selectedObjectType, Constant.ExportUtilityJob.JobType.VALIDATE_SUBMIT);
					}
					break;
				case Constant.Buttons.CANCEL:
					logger.LogDebug($"{Constant.Names.ApplicationName} - {Constant.Buttons.CANCEL} button clicked.");
					var id = await RetrieveIdByArtifactIdAsync();
					if (id > 0)
					{
						await SqlQueryHelper.RemoveRecordFromTableByIdAsync(Helper.GetDBContext(-1), Constant.Tables.ImportManagerQueue, id);
					}
					break;
			}
		}

		public override FieldCollection RequiredFields
		{
			get
			{
				var retVal = new FieldCollection();
				return retVal;
			}
		}

		private async Task<Boolean> DoesRecordExistAsync()
		{
			var dataRow = await SqlQueryHelper.RetrieveSingleInImportManagerQueueByArtifactIdAsync(Helper.GetDBContext(-1), ActiveArtifact.ArtifactID, Helper.GetActiveCaseID());
			return dataRow != null;
		}

		private async Task<Int32> RetrieveIdByArtifactIdAsync()
		{
			var dataRow = await SqlQueryHelper.RetrieveSingleInImportManagerQueueByArtifactIdAsync(Helper.GetDBContext(-1), ActiveArtifact.ArtifactID, Helper.GetActiveCaseID());
			if (dataRow != null)
			{
				return Convert.ToInt32(dataRow["ID"]);
			}
			return 0;
		}

		private async Task<IAPILog> GetLogger()
		{
			var logger = await Task.Run(() => Helper.GetLoggerFactory().GetLogger());
			return logger;
		}
	}
}
