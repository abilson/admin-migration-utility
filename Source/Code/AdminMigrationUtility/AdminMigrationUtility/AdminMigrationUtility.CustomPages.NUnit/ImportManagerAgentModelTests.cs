﻿using AdminMigrationUtility.CustomPages.Models;
using AdminMigrationUtility.Helpers.SQL;
using Moq;
using NUnit.Framework;
using Relativity.API;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AdminMigrationUtility.CustomPages.NUnit
{
	[TestFixture]
	public class ImportManagerAgentModelTests
	{
		#region Tests

		[Test]
		public async Task GetAllImportManagerRecordsTestAsync()
		{
			//Arrange
			var sqlQueryHelperMock = new Mock<ISqlQueryHelper>();
			var managerAgentModel = new ManagerAgentModel(sqlQueryHelperMock.Object);
			var dbContextMock = new Mock<IDBContext>();
			var dt = await DataHelpers.ManagerAgentData.BuildDataTableAsync();

			const Int32 workspaceArtifactId = 12345;
			const String workspaceName = "Test Workspace";

			const Int32 row1Id = 1;
			var row1AddedOn = DateTime.UtcNow.AddTicks(-(DateTime.UtcNow.Ticks % TimeSpan.TicksPerSecond));
			const String row1Status = "Waiting";
			const Int32 row1Priority = 10;
			const String row1AddedBy = "Doe, Jane";
			const Int32 row1ArtifactId = 88888;

			const Int32 row2Id = 2;
			var row2AddedOn = DateTime.UtcNow.AddTicks(-(DateTime.UtcNow.Ticks % TimeSpan.TicksPerSecond));
			const String row2Status = "In Progress";
			const Int32 row2AgentId = 11111;
			const Int32 row2Priority = 5;
			const String row2AddedBy = "Doe, John";
			const Int32 row2ArtifactId = 9999999;

			var row1 = await DataHelpers.ManagerAgentData.BuildDataRowAsync(dt, row1Id, row1AddedOn, workspaceArtifactId, workspaceName, row1Status, null, row1Priority, row1AddedBy, row1ArtifactId);
			var row2 = await DataHelpers.ManagerAgentData.BuildDataRowAsync(dt, row2Id, row2AddedOn, workspaceArtifactId, workspaceName, row2Status, row2AgentId, row2Priority, row2AddedBy, row2ArtifactId);
			dt.Rows.Add(row1);
			dt.Rows.Add(row2);
			sqlQueryHelperMock.Setup(x => x.RetrieveAllInImportManagerQueueAsync(It.IsAny<IDBContext>())).ReturnsAsync(dt);

			//Act
			await managerAgentModel.GetAllImportAsync(dbContextMock.Object);

			//Assert
			//There are 2 records
			Assert.AreEqual(2, managerAgentModel.Records.Count);

			//The first row is set correctly
			Assert.AreEqual(workspaceArtifactId, managerAgentModel.Records.ElementAt(0).WorkspaceArtifactId);
			Assert.AreEqual(workspaceName, managerAgentModel.Records.ElementAt(0).WorkspaceName);
			Assert.AreEqual(row1Id, managerAgentModel.Records.ElementAt(0).ID);
			Assert.AreEqual(row1AddedOn, managerAgentModel.Records.ElementAt(0).AddedOn);
			Assert.AreEqual(row1Status, managerAgentModel.Records.ElementAt(0).Status);
			Assert.AreEqual(null, managerAgentModel.Records.ElementAt(0).AgentId);
			Assert.AreEqual(row1Priority, managerAgentModel.Records.ElementAt(0).Priority);
			Assert.AreEqual(row1AddedBy, managerAgentModel.Records.ElementAt(0).AddedBy);
			Assert.AreEqual(row1ArtifactId, managerAgentModel.Records.ElementAt(0).RecordArtifactId);

			//The second row is set correctly
			Assert.AreEqual(workspaceArtifactId, managerAgentModel.Records.ElementAt(1).WorkspaceArtifactId);
			Assert.AreEqual(workspaceName, managerAgentModel.Records.ElementAt(1).WorkspaceName);
			Assert.AreEqual(row2Id, managerAgentModel.Records.ElementAt(1).ID);
			Assert.AreEqual(row2AddedOn, managerAgentModel.Records.ElementAt(1).AddedOn);
			Assert.AreEqual(row2Status, managerAgentModel.Records.ElementAt(1).Status);
			Assert.AreEqual(row2AgentId, managerAgentModel.Records.ElementAt(1).AgentId);
			Assert.AreEqual(row2Priority, managerAgentModel.Records.ElementAt(1).Priority);
			Assert.AreEqual(row2AddedBy, managerAgentModel.Records.ElementAt(1).AddedBy);
			Assert.AreEqual(row2ArtifactId, managerAgentModel.Records.ElementAt(1).RecordArtifactId);
		}

		#endregion Tests
	}
}