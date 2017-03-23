using AdminMigrationUtility.Helpers.SQL;
using Relativity.API;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace AdminMigrationUtility.CustomPages.Models
{
	public class WorkerAgentModel
	{
		public List<WorkerQueueRecordModel> Records { get; set; }
		public ISqlQueryHelper SqlQueryHelper;

		public WorkerAgentModel(ISqlQueryHelper sqlQueryHelperModel)
		{
			SqlQueryHelper = sqlQueryHelperModel;
			Records = new List<WorkerQueueRecordModel>();
		}

		public WorkerAgentModel()
		{
			SqlQueryHelper = new SqlQueryHelper();
			Records = new List<WorkerQueueRecordModel>();
		}

		public async Task GetAllExportAsync(IDBContext eddsDbContext)
		{
			var dt = await SqlQueryHelper.RetrieveAllInExportWorkerQueueAsync(eddsDbContext);

			foreach (DataRow thisRow in dt.Rows)
			{
				Records.Add(new WorkerQueueRecordModel(thisRow, SqlQueryHelper));
			}
		}

		public async Task GetAllImportAsync(IDBContext eddsDbContext)
		{
			var dt = await SqlQueryHelper.RetrieveAllInImportWorkerQueueAsync(eddsDbContext);

			foreach (DataRow thisRow in dt.Rows)
			{
				Records.Add(new WorkerQueueRecordModel(thisRow, SqlQueryHelper));
			}
		}
	}
}