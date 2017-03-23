using AdminMigrationUtility.Helpers;
using AdminMigrationUtility.Helpers.Exceptions;
using AdminMigrationUtility.Helpers.Rsapi;
using AdminMigrationUtility.Helpers.Rsapi.Interfaces;
using AdminMigrationUtility.Helpers.SQL;
using kCura.EventHandler;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdminMigrationUtility.EventHandlers.Export
{
	[kCura.EventHandler.CustomAttributes.Description("Allows a user to submit a job to the Export Utility Job Manager queue.")]
	[System.Runtime.InteropServices.Guid("604F416C-E91F-418D-9E2A-F106D6257515")]
	public class ExportConsole : ConsoleEventHandler
	{
		public ISqlQueryHelper SqlQueryHelper = new SqlQueryHelper();
		public IArtifactQueries ArtifactQueries = new ArtifactQueries();
		public IAPILog Logger;
		public string SelectedObjectType = string.Empty;
		public IServicesMgr SvcManager;
		public int WorkspaceArtifactId;
		public ExecutionIdentity IdentityCurrentUser;
		public ExecutionIdentity IdentitySystem;
		public int CurrentArtifactId;
		public IDBContext DbContextEdds;
		public IDBContext DbContext;
		public string ExportManagerResultsTableName = string.Empty;
		public ChoiceCollection ColObjectType;
		public ExportConsoleJob ExportUtilityJob;
		public Int32 Priority;

		private kCura.EventHandler.Console _console;
		private ConsoleButton _submitButton;
		private ConsoleButton _cancelButton;

		public override kCura.EventHandler.Console GetConsole(PageEvent pageEvent)
		{
			LoadVariables();
			return LoadConsoleAsync(pageEvent).Result;
		}

		public async Task<kCura.EventHandler.Console> LoadConsoleAsync(PageEvent pageEvent)
		{
			_console = new kCura.EventHandler.Console { Items = new List<IConsoleItem>(), Title = "Manage Export Job" };

			_submitButton = new ConsoleButton
			{
				Name = Constant.Buttons.SUBMIT,
				DisplayText = "Submit",
				ToolTip = "Click here to add this job to the queue",
				RaisesPostBack = true
			};

			_cancelButton = new ConsoleButton
			{
				Name = Constant.Buttons.CANCEL,
				DisplayText = "Cancel",
				ToolTip = "Click here to cancel this job",
				RaisesPostBack = true
			};

			if (Utility.UserIsAdmin(Helper, Helper.GetAuthenticationManager().UserInfo.ArtifactID, ArtifactQueries))
			{
				if (pageEvent == PageEvent.PreRender)
				{
					var recordExists = await ExportUtilityJob.DoesRecordExistAsync();
					if (recordExists)
					{
						var jobStatus = await ExportUtilityJob.RetrieveJobStatusAsync(SvcManager, WorkspaceArtifactId, IdentityCurrentUser, CurrentArtifactId);

						switch (jobStatus)
						{
							case Constant.Status.Job.NEW:
								_submitButton.Enabled = true;
								_cancelButton.Enabled = false;
								break;

							case Constant.Status.Job.SUBMITTED:
							case Constant.Status.Job.IN_PROGRESS_MANAGER:
							case Constant.Status.Job.COMPLETED_MANAGER:
							case Constant.Status.Job.IN_PROGRESS_WORKER:
							case Constant.Status.Job.RETRY:
								_submitButton.Enabled = false;
								_cancelButton.Enabled = true;
								break;

							case Constant.Status.Job.COMPLETED:
							case Constant.Status.Job.COMPLETED_WITH_ERRORS:
							case Constant.Status.Job.ERROR:
							case Constant.Status.Job.CANCELLED:
							case Constant.Status.Job.CANCELREQUESTED:
								_submitButton.Enabled = false;
								_cancelButton.Enabled = false;
								break;

							default:
								_submitButton.Enabled = false;
								_cancelButton.Enabled = false;
								break;
						}
					}
					else
					{
						_submitButton.Enabled = true;
						_cancelButton.Enabled = false;
					}
				}
			}
			else
			{
				_console.ScriptBlocks.Add(new ScriptBlock() { Key = "DisableButtonsForNonAdmins", Script = "<script type='text/javascript'>alert('Buttons Disabled. Only system administrators are allowed to submit jobs.')</script>" });
				_submitButton.Enabled = false;
				_cancelButton.Enabled = false;
			}

			_console.Items.Add(_submitButton);
			_console.Items.Add(_cancelButton);
			_console.AddRefreshLinkToConsole().Enabled = true;

			return _console;
		}

		private void LoadVariables()
		{
			SelectedObjectType = string.Empty;
			SvcManager = Helper.GetServicesManager();
			Logger = GetLoggerAsync().Result;
			WorkspaceArtifactId = Helper.GetActiveCaseID();
			IdentityCurrentUser = ExecutionIdentity.CurrentUser;
			IdentitySystem = ExecutionIdentity.System;
			CurrentArtifactId = ActiveArtifact.ArtifactID;
			DbContextEdds = Helper.GetDBContext(-1);
			DbContext = Helper.GetDBContext(WorkspaceArtifactId);
            ColObjectType = (ChoiceCollection)ActiveArtifact.Fields[Constant.Guids.Field.ExportUtilityJob.ObjectType.ToString()].Value.Value;

            var priority = ActiveArtifact.Fields[Constant.Guids.Field.ExportUtilityJob.Priority.ToString()];
			if (priority.Value?.Value != null)
			{
				Priority = (int)priority.Value.Value;
			}
			else
			{
				Priority = 0;
			}

			//Set the selected Object Type from the record
			foreach (Choice objectType in ColObjectType)
			{
				if (!objectType.IsSelected)
					continue;
				SelectedObjectType = objectType.Name;
				break;
			}

			ExportUtilityJob = new ExportConsoleJob(SvcManager, DbContextEdds, DbContext, IdentityCurrentUser, IdentitySystem, ArtifactQueries, SqlQueryHelper, Logger, WorkspaceArtifactId, CurrentArtifactId, string.Empty, SelectedObjectType, Priority);
		}

		public override void OnButtonClick(ConsoleButton consoleButton)
		{
			OnButtonClickAsync(consoleButton).Wait();
		}

		public async Task OnButtonClickAsync(ConsoleButton consoleButton)
		{
			ExportUtilityJob.ButtonName = consoleButton.Name;

			try
			{
				await Task.Run(() => ExportUtilityJob.ExecuteAsync());
			}
			catch (AdminMigrationUtilityException ex)
			{
				throw new Exception(ex.Message);
			}
		}

		private async Task<IAPILog> GetLoggerAsync()
		{
			return await Task.Run(() => Helper.GetLoggerFactory().GetLogger());
		}

		public override FieldCollection RequiredFields => new FieldCollection();
	}
}