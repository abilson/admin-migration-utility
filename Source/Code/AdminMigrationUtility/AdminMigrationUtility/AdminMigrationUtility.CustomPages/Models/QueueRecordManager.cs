using AdminMigrationUtility.Helpers;
using AdminMigrationUtility.Helpers.Models;
using AdminMigrationUtility.Helpers.SQL;
using Relativity.API;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace AdminMigrationUtility.CustomPages.Models
{
	public class QueueRecordManager
	{
		public ISqlQueryHelper SqlQueryHelper;
		public IEnumerable<ExportManagerQueueRecord> ExportManagerQueue;
		public IEnumerable<ImportManagerQueueRecord> ImportManagerQueue;
		public IEnumerable<ExportWorkerQueueRecord> ExportWorkerQueue;
		public IEnumerable<ImportWorkerQueueRecord> ImportWorkerQueue;

		public QueueRecordManager(ISqlQueryHelper sqlQueryHelperModel)
		{
			SqlQueryHelper = sqlQueryHelperModel;
		}

		public QueueRecordManager()
		{
			SqlQueryHelper = new SqlQueryHelper();
		}

		public async Task Initialize(IDBContext eddsDbContext)
		{
			ExportManagerQueue = await GetAllExportManagerQueueAsync(eddsDbContext);
			ImportManagerQueue = await GetAllImportManagerQueueAsync(eddsDbContext);
			ExportWorkerQueue = await GetAllExportWorkerQueueAsync(eddsDbContext);
			ImportWorkerQueue = await GetAllImportWorkerQueueAsync(eddsDbContext);
		}

		public async Task<IEnumerable<ExportManagerQueueRecord>> GetAllExportManagerQueueAsync(IDBContext eddsDbContext)
		{
			List<ExportManagerQueueRecord> records = new List<ExportManagerQueueRecord>();
			DataTable dt = await SqlQueryHelper.RetrieveAllQueueRecordsAsync(eddsDbContext, Constant.Tables.ExportManagerQueue, new[] { Constant.Sql.ColumnsNames.ExportManagerQueue.Priority, Constant.Sql.ColumnsNames.ExportManagerQueue.TimeStampUtc });

			foreach (DataRow thisRow in dt.Rows)
			{
				records.Add(new ExportManagerQueueRecord(thisRow));
			}
			return records;
		}

		public async Task<IEnumerable<ImportManagerQueueRecord>> GetAllImportManagerQueueAsync(IDBContext eddsDbContext)
		{
			List<ImportManagerQueueRecord> records = new List<ImportManagerQueueRecord>();
			DataTable dt = await SqlQueryHelper.RetrieveAllQueueRecordsAsync(eddsDbContext, Constant.Tables.ImportManagerQueue, new[] { Constant.Sql.ColumnsNames.ImportManagerQueue.Priority, Constant.Sql.ColumnsNames.ImportManagerQueue.TimeStampUTC });

			foreach (DataRow thisRow in dt.Rows)
			{
				records.Add(new ImportManagerQueueRecord(thisRow));
			}
			return records;
		}

		public async Task<IEnumerable<ExportWorkerQueueRecord>> GetAllExportWorkerQueueAsync(IDBContext eddsDbContext)
		{
			List<ExportWorkerQueueRecord> records = new List<ExportWorkerQueueRecord>();
			DataTable dt = await SqlQueryHelper.RetrieveAllQueueRecordsAsync(eddsDbContext, Constant.Tables.ExportWorkerQueue, new[] { Constant.Sql.ColumnsNames.ExportWorkerQueue.Priority, Constant.Sql.ColumnsNames.ExportWorkerQueue.TimeStampUtc });

			foreach (DataRow thisRow in dt.Rows)
			{
				records.Add(new ExportWorkerQueueRecord(thisRow));
			}
			return records;
		}

		public async Task<IEnumerable<ImportWorkerQueueRecord>> GetAllImportWorkerQueueAsync(IDBContext eddsDbContext)
		{
			List<ImportWorkerQueueRecord> records = new List<ImportWorkerQueueRecord>();
			DataTable dt = await SqlQueryHelper.RetrieveAllQueueRecordsAsync(eddsDbContext, Constant.Tables.ImportWorkerQueue, new[] { Constant.Sql.ColumnsNames.ImportWorkerQueue.Priority, Constant.Sql.ColumnsNames.ImportWorkerQueue.TimeStampUTC });

			foreach (DataRow thisRow in dt.Rows)
			{
				records.Add(new ImportWorkerQueueRecord(thisRow));
			}
			return records;
		}
	}
}