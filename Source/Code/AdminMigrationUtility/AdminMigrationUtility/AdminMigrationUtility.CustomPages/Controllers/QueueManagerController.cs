using AdminMigrationUtility.CustomPages.Models;
using Relativity.CustomPages;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace AdminMigrationUtility.CustomPages.Controllers
{
	public class QueueManagerController : Controller
	{
		//
		// GET: /QueueManager/

		public async Task<ActionResult> Index()
		{
			var queueRecordManager = new QueueRecordManager();
			await queueRecordManager.Initialize(ConnectionHelper.Helper().GetDBContext(-1));
			return View(queueRecordManager);
		}
	}
}