using AdminMigrationUtility.Agents.Export;
using AdminMigrationUtility.Helpers;
using AdminMigrationUtility.Helpers.Kepler;
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
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Security.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Agents.NUnit.Export
{
	[TestFixture]
	public class ExportWorkerJobTests
	{
		public Int32 AgentId;
		public Mock<ISqlQueryHelper> MockSqlQueryHelper;
		public Mock<IAgentHelper> MockAgentHelper;
		public Mock<IAPILog> MockLogger;
		public Mock<IArtifactQueries> MockArtifactQueries;
		public APIOptions RsapiApiOptions;
		public Mock<IRsapiRepositoryGroup> MockRsapiRepositoryGroup;
		public Mock<IAuthenticationHelper> MockAuthenticationHelper;
		public Mock<ISerializationHelper> MockSerializationHelper;
		public const Int32 AgentID = 123;
		public List<Int32> WorkspaceResourceGroupIdList;
		public ExportWorkerJob Sut { get; set; }

		#region Setup and TearDown

		[SetUp]
		public void Setup()
		{
			AgentId = 1234567;
			MockSqlQueryHelper = new Mock<ISqlQueryHelper>();
			MockAgentHelper = new Mock<IAgentHelper>();
			MockLogger = new Mock<IAPILog>();
			MockArtifactQueries = new Mock<IArtifactQueries>();
			RsapiApiOptions = new APIOptions();
			MockRsapiRepositoryGroup = new Mock<IRsapiRepositoryGroup>();
			MockAuthenticationHelper = new Mock<IAuthenticationHelper>();
			MockSerializationHelper = new Mock<ISerializationHelper>();
			WorkspaceResourceGroupIdList = new List<Int32>
			{
				10000,
				20000
			};

			Sut = new ExportWorkerJob(
				agentId: AgentID,
				agentHelper: MockAgentHelper.Object,
				sqlQueryHelper: MockSqlQueryHelper.Object,
				processedOnDateTime: DateTime.Now,
				resourceGroupIds: WorkspaceResourceGroupIdList,
				logger: MockLogger.Object,
				artifactQueries: MockArtifactQueries.Object,
				rsapiApiOptions: RsapiApiOptions,
				rsapiRepositoryGroup: MockRsapiRepositoryGroup.Object,
				authenticationHelper: MockAuthenticationHelper.Object,
				serializationHelper: MockSerializationHelper.Object);
		}

		[TearDown]
		public void TearDown()
		{
			MockSqlQueryHelper = null;
			MockAgentHelper = null;
			MockLogger = null;
			MockArtifactQueries = null;
			RsapiApiOptions = null;
			MockRsapiRepositoryGroup = null;
			MockAuthenticationHelper = null;
			WorkspaceResourceGroupIdList = null;
			MockSerializationHelper = null;
			Sut = null;
		}

		#endregion Setup and TearDown

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

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void ExecuteAsyncTest_AgentNotDuringOffHours()
		{
			//Arrange
			SetAgentOffHours("00:00:00", "02:00:00");
			SetAgentExecutionTime(new DateTime(2016, 01, 25, 03, 00, 00));
			MockResetUnfishedExportWorkerJobsAsync();
			MockRetrieveNextBatchOfRecordsInExportWorkerQueueTableAsync_ReturnDataTableWithZeroRows();
			MockRetrieveRdoTextFieldValueAsync();
			MockUpdateRdoTextFieldValueAsync();
			MockCreateExportWorkerResultsTableIfItDoesNotAlreadyExistsAsync();
			MockDeserializeToAdminObjectAsync();
			MockRetrieveExistingUserLoginProfileAsync();
			MockRetrieveKeywordsAndNotesForUserAsync();
			MockQueryGroupsUserIsPartOfAsync();
			MockQueryGroupsNamesForArtifactIdsAsync();
			MockSerializeAdminObjectAsync();
			MockInsertRowIntoExportWorkerResultsTableAsync();
			MockRemoveBatchFromExportWorkerQueueTableAsync();
			MockDropTableAsync();

			//Act
			//Assert
			//Assert
			Assert.That(async () => await Sut.ExecuteAsync()
			, Throws.Nothing);
			VerifyResetUnfishedExportWorkerJobsAsyncWasCalled(0);
			VerifyRetrieveNextBatchOfRecordsInExportWorkerQueueTableAsyncWasCalled(0);
			VerifyRetrieveRdoTextFieldValueAsyncWasCalled(0);
			VerifyRetrieveRdoTextFieldValueAsyncWasCalled(0);
			VerifyUpdateRdoTextFieldValueAsyncWasCalled(0);
			VerifyCreateExportWorkerResultsTableIfItDoesNotAlreadyExistsAsyncWasCalled(0);
			VerifyRetrieveExistingUserLoginProfileAsyncWasCalled(0);
			VerifyRetrieveKeywordsAndNotesForUserAsyncWasCalled(0);
			VerifyQueryGroupsUserIsPartOfAsyncWasCalled(0);
			VerifyQueryGroupsNamesForArtifactIdsAsyncWasCalled(0);
			VerifySerializeAdminObjectAsyncWasCalled(0);
			VerifyInsertRowIntoExportWorkerResultsTableAsyncWasCalled(0);
			VerifyRemoveBatchFromExportWorkerQueueTableAsyncWasCalled(0);
			VerifyDropTableAsyncWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void ExecuteAsyncTest_WithZeroRecordsInExportWorkerQueueTable()
		{
			//Arrange
			SetAgentOffHours("00:00:00", "02:00:00");
			SetAgentExecutionTime(new DateTime(2016, 01, 25, 01, 00, 00));
			MockResetUnfishedExportWorkerJobsAsync();
			MockRetrieveNextBatchOfRecordsInExportWorkerQueueTableAsync_ReturnDataTableWithZeroRows();
			MockRetrieveRdoTextFieldValueAsync();
			MockUpdateRdoTextFieldValueAsync();
			MockCreateExportWorkerResultsTableIfItDoesNotAlreadyExistsAsync();
			MockDeserializeToAdminObjectAsync();
			MockRetrieveExistingUserLoginProfileAsync();
			MockRetrieveKeywordsAndNotesForUserAsync();
			MockQueryGroupsUserIsPartOfAsync();
			MockQueryGroupsNamesForArtifactIdsAsync();
			MockSerializeAdminObjectAsync();
			MockInsertRowIntoExportWorkerResultsTableAsync();
			MockRemoveBatchFromExportWorkerQueueTableAsync();
			MockDropTableAsync();

			//Act
			//Assert
			//Assert
			Assert.That(async () => await Sut.ExecuteAsync()
			, Throws.Nothing);
			VerifyResetUnfishedExportWorkerJobsAsyncWasCalled(1);
			VerifyRetrieveNextBatchOfRecordsInExportWorkerQueueTableAsyncWasCalled(1);
			VerifyRetrieveRdoTextFieldValueAsyncWasCalled(0);
			VerifyRetrieveRdoTextFieldValueAsyncWasCalled(0);
			VerifyUpdateRdoTextFieldValueAsyncWasCalled(0);
			VerifyCreateExportWorkerResultsTableIfItDoesNotAlreadyExistsAsyncWasCalled(0);
			VerifyRetrieveExistingUserLoginProfileAsyncWasCalled(0);
			VerifyRetrieveKeywordsAndNotesForUserAsyncWasCalled(0);
			VerifyQueryGroupsUserIsPartOfAsyncWasCalled(0);
			VerifyQueryGroupsNamesForArtifactIdsAsyncWasCalled(0);
			VerifySerializeAdminObjectAsyncWasCalled(0);
			VerifyInsertRowIntoExportWorkerResultsTableAsyncWasCalled(0);
			VerifyRemoveBatchFromExportWorkerQueueTableAsyncWasCalled(0);
			VerifyDropTableAsyncWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void ExecuteAsyncTest_WithSingleRecordInExportWorkerQueueTable()
		{
			//Arrange
			SetAgentOffHours("00:00:00", "02:00:00");
			SetAgentExecutionTime(new DateTime(2016, 01, 25, 01, 00, 00));
			MockResetUnfishedExportWorkerJobsAsync();
			MockRetrieveNextBatchOfRecordsInExportWorkerQueueTableAsync_ReturnDataTableWithOneRow();
			MockRetrieveRdoTextFieldValueAsync();
			MockUpdateRdoTextFieldValueAsync();
			MockCreateExportWorkerResultsTableIfItDoesNotAlreadyExistsAsync();
			MockDeserializeToAdminObjectAsync();
			MockRetrieveSingleUserAsync();
			MockRetrieveExistingUserLoginProfileAsync();
			MockRetrieveKeywordsAndNotesForUserAsync();
			MockQueryGroupsUserIsPartOfAsync();
			MockQueryGroupsNamesForArtifactIdsAsync();
			MockSerializeAdminObjectAsync();
			MockInsertRowIntoExportWorkerResultsTableAsync();
			MockRemoveBatchFromExportWorkerQueueTableAsync();
			MockDropTableAsync();

			//Act
			//Assert
			//Assert
			Assert.That(async () => await Sut.ExecuteAsync()
			, Throws.Nothing);
			VerifyResetUnfishedExportWorkerJobsAsyncWasCalled(1);
			VerifyRetrieveNextBatchOfRecordsInExportWorkerQueueTableAsyncWasCalled(1);
			VerifyRetrieveRdoTextFieldValueAsyncWasCalled(1);
			VerifyRetrieveRdoTextFieldValueAsyncWasCalled(1);
			VerifyUpdateRdoTextFieldValueAsyncWasCalled(1);
			VerifyCreateExportWorkerResultsTableIfItDoesNotAlreadyExistsAsyncWasCalled(1);
			VerifyRetrieveExistingUserLoginProfileAsyncWasCalled(1);
			VerifyRetrieveKeywordsAndNotesForUserAsyncWasCalled(1);
			VerifyQueryGroupsNamesForArtifactIdsAsyncWasCalled(1);
			VerifySerializeAdminObjectAsyncWasCalled(1);
			VerifyInsertRowIntoExportWorkerResultsTableAsyncWasCalled(1);
			VerifyRemoveBatchFromExportWorkerQueueTableAsyncWasCalled(1);
			VerifyDropTableAsyncWasCalled(1);
		}

		[Test]
		[Category(Constant.TestCategories.EXPORT_USERS)]
		public void ExecuteAsyncTest_WithTwoRecordsInExportWorkerQueueTable()
		{
			//Arrange
			SetAgentOffHours("00:00:00", "02:00:00");
			SetAgentExecutionTime(new DateTime(2016, 01, 25, 01, 00, 00));
			MockResetUnfishedExportWorkerJobsAsync();
			MockRetrieveNextBatchOfRecordsInExportWorkerQueueTableAsync_ReturnDataTableWithTwoRows();
			MockRetrieveRdoTextFieldValueAsync();
			MockUpdateRdoTextFieldValueAsync();
			MockCreateExportWorkerResultsTableIfItDoesNotAlreadyExistsAsync();
			MockDeserializeToAdminObjectAsync();
			MockRetrieveSingleUserAsync();
			MockRetrieveExistingUserLoginProfileAsync();
			MockRetrieveKeywordsAndNotesForUserAsync();
			MockQueryGroupsUserIsPartOfAsync();
			MockQueryGroupsNamesForArtifactIdsAsync();
			MockSerializeAdminObjectAsync();
			MockInsertRowIntoExportWorkerResultsTableAsync();
			MockRemoveBatchFromExportWorkerQueueTableAsync();
			MockDropTableAsync();

			//Act
			//Assert
			Assert.That(async () => await Sut.ExecuteAsync()
			, Throws.Nothing);
			VerifyResetUnfishedExportWorkerJobsAsyncWasCalled(1);
			VerifyRetrieveNextBatchOfRecordsInExportWorkerQueueTableAsyncWasCalled(1);
			VerifyRetrieveRdoTextFieldValueAsyncWasCalled(1);
			VerifyRetrieveRdoTextFieldValueAsyncWasCalled(1);
			VerifyUpdateRdoTextFieldValueAsyncWasCalled(1);
			VerifyCreateExportWorkerResultsTableIfItDoesNotAlreadyExistsAsyncWasCalled(1);
			VerifyRetrieveExistingUserLoginProfileAsyncWasCalled(2);
			VerifyRetrieveKeywordsAndNotesForUserAsyncWasCalled(2);
			VerifyQueryGroupsNamesForArtifactIdsAsyncWasCalled(2);
			VerifySerializeAdminObjectAsyncWasCalled(2);
			VerifyInsertRowIntoExportWorkerResultsTableAsyncWasCalled(2);
			VerifyRemoveBatchFromExportWorkerQueueTableAsyncWasCalled(1);
			VerifyDropTableAsyncWasCalled(1);
		}

		#endregion Tests

		#region Mocks

		public void MockResetUnfishedExportWorkerJobsAsync()
		{
			MockSqlQueryHelper
				.Setup(x => x.ResetUnfishedExportWorkerJobsAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<Int32>()
				))
				.Returns(Task.FromResult(default(Task)));
		}

		public void MockRetrieveNextBatchOfRecordsInExportWorkerQueueTableAsync_ReturnDataTableWithZeroRows()
		{
			DataTable batchDataTable = GetExportWorkerQueueDataTableWithZeroDataRows();

			MockSqlQueryHelper
				.Setup(x => x.RetrieveNextBatchInExportWorkerQueueAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<Int32>(),
					It.IsAny<Int32>(),
					It.IsAny<String>(),
					It.IsAny<String>()
				))
				.Returns(Task.FromResult(batchDataTable));
		}

		public void MockRetrieveNextBatchOfRecordsInExportWorkerQueueTableAsync_ReturnDataTableWithOneRow()
		{
			DataTable batchDataTable = GetExportWorkerQueueDataTableWithSingleDataRow(
				queueRowId: 123,
				agentId: AgentID,
				workspaceArtifactId: 123,
				exportJobArtifactId: 123,
				objectType: Constant.Enums.SupportedObjects.User.ToString(),
				priority: 123,
				workspaceResourceGroupArtifactId: 123,
				queueStatus: Constant.Status.Queue.NOT_STARTED,
				artifactId: 123,
				resultsTableName: "resultsTableName",
				metaData: "metaData",
				timeStampUtc: DateTime.UtcNow);

			MockSqlQueryHelper
				.Setup(x => x.RetrieveNextBatchInExportWorkerQueueAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<Int32>(),
					It.IsAny<Int32>(),
					It.IsAny<String>(),
					It.IsAny<String>()
				))
				.Returns(Task.FromResult(batchDataTable));
		}

		public void MockRetrieveNextBatchOfRecordsInExportWorkerQueueTableAsync_ReturnDataTableWithTwoRows()
		{
			DataTable batchDataTable = GetExportWorkerQueueDataTableWithTwoDataRows(
				queueRowId: 123,
				agentId: AgentID,
				workspaceArtifactId: 123,
				exportJobArtifactId: 123,
				objectType: Constant.Enums.SupportedObjects.User.ToString(),
				priority: 123,
				workspaceResourceGroupArtifactId: 123,
				queueStatus: Constant.Status.Queue.NOT_STARTED,
				artifactId: 123,
				resultsTableName: "resultsTableName",
				metaData: "metaData",
				timeStampUtc: DateTime.UtcNow);

			MockSqlQueryHelper
				.Setup(x => x.RetrieveNextBatchInExportWorkerQueueAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<Int32>(),
					It.IsAny<Int32>(),
					It.IsAny<String>(),
					It.IsAny<String>()
				))
				.Returns(Task.FromResult(batchDataTable));
		}

		public void MockRetrieveRdoTextFieldValueAsync()
		{
			MockArtifactQueries
				.Setup(x => x.RetrieveRdoTextFieldValueAsync(
					It.IsAny<APIOptions>(),
					It.IsAny<Int32>(),
					It.IsAny<IGenericRepository<RDO>>(),
					It.IsAny<Int32>(),
					It.IsAny<Guid>()
				))
				.Returns(Task.FromResult("exportJobStatus"));
		}

		public void MockUpdateRdoTextFieldValueAsync()
		{
			MockArtifactQueries
				.Setup(x => x.UpdateRdoTextFieldValueAsync(
					It.IsAny<APIOptions>(),
					It.IsAny<Int32>(),
					It.IsAny<IGenericRepository<RDO>>(),
					It.IsAny<Int32>(),
					It.IsAny<Guid>(),
					It.IsAny<Guid>(),
					It.IsAny<String>()
				))
				.Returns(Task.FromResult(default(Task)));
		}

		public void MockCreateExportWorkerResultsTableIfItDoesNotAlreadyExistsAsync()
		{
			MockSqlQueryHelper
				.Setup(x => x.CreateExportWorkerResultsTableIfItDoesNotAlreadyExistsAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<String>()
				))
				.Returns(Task.FromResult(default(Task)));
		}

		public void MockDeserializeToAdminObjectAsync()
		{
			IAdminObject userAdminObject = new UserAdminObject();

			MockSerializationHelper
				.Setup(x => x.DeserializeToAdminObjectAsync(
					It.IsAny<String>()
				))
				.Returns(Task.FromResult(userAdminObject));
		}

		public void MockRetrieveSingleUserAsync()
		{
			MockArtifactQueries
				.Setup(x => x.RetrieveUserAsync(
					It.IsAny<APIOptions>(), It.IsAny<IGenericRepository<kCura.Relativity.Client.DTOs.User>>(), It.IsAny<Int32>()
				))
				.Returns(Task.FromResult(RetrieveSampleUserArtifact("Test", "user", "tuser123@test.com")));
		}

		public void MockRetrieveExistingUserLoginProfileAsync()
		{
			LoginProfile loginProfile = GetLoginProfile();

			MockAuthenticationHelper
				.Setup(x => x.RetrieveExistingUserLoginProfileAsync(
					It.IsAny<Int32>()
				))
				.Returns(Task.FromResult(loginProfile));
		}

		public void MockRetrieveKeywordsAndNotesForUserAsync()
		{
			KeywordsNotesModel userKeywordsNotesModel = new KeywordsNotesModel();

			MockSqlQueryHelper
				.Setup(x => x.RetrieveKeywordsAndNotesForUserAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<Int32>()
				))
				.Returns(Task.FromResult(userKeywordsNotesModel));
		}

		public void MockQueryGroupsUserIsPartOfAsync()
		{
			MockArtifactQueries
				.Setup(x => x.QueryGroupsNamesForArtifactIdsAsync(
					It.IsAny<APIOptions>(),
					It.IsAny<Int32>(),
					It.IsAny<IGenericRepository<Group>>(),
					It.IsAny<List<Int32>>()
				))
				.Returns(Task.FromResult<IEnumerable<String>>(new[] { "Group1", "Group2" }));
		}

		public void MockQueryGroupsNamesForArtifactIdsAsync()
		{
			List<String> groupNameList = new List<String>();

			MockArtifactQueries
				.Setup(x => x.QueryGroupsNamesForArtifactIdsAsync(
					It.IsAny<APIOptions>(),
					It.IsAny<Int32>(),
					It.IsAny<IGenericRepository<Group>>(),
					It.IsAny<List<Int32>>()
				))
				.Returns(Task.FromResult<IEnumerable<String>>(groupNameList));
		}

		public void MockSerializeAdminObjectAsync()
		{
			MockSerializationHelper
				.Setup(x => x.SerializeAdminObjectAsync(
					It.IsAny<IAdminObject>()
				))
				.Returns(Task.FromResult("metadata"));
		}

		public void MockInsertRowIntoExportWorkerResultsTableAsync()
		{
			MockSqlQueryHelper
				.Setup(x => x.InsertRowIntoExportWorkerResultsTableAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<String>(),
					It.IsAny<Int32>(),
					It.IsAny<Int32>(),
					It.IsAny<String>(),
					It.IsAny<Int32>(),
					It.IsAny<Int32>(),
					It.IsAny<String>(),
					It.IsAny<DateTime>()
				))
				.Returns(Task.FromResult(default(Task)));
		}

		public void MockRemoveBatchFromExportWorkerQueueTableAsync()
		{
			MockSqlQueryHelper
				.Setup(x => x.RemoveBatchFromExportWorkerQueueTableAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<String>()
				))
				.Returns(Task.FromResult(default(Task)));
		}

		public void MockDropTableAsync()
		{
			MockSqlQueryHelper
				.Setup(x => x.DropTableAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<String>()
				))
				.Returns(Task.FromResult(default(Task)));
		}

		#endregion Mocks

		#region Verify

		public void VerifyResetUnfishedExportWorkerJobsAsyncWasCalled(Int32 timesCalled)
		{
			MockSqlQueryHelper
				.Verify(x => x.ResetUnfishedExportWorkerJobsAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<Int32>()),
					Times.Exactly(timesCalled));
		}

		public void VerifyRetrieveNextBatchOfRecordsInExportWorkerQueueTableAsyncWasCalled(Int32 timesCalled)
		{
			MockSqlQueryHelper
				.Verify(x => x.RetrieveNextBatchInExportWorkerQueueAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<Int32>(),
					It.IsAny<Int32>(),
					It.IsAny<String>(),
					It.IsAny<String>()
				),
			Times.Exactly(timesCalled));
		}

		public void VerifyRetrieveRdoTextFieldValueAsyncWasCalled(Int32 timesCalled)
		{
			MockArtifactQueries
				.Verify(x => x.RetrieveRdoTextFieldValueAsync(
					It.IsAny<APIOptions>(),
					It.IsAny<Int32>(),
					It.IsAny<IGenericRepository<RDO>>(),
					It.IsAny<Int32>(),
					It.IsAny<Guid>()
				),
				Times.Exactly(timesCalled));
		}

		public void VerifyUpdateRdoTextFieldValueAsyncWasCalled(Int32 timesCalled)
		{
			MockArtifactQueries
				.Verify(x => x.UpdateRdoTextFieldValueAsync(
					It.IsAny<APIOptions>(),
					It.IsAny<Int32>(),
					It.IsAny<IGenericRepository<RDO>>(),
					It.IsAny<Int32>(),
					It.IsAny<Guid>(),
					It.IsAny<Guid>(),
					It.IsAny<String>()
				),
				Times.Exactly(timesCalled));
		}

		public void VerifyCreateExportWorkerResultsTableIfItDoesNotAlreadyExistsAsyncWasCalled(Int32 timesCalled)
		{
			MockSqlQueryHelper
				.Verify(x => x.CreateExportWorkerResultsTableIfItDoesNotAlreadyExistsAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<String>()
				),
				Times.Exactly(timesCalled));
		}

		public void VerifyRetrieveExistingUserLoginProfileAsyncWasCalled(Int32 timesCalled)
		{
			MockAuthenticationHelper
				.Verify(x => x.RetrieveExistingUserLoginProfileAsync(
					It.IsAny<Int32>()
				),
				Times.Exactly(timesCalled));
		}

		public void VerifyRetrieveKeywordsAndNotesForUserAsyncWasCalled(Int32 timesCalled)
		{
			MockSqlQueryHelper
				.Verify(x => x.RetrieveKeywordsAndNotesForUserAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<Int32>()
				),
				Times.Exactly(timesCalled));
		}

		public void VerifyQueryGroupsUserIsPartOfAsyncWasCalled(Int32 timesCalled)
		{
			MockArtifactQueries
				.Verify(x => x.QueryGroupArtifactIdsUserIsPartOfAsync(
					It.IsAny<APIOptions>(),
					It.IsAny<Int32>(),
					It.IsAny<IGenericRepository<kCura.Relativity.Client.DTOs.User>>(),
					It.IsAny<Int32>()
				),
				Times.Exactly(timesCalled));
		}

		public void VerifyQueryGroupsNamesForArtifactIdsAsyncWasCalled(Int32 timesCalled)
		{
			MockArtifactQueries
				.Verify(x => x.QueryGroupsNamesForArtifactIdsAsync(
					It.IsAny<APIOptions>(),
					It.IsAny<Int32>(),
					It.IsAny<IGenericRepository<Group>>(),
					It.IsAny<List<Int32>>()
				),
				Times.Exactly(timesCalled));
		}

		public void VerifySerializeAdminObjectAsyncWasCalled(Int32 timesCalled)
		{
			MockSerializationHelper
				.Verify(x => x.SerializeAdminObjectAsync(
					It.IsAny<IAdminObject>()
				),
				Times.Exactly(timesCalled));
		}

		public void VerifyInsertRowIntoExportWorkerResultsTableAsyncWasCalled(Int32 timesCalled)
		{
			MockSqlQueryHelper
				.Verify(x => x.InsertRowIntoExportWorkerResultsTableAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<String>(),
					It.IsAny<Int32>(),
					It.IsAny<Int32>(),
					It.IsAny<String>(),
					It.IsAny<Int32>(),
					It.IsAny<Int32>(),
					It.IsAny<String>(),
					It.IsAny<DateTime>()
				),
				Times.Exactly(timesCalled));
		}

		public void VerifyRemoveBatchFromExportWorkerQueueTableAsyncWasCalled(Int32 timesCalled)
		{
			MockSqlQueryHelper
				.Verify(x => x.RemoveBatchFromExportWorkerQueueTableAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<String>()
				),
				Times.Exactly(timesCalled));
		}

		public void VerifyDropTableAsyncWasCalled(Int32 timesCalled)
		{
			MockSqlQueryHelper
				.Verify(x => x.DropTableAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<String>()
				),
				Times.Exactly(timesCalled));
		}

		#endregion Verify

		#region Test Helpers

		private DataTable GetOffHoursTable(String startTime, String endTime)
		{
			DataTable dataTable = new DataTable();
			dataTable.Columns.Add("AgentOffHourStartTime", typeof(String));
			dataTable.Columns.Add("AgentOffHourEndTime", typeof(String));
			dataTable.Rows.Add(startTime, endTime);
			return dataTable;
		}

		private void SetAgentOffHours(String offHoursStartTime, String offHoursEndTime)
		{
			//Returns off-hours from configuration table
			DataTable offHoursDataTable = GetOffHoursTable(offHoursStartTime, offHoursEndTime);

			MockSqlQueryHelper
				.Setup(x => x.RetrieveOffHoursAsync(It.IsAny<IDBContext>()))
				.Returns(Task.FromResult(offHoursDataTable));
		}

		private void SetAgentExecutionTime(DateTime agentExecutionTime)
		{
			Sut.ProcessedOnDateTime = agentExecutionTime;
		}

		private DataTable GetExportWorkerQueueDataTable()
		{
			DataTable exportWorkerQueueDataTable = new DataTable();
			exportWorkerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.TableRowId, typeof(Int32));
			exportWorkerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.AgentId, typeof(Int32));
			exportWorkerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.WorkspaceArtifactId, typeof(Int32));
			exportWorkerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.ExportJobArtifactId, typeof(Int32));
			exportWorkerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.ObjectType, typeof(String));
			exportWorkerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.Priority, typeof(Int32));
			exportWorkerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.WorkspaceResourceGroupArtifactId, typeof(Int32));
			exportWorkerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.QueueStatus, typeof(Int32));
			exportWorkerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.ArtifactId, typeof(Int32));
			exportWorkerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.ResultsTableName, typeof(String));
			exportWorkerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.MetaData, typeof(String));
			exportWorkerQueueDataTable.Columns.Add(Constant.Sql.ColumnsNames.ExportWorkerQueue.TimeStampUtc, typeof(DateTime));
			return exportWorkerQueueDataTable;
		}

		private DataRow GetSingleExportWorkerQueueDataRow(Int32 queueRowId, Int32 agentId, Int32 workspaceArtifactId, Int32 exportJobArtifactId, String objectType, Int32 priority, Int32 workspaceResourceGroupArtifactId, Int32 queueStatus, Int32 artifactId, String resultsTableName, String metaData, DateTime timeStampUtc, DataTable exportWorkerQueueDataTable)
		{
			DataRow exportWorkerQueueDataRow = exportWorkerQueueDataTable.NewRow();
			exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.TableRowId] = queueRowId;
			exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.AgentId] = agentId;
			exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.WorkspaceArtifactId] = workspaceArtifactId;
			exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.ExportJobArtifactId] = exportJobArtifactId;
			exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.ObjectType] = objectType;
			exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.Priority] = priority;
			exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.WorkspaceResourceGroupArtifactId] = workspaceResourceGroupArtifactId;
			exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.QueueStatus] = queueStatus;
			exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.ArtifactId] = artifactId;
			exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.ResultsTableName] = resultsTableName;
			exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.MetaData] = metaData;
			exportWorkerQueueDataRow[Constant.Sql.ColumnsNames.ExportWorkerQueue.TimeStampUtc] = timeStampUtc;
			return exportWorkerQueueDataRow;
		}

		private DataTable GetExportWorkerQueueDataTableWithZeroDataRows()
		{
			DataTable exportWorkerQueueDataTable = GetExportWorkerQueueDataTable();
			return exportWorkerQueueDataTable;
		}

		private DataTable GetExportWorkerQueueDataTableWithSingleDataRow(Int32 queueRowId, Int32 agentId, Int32 workspaceArtifactId, Int32 exportJobArtifactId, String objectType, Int32 priority, Int32 workspaceResourceGroupArtifactId, Int32 queueStatus, Int32 artifactId, String resultsTableName, String metaData, DateTime timeStampUtc)
		{
			DataTable exportWorkerQueueDataTable = GetExportWorkerQueueDataTable();

			DataRow exportWorkerQueueDataRow = GetSingleExportWorkerQueueDataRow(
				queueRowId: queueRowId,
				agentId: agentId,
				workspaceArtifactId: workspaceArtifactId,
				exportJobArtifactId: exportJobArtifactId,
				objectType: objectType,
				priority: priority,
				workspaceResourceGroupArtifactId: workspaceResourceGroupArtifactId,
				queueStatus: queueStatus,
				artifactId: artifactId,
				resultsTableName: resultsTableName,
				metaData: metaData,
				timeStampUtc: timeStampUtc,
				exportWorkerQueueDataTable: exportWorkerQueueDataTable);

			exportWorkerQueueDataTable.Rows.Add(exportWorkerQueueDataRow);
			return exportWorkerQueueDataTable;
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
				Groups = new FieldValueList<Group>() { new Group(123), new Group(456) },
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

		private DataTable GetExportWorkerQueueDataTableWithTwoDataRows(Int32 queueRowId, Int32 agentId, Int32 workspaceArtifactId, Int32 exportJobArtifactId, String objectType, Int32 priority, Int32 workspaceResourceGroupArtifactId, Int32 queueStatus, Int32 artifactId, String resultsTableName, String metaData, DateTime timeStampUtc)
		{
			DataTable exportWorkerQueueDataTable = GetExportWorkerQueueDataTable();

			DataRow exportWorkerQueueDataRow1 = GetSingleExportWorkerQueueDataRow(
				queueRowId: queueRowId,
				agentId: agentId,
				workspaceArtifactId: workspaceArtifactId,
				exportJobArtifactId: exportJobArtifactId,
				objectType: objectType,
				priority: priority,
				workspaceResourceGroupArtifactId: workspaceResourceGroupArtifactId,
				queueStatus: queueStatus,
				artifactId: artifactId,
				resultsTableName: resultsTableName,
				metaData: metaData,
				timeStampUtc: timeStampUtc,
				exportWorkerQueueDataTable: exportWorkerQueueDataTable);

			DataRow exportWorkerQueueDataRow2 = GetSingleExportWorkerQueueDataRow(
				queueRowId: queueRowId,
				agentId: agentId,
				workspaceArtifactId: workspaceArtifactId,
				exportJobArtifactId: exportJobArtifactId,
				objectType: objectType,
				priority: priority,
				workspaceResourceGroupArtifactId: workspaceResourceGroupArtifactId,
				queueStatus: queueStatus,
				artifactId: artifactId,
				resultsTableName: resultsTableName,
				metaData: metaData,
				timeStampUtc: timeStampUtc,
				exportWorkerQueueDataTable: exportWorkerQueueDataTable);

			exportWorkerQueueDataTable.Rows.Add(exportWorkerQueueDataRow1);
			exportWorkerQueueDataTable.Rows.Add(exportWorkerQueueDataRow2);
			return exportWorkerQueueDataTable;
		}

		private LoginProfile GetLoginProfile()
		{
			LoginProfile loginProfile = new LoginProfile
			{
				IntegratedAuthentication = new IntegratedAuthenticationMethod
				{
					IsEnabled = true,
					Account = "account"
				},
				Password = new PasswordMethod
				{
					MustResetPasswordOnNextLogin = true,
					UserCanChangePassword = true,
					PasswordExpirationInDays = 1,
					TwoFactorMode = TwoFactorMode.Always,
					TwoFactorInfo = "twoFactorInfo"
				}
			};
			return loginProfile;
		}

		#endregion Test Helpers
	}
}