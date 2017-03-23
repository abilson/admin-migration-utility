using AdminMigrationUtility.EventHandlers.Application;
using AdminMigrationUtility.Helpers.SQL;
using Moq;
using NUnit.Framework;
using Relativity.API;
using System;

namespace AdminMigrationUtility.EventHandlers.NUnit.Application
{
	[TestFixture]
	public class PostInstallSetupJobTests
	{
		public Mock<IEHHelper> MockEhHelper { get; set; }
		public Mock<IDBContext> MockDbContextEdds { get; set; }
		public Mock<ISqlQueryHelper> MockSqlQueryHelper { get; set; }
		public Mock<IAPILog> MockLogger { get; set; }
		public Mock<PostInstallSetupJob> MockPostInstallJob { get; set; }

		[SetUp]
		public void SetUp()
		{
			MockEhHelper = new Mock<IEHHelper>();
			MockSqlQueryHelper = new Mock<ISqlQueryHelper>();
			MockLogger = new Mock<IAPILog>();
			MockPostInstallJob = new Mock<PostInstallSetupJob>();
			MockDbContextEdds = new Mock<IDBContext>();
			AddMockLogger();
		}

		#region Tests

		[Test]
		[Description("This will test when the solution's queue and error table creation when the solution has not been installed in the environment.")]
		public void ExecuteAsync_NotInstalledInEnvironment()
		{
			// Arrange
			AddMockEhHelperGetActiveCaseId();
			AddMockSvcManager();
			AddMockDbContextEdds();
			AddMockGetAuthenticationManager();
			var postInstallJob = GetPostInstallJob();

			// Act

			// Assert
			Assert.DoesNotThrow(async () => await postInstallJob.ExecuteAsync());
			VerifyLoggerWithoutObjectWasCalled(12);
			VerifyLoggerWithObjectWasCalled(2);
		}

		[Test]
		[Description("This will test when the solution's queue and error table creation when the solution has already been installed in the environment.")]
		public void ExecuteAsync_AlreadyInstalledInEnvironment()
		{
			// Arrange
			AddMockEhHelperGetActiveCaseId();
			AddMockSvcManager();
			AddMockDbContextEdds();
			AddMockGetAuthenticationManager();
			var postInstallJob = GetPostInstallJob();

			// Act

			// Assert
			Assert.DoesNotThrow(async () => await postInstallJob.ExecuteAsync());
			VerifyLoggerWithoutObjectWasCalled(12);
			VerifyLoggerWithObjectWasCalled(2);
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

		#endregion Mocks

		#region Helper Methods

		private PostInstallSetupJob GetPostInstallJob()
		{
			var postInstallJob = new PostInstallSetupJob(
					MockDbContextEdds.Object,
					MockSqlQueryHelper.Object,
					MockLogger.Object);
			return postInstallJob;
		}

		public void VerifyLoggerWithoutObjectWasCalled(Int32 timesCalled)
		{
			MockLogger.Verify(x => x.LogDebug(It.IsAny<string>()), Times.Exactly(timesCalled));
		}

		public void VerifyLoggerWithObjectWasCalled(Int32 timesCalled)
		{
			MockLogger.Verify(x => x.LogDebug(It.IsAny<string>(), It.IsAny<PostInstallSetupJob>()), Times.Exactly(timesCalled));
		}

		#endregion Helper Methods
	}
}