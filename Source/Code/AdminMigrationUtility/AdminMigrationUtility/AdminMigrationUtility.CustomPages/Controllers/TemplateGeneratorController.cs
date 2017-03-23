using AdminMigrationUtility.Helpers.Exceptions;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace AdminMigrationUtility.CustomPages.Controllers
{
	public class TemplateGeneratorController : Controller
	{
		[System.Web.Http.HttpGet]
		public async Task<ActionResult> Download(String migrationObjectType)
		{
			ActionResult result = null;
			try
			{
				if (!String.IsNullOrWhiteSpace(migrationObjectType))
				{
					var fileName = migrationObjectType + "-Template.csv";
					var migrationObjectInstance = await AdminMigrationUtility.Helpers.Utility.GetImportObjectSelectionAsync(migrationObjectType.Trim());
					var columns = (await migrationObjectInstance.GetColumnsAsync()).Select(x => x.ColumnName).ToList();
					var columnTemplate = await AdminMigrationUtility.Helpers.Utility.FormatCsvLineAsync(columns);

					var content = Encoding.UTF8.GetBytes(columnTemplate);
					result = File(content, "text/plain", fileName);
				}
				else
				{
					throw new AdminMigrationUtilityException();
				}
			}
			catch (Exception ex)
			{
				//log
			}
			return result;
		}
	}
}