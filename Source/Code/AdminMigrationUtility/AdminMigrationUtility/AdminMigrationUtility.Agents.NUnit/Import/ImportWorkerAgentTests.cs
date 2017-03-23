using AdminMigrationUtility.Agents.Import;
using kCura.Relativity.Client.DTOs;
using Moq;
using NUnit.Framework;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Agents.NUnit.Import
{
	[TestFixture]
	public class ImportWorkerAgentTests
	{
		private ImportTestHelper _importHelper;

		[TearDown]
		public void TearDown()
		{
			if (_importHelper != null)
			{
				_importHelper.Dispose();
			}
		}

		#region Tests

		[Description("When a record is picked up by the agent, should complete execution process")]
		[Test]
		public async Task ExecuteAsync_QueueHasARecord_ExecuteAll()
		{
			// Arrange
			_importHelper = new ImportTestHelper(duringOffHours: true);
			_importHelper.Initialize();
			var workerJob = GetImportWorkerJob();

			//Act
			await workerJob.ExecuteAsync();

			// Assert
			AssertRecordWasProcessed();
		}

		[Description("When no record is picked up by the agent, should not process")]
		[Test]
		public async Task ExecuteAsync_QueueHasNoRecord_DoNotExecute()
		{
			// Arrange
			_importHelper = new ImportTestHelper(duringOffHours: true)
			{
				RecordsReturedFromWorkerBatch = 0
			};
			_importHelper.Initialize();
			var workerJob = GetImportWorkerJob();

			// Act
			await workerJob.ExecuteAsync();

			// Assert
			AssertRecordWasSkipped();
		}

		[Description("When it's not during configured off-hours, record is not processed")]
		[Test]
		public async Task ExecuteAsync_QueueHasARecord_NotDuringOffHours()
		{
			// Arrange
			_importHelper = new ImportTestHelper(duringOffHours: false);
			_importHelper.Initialize();
			var workerJob = GetImportWorkerJob();

			// Act
			await workerJob.ExecuteAsync();

			// Assert
			AssertRecordWasSkipped();
		}

		[TestCase(new Int32[] { 1000001, 1000002, 1000003 }, "1000001,1000002,1000003")]
		[TestCase(new Int32[] { }, "")]
		[TestCase(new Int32[] { 1000001 }, "1000001")]
		[Description("This will test getting a comma delimited list of resource IDs from a list of integers.")]
		public void GetCommaDelimitedListOfResourceIds(IEnumerable<Int32> resourceIdsList, String expectedResult)
		{
			// Arrange
			_importHelper = new ImportTestHelper(duringOffHours: true);
			_importHelper.Initialize();
			var workerJob = GetImportWorkerJob();

			//Act
			var observedResult = workerJob.GetCommaDelimitedListOfResourceIds(resourceIdsList);

			//Assert
			Assert.AreEqual(expectedResult, observedResult);
		}

		[Description("Job goes on to next record in batch after one throws exception")]
		[Test]
		public async Task ExecuteAsync_JobContinuesToNextRecordAfterOneCausesException_ExecuteAll()
		{
			// Arrange
			_importHelper = new ImportTestHelper(duringOffHours: true)
			{
				RecordsReturedFromWorkerBatch = 2
			};
			_importHelper.Initialize();
			_importHelper.MockClientRepo
				.SetupSequence(x => x.Query(It.IsAny<Query<Client>>(), It.IsAny<Int32>()))
				.Throws(new Exception())
				.Returns(_importHelper.MockClientResult);

			var workerJob = GetImportWorkerJob();

			// Act
			await workerJob.ExecuteAsync();

			// Assert
			AsserUserWasCreated(1);
			AssertClientWasQueried(2);
		}

		[Description("Errors are recorded when one user fails and another succeeds")]
		[Test]
		public async Task ExecuteAsync_ErrorsAreRecordedWhenOneFailsAndOtherSucceeds_ExecuteAll()
		{
			// Arrange
			_importHelper = new ImportTestHelper(duringOffHours: true)
			{
				RecordsReturedFromWorkerBatch = 2
			};
			_importHelper.Initialize();
			_importHelper.MockClientRepo
				.SetupSequence(x => x.Query(It.IsAny<Query<Client>>(), It.IsAny<Int32>()))
				.Throws(new Exception())
				.Returns(_importHelper.MockClientResult);

			var workerJob = GetImportWorkerJob();

			// Act
			await workerJob.ExecuteAsync();

			// Assert
			AssertClientWasQueried(2);
			Assert.IsTrue(_importHelper.RecordedImportJobErrors.Any());
		}

		#endregion Tests

		#region privateMethods

		private ImportWorkerJob GetImportWorkerJob()
		{
			return new ImportWorkerJob(
				agentId: _importHelper.AgentId,
				agentHelper: _importHelper.MockAgentHelper.Object,
				sqlQueryHelper: _importHelper.MockSqlQueryHelper.Object,
				artifactQueryHelper: _importHelper.MockArtifactQueryHelper.Object,
				processedOnDateTime: _importHelper.ProcessedTime,
				rsapiRepositoryGroup: _importHelper.MockRSAPIRepositoryGroup.Object,
				resourceGroupIds: _importHelper.ResourceGroupIdList,
				logger: _importHelper.MockLogger.Object,
				apiOptions: _importHelper.APIOptions,
				serializationHelper: _importHelper.MockSerializationHelper.Object);
		}

		#endregion privateMethods

		#region assertions

		public void AsserUserWasCreated(Int32 numberOfTimes)
		{
			_importHelper.MockUserRepo.Verify(x => x.CreateSingle(It.IsAny<User>()), Times.Exactly(numberOfTimes));
		}

		public void AssertClientWasQueried(Int32 numberOfTimes)
		{
			_importHelper.MockClientRepo.Verify(x => x.Query(It.IsAny<Query<Client>>(), It.IsAny<Int32>()), Times.Exactly(numberOfTimes));
		}

		public void AssertRecordWasSkipped()
		{
			// User was created
			_importHelper.MockSerializationHelper
				.Verify(x =>
					x.DeserializeToAdminObjectAsync(It.IsAny<String>()),
					Times.Never);
			_importHelper.MockSqlQueryHelper
				.Verify(x =>
					x.DeleteRecordFromQueueAsync(It.IsAny<IDBContext>(), It.IsAny<Int32>(), It.IsAny<Int32>(), It.IsAny<Int32>(), It.IsAny<String>()),
					Times.Never);
		}

		public void AssertRecordWasProcessed()
		{
			// User was created
			_importHelper.MockSerializationHelper
				.Verify(x =>
					x.DeserializeToAdminObjectAsync(It.IsAny<String>()),
					Times.Once);
			_importHelper.MockSqlQueryHelper
				.Verify(x =>
					x.DeleteRecordFromQueueAsync(It.IsAny<IDBContext>(), It.IsAny<Int32>(), It.IsAny<Int32>(), It.IsAny<Int32>(), It.IsAny<String>()),
					Times.Once);
		}

		#endregion assertions
	}
}