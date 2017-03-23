using AdminMigrationUtility.CustomPages.Models;
using AdminMigrationUtility.Helpers.SQL;
using Moq;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace AdminMigrationUtility.CustomPages.NUnit
{
	[TestFixture]
	internal class ManagerQueueRecordModelTests
	{
		#region Tests

		[Test]
		public async Task NewManagerQueueRecordTest_NoAgentIdAsync()
		{
			//Arrange
			var sqlQueryHelperMock = new Mock<ISqlQueryHelper>();
			var dt = await DataHelpers.ManagerAgentData.BuildDataTableAsync();

			const Int32 workspaceArtifactId = 12345;
			const String workspaceName = "Test Workspace";

			const Int32 row1Id = 1;
			var row1AddedOn = DateTime.UtcNow.AddTicks(-(DateTime.UtcNow.Ticks % TimeSpan.TicksPerSecond));
			const String row1Status = "Waiting";
			const Int32 row1Priority = 10;
			const String row1AddedBy = "Doe, Jane";
			const Int32 row1ArtifactId = 88888;

			var dataRow = await DataHelpers.ManagerAgentData.BuildDataRowAsync(dt, row1Id, row1AddedOn, workspaceArtifactId, workspaceName, row1Status, null, row1Priority, row1AddedBy, row1ArtifactId);

			//Act
			var record = new ManagerQueueRecordModel(dataRow, sqlQueryHelperMock.Object);

			//Assert
			Assert.AreEqual(row1Id, record.ID);
			Assert.AreEqual(row1AddedOn, record.AddedOn);
			Assert.AreEqual(workspaceArtifactId, record.WorkspaceArtifactId);
			Assert.AreEqual(workspaceName, record.WorkspaceName);
			Assert.AreEqual(row1Status, record.Status);
			Assert.AreEqual(null, record.AgentId);
			Assert.AreEqual(row1Priority, record.Priority);
			Assert.AreEqual(row1AddedBy, record.AddedBy);
			Assert.AreEqual(row1ArtifactId, record.RecordArtifactId);
		}

		[Test]
		public async Task NewManagerQueueRecordTest_AgentIdAsync()
		{
			//Arrange
			var sqlQueryHelperMock = new Mock<ISqlQueryHelper>();
			var dt = await DataHelpers.ManagerAgentData.BuildDataTableAsync();

			const Int32 workspaceArtifactId = 12345;
			const String workspaceName = "Test Workspace";

			const Int32 row1Id = 1;
			var row1AddedOn = DateTime.UtcNow.AddTicks(-(DateTime.UtcNow.Ticks % TimeSpan.TicksPerSecond));
			const String row1Status = "Waiting";
			const Int32 row1Priority = 10;
			const String row1AddedBy = "Doe, Jane";
			const Int32 row1ArtifactId = 88888;
			const Int32 row1AgentId = 999999;

			var dataRow = await DataHelpers.ManagerAgentData.BuildDataRowAsync(dt, row1Id, row1AddedOn, workspaceArtifactId, workspaceName, row1Status, row1AgentId, row1Priority, row1AddedBy, row1ArtifactId);

			//Act
			var record = new ManagerQueueRecordModel(dataRow, sqlQueryHelperMock.Object);

			//Assert
			Assert.AreEqual(row1Id, record.ID);
			Assert.AreEqual(row1AddedOn, record.AddedOn);
			Assert.AreEqual(workspaceArtifactId, record.WorkspaceArtifactId);
			Assert.AreEqual(workspaceName, record.WorkspaceName);
			Assert.AreEqual(row1Status, record.Status);
			Assert.AreEqual(row1AgentId, record.AgentId);
			Assert.AreEqual(row1Priority, record.Priority);
			Assert.AreEqual(row1AddedBy, record.AddedBy);
			Assert.AreEqual(row1ArtifactId, record.RecordArtifactId);
		}

		[Test]
		public async Task NewManagerQueueRecordTest_NoValuesAsync()
		{
			//Arrange
			var sqlQueryHelperMock = new Mock<ISqlQueryHelper>();
			var dt = await DataHelpers.ManagerAgentData.BuildDataTableAsync();
			var dataRow = await DataHelpers.ManagerAgentData.BuildEmptyDataRowAsync(dt);

			//Act
			var record = new ManagerQueueRecordModel(dataRow, sqlQueryHelperMock.Object);

			//Assert
			Assert.AreEqual(0, record.ID);
			Assert.AreEqual(new DateTime(), record.AddedOn);
			Assert.AreEqual(0, record.WorkspaceArtifactId);
			Assert.AreEqual(String.Empty, record.WorkspaceName);
			Assert.AreEqual(String.Empty, record.Status);
			Assert.AreEqual(null, record.AgentId);
			Assert.AreEqual(0, record.Priority);
			Assert.AreEqual(String.Empty, record.AddedBy);
			Assert.AreEqual(0, record.RecordArtifactId);
		}

		#endregion Tests
	}
}