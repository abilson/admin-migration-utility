using AdminMigrationUtility.EventHandlers.Application;
using AdminMigrationUtility.Helpers.SQL;
using Moq;
using NUnit.Framework;
using Relativity.API;
using System;
using System.Data;
using System.Threading.Tasks;

namespace AdminMigrationUtility.EventHandlers.NUnit.Application
{
	[TestFixture]
	public class PreInstallValidationJobTests
	{
		public Mock<IEHHelper> MockEhHelper { get; set; }
		public Mock<IDBContext> MockDbContextEdds { get; set; }
		public Mock<ISqlQueryHelper> MockSqlQueryHelper { get; set; }
		public Mock<IAPILog> MockLogger { get; set; }
		public Mock<PreInstallValidationJob> MockPreInstallJob { get; set; }
		public int WorkspaceArtifactId { get; set; }

		[SetUp]
		public void SetUp()
		{
			MockEhHelper = new Mock<IEHHelper>();
			MockSqlQueryHelper = new Mock<ISqlQueryHelper>();
			MockLogger = new Mock<IAPILog>();
			MockPreInstallJob = new Mock<PreInstallValidationJob>();
			MockDbContextEdds = new Mock<IDBContext>();
			AddMockLogger();
		}

		#region Tests

		[Test]
		[Description("This will test when the application has not been installed in any Workspace in the environment.")]
		public void ExecuteAsync_NotInstalledInEnvironment()
		{
			// Arrange
			WorkspaceArtifactId = 1000123;
			AddMockEhHelperGetActiveCaseId();
			AddMockSvcManager();
			AddMockDbContext();
			AddMockGetAuthenticationManager();
			MockRetrieveWorkspacesWhereAppIsInstalledAsync(false, null);
			var preInstallJob = GetPreInstallJob();

			// Act

			// Assert
			Assert.DoesNotThrow(async () => await preInstallJob.ExecuteAsync());
			VerifyRetrieveWorkspacesWhereAppIsInstalledWasCalled(1);
			VerifyLoggerWithoutObjectWasCalled(1);
			VerifyLoggerWithObjectWasCalled(3);
		}

		[Test]
		[Description("This will test when the application has already been installed in the environment, in a different Workspace.")]
		public void ExecuteAsync_AlreadyInstalledInEnvironment_DifferentWorkspace()
		{
			// Arrange
			WorkspaceArtifactId = 1000123;
			AddMockEhHelperGetActiveCaseId();
			AddMockSvcManager();
			AddMockDbContext();
			AddMockGetAuthenticationManager();
			MockRetrieveWorkspacesWhereAppIsInstalledAsync(true, WorkspaceArtifactId + 1);
			var preInstallJob = GetPreInstallJob();

			// Act

			// Assert
			Assert.DoesNotThrow(async () => await preInstallJob.ExecuteAsync());
			VerifyRetrieveWorkspacesWhereAppIsInstalledWasCalled(1);
			VerifyLoggerWithoutObjectWasCalled(1);
			VerifyLoggerWithObjectWasCalled(3);
		}

		[Test]
		[Description("This will test when the application has already been installed in the environment, in the same Workspace.")]
		public void ExecuteAsync_AlreadyInstalledInEnvironment_SameWorkspace()
		{
			// Arrange
			WorkspaceArtifactId = 1000123;
			AddMockEhHelperGetActiveCaseId();
			AddMockSvcManager();
			AddMockDbContext();
			AddMockGetAuthenticationManager();
			MockRetrieveWorkspacesWhereAppIsInstalledAsync(true, WorkspaceArtifactId);
			var preInstallJob = GetPreInstallJob();

			// Act

			// Assert
			Assert.DoesNotThrow(async () => await preInstallJob.ExecuteAsync());
			VerifyRetrieveWorkspacesWhereAppIsInstalledWasCalled(1);
			VerifyLoggerWithoutObjectWasCalled(1);
			VerifyLoggerWithObjectWasCalled(3);
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

		public void MockRetrieveWorkspacesWhereAppIsInstalledAsync(bool applicationPreviouslyInstalled, int? workspaceArtifactId)
		{
			if (!applicationPreviouslyInstalled)
			{
				MockSqlQueryHelper
					.Setup(x => x.RetrieveWorkspacesWhereAppIsInstalledAsync(MockDbContextEdds.Object))
					.Returns(Task.FromResult<DataRow>(null));
			}
			else
			{
				var dtWorkspace = new DataTable();
				var colCaseArtifactId = new DataColumn
				{
					DataType = typeof(int),
					ColumnName = "CaseArtifactId"
				};
				dtWorkspace.Columns.Add(colCaseArtifactId);

				var drWorkspace = dtWorkspace.NewRow();
				drWorkspace["CaseArtifactId"] = workspaceArtifactId;
				dtWorkspace.Rows.Add(drWorkspace);

				MockSqlQueryHelper
					.Setup(x => x.RetrieveWorkspacesWhereAppIsInstalledAsync(MockDbContextEdds.Object))
					.Returns(Task.FromResult(dtWorkspace.Rows[0]));
			}
		}

		#endregion Mocks

		#region Helper Methods

		private PreInstallValidationJob GetPreInstallJob()
		{
			var preInstallJob = new PreInstallValidationJob(
					WorkspaceArtifactId,
					MockDbContextEdds.Object,
					MockSqlQueryHelper.Object,
					MockLogger.Object);
			return preInstallJob;
		}

		public void VerifyLoggerWithoutObjectWasCalled(Int32 timesCalled)
		{
			MockLogger.Verify(x => x.LogDebug(It.IsAny<string>()), Times.Exactly(timesCalled));
		}

		public void VerifyLoggerWithObjectWasCalled(Int32 timesCalled)
		{
			MockLogger.Verify(x => x.LogDebug(It.IsAny<string>(), It.IsAny<PreInstallValidationJob>()), Times.Exactly(timesCalled));
		}

		public void VerifyRetrieveWorkspacesWhereAppIsInstalledWasCalled(Int32 timesCalled)
		{
			MockSqlQueryHelper.Verify(x => x.RetrieveWorkspacesWhereAppIsInstalledAsync(MockDbContextEdds.Object), Times.Exactly(timesCalled));
		}

		#endregion Helper Methods
	}
}