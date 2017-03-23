using AdminMigrationUtility.Helpers.SQL;
using Relativity.API;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace AdminMigrationUtility.CustomPages.Models
{
	public class ManagerAgentModel
	{
		public List<ManagerQueueRecordModel> Records { get; set; }
		public ISqlQueryHelper SqlQueryHelper;

		public ManagerAgentModel(ISqlQueryHelper sqlQueryHelperModel)
		{
			SqlQueryHelper = sqlQueryHelperModel;
			Records = new List<ManagerQueueRecordModel>();
		}

		public ManagerAgentModel()
		{
			SqlQueryHelper = new SqlQueryHelper();
			Records = new List<ManagerQueueRecordModel>();
		}

		public async Task GetAllExportAsync(IDBContext eddsDbContext)
		{
			DataTable dt = await SqlQueryHelper.RetrieveAllInExportManagerQueueAsync(eddsDbContext);

			foreach (DataRow thisRow in dt.Rows)
			{
				Records.Add(new ManagerQueueRecordModel(thisRow, SqlQueryHelper));
			}
		}

		public async Task GetAllImportAsync(IDBContext eddsDbContext)
		{
			DataTable dt = await SqlQueryHelper.RetrieveAllInImportManagerQueueAsync(eddsDbContext);

			foreach (DataRow thisRow in dt.Rows)
			{
				Records.Add(new ManagerQueueRecordModel(thisRow, SqlQueryHelper));
			}
		}
	}
}