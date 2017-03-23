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
	[kCura.EventHandler.CustomAttributes.Description("Allows a user to submit a job to the Export Utility Job Manager queue.")]
	[System.Runtime.InteropServices.Guid("604F416C-E91F-418D-9E2A-F106D6257515")]
	public class ExportManagerJobConsole : ConsoleEventHandler
	{
		public ISqlQueryHelper SqlQueryHelper = new SqlQueryHelper();
		public IArtifactQueries ArtifactQueries = new ArtifactQueries();

		public override Console GetConsole(PageEvent pageEvent)
		{
			var console = GetConsoleAsync(pageEvent, GetLogger().Result).Result;
			return console;
		}

		public async Task<Console> GetConsoleAsync(PageEvent pageEvent, IAPILog logger)
		{
			var console = new Console { Items = new List<IConsoleItem>(), Title = "Manage Export Job" };

			var submitButton = new ConsoleButton
			{
				Name = Constant.Buttons.SUBMIT,
				DisplayText = "Submit",
				ToolTip = "Click here to add this job to the queue",
				RaisesPostBack = true
			};

			var downloadFileButton = new ConsoleButton
			{
				Name = Constant.Buttons.DOWNLOAD_FILE,
				DisplayText = "Download File",
				ToolTip = "Click here to download the export file",
				RaisesPostBack = true
			};

			if (pageEvent == PageEvent.PreRender)
			{
				var recordExists = await DoesRecordExistAsync();
				if (recordExists)
				{
					submitButton.Enabled = false;
					downloadFileButton.Enabled = true;
				}
				else
				{
					submitButton.Enabled = true;
					downloadFileButton.Enabled = false;
				}
			}

			console.Items.Add(submitButton);
			console.Items.Add(downloadFileButton);

			return console;
		}

		public override void OnButtonClick(ConsoleButton consoleButton)
		{
			OnButtonClickAsync(consoleButton, GetLogger().Result).Wait();
		}

		public async Task OnButtonClickAsync(ConsoleButton consoleButton, IAPILog logger)
		{
			var selectedObjectType = string.Empty;

			var colObjectType = (ChoiceCollection)ActiveArtifact.Fields[GetArtifactIdByGuid(Constant.Guids.Field.ExportUtilityJob.ObjectType)].Value.Value;
			foreach (Choice objectType in colObjectType)
			{
				if (!objectType.IsSelected)
					continue;
				selectedObjectType = objectType.Name;
				break;
			}

			var exportManagerResultsTableName = $"{Constant.Names.TablePrefix}Export_Manager_Results_{Guid.NewGuid()}";

			switch (consoleButton.Name)
			{
				case Constant.Buttons.SUBMIT:
					logger.LogDebug($"{Constant.Names.ApplicationName} - {Constant.Buttons.SUBMIT} button clicked.");
					var recordExists = await DoesRecordExistAsync();
					if (recordExists == false)
					{
						var resourceGroupId = await ArtifactQueries.GetResourcePoolArtifactId(Helper.GetServicesManager(), ExecutionIdentity.System, Helper.GetActiveCaseID());
						await SqlQueryHelper.InsertRowIntoExportManagerQueueAsync(Helper.GetDBContext(-1), Helper.GetActiveCaseID(), 1, Helper.GetAuthenticationManager().UserInfo.ArtifactID, ActiveArtifact.ArtifactID, resourceGroupId, selectedObjectType, exportManagerResultsTableName);
					}
					break;
				case Constant.Buttons.DOWNLOAD_FILE:
					logger.LogDebug($"{Constant.Names.ApplicationName} - {Constant.Buttons.DOWNLOAD_FILE} button clicked.");
					var id = await RetrieveIdByArtifactIdAsync();
					if (id > 0)
					{
						await SqlQueryHelper.RemoveRecordFromTableByIdAsync(Helper.GetDBContext(-1), Constant.Tables.ExportManagerQueue, id);
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
			var dataRow = await SqlQueryHelper.RetrieveSingleInExportManagerQueueByArtifactIdAsync(Helper.GetDBContext(-1), ActiveArtifact.ArtifactID, Helper.GetActiveCaseID());
			return dataRow != null;
		}

		private async Task<Int32> RetrieveIdByArtifactIdAsync()
		{
			var dataRow = await SqlQueryHelper.RetrieveSingleInExportManagerQueueByArtifactIdAsync(Helper.GetDBContext(-1), ActiveArtifact.ArtifactID, Helper.GetActiveCaseID());
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
