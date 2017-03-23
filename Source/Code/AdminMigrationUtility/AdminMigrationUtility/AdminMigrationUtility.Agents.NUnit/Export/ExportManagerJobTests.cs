using AdminMigrationUtility.Agents.Export;
using AdminMigrationUtility.Helpers;
using AdminMigrationUtility.Helpers.Exceptions;
using AdminMigrationUtility.Helpers.Rsapi.Interfaces;
using AdminMigrationUtility.Helpers.Serialization;
using AdminMigrationUtility.Helpers.SQL;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using kCura.Relativity.Client.Repositories;
using Moq;
using NUnit.Framework;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Agents.NUnit.Export
{
	[TestFixture]
	public class ExportManagerJobTests
	{
		public Mock<ISqlQueryHelper> MockSqlQueryHelper;
		public Mock<IAgentHelper> MockAgentHelper;
		public Mock<IAPILog> MockLogger;
		public Mock<IArtifactQueries> MockArtifactQueries;
		public APIOptions RsapiApiOptions;
		public Mock<IRsapiRepositoryGroup> MockRsapiRepositoryGroup;
		public Mock<ISerializationHelper> MockSerializationHelper;
		public const Int32 USER_REPOSITORY_QUERY_LENGTH = Int32.MaxValue;
		public const Int32 AGENT_ID = 123;
		public List<Int32> WorkspaceResourceGroupIdList;
		public ExportManagerJob Sut { get; set; }

		[SetUp]
		public void Setup()
		{
			MockSqlQueryHelper = new Mock<ISqlQueryHelper>();
			MockAgentHelper = new Mock<IAgentHelper>();
			MockLogger = new Mock<IAPILog>();
			MockArtifactQueries = new Mock<IArtifactQueries>();
			RsapiApiOptions = new APIOptions();
			MockRsapiRepositoryGroup = new Mock<IRsapiRepositoryGroup>();
			MockSerializationHelper = new Mock<ISerializationHelper>();
			WorkspaceResourceGroupIdList = new List<Int32>
			{
				10000,
				20000
			};
			Sut = new ExportManagerJob(
				agentId: AGENT_ID,
				agentHelper: MockAgentHelper.Object,
				sqlQueryHelper: MockSqlQueryHelper.Object,
				processedOnDateTime: DateTime.Now,
				resourceGroupIds: WorkspaceResourceGroupIdList,
				logger: MockLogger.Object,
				artifactQueries: MockArtifactQueries.Object,
				rsapiApiOptions: RsapiApiOptions,
				rsapiRepositoryGroup: MockRsapiRepositoryGroup.Object,
				serializationHelper: MockSerializationHelper.Object
				);

			MockSqlQueryHelper.Setup(x => x.RetrieveAllExportManagerQueueRecords(It.IsAny<IDBContext>())).Returns(Task.FromResult(new DataTable()));
		}

		[TearDown]
		public void TearDown()
		{
			MockSqlQueryHelper = null;
			MockAgentHelper = null;
			MockLogger = null;
			MockArtifactQueries = null;
			MockRsapiRepositoryGroup = null;
			WorkspaceResourceGroupIdList = null;
			RsapiApiOptions = null;
			MockSerializationHelper = null;
			Sut = null;
		}

		#region " Dynamic Test Case Data "

		public IEnumerable<TestCaseData> TestCasesForGetCommaDelimitedListOfResourceIds
		{
			get
			{
				Setup();
				yield return new TestCaseData(new List<Int32> { 1000001, 1000002, 1000003 }, "1000001,1000002,1000003");
				yield return new TestCaseData(new List<Int32>(), "");
				yield return new TestCaseData(new List<Int32> { 1000001 }, "1000001");
			}
		}

		#endregion " Dynamic Test Case Data "

		#region Tests

		[Description("When no record is picked up by the agent, should not process")]
		[Test]
		public async Task ExecuteAsync_QueueHasNoRecord_DoNotExecute()
		{
			// Arrange
			WhenQueueReturnsNull();
			SetAgentOffHours("00:00:00", "23:59:59");
			Sut.ProcessedOnDateTime = new DateTime(2016, 01, 25, 01, 00, 00);

			// Act
			await Sut.ExecuteAsync();

			// Assert
			AssertRecordWasSkipped();
		}

		[Description("When it's not during configured off-hours, record is not processed")]
		[Test]
		public async Task ExecuteAsync_QueueHasARecord_NotDuringOffHours()
		{
			// Arrange
			WhenQueueReturnsARecord();
			SetAgentOffHours("00:00:00", "02:00:00");
			Sut.ProcessedOnDateTime = new DateTime(2016, 01, 25, 03, 00, 00);

			// Act
			await Sut.ExecuteAsync();

			// Assert
			AssertRecordWasSkipped();
		}

		[TestCaseSource("TestCasesForGetCommaDelimitedListOfResourceIds")]
		[Test]
		[Description("This will test getting a comma delimited list of resource IDs from a list of integers.")]
		public void GetCommaDelimitedListOfResourceIds(IEnumerable<Int32> resourceIdsList, String expectedResult)
		{
			// Arrange
			WhenQueueReturnsARecord();
			SetAgentOffHours("00:00:00", "02:00:00");
			Sut.ProcessedOnDateTime = new DateTime(2016, 01, 25, 03, 00, 00);

			//Act
			String observedResult = Sut.GetCommaDelimitedListOfResourceIds(resourceIdsList);

			//Assert
			Assert.AreEqual(expectedResult, observedResult);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public async Task QueryAllUserAdminObjectsTest_TwoUsers()
		{
			//Arrange
			MockQueryAllUsersAsync_ReturnsTwoUsers();

			//Act
			var userResults = await Sut.QueryAllUserArtifactIDs();

			//Assert
			Assert.That(userResults.TotalCount, Is.EqualTo(2));
			VerifyQueryAllUsersAsyncWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void QueryAllUserAdminObjectsTest_ThrowsException()
		{
			//Arrange
			MockQueryAllUsersAsync_ThrowsException();

			//Act
			//Assert
			AdminMigrationUtilityException adminMigrationUtilityException = Assert.Throws<AdminMigrationUtilityException>(async () => await Sut.QueryAllUserArtifactIDs());
			Assert.That(adminMigrationUtilityException.ToString(), Is.StringContaining(Constant.ErrorMessages.QueryAllUserAdminObjectsError));
			VerifyQueryAllUsersAsyncWasCalled(1);
		}


		#endregion Tests

		#region Mocks

		public void MockQueryAllUsersAsync_ReturnsTwoUsers()
		{
			QueryResultSet<kCura.Relativity.Client.DTOs.User> retVal = new QueryResultSet<kCura.Relativity.Client.DTOs.User>
			{
				TotalCount = 2,
				Success = true,
				Results = new List<Result<kCura.Relativity.Client.DTOs.User>>
				{
					new Result<kCura.Relativity.Client.DTOs.User>() { Success = true, Artifact = RetrieveSampleUserArtifact("firstname1", "lastname1", "email1@email.com") },
					new Result<kCura.Relativity.Client.DTOs.User>() { Success = true, Artifact = RetrieveSampleUserArtifact("firstname2", "lastname2", "email2@email.com") }
				}
			};

			MockArtifactQueries
				.Setup(x => x.QueryAllUsersArtifactIDsAsync(It.IsAny<APIOptions>(), It.IsAny<IGenericRepository<kCura.Relativity.Client.DTOs.User>>()))
				.Returns(Task.FromResult(retVal));
		}

		public void MockQueryAllUsersAsync_ReturnsOneUser()
		{
			List<Result<kCura.Relativity.Client.DTOs.User>> userArtifacts = new List<Result<kCura.Relativity.Client.DTOs.User>>();
			Result<kCura.Relativity.Client.DTOs.User> result1 = new Result<kCura.Relativity.Client.DTOs.User>() { Artifact = RetrieveSampleUserArtifact("firstname1", "lastname1", "email1@email.com") };
			userArtifacts.Add(result1);

			MockArtifactQueries
				.Setup(x => x.QueryAllUsersAsync(It.IsAny<APIOptions>(), It.IsAny<IGenericRepository<kCura.Relativity.Client.DTOs.User>>()))
				.Returns(Task.FromResult<IEnumerable<Result<kCura.Relativity.Client.DTOs.User>>>(userArtifacts));
		}

		public void MockQueryAllUsersAsync_ReturnsZeroUsers()
		{
			List<Result<kCura.Relativity.Client.DTOs.User>> userArtifacts = new List<Result<kCura.Relativity.Client.DTOs.User>>();

			MockArtifactQueries
				.Setup(x => x.QueryAllUsersAsync(It.IsAny<APIOptions>(), It.IsAny<IGenericRepository<kCura.Relativity.Client.DTOs.User>>()))
				.Returns(Task.FromResult<IEnumerable<Result<kCura.Relativity.Client.DTOs.User>>>(userArtifacts));
		}

		public void MockQueryAllUsersAsync_ThrowsException()
		{
			MockArtifactQueries
				.Setup(x => x.QueryAllUsersArtifactIDsAsync(It.IsAny<APIOptions>(), It.IsAny<IGenericRepository<kCura.Relativity.Client.DTOs.User>>()))
				.Throws<AdminMigrationUtilityException>();
		}

		public void MockRetrieveWorkspaceResourcePoolArtifactIdAsync()
		{
			MockArtifactQueries
				.Setup(x => x.RetrieveWorkspaceResourcePoolArtifactIdAsync(
					It.IsAny<APIOptions>(),
					It.IsAny<Int32>(),
					It.IsAny<IGenericRepository<Workspace>>(),
					It.IsAny<Int32>()))
				.Returns(Task.FromResult(123));
		}

		public void MockRetrieveWorkspaceResourcePoolArtifactIdAsync_ThrowsException()
		{
			MockArtifactQueries
				.Setup(x => x.RetrieveWorkspaceResourcePoolArtifactIdAsync(
					It.IsAny<APIOptions>(),
					It.IsAny<Int32>(),
					It.IsAny<IGenericRepository<Workspace>>(),
					It.IsAny<Int32>()))
				.Throws<Exception>();
		}

		public void MockInsertRowIntoExportWorkerQueueTableAsync()
		{
			MockSqlQueryHelper
				.Setup(x => x.InsertRowIntoExportWorkerQueueTableAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<Int32>(),
					It.IsAny<Int32>(),
					It.IsAny<String>(),
					It.IsAny<Int32>(),
					It.IsAny<Int32>(),
					It.IsAny<Int32>(),
					It.IsAny<Int32>(),
					It.IsAny<String>(),
					It.IsAny<String>(),
					It.IsAny<DateTime>()
				))
				.Returns(Task.FromResult(default(Task)));
		}

		public void MockInsertRowIntoExportWorkerQueueTableAsync_ThrowsException()
		{
			MockSqlQueryHelper
				.Setup(x => x.InsertRowIntoExportWorkerQueueTableAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<Int32>(),
					It.IsAny<Int32>(),
					It.IsAny<String>(),
					It.IsAny<Int32>(),
					It.IsAny<Int32>(),
					It.IsAny<Int32>(),
					It.IsAny<Int32>(),
					It.IsAny<String>(),
					It.IsAny<String>(),
					It.IsAny<DateTime>()
					))
				.Throws<Exception>();
		}

		public void MockRetrieveNextInExportManagerQueueAsync_ReturnDataTableWithOneRow()
		{
			DataTable exportManagerQueueDataTable = GetExportManagerQueueDataTableWithSingleDataRow(
				queueRowId: 123,
				agentId: AGENT_ID,
				workspaceArtifactId: 123,
				exportJobArtifactId: 123,
				objectType: Constant.Enums.SupportedObjects.User.ToString(),
				priority: 123,
				workspaceResourceGroupArtifactId: 123,
				queueStatus: Constant.Status.Queue.NOT_STARTED,
				timeStampUtc: DateTime.UtcNow
				);

			MockSqlQueryHelper
				.Setup(x => x.RetrieveNextRowInExportManagerQueueAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<Int32>(),
					It.IsAny<String>()))
				.Returns(Task.FromResult(exportManagerQueueDataTable));
		}

		public void MockUpdateRdoTextFieldValueAsync_UpdatesStatusOfExportJob()
		{
			MockArtifactQueries
				.Setup(x => x.UpdateRdoTextFieldValueAsync(
					It.IsAny<APIOptions>(),
					It.IsAny<Int32>(),
					It.IsAny<IGenericRepository<RDO>>(),
					It.IsAny<Int32>(),
					It.IsAny<Guid>(),
					It.IsAny<Guid>(),
					It.IsAny<String>()))
				.Returns(Task.FromResult(default(Task)));
		}

		public void MockCreateJobErrorRecordAsync_SuccessfullyCreates()
		{
			MockArtifactQueries
				.Setup(x => x.CreateJobErrorRecordAsync(
					It.IsAny<APIOptions>(),
					It.IsAny<Int32>(),
					It.IsAny<IGenericRepository<RDO>>(),
					It.IsAny<Guid>(),
					It.IsAny<Int32>(),
					It.IsAny<String>(),
					It.IsAny<String>(),
					It.IsAny<String>()
				))
				.Returns(Task.FromResult(123));
		}

		#endregion Mocks

		#region Verify

		public void VerifyQueryAllUsersAsyncWasCalled(Int32 timesCalled)
		{
			MockArtifactQueries
				.Verify(x => x.QueryAllUsersArtifactIDsAsync(
					It.IsAny<APIOptions>(),
					It.IsAny<IGenericRepository<kCura.Relativity.Client.DTOs.User>>()
					),
					Times.Exactly(timesCalled));
		}

		public void VerifyInsertRowIntoExportWorkerQueueTableAsyncWasCalled(Int32 timesCalled)
		{
			MockSqlQueryHelper
				.Verify(x => x.InsertRowIntoExportWorkerQueueTableAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<Int32>(),
					It.IsAny<Int32>(),
					It.IsAny<String>(),
					It.IsAny<Int32>(),
					It.IsAny<Int32>(),
					It.IsAny<Int32>(),
					It.IsAny<Int32>(),
					It.IsAny<String>(),
					It.IsAny<String>(),
					It.IsAny<DateTime>()
					),
					Times.Exactly(timesCalled));
		}

		public void VerifyRetrieveWorkspaceResourcePoolArtifactIdWasCalled(Int32 timesCalled)
		{
			MockArtifactQueries
				.Verify(x => x.RetrieveWorkspaceResourcePoolArtifactIdAsync(
					It.IsAny<APIOptions>(),
					It.IsAny<Int32>(),
					It.IsAny<IGenericRepository<Workspace>>(),
					It.IsAny<Int32>()
					),
					Times.Exactly(timesCalled));
		}

		public void VerifyRetrieveNextInExportManagerQueueAsyncWasCalled(Int32 timesCalled)
		{
			MockSqlQueryHelper
				.Verify(x => x.RetrieveNextRowInExportManagerQueueAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<Int32>(),
					It.IsAny<String>()
					),
					Times.Exactly(timesCalled));
		}

		public void VerifyUpdateRdoTextFieldValueAsyncWasCalled(Int32 timesCalled, String status)
		{
			MockArtifactQueries
				.Verify(x => x.UpdateRdoTextFieldValueAsync(
					It.IsAny<APIOptions>(),
					It.IsAny<Int32>(),
					It.IsAny<IGenericRepository<RDO>>(),
					It.IsAny<Int32>(),
					It.IsAny<Guid>(),
					It.IsAny<Guid>(),
					status
					),
					Times.Exactly(timesCalled));
		}

		public void VerifyCreateJobErrorRecordAsyncWasCalled(Int32 timesCalled)
		{
			MockArtifactQueries
				.Verify(x => x.CreateJobErrorRecordAsync(
					It.IsAny<APIOptions>(),
					It.IsAny<Int32>(),
					It.IsAny<IGenericRepository<RDO>>(),
					It.IsAny<Guid>(),
					It.IsAny<Int32>(),
					It.IsAny<String>(),
					It.IsAny<String>(),
					It.IsAny<String>()
				),
				Times.Exactly(timesCalled));
		}

		#endregion Verify

		#region Test Helpers

		private void WhenQueueReturnsNull()
		{
			MockSqlQueryHelper.Setup(x => x.RetrieveNextRowInExportManagerQueueAsync(It.IsAny<IDBContext>(), It.IsAny<Int32>(), It.IsAny<String>())).ReturnsAsync(null);
		}

		private void WhenQueueReturnsARecord()
		{
			DataTable table = GetManagerTable();
			MockSqlQueryHelper.Setup(x => x.RetrieveNextRowInExportManagerQueueAsync(It.IsAny<IDBContext>(), It.IsAny<Int32>(), It.IsAny<String>())).ReturnsAsync(table);
		}

		private void SetAgentOffHours(String offHoursStartTime, String offHoursEndTime)
		{
			// Deletes the completed job from the Manager Queue
			MockSqlQueryHelper.Setup(
				x =>
					x.RemoveRecordFromTableByIdAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<String>(),
					It.IsAny<Int32>()))
					.Returns(Task.FromResult(false))
					.Verifiable(); 

			//Returns off-hours from configuration table
			DataTable offHoursDataTable = GetOffHoursTable(offHoursStartTime, offHoursEndTime);
			MockSqlQueryHelper.Setup(
				x =>
				x.RetrieveOffHoursAsync(
				It.IsAny<IDBContext>()))
				.Returns(Task.FromResult(offHoursDataTable))
				.Verifiable();
		}

		private void AssertRecordWasSkipped()
		{
			// Remove record was never called
			MockSqlQueryHelper.Verify(
				x =>
					x.RemoveRecordFromTableByIdAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<String>(),
					It.IsAny<Int32>()),
					Times.Never);
		}

	
		private DataTable GetManagerTable()
		{
			DataTable table = new DataTable("Test Manager Table");
			table.Columns.Add("WorkspaceArtifactId", typeof(Int32));
			table.Columns.Add("ID", typeof(Int32));
			table.Columns.Add("ArtifactId", typeof(Int32));
			table.Columns.Add("Priority", typeof(Int32));
			table.Columns.Add("ResourceGroupId", typeof(Int32));
			table.Rows.Add(2345678, 1, 3456789, 3, 1000001);
			return table;
		}

		private DataTable GetOffHoursTable(String startTime, String endTime)
		{
			DataTable dataTable = new DataTable();
			dataTable.Columns.Add("AgentOffHourStartTime", typeof(String));
			dataTable.Columns.Add("AgentOffHourEndTime", typeof(String));
			dataTable.Rows.Add(startTime, endTime);
			return dataTable;
		}


		private kCura.Relativity.Client.DTOs.User RetrieveSampleUserArtifact(String firstName, String lastName, String emailAddress)
		{
			kCura.Relativity.Client.DTOs.User userArtifact = new kCura.Relativity.Client.DTOs.User
			{
				ArtifactTypeName = ArtifactTypeNames.User,
				TextIdentifier = $"{firstName} {lastName}",
				FirstName = firstName,
				LastName = lastName,
				EmailAddress = emailAddress,
				Type = new kCura.Relativity.Client.DTOs.Choice { Name = "Type" },
				Client = new Client { Name = "Client" },
				RelativityAccess = true,
				DocumentSkip = new kCura.Relativity.Client.DTOs.Choice { Name = "DocumentSkip" },
				BetaUser = true,
				ChangeSettings = true,
				KeyboardShortcuts = true,
				ItemListPageLength = 12345,
				DefaultSelectedFileType = new kCura.Relativity.Client.DTOs.Choice { Name = "DefaultSelectedFileType" },
				SkipDefaultPreference = new kCura.Relativity.Client.DTOs.Choice { Name = "SkipDefaultPreference" },
				EnforceViewerCompatibility = true,
				AdvancedSearchPublicByDefault = true,
				NativeViewerCacheAhead = true,
				CanChangeDocumentViewer = true,
				ShowFilters = true,
				DocumentViewer = new kCura.Relativity.Client.DTOs.Choice { Name = "DocumentViewer" }
			};

			return userArtifact;
		}

		private DataTable GetExportManagerQueueDataTableWithSingleDataRow(Int32 queueRowId, Int32 agentId, Int32 workspaceArtifactId, Int32 exportJobArtifactId, String objectType, Int32 priority, Int32 workspaceResourceGroupArtifactId, Int32 queueStatus, DateTime timeStampUtc)
		{
			DataTable exportManagerQueueDataTable = new DataTable();
			exportManagerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportManagerQueue.TableRowId, typeof(Int32));
			exportManagerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportManagerQueue.AgentId, typeof(Int32));
			exportManagerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportManagerQueue.WorkspaceArtifactId, typeof(Int32));
			exportManagerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportManagerQueue.ExportJobArtifactId, typeof(Int32));
			exportManagerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportManagerQueue.ObjectType, typeof(String));
			exportManagerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportManagerQueue.Priority, typeof(Int32));
			exportManagerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportManagerQueue.WorkspaceResourceGroupArtifactId, typeof(Int32));
			exportManagerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportManagerQueue.QueueStatus, typeof(Int32));
			exportManagerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportManagerQueue.TimeStampUtc, typeof(DateTime));

			DataRow exportManagerQueueDataRow = exportManagerQueueDataTable.NewRow();
			exportManagerQueueDataRow[Constant.Sql.ColumnsNames.ExportManagerQueue.TableRowId] = queueRowId;
			exportManagerQueueDataRow[Constant.Sql.ColumnsNames.ExportManagerQueue.AgentId] = agentId;
			exportManagerQueueDataRow[Constant.Sql.ColumnsNames.ExportManagerQueue.WorkspaceArtifactId] = workspaceArtifactId;
			exportManagerQueueDataRow[Constant.Sql.ColumnsNames.ExportManagerQueue.ExportJobArtifactId] = exportJobArtifactId;
			exportManagerQueueDataRow[Constant.Sql.ColumnsNames.ExportManagerQueue.ObjectType] = objectType;
			exportManagerQueueDataRow[Constant.Sql.ColumnsNames.ExportManagerQueue.Priority] = priority;
			exportManagerQueueDataRow[Constant.Sql.ColumnsNames.ExportManagerQueue.WorkspaceResourceGroupArtifactId] = workspaceResourceGroupArtifactId;
			exportManagerQueueDataRow[Constant.Sql.ColumnsNames.ExportManagerQueue.QueueStatus] = queueStatus;
			exportManagerQueueDataRow[Constant.Sql.ColumnsNames.ExportManagerQueue.TimeStampUtc] = timeStampUtc;

			exportManagerQueueDataTable.Rows.Add(exportManagerQueueDataRow);
			return exportManagerQueueDataTable;
		}

		#endregion Test Helpers
	}
}