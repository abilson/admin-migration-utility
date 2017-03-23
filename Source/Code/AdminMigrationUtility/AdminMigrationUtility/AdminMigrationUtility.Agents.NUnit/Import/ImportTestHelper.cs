using AdminMigrationUtility.Helpers;
using AdminMigrationUtility.Helpers.Models;
using AdminMigrationUtility.Helpers.Models.AdminObject;
using AdminMigrationUtility.Helpers.Models.Interfaces;
using AdminMigrationUtility.Helpers.Rsapi.Interfaces;
using AdminMigrationUtility.Helpers.Serialization;
using AdminMigrationUtility.Helpers.SQL;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using kCura.Relativity.Client.Repositories;
using Moq;
using Newtonsoft.Json;
using Relativity.API;
using Relativity.Services.Security;
using Relativity.Services.Security.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Agents.NUnit.Import
{
	public class ImportTestHelper : IDisposable
	{
		private bool _disposed;

		public Mock<IAgentHelper> MockAgentHelper;
		public Mock<ISqlQueryHelper> MockSqlQueryHelper;
		public Mock<IAPILog> MockLogger;
		public Mock<IArtifactQueries> MockArtifactQueryHelper;
		public Mock<ILoginProfileManager> MockLoginManager;
		public Mock<IGenericRepository<kCura.Relativity.Client.DTOs.User>> MockUserRepo;
		public Mock<IGenericRepository<Client>> MockClientRepo;
		public Mock<IGenericRepository<Group>> MockGroupRepo;
		public Mock<IRsapiRepositoryGroup> MockRSAPIRepositoryGroup;
		public Mock<IRSAPIClient> MockRSAPIClient;
		public Mock<ISerializationHelper> MockSerializationHelper;

		public Boolean DuringOffHours { get; }
		public String OffHoursStartTime;
		public String OffHoursEndTime;
		public Int32 AgentId = 1234567;
		public Int32 JobArtifactID = 123;
		public String FileName = "Mock.csv";
		public String MockGroupName = "MockGroupName";
		public Int32 MockUserArtifactID = 12345;
		public String MockClientName = "MockClientName";
		public Int32 FileFieldArtifactID = 1034777;
		public List<Int32> ResourceGroupIdList = new List<int> { 10000, 20000 };
		public APIOptions APIOptions = new APIOptions() { WorkspaceID = -1 };
		public List<String> RecordedImportJobErrors = new List<string>();
		public List<String> LoggedEntries = new List<string>();
		public DataTable MockChoiceResult;
		public DateTime ProcessedTime = DateTime.Now;
		public DataTable InsertedWorkerQueueRecords;
		public Int32 WorkspaceArtifactID = 111117;
		public String ImportObjectType = "User";
		public String RDOJobStatus;

		public QueryResultSet<kCura.Relativity.Client.DTOs.Client> MockClientResult;
		public QueryResultSet<kCura.Relativity.Client.DTOs.Group> MockGroupResult;
		public RDO MockImportJobRDO;

		public Int32 ManagerQueueRecordStatus = Constant.Status.Queue.NOT_STARTED;
		public Int32 WorkerQueueRecordStatus = Constant.Status.Queue.NOT_STARTED;
		public Int32 RecordsReturedFromManagerBatch = 1;
		public Int32 RecordsReturedFromWorkerBatch = 1;

		public ImportTestHelper(Boolean duringOffHours)
		{
			DuringOffHours = duringOffHours;

			MockAgentHelper = new Mock<IAgentHelper>();
			MockSqlQueryHelper = new Mock<ISqlQueryHelper>();
			MockLogger = new Mock<IAPILog>();
			MockArtifactQueryHelper = new Mock<IArtifactQueries>();
			MockLoginManager = new Mock<ILoginProfileManager>();
			MockRSAPIClient = new Mock<IRSAPIClient>();
			MockSerializationHelper = new Mock<ISerializationHelper>();

			// Repositories
			MockUserRepo = new Mock<IGenericRepository<kCura.Relativity.Client.DTOs.User>>();
			MockClientRepo = new Mock<IGenericRepository<Client>>();
			MockGroupRepo = new Mock<IGenericRepository<Group>>();
			MockRSAPIRepositoryGroup = new Mock<IRsapiRepositoryGroup>();
		}

		public void Initialize()
		{
			InitializeChoiceResult();
			DetermineOnOffHours();
			InitializeImportJobRDO();
			InitializeClientQueryResults();
			InitializeGroupQueryResults();
			SetupHelper();
			SetupSQLQueryHelper();
			SetupArtifactQueryHelper();
			SetupLoginManager();
			SetupLogger();
			SetupUserRepo();
			SetupClientRepo();
			SetupGroupRepo();
			SetupRepositoryGroupWrapper();
			SetupSerializationHelper();
		}

		#region initializations

		// Used when the SqlQueryHelper queries choices
		private void InitializeChoiceResult()
		{
			MockChoiceResult = new DataTable();
			MockChoiceResult.Columns.Add("ArtifactID", typeof(Int32));
			MockChoiceResult.Rows.Add(1234);
		}

		private void InitializeImportJobRDO()
		{
			MockImportJobRDO = new RDO(JobArtifactID)
			{
				Guids = new List<Guid> { Constant.Guids.ObjectType.ImportUtilityJob },
				Fields = new List<FieldValue>
					{
						new FieldValue
						{
							ArtifactID = FileFieldArtifactID,
							Guids = new List<Guid> { Constant.Guids.Field.ImportUtilityJob.ImportFile },
							ValueAsFixedLengthText = FileName
						},
						new FieldValue
						{
							ArtifactID = 12345,
							Guids = new List<Guid> { Constant.Guids.Field.ImportUtilityJob.EmailAddresses },
							ValueAsFixedLengthText = "johnDoe@test.com"
						},
						new FieldValue
						{
							ArtifactID = 423424,
							Guids = new List<Guid> { Constant.Guids.Field.ImportUtilityJob.Name },
							ValueAsFixedLengthText = "Test Import Job"
						},
						new FieldValue
						{
							ArtifactID = 423424,
							Guids = new List<Guid> { Constant.Guids.Field.ImportUtilityJob.Expected },
							ValueAsWholeNumber = 1
						},
						new FieldValue
						{
							ArtifactID = 423424,
							Guids = new List<Guid> { Constant.Guids.Field.ImportUtilityJob.Status },
							ValueAsFixedLengthText = Constant.Status.Job.SUBMITTED
						}
					}
			};
		}

		private void InitializeClientQueryResults()
		{
			MockClientResult = new QueryResultSet<Client>
			{
				Success = true,
				Results = new List<Result<Client>>
				{
					new Result<Client>
					{
						Artifact = new Client() {
							Name = MockClientName
						}
					}
				}
			};
		}

		private void InitializeGroupQueryResults()
		{
			MockGroupResult = new QueryResultSet<Group>
			{
				Success = true,
				Results = new List<Result<Group>>
				{
					new Result<Group>
					{
						Artifact = new Group() {
							Name = MockGroupName
						}
					}
				}
			};
		}

		#endregion initializations

		#region privateMethods

		private void DetermineOnOffHours()
		{
			DateTime currentTime = DateTime.Now;
			if (DuringOffHours)
			{
				OffHoursStartTime = currentTime.AddHours(-1).ToString("HH:mm:ss");
				OffHoursEndTime = currentTime.AddHours(1).ToString("HH:mm:ss");
			}
			else
			{
				OffHoursStartTime = currentTime.AddHours(1).ToString("HH:mm:ss");
				OffHoursEndTime = currentTime.AddHours(2).ToString("HH:mm:ss");
			}
		}

		private void SetupHelper()
		{
			MockAgentHelper.Setup(x => x.GetServicesManager().CreateProxy<ILoginProfileManager>(It.IsAny<ExecutionIdentity>())).Returns(MockLoginManager.Object);
		}

		private void SetupSQLQueryHelper()
		{
			MockSqlQueryHelper
				.Setup(x => x.DeleteRecordFromQueueAsync(It.IsAny<IDBContext>(), It.IsAny<String>(), It.IsAny<Int32>(), It.IsAny<String>(), It.IsAny<Int32>(), It.IsAny<String>()))
				.Returns(Task.FromResult(0));

			MockSqlQueryHelper
				.Setup(x => x.UpdateKeywordAndNotes(It.IsAny<IDBContext>(), It.IsAny<Int32>(), It.IsAny<String>(), It.IsAny<String>())).Returns(Task.FromResult(0));

			var mockChoiceResult = new DataTable();
			mockChoiceResult.Columns.Add("ArtifactID", typeof(Int32));
			mockChoiceResult.Rows.Add(1234);

			MockSqlQueryHelper
				.Setup(x => x.QueryChoiceArtifactId(It.IsAny<IDBContext>(), It.IsAny<String>(), It.IsAny<String>()))
				.Returns(Task.FromResult(MockChoiceResult));

			MockSqlQueryHelper.Setup(
				x =>
					x.RetrieveNextBatchInImportWorkerQueueAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<Int32>(),
					It.IsAny<Int32>(),
					It.IsAny<String>()))
					.ReturnsAsync(GetWorkerTable(Constant.ImportUtilityJob.JobType.VALIDATE_SUBMIT, GenerateMetaDataForAdminObject(GenerateValidUser()), RecordsReturedFromWorkerBatch));

			// Removes the batch of work from the Worker Queue
			MockSqlQueryHelper.Setup(
				x =>
					x.RemoveBatchFromImportWorkerQueueAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<String>()))
					.Returns(Task.FromResult(false))
					.Verifiable();

			// Drops the temporary table created for this batch of work
			MockSqlQueryHelper.Setup(
				x =>
					x.DropTableAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<String>()))
					.Returns(Task.FromResult(false))
					.Verifiable();

			//Returns off-hours from configuration table
			var offHoursDt = GetOffHoursTable(OffHoursStartTime, OffHoursEndTime);
			MockSqlQueryHelper.Setup(
				x =>
				x.RetrieveOffHoursAsync(
				It.IsAny<IDBContext>()))
				.Returns(Task.FromResult(offHoursDt))
				.Verifiable();

			// Manager uses this to insert records into worker queue
			MockSqlQueryHelper
				.Setup(x => x.BulkInsertIntoTableAsync(It.IsAny<IDBContext>(), It.IsAny<DataTable>(), It.IsAny<String>(), It.IsAny<Int32>()))
				.Callback<IDBContext, DataTable, String, Int32>((dbContext, sourceDataTable, destinationTableName, batchSize) =>
				{
					InsertedWorkerQueueRecords = sourceDataTable;
				})
				.Returns(() => Task.FromResult(0));

			MockSqlQueryHelper
				.Setup(x => x.RetrieveAllImportManagerQueueRecords(It.IsAny<IDBContext>()))
				.Returns(Task.FromResult(GetManagerTable(RecordsReturedFromManagerBatch)));

			MockSqlQueryHelper
				.Setup(x => x.CountImportWorkerRecordsAsync(It.IsAny<IDBContext>(), It.IsAny<Int32>(), It.IsAny<Int32>()))
				.Returns(Task.FromResult(1));

			MockSqlQueryHelper
				.Setup(x => x.UpdateQueueStatusAsync(It.IsAny<IDBContext>(), It.IsAny<Int32>(), It.IsAny<Int32>(), It.IsAny<String>(), It.IsAny<String>(), It.IsAny<Int32>()))
				.Returns(Task.FromResult(0));

			MockArtifactQueryHelper
				.Setup(x => x.UpdateRdoTextFieldValueAsync(It.IsAny<APIOptions>(), It.IsAny<Int32>(), It.IsAny<IGenericRepository<RDO>>(), It.IsAny<Int32>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Object>()))
				.Callback<APIOptions, Int32, IGenericRepository<RDO>, Int32, Guid, Guid, Object>((rsapiApiOptions, workspaceArtifactId, rdoRepository, rdoArtifactId, jobObjectGuid, textFieldGuid, textFieldValue) =>
				{
					RDOJobStatus = textFieldValue.ToString();
				})
				.Returns(() => Task.FromResult(0));

			MockSqlQueryHelper
				.Setup(x => x.RetrieveNextInImportManagerQueueAsync(It.IsAny<IDBContext>(), It.IsAny<Int32>(), It.IsAny<String>())).ReturnsAsync(GetManagerTable(RecordsReturedFromManagerBatch));

			MockSqlQueryHelper
				.Setup(x => x.RetrieveConfigurationValue(It.IsAny<IDBContext>(), It.IsAny<String>(), It.IsAny<String>())).Returns(Task.FromResult(String.Empty));
		}

		private void SetupArtifactQueryHelper()
		{
			MockArtifactQueryHelper.Setup(x => x.RetrieveJob(It.IsAny<IGenericRepository<RDO>>(), It.IsAny<APIOptions>(), It.IsAny<Int32>(), It.IsAny<Int32>())).Returns(Task.FromResult(MockImportJobRDO));

			MockArtifactQueryHelper.Setup(x => x.DownloadFile(It.IsAny<IRSAPIClient>(), It.IsAny<APIOptions>(), It.IsAny<Int32>(), It.IsAny<Int32>(), It.IsAny<Int32>(), It.IsAny<String>())).Returns(Task.FromResult(0));

			MockArtifactQueryHelper
				.Setup(x => x.JobErrorRecordExistsAsync(It.IsAny<APIOptions>(), It.IsAny<Int32>(), It.IsAny<IGenericRepository<RDO>>(), It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<Int32>()))
				.Returns(() => Task.FromResult(false));

			MockArtifactQueryHelper
				.Setup(x => x.CreateImportJobErrorRecordsAsync(It.IsAny<APIOptions>(), It.IsAny<Int32>(), It.IsAny<Int32>(), It.IsAny<IGenericRepository<RDO>>(), It.IsAny<IEnumerable<ImportJobError>>(), It.IsAny<String>()))
				.Callback<APIOptions, Int32, Int32, IGenericRepository<RDO>, IEnumerable<ImportJobError>, String>((apiOptions, workspaceArtifactID, jobArtifactID, rdoRepo, errors, status) =>
				{
					RecordedImportJobErrors.AddRange(errors.Select(x => x.Message).ToList());
				})
				.Returns(Task.FromResult(0));
		}

		private void SetupLoginManager()
		{
			MockLoginManager
				.Setup(x => x.GetLoginProfileAsync(It.IsAny<Int32>())).Returns(Task.FromResult(new LoginProfile() { UserId = MockUserArtifactID }));
			MockLoginManager
				.Setup(x => x.SaveLoginProfileAsync(It.IsAny<LoginProfile>())).Returns(Task.FromResult<Task>(null));
		}

		private void SetupUserRepo()
		{
			MockUserRepo.Setup(x => x.DeleteSingle(It.IsAny<Int32>()));
			MockUserRepo.Setup(x => x.CreateSingle(It.IsAny<kCura.Relativity.Client.DTOs.User>())).Returns(MockUserArtifactID);
		}

		private void SetupClientRepo()
		{
			MockClientRepo.Setup(x => x.Query(It.IsAny<Query<Client>>(), It.IsAny<Int32>())).Returns(MockClientResult);
		}

		private void SetupGroupRepo()
		{
			MockGroupRepo.Setup(x => x.Query(It.IsAny<Query<Group>>(), It.IsAny<Int32>())).Returns(MockGroupResult);
		}

		private void SetupRepositoryGroupWrapper()
		{
			MockRSAPIRepositoryGroup.Setup(x => x.UserRepository).Returns(MockUserRepo.Object);
			MockRSAPIRepositoryGroup.Setup(x => x.GroupRepository).Returns(MockGroupRepo.Object);
			MockRSAPIRepositoryGroup.Setup(x => x.ClientRepository).Returns(MockClientRepo.Object);
		}

		private void SetupLogger()
		{
			MockLogger
				.Setup(x => x.LogError(It.IsAny<Exception>(), It.IsAny<String>(), It.IsAny<Object>()))
				.Callback<Exception, String, Object>((exception, context, obj) =>
				{
					LoggedEntries.Add(context);
				});
		}

		private void SetupSerializationHelper()
		{
			var realSerializationHelper = new SerializationHelper();

			MockSerializationHelper
				.Setup(x => x.SerializeAdminObjectAsync(It.IsAny<IAdminObject>()))
				.Returns((IAdminObject obj) => realSerializationHelper.SerializeAdminObjectAsync(obj));

			MockSerializationHelper
				.Setup(x => x.DeserializeToAdminObjectAsync(It.IsAny<String>()))
				.Returns((String metadata) => realSerializationHelper.DeserializeToAdminObjectAsync(metadata));
		}

		#endregion privateMethods

		#region utilityMethods

		private DataTable GetManagerTable(Int32 numRows)
		{
			var table = new DataTable("Test Manager Table");
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportManagerQueue.AgentID, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportManagerQueue.WorkspaceArtifactID, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportManagerQueue.ID, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportManagerQueue.JobID, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportManagerQueue.Priority, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportManagerQueue.ResourceGroupID, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportManagerQueue.ObjectType, typeof(String));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportManagerQueue.JobType, typeof(String));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportManagerQueue.QueueStatus, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportManagerQueue.WorkspaceName, typeof(String));

			for (var i = 0; i < numRows; i++)
			{
				table.Rows.Add(1, WorkspaceArtifactID, i, JobArtifactID, 3, 1000001, ImportObjectType, Constant.ImportUtilityJob.JobType.VALIDATE_SUBMIT, ManagerQueueRecordStatus, "Random Workspace Name");
			}
			return table;
		}

		private DataTable GetWorkerTable(String jobType, String metaData, Int32 numRows)
		{
			var table = new DataTable("Test Worker Table");
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.ID, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.AgentID, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.JobID, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.WorkspaceArtifactID, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.ObjectType, typeof(String));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.JobType, typeof(String));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.MetaData, typeof(String));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.ImportRowID, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.QueueStatus, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.ResourceGroupID, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.Priority, typeof(Int32));
			table.Columns.Add(Constant.Sql.ColumnsNames.ImportWorkerQueue.TimeStampUTC, typeof(DateTime));
			for (var i = 0; i < numRows; i++)
			{
				table.Rows.Add(i, 123, JobArtifactID, 1014750, "User", jobType, metaData, i, WorkerQueueRecordStatus, 123, 1, DateTime.UtcNow);
			}
			return table;
		}

		private UserAdminObject GenerateValidUser()
		{
			var user = new UserAdminObject
			{
				FirstName = { Data = "John" },
				LastName = { Data = "Doe" },
				EmailAddress = { Data = "JohnDoe@adminMigrationUtlity.com" },
				Keywords = { Data = "Cool User" },
				Notes = { Data = "Process case after 5pm" },
				Groups = { Data = MockGroupName },
				Type = { Data = "Internal" },
				Client = { Data = "Acme" },
				RelativityAccess = { Data = "true" },
				DocumentSkip = { Data = "100100" },
				BetaUser = { Data = "false" },
				ChangeSettings = { Data = "true" },
				KeyboardShortcuts = { Data = "true" },
				ItemListPageLength = { Data = "100" },
				DefaultSelectedFileType = { Data = "100200" },
				SkipDefaultPreference = { Data = "100300" },
				EnforceViewerCompatibility = { Data = "true" },
				AdvancedSearchPublicByDefault = { Data = "false" },
				NativeViewerCacheAhead = { Data = "true" },
				ChangeDocumentViewer = { Data = "true" },
				DocumentViewer = { Data = "100400" },
				WindowsAccount = { Data = "JohnDoe@adminMigrationUtlity.com" },
				CanChangePassword = { Data = "true" },
				MaximumPasswordAgeInDays = { Data = "30" },
				UserMustChangePasswordOnNextLogin = { Data = "false" }
			};
			return user;
		}

		private String GenerateMetaDataForAdminObject(IAdminObject adminObject)
		{
			var serializedJson = JsonConvert.SerializeObject(adminObject, Formatting.None, new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.Objects,
				TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
			});
			return serializedJson;
		}

		private DataTable GetOffHoursTable(String startTime, String endTime)
		{
			var table = new DataTable();
			table.Columns.Add("AgentOffHourStartTime", typeof(String));
			table.Columns.Add("AgentOffHourEndTime", typeof(String));
			table.Rows.Add(startTime, endTime);
			return table;
		}

		#endregion utilityMethods

		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					MockAgentHelper = null;
					MockSqlQueryHelper = null;
					MockLogger = null;
					MockArtifactQueryHelper = null;
					MockLoginManager = null;
					MockUserRepo = null;
					MockClientRepo = null;
					MockGroupRepo = null;
					MockRSAPIRepositoryGroup = null;

					OffHoursStartTime = null;
					OffHoursEndTime = null;
					AgentId = 0;
					JobArtifactID = 0;
					FileName = null;
					MockGroupName = null;
					MockUserArtifactID = 0;
					MockClientName = null;
					FileFieldArtifactID = 0;
					ResourceGroupIdList = null;
					APIOptions = null;
					RecordedImportJobErrors = null;
					MockChoiceResult = null;
					MockClientResult = null;
					MockGroupResult = null;
				}
			}
			_disposed = true;
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}