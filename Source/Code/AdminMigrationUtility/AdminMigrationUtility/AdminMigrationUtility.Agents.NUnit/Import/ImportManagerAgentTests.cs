using AdminMigrationUtility.Agents.Import;
using AdminMigrationUtility.Helpers;
using AdminMigrationUtility.Helpers.Models.AdminObject;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Agents.NUnit.Import
{
	[TestFixture]
	public class ImportManagerAgentTests
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

			var managerJob = GetManagerImportJob();
			await CreateValidFile(1, managerJob.LocalTempFilePath);

			// Act
			await managerJob.ExecuteAsync();
			managerJob.Dispose();

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
				RecordsReturedFromManagerBatch = 0
			};
			_importHelper.Initialize();

			var managerJob = GetManagerImportJob();

			// Act
			await managerJob.ExecuteAsync();

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

			var managerJob = GetManagerImportJob();

			// Act
			await managerJob.ExecuteAsync();

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

			var managerJob = GetManagerImportJob();

			//Act
			var observedResult = managerJob.GetCommaDelimitedListOfResourceIds(resourceIdsList);

			//Assert
			Assert.AreEqual(expectedResult, observedResult);
		}

		[Test]
		[Description("Make sure downloaded file id deleted after the job is disposed.")]
		public async Task CsvIsDeletedAfterImportManagerJobIsDisposed()
		{
			// Arrange
			_importHelper = new ImportTestHelper(duringOffHours: true);
			_importHelper.Initialize();

			var managerJob = GetManagerImportJob();
			var filePath = managerJob.LocalTempFilePath;
			await CreateValidFile(1, filePath);

			// Act
			await managerJob.ExecuteAsync();

			managerJob.Dispose();

			// Assert
			Assert.IsTrue(!File.Exists(filePath));
		}

		[Test]
		[Description("Make sure make sure a violation is reported when file is missing a column from the object template.")]
		public async Task ViolationReportedWhenColumnMissing()
		{
			// Arrange
			_importHelper = new ImportTestHelper(duringOffHours: true);
			_importHelper.Initialize();
			var user = new UserAdminObject();
			var userColumns = new Stack<String>((await user.GetColumnsAsync()).Select(x => x.ColumnName).ToList());
			var missingColumn = userColumns.Pop();
			var fileContents = String.Join(Constant.CommaSeparator, userColumns);

			var managerJob = GetManagerImportJob();
			await CreateFile(fileContents, managerJob.LocalTempFilePath);

			// Act
			await managerJob.ExecuteAsync();
			managerJob.Dispose();

			// Assert
			var expectedMsg = String.Format(Constant.ErrorMessages.MissingColumnsError, missingColumn);
			Assert.IsTrue(_importHelper.RecordedImportJobErrors.Contains(expectedMsg));
		}

		[Test]
		[Description("Make sure make sure a violation is reported when file has unexpected columns for the selected object type.")]
		public async Task ViolationReportedWhenFileHasUnsupportedColumns()
		{
			// Arrange
			var unsupportedColumnName = "ExtraUnsupportedColumn";
			_importHelper = new ImportTestHelper(duringOffHours: true);
			_importHelper.Initialize();
			var user = new UserAdminObject();
			var userColumns = (await user.GetColumnsAsync()).Select(x => x.ColumnName).ToList();
			userColumns.Add(unsupportedColumnName);
			var fileContents = String.Join(Constant.CommaSeparator, userColumns);

			var managerJob = GetManagerImportJob();
			await CreateFile(fileContents, managerJob.LocalTempFilePath);

			// Act
			await managerJob.ExecuteAsync();
			managerJob.Dispose();

			// Assert
			var expectedMsg = String.Format(Constant.ErrorMessages.ExtraColumnsError, _importHelper.ImportObjectType, unsupportedColumnName);
			Assert.IsTrue(_importHelper.RecordedImportJobErrors.Contains(expectedMsg));
		}

		[Test]
		[Description("Make sure make sure a violation is reported when a user tries to import an unsupported object.")]
		public async Task ViolationReportedWhenUnsupportedObjectSelected()
		{
			// Arrange
			_importHelper = new ImportTestHelper(duringOffHours: true)
			{
				ImportObjectType = "RandomFakeNonExistingObject"
			};
			_importHelper.Initialize();

			var managerJob = GetManagerImportJob();
			await CreateValidFile(1, managerJob.LocalTempFilePath);

			// Act
			await managerJob.ExecuteAsync();
			managerJob.Dispose();

			// Assert
			Assert.IsTrue(_importHelper.RecordedImportJobErrors.FirstOrDefault(x => x.ToLower().Contains("unsupported object")) != null);
		}

		[Test]
		[Description("Make sure make sure a violation is reported when file has columns but no meta data to process")]
		public async Task ViolationReportedWhenFileContainsColumnsButNoMetaData()
		{
			// Arrange
			_importHelper = new ImportTestHelper(duringOffHours: true);
			_importHelper.Initialize();

			var managerJob = GetManagerImportJob();
			await CreateValidFile(0, managerJob.LocalTempFilePath);

			// Act
			await managerJob.ExecuteAsync();
			managerJob.Dispose();

			// Assert
			var expectedMsg = Constant.ErrorMessages.FileContainsColumnsButNoMetaDataError;
			Assert.IsTrue(_importHelper.RecordedImportJobErrors.Contains(expectedMsg));
		}

		[Test]
		[Description("Make sure make sure a violation is reported when the manager is unable to insert a batch into the worker queue")]
		public async Task ViolationReportedWhenUnableToInsertBatchIntoWorkerQueue()
		{
			// Arrange
			_importHelper = new ImportTestHelper(duringOffHours: true);
			_importHelper.Initialize();

			_importHelper.MockSqlQueryHelper
				.Setup(x => x.BulkInsertIntoTableAsync(It.IsAny<IDBContext>(), It.IsAny<DataTable>(), It.IsAny<String>(), It.IsAny<Int32>()))
				.Callback<IDBContext, DataTable, String, Int32>((dbContext, sourceDataTable, destinationTableName, batchSize) =>
				{
					_importHelper.InsertedWorkerQueueRecords = sourceDataTable;
				})
				.Throws(new Exception());

			var managerJob = GetManagerImportJob();
			await CreateValidFile(1, managerJob.LocalTempFilePath);

			// Act
			await managerJob.ExecuteAsync();
			managerJob.Dispose();

			// Assert
			var expectedMsg = String.Format(Constant.ErrorMessages.ImportQueueManagerPopulatingImportWorkerQueueError, 2, 2);
			Assert.IsTrue(_importHelper.RecordedImportJobErrors.Contains(expectedMsg));
		}

		[Test]
		[Description("Make sure that the Manager attempts to insert the next batch of queue records even if the first batch throws an exception")]
		public async Task ManagerContinuesToNextBatchEvenWhenThePreviousBatchErrorsOut()
		{
			// Arrange
			_importHelper = new ImportTestHelper(duringOffHours: true);
			_importHelper.Initialize();

			_importHelper.MockSqlQueryHelper
				.SetupSequence(x => x.BulkInsertIntoTableAsync(It.IsAny<IDBContext>(), It.IsAny<DataTable>(), It.IsAny<String>(), It.IsAny<Int32>()))
				.Throws(new Exception())
				.Returns(Task.FromResult(0));

			var managerJob = GetManagerImportJob();
			// This file should contain twice as many records as the batch size to make sure the logic is executed twice
			await CreateValidFile((Constant.BatchSizes.ImportManagerIntoWorkerQueue * 2), managerJob.LocalTempFilePath);

			// Act
			await managerJob.ExecuteAsync();
			managerJob.Dispose();

			// Assert
			_importHelper.MockSqlQueryHelper.Verify(x => x.BulkInsertIntoTableAsync(It.IsAny<IDBContext>(), It.IsAny<DataTable>(), It.IsAny<String>(), It.IsAny<Int32>()), Times.Exactly(2));
		}

		[Test]
		[Description("Validates that the worker data is serialized correctly")]
		public async Task WorkerQueueDataCorrectlySerialized()
		{
			// Arrange
			_importHelper = new ImportTestHelper(duringOffHours: true);
			_importHelper.Initialize();

			var managerJob = GetManagerImportJob();
			await CreateValidFile(1, managerJob.LocalTempFilePath);

			// Act
			await managerJob.ExecuteAsync();
			managerJob.Dispose();
			var originalUserDictionary = await GetDummyUserMetaData();
			var userMetaData = _importHelper.InsertedWorkerQueueRecords.Rows[0][Constant.Sql.ColumnsNames.ImportWorkerQueue.MetaData].ToString();
			var obj = JsonConvert.DeserializeObject<UserAdminObject>(userMetaData, new JsonSerializerSettings
			{
				TypeNameHandling = TypeNameHandling.Objects
			});
			var processedUserValues = await obj.GetColumnsAsDictionaryAsync();

			// Assert
			Assert.IsTrue(originalUserDictionary.Count == processedUserValues.Count);
			Assert.IsTrue(!originalUserDictionary.Except(processedUserValues).Any());
		}

		[Test]
		[Description("Worker queue records are not written, status updated and queue records deleted to error when the file contain violations")]
		public async Task WorkerRecordsAreNotWrittenAndStatusUpdatedAndQueueRecordsDeletedWhenFileContainsViolations()
		{
			// Arrange
			_importHelper = new ImportTestHelper(duringOffHours: true)
			{
				ImportObjectType = "RandomFakeNonExistingObject"
			};
			_importHelper.Initialize();

			var managerJob = GetManagerImportJob();
			await CreateValidFile(1, managerJob.LocalTempFilePath);

			// Act
			await managerJob.ExecuteAsync();
			managerJob.Dispose();

			// Assert
			_importHelper.MockSqlQueryHelper.Verify(x => x.BulkInsertIntoTableAsync(It.IsAny<IDBContext>(), It.IsAny<DataTable>(), It.IsAny<String>(), It.IsAny<Int32>()), Times.Never);
			_importHelper.MockSqlQueryHelper.Verify(x => x.DeleteRecordFromQueueAsync(It.IsAny<IDBContext>(), It.IsAny<String>(), It.IsAny<Int32>(), It.IsAny<String>(), It.IsAny<Int32>(), It.IsAny<String>()), Times.Exactly(2));
			Assert.AreEqual(Constant.Status.Job.ERROR, _importHelper.RDOJobStatus);
		}

		[Test]
		[Description("Worker Records Populated, Queue table status updated, Job Rdo status updated when manager processes queue record successfully")]
		public async Task WorkerTablePopulatedQueueTableStatusUpdatedRdoStatusUpdatedWhenManagerSuccessfullyProcessesQueueRecord()
		{
			// Arrange
			_importHelper = new ImportTestHelper(duringOffHours: true);
			_importHelper.Initialize();

			var managerJob = GetManagerImportJob();
			await CreateValidFile(1, managerJob.LocalTempFilePath);

			// Act
			await managerJob.ExecuteAsync();
			managerJob.Dispose();

			// Assert
			_importHelper.MockSqlQueryHelper.Verify(x => x.BulkInsertIntoTableAsync(It.IsAny<IDBContext>(), It.IsAny<DataTable>(), It.IsAny<String>(), It.IsAny<Int32>()), Times.AtLeastOnce);
			_importHelper.MockSqlQueryHelper.Verify(x => x.UpdateQueueStatusAsync(It.IsAny<IDBContext>(), It.IsAny<Int32>(), It.IsAny<Int32>(), It.IsAny<String>(), It.IsAny<String>(), It.IsAny<Int32>()), Times.AtLeastOnce);
			Assert.AreEqual(Constant.Status.Job.COMPLETED_MANAGER, _importHelper.RDOJobStatus);
		}

		[Test]
		[Description("Validates that the status updated correctly after all worker records have been processed")]
		public async Task JobStatusUpdatedAfterWorkerProcessessAllRecords()
		{
			// Arrange
			_importHelper = new ImportTestHelper(duringOffHours: true);
			_importHelper.Initialize();

			var managerJob = GetManagerImportJob();
			await CreateValidFile(1, managerJob.LocalTempFilePath);

			// Act
			await managerJob.ExecuteAsync();
			managerJob.Dispose();

			// Assert
			Assert.AreEqual(Constant.Status.Job.COMPLETED_MANAGER, _importHelper.RDOJobStatus);
		}

		[Test]
		[Description("Validates that the queue records are deleted after a job cancellation is requested")]
		public async Task QueueRecordsDeletedAndJobStatusUpdatedAfterJobCancellationRequested()
		{
			// Arrange
			_importHelper = new ImportTestHelper(duringOffHours: true)
			{
				ManagerQueueRecordStatus = Constant.Status.Queue.CANCELLATION_REQUESTED
			};
			_importHelper.Initialize();

			var managerJob = GetManagerImportJob();
			await CreateValidFile(1, managerJob.LocalTempFilePath);

			// Act
			await managerJob.ExecuteAsync();
			managerJob.Dispose();

			// Assert
			var expectedStatus = Constant.Status.Job.CANCELLED;
			_importHelper.MockSqlQueryHelper.Verify(x => x.DeleteRecordFromQueueAsync(It.IsAny<IDBContext>(), It.IsAny<String>(), It.IsAny<Int32>(), It.IsAny<String>(), It.IsAny<Int32>(), It.IsAny<String>()), Times.Exactly(2));
			Assert.AreEqual(expectedStatus, _importHelper.RDOJobStatus);
		}

		#endregion Tests

		#region Assertions

		private void AssertRecordWasSkipped()
		{
			// Insert was never called
			_importHelper.MockSqlQueryHelper.Verify(
				x =>
					x.InsertRowsIntoImportWorkerQueueAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<Int32>(),
					It.IsAny<Int32>(),
					It.IsAny<Int32>(),
					It.IsAny<Int32>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<Int32>()),
					Times.Never);

			// Remove record was never called
			_importHelper.MockSqlQueryHelper.Verify(
				x =>
					x.RemoveRecordFromTableByIdAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<String>(),
					It.IsAny<Int32>()),
					Times.Never);
		}

		private void AssertRecordWasProcessed()
		{
			// Insert was called once
			_importHelper.MockSqlQueryHelper.Verify(
				x =>
					x.BulkInsertIntoTableAsync(
					It.IsAny<IDBContext>(),
					It.IsAny<DataTable>(),
					It.IsAny<String>(),
					It.IsAny<Int32>()),
					Times.AtLeastOnce);
		}

		#endregion Assertions

		#region Test Helpers

		private ImportManagerJob GetManagerImportJob()
		{
			return new ImportManagerJob(
				agentId: _importHelper.AgentId,
				agentHelper: _importHelper.MockAgentHelper.Object,
				sqlQueryHelper: _importHelper.MockSqlQueryHelper.Object,
				artifactQueryHelper: _importHelper.MockArtifactQueryHelper.Object,
				processedOnDateTime: _importHelper.ProcessedTime,
				resourceGroupIds: _importHelper.ResourceGroupIdList,
				logger: _importHelper.MockLogger.Object,
				rsapiClient: _importHelper.MockRSAPIClient.Object,
				rsapiRepositoryGroup: _importHelper.MockRSAPIRepositoryGroup.Object,
				apiOptions: _importHelper.APIOptions,
				serializationHelper: _importHelper.MockSerializationHelper.Object);
		}

		//Generates a dictionary populated with the user object's columns as keys and the same values as dummy data
		private async Task<Dictionary<String, String>> GetDummyUserMetaData()
		{
			var user = new UserAdminObject();
			var userColumns = new Stack<String>((await user.GetColumnsAsync()).Select(x => x.ColumnName).ToList());
			var userDictionary = new Dictionary<String, String>();
			foreach (var column in userColumns)
			{
				userDictionary.Add(column, column);
			}
			return userDictionary;
		}

		private async Task CreateValidFile(Int32 numberOfMetaDataRows, String fullFilePath)
		{
			var userDictionary = await GetDummyUserMetaData();

			var columns = String.Join(Constant.CommaSeparator, userDictionary.Keys);
			var data = String.Join(Constant.CommaSeparator, userDictionary.Values);

			var fileContents = columns;

			for (var i = 0; i < numberOfMetaDataRows; i++)
			{
				fileContents = fileContents + Environment.NewLine + data;
			}

			await CreateFile(fileContents, fullFilePath);
		}

		private async Task CreateFile(String contents, String fullFilePath)
		{
			await Task.Run(() =>
			{
				using (FileStream target = File.Create(fullFilePath))
				{
					using (StreamWriter writer = new StreamWriter(target))
					{
						writer.WriteLine(contents);
					}
				}
			});
		}

		#endregion Test Helpers
	}
}