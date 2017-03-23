using AdminMigrationUtility.EventHandlers.Import;
using AdminMigrationUtility.Helpers;
using AdminMigrationUtility.Helpers.Rsapi.Interfaces;
using AdminMigrationUtility.Helpers.SQL;
using Moq;
using NUnit.Framework;
using Relativity.API;
using System;
using System.Data;
using System.Threading.Tasks;

namespace AdminMigrationUtility.EventHandlers.NUnit.Import
{
	[TestFixture]
	public class ImportConsoleJobTests
	{
		public Mock<IEHHelper> MockEhHelper { get; set; }
		public Mock<ISqlQueryHelper> MockSqlQueryHelper { get; set; }
		public Mock<IArtifactQueries> MockArtifactQueries { get; set; }
		public Mock<IAPILog> MockLogger { get; set; }
		public Mock<IServicesMgr> MockSvcManager { get; set; }
		public Mock<IDBContext> MockDbContextEdds { get; set; }
		public Mock<IDBContext> MockDbContext { get; set; }
		public Mock<ImportConsoleJob> MockImportUtilityJob { get; set; }

		[SetUp]
		public void SetUp()
		{
			MockEhHelper = new Mock<IEHHelper>();
			MockSqlQueryHelper = new Mock<ISqlQueryHelper>();
			MockArtifactQueries = new Mock<IArtifactQueries>();
			MockLogger = new Mock<IAPILog>();
			MockSvcManager = new Mock<IServicesMgr>();
			MockImportUtilityJob = new Mock<ImportConsoleJob>();
			MockDbContextEdds = new Mock<IDBContext>();
			MockDbContext = new Mock<IDBContext>();
			AddMockLogger();
		}

		#region Tests

		[Test]
		[Description("This will test when no records exist in the queue and the Submit button is clicked.")]
		public void ExecuteAsync_NoRecordExists_SubmitClicked()
		{
			// Arrange
			AddMockEhHelperGetActiveCaseId();
			AddMockSvcManager();
			AddMockDbContext();
			AddMockDbContextEdds();
			AddMockGetAuthenticationManager();
			MockRetrieveSingleInImportManagerQueueByArtifactIdAsync_ReturnsNoDataRows();
			var importUtilityJob = GetImportUtilityJob(Constant.Buttons.SUBMIT);

			// Act

			// Assert
			Assert.DoesNotThrow(async () => await importUtilityJob.ExecuteAsync());
			VerifyLoggerWasCalled(1);
			VerifyGetResourcePoolArtifactIdWasCalled(1);
			VerifyInsertRowIntoImportManagerQueueAsyncWasCalled(1);
			VerifyUpdateStatusAsyncWasCalled(1);
			VerifyRetrieveSingleInImportManagerQueueWasCalled(1);
		}

		[Test]
		[Description("This will test when a record exists in the queue and the Submit button is clicked.")]
		public void ExecuteAsync_RecordExists_SubmitClicked()
		{
			// Arrange
			AddMockEhHelperGetActiveCaseId();
			AddMockSvcManager();
			AddMockDbContext();
			AddMockDbContextEdds();
			AddMockGetAuthenticationManager();
			MockRetrieveSingleInImportManagerQueueByArtifactIdAsync_ReturnsDataRow();
			var importUtilityJob = GetImportUtilityJob(Constant.Buttons.SUBMIT);

			// Act

			// Assert
			Assert.DoesNotThrow(async () => await importUtilityJob.ExecuteAsync());
			VerifyLoggerWasCalled(1);
			VerifyGetResourcePoolArtifactIdWasCalled(0);
			VerifyInsertRowIntoImportManagerQueueAsyncWasCalled(0);
			VerifyUpdateStatusAsyncWasCalled(0);
			VerifyRetrieveSingleInImportManagerQueueWasCalled(1);
		}

		[Test]
		[Description("This will test when a record exists in the queue and the Cancel button is clicked.")]
		public void ExecuteAsync_RecordExists_CancelClicked()
		{
			// Arrange
			AddMockEhHelperGetActiveCaseId();
			AddMockSvcManager();
			AddMockDbContext();
			AddMockDbContextEdds();
			AddMockGetAuthenticationManager();
			MockRetrieveSingleInImportManagerQueueByArtifactIdAsync_ReturnsDataRow();
			var importUtilityJob = GetImportUtilityJob(Constant.Buttons.CANCEL);

			// Act

			// Assert
			Assert.DoesNotThrow(async () => await importUtilityJob.ExecuteAsync());
			VerifyLoggerWasCalled(1);
			VerifyUpdateStatusAsyncWasCalled(1);
			VerifyUpdateQueueStatusAsyncWasCalled(1);
			VerifyRetrieveSingleInImportManagerQueueWasCalled(1);
		}

		[Test]
		[Description("This will test when no records exist in the queue and the Validate button is clicked.")]
		public void ExecuteAsync_NoRecordExists_ValidateClicked()
		{
			// Arrange
			AddMockEhHelperGetActiveCaseId();
			AddMockSvcManager();
			AddMockDbContext();
			AddMockDbContextEdds();
			AddMockGetAuthenticationManager();
			MockRetrieveSingleInImportManagerQueueByArtifactIdAsync_ReturnsNoDataRows();
			var importUtilityJob = GetImportUtilityJob(Constant.Buttons.VALIDATE);

			// Act

			// Assert
			Assert.DoesNotThrow(async () => await importUtilityJob.ExecuteAsync());
			VerifyLoggerWasCalled(1);
			VerifyGetResourcePoolArtifactIdWasCalled(1);
			VerifyInsertRowIntoImportManagerQueueAsyncWasCalled(1);
			VerifyUpdateStatusAsyncWasCalled(1);
			VerifyRetrieveSingleInImportManagerQueueWasCalled(1);
		}

		[Test]
		[Description("This will test when a record exists in the queue and the Validate button is clicked.")]
		public void ExecuteAsync_RecordExists_ValidateClicked()
		{
			// Arrange
			AddMockEhHelperGetActiveCaseId();
			AddMockSvcManager();
			AddMockDbContext();
			AddMockDbContextEdds();
			AddMockGetAuthenticationManager();
			MockRetrieveSingleInImportManagerQueueByArtifactIdAsync_ReturnsDataRow();
			var importUtilityJob = GetImportUtilityJob(Constant.Buttons.VALIDATE);

			// Act

			// Assert
			Assert.DoesNotThrow(async () => await importUtilityJob.ExecuteAsync());
			VerifyLoggerWasCalled(1);
			VerifyGetResourcePoolArtifactIdWasCalled(0);
			VerifyInsertRowIntoImportManagerQueueAsyncWasCalled(0);
			VerifyUpdateStatusAsyncWasCalled(0);
			VerifyRetrieveSingleInImportManagerQueueWasCalled(1);
		}

		#endregion Tests

		#region Mocks

		private void AddMockLogger()
		{
			var mockILogFactory = new Mock<ILogFactory>();
			var mockIapiLog = new Mock<IAPILog>();

			MockEhHelper.Setup(
				x =>
				x.GetLoggerFactory())
					.Returns(mockILogFactory.Object)
					.Verifiable();

			mockILogFactory.Setup(
				x =>
				x.GetLogger())
					.Returns(mockIapiLog.Object)
					.Verifiable();

			mockIapiLog.Setup(
			x =>
			x.LogDebug(It.IsAny<string>()))
				.Verifiable();
		}

		private void AddMockSvcManager()
		{
			MockEhHelper.Setup(
				x =>
				x.GetServicesManager())
					.Returns(It.IsAny<IServicesMgr>())
					.Verifiable();
		}

		private void AddMockDbContext()
		{
			MockEhHelper.Setup(
				x =>
				x.GetDBContext(-1))
					.Returns(It.IsAny<IDBContext>())
					.Verifiable();
		}

		private void AddMockDbContextEdds()
		{
			MockEhHelper.Setup(
				x =>
				x.GetDBContext(-1))
					.Returns(It.IsAny<IDBContext>())
					.Verifiable();
		}

		private void AddMockEhHelperGetActiveCaseId()
		{
			MockEhHelper.Setup(
				x =>
				x.GetActiveCaseID())
					.Returns(It.IsAny<int>())
					.Verifiable();
		}

		private void AddMockGetAuthenticationManager()
		{
			var mockIAuthenticationMgr = new Mock<IAuthenticationMgr>();

			MockEhHelper.Setup(
				x =>
				x.GetAuthenticationManager())
					.Returns(It.IsAny<IAuthenticationMgr>())
					.Verifiable();

			mockIAuthenticationMgr.Setup(
				x =>
				x.UserInfo.ArtifactID)
					.Returns(It.IsAny<int>())
					.Verifiable();
		}

		public void MockRetrieveSingleInImportManagerQueueByArtifactIdAsync_ReturnsDataRow()
		{
			DataTable dtQueue = new DataTable();
			dtQueue.Columns.Add(new DataColumn("ID", typeof(Int32)));
			dtQueue.Columns.Add(new DataColumn("AgentID", typeof(Int32)));
			dtQueue.Columns.Add(new DataColumn("JobID", typeof(Int32)));
			dtQueue.Columns.Add(new DataColumn("WorkspaceArtifactID", typeof(Int32)));
			dtQueue.Columns.Add(new DataColumn("ObjectType", typeof(string)));
			dtQueue.Columns.Add(new DataColumn("QueueStatus", typeof(Int32)));
			dtQueue.Columns.Add(new DataColumn("ResultTableName", typeof(string)));
			dtQueue.Columns.Add(new DataColumn("Priority", typeof(Int32)));
			dtQueue.Columns.Add(new DataColumn("ResourceGroupID", typeof(Int32)));
			dtQueue.Columns.Add(new DataColumn("CreatedBy", typeof(Int32)));
			dtQueue.Columns.Add(new DataColumn("TimeStampUTC", typeof(DateTime)));
			dtQueue.Rows.Add(1, 2, 3, 4, "User", 0, "tempTableName", 1, 5, 6, DateTime.Now);

			MockSqlQueryHelper
				.Setup(x => x.RetrieveSingleInImportManagerQueueByArtifactIdAsync(MockDbContextEdds.Object, It.IsAny<int>(), It.IsAny<int>()))
				.Returns(Task.FromResult(dtQueue.Rows[0]));
		}

		public void MockRetrieveSingleInImportManagerQueueByArtifactIdAsync_ReturnsNoDataRows()
		{
			MockSqlQueryHelper
				.Setup(x => x.RetrieveSingleInImportManagerQueueByArtifactIdAsync(MockDbContextEdds.Object, It.IsAny<int>(), It.IsAny<int>()))
				.Returns(Task.FromResult<DataRow>(null));
		}

		#endregion Mocks

		#region Helper Methods

		private ImportConsoleJob GetImportUtilityJob(string buttonName)
		{
			var importUtilityJob = new ImportConsoleJob(
					MockSvcManager.Object,
					MockDbContextEdds.Object,
					MockDbContext.Object,
					ExecutionIdentity.CurrentUser,
					ExecutionIdentity.System,
					MockArtifactQueries.Object,
					MockSqlQueryHelper.Object,
					MockLogger.Object,
					1234567,
					2345678,
					3456789,
					buttonName,
					Constant.Enums.SupportedObjects.User.ToString(),
					$"AnyTableName_{Guid.NewGuid()}");
			return importUtilityJob;
		}

		public void VerifyLoggerWasCalled(Int32 timesCalled)
		{
			MockLogger.Verify(x => x.LogDebug(It.IsAny<string>()), Times.Exactly(timesCalled));
		}

		public void VerifyGetResourcePoolArtifactIdWasCalled(Int32 timesCalled)
		{
			MockArtifactQueries.Verify(x => x.GetResourcePoolArtifactIdAsync(MockSvcManager.Object, ExecutionIdentity.System, It.IsAny<int>()), Times.Exactly(timesCalled));
		}

		public void VerifyInsertRowIntoImportManagerQueueAsyncWasCalled(Int32 timesCalled)
		{
			MockSqlQueryHelper.Verify(x => x.InsertRowIntoImportManagerQueueAsync(MockDbContextEdds.Object, It.IsAny<int>(), 1, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()), Times.Exactly(timesCalled));
		}

		public void VerifyUpdateStatusAsyncWasCalled(Int32 timesCalled)
		{
			MockArtifactQueries.Verify(x => x.UpdateRdoJobTextFieldAsync(MockSvcManager.Object, It.IsAny<int>(), ExecutionIdentity.CurrentUser, It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<string>()), Times.Exactly(timesCalled));
		}

		public void VerifyUpdateQueueStatusAsyncWasCalled(Int32 timesCalled)
		{
			MockSqlQueryHelper.Verify(x => x.UpdateStatusInImportManagerQueueByWorkspaceJobIdAsync(MockDbContextEdds.Object, It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(timesCalled));
		}

		public void VerifyRetrieveSingleInImportManagerQueueWasCalled(Int32 timesCalled)
		{
			MockSqlQueryHelper.Verify(x => x.RetrieveSingleInImportManagerQueueByArtifactIdAsync(MockDbContextEdds.Object, It.IsAny<int>(), It.IsAny<int>()), Times.Exactly(timesCalled));
		}

		#endregion Helper Methods
	}
}