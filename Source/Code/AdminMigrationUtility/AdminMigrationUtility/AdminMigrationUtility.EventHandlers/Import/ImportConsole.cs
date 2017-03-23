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

namespace AdminMigrationUtility.EventHandlers.Import
{
	[kCura.EventHandler.CustomAttributes.Description("Allows a user to submit a job to the Import Utility Job Manager queue")]
	[System.Runtime.InteropServices.Guid("3A5D193B-B91F-4D1C-B131-DA3B664373D4")]
	public class ImportConsole : ConsoleEventHandler
	{
		public ISqlQueryHelper SqlQueryHelper = new SqlQueryHelper();
		public IArtifactQueries ArtifactQueries = new ArtifactQueries();
		public IServicesMgr SvcManager;
		public IAPILog Logger;
		public Int32 WorkspaceArtifactId;
		public string SelectedObjectType = string.Empty;
		public ExecutionIdentity IdentityCurrentUser;
		public ExecutionIdentity IdentitySystem;
		public int CurrentArtifactId;
		public int CurrentUserArtifactId;
		public IDBContext DbContextEdds;
		public IDBContext DbContext;
		public string ImportManagerResultsTableName = string.Empty;
		public ChoiceCollection ColObjectType;
		public ImportConsoleJob ImportUtilityJob;

		private kCura.EventHandler.Console _console;
		private ConsoleButton _validateButton;
		private ConsoleButton _submitButton;
		private ConsoleButton _cancelButton;

		public override kCura.EventHandler.Console GetConsole(PageEvent pageEvent)
		{
			LoadVariables();
			return LoadConsoleAsync(pageEvent).Result;
		}

		public async Task<kCura.EventHandler.Console> LoadConsoleAsync(PageEvent pageEvent)
		{
			_console = new kCura.EventHandler.Console { Items = new List<IConsoleItem>(), Title = "Manage Import Job" };

			_validateButton = new ConsoleButton
			{
				Name = Constant.Buttons.VALIDATE,
				DisplayText = "Validate",
				ToolTip = "Click here to validate this job",
				RaisesPostBack = true
			};

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
				var jobStatus = await ImportUtilityJob.RetrieveJobStatusAsync(SvcManager, WorkspaceArtifactId, IdentityCurrentUser, CurrentArtifactId);
				var recordExists = await ImportUtilityJob.DoesRecordExistAsync();
				var submittedForMigration = ((Boolean?)(ActiveArtifact.Fields[Constant.Guids.Field.ImportUtilityJob.SubmittedForMigration.ToString()].Value).Value).GetValueOrDefault();

				if (pageEvent == PageEvent.PreRender)
				{
					if (recordExists)
					{
						switch (jobStatus)
						{
							case Constant.Status.Job.NEW:
								_validateButton.Enabled = true;
								_submitButton.Enabled = true;
								_cancelButton.Enabled = false;
								break;

							case Constant.Status.Job.SUBMITTED:
							case Constant.Status.Job.IN_PROGRESS_MANAGER:
							case Constant.Status.Job.COMPLETED_MANAGER:
							case Constant.Status.Job.IN_PROGRESS_WORKER:
							case Constant.Status.Job.RETRY:
								_validateButton.Enabled = false;
								_submitButton.Enabled = false;
								_cancelButton.Enabled = true;
								break;

							case Constant.Status.Job.COMPLETED:
							case Constant.Status.Job.COMPLETED_WITH_ERRORS:
							case Constant.Status.Job.ERROR:
							case Constant.Status.Job.CANCELLED:
							case Constant.Status.Job.CANCELREQUESTED:
								_validateButton.Enabled = false;
								_submitButton.Enabled = false;
								_cancelButton.Enabled = false;
								break;

							default:
								_validateButton.Enabled = false;
								_submitButton.Enabled = false;
								_cancelButton.Enabled = false;
								break;
						}
					}
					else
					{
						if (submittedForMigration && (jobStatus == Constant.Status.Job.COMPLETED || jobStatus == Constant.Status.Job.COMPLETED_WITH_ERRORS || jobStatus == Constant.Status.Job.ERROR))
						{
							_validateButton.Enabled = false;
							_submitButton.Enabled = false;
							_cancelButton.Enabled = false;
						}
						else
						{
							_validateButton.Enabled = true;
							_submitButton.Enabled = true;
							_cancelButton.Enabled = false;
						}
					}
				}
			}
			else
			{
				_console.ScriptBlocks.Add(new ScriptBlock() { Key = "DisableButtonsForNonAdmins", Script = "<script type='text/javascript'>alert('Buttons Disabled. Only system administrators are allowed to submit jobs.')</script>" });
				_validateButton.Enabled = false;
				_submitButton.Enabled = false;
				_cancelButton.Enabled = false;
			}

			_console.Items.Add(_validateButton);
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
			CurrentUserArtifactId = Helper.GetAuthenticationManager().UserInfo.ArtifactID;
			DbContextEdds = Helper.GetDBContext(-1);
			DbContext = Helper.GetDBContext(WorkspaceArtifactId);
			ImportManagerResultsTableName = $"{Constant.Names.TablePrefix}Import_Manager_Results_{Guid.NewGuid()}";
			ColObjectType = (ChoiceCollection)ActiveArtifact.Fields[GetArtifactIdByGuid(Constant.Guids.Field.ImportUtilityJob.ObjectType)].Value.Value;

			//Set the selected Object Type from the record
			foreach (Choice objectType in ColObjectType)
			{
				if (!objectType.IsSelected)
					continue;
				SelectedObjectType = objectType.Name;
				break;
			}

			ImportUtilityJob = new ImportConsoleJob(SvcManager, DbContextEdds, DbContext, IdentityCurrentUser, IdentitySystem, ArtifactQueries, SqlQueryHelper, Logger, WorkspaceArtifactId, CurrentArtifactId, CurrentUserArtifactId, string.Empty, SelectedObjectType, ImportManagerResultsTableName);
		}

		public override void OnButtonClick(ConsoleButton consoleButton)
		{
			OnButtonClickAsync(consoleButton).Wait();
		}

		public async Task OnButtonClickAsync(ConsoleButton consoleButton)
		{
			ImportUtilityJob.ButtonName = consoleButton.Name;

			try
			{
				await Task.Run(() => ImportUtilityJob.ExecuteAsync());
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

		public override FieldCollection RequiredFields
		{
			get
			{
				FieldCollection retVal = new FieldCollection { new kCura.EventHandler.Field(Constant.Guids.Field.ImportUtilityJob.SubmittedForMigration) };
				return retVal;
			}
		}
	}
}