using AdminMigrationUtility.Helpers.Exceptions;
using AdminMigrationUtility.Helpers.Models.Interfaces;
using AdminMigrationUtility.Helpers.Rsapi.Interfaces;
using AdminMigrationUtility.Helpers.SQL;
using kCura.Relativity.Client;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.Models.AdminObject
{
	public abstract class AdminObjectBase : IAdminObject
	{
		public abstract Task<IEnumerable<String>> ImportAsync(APIOptions apiOptions, IRsapiRepositoryGroup repositoryGroup, IArtifactQueries artifactQueryHelper, IHelper helper, IDBContext eddsDbContext, ISqlQueryHelper sqlQueryHelper);

		public abstract Task<IEnumerable<String>> ValidateAsync(APIOptions apiOptions, IRsapiRepositoryGroup rsapiRepositoryGroup, IArtifactQueries artifactQueryHelper, IDBContext eddsDbContext, ISqlQueryHelper sqlQueryHelper);

		public async Task<IEnumerable<IObjectColumn>> GetColumnsAsync()
		{
			return await Task.Run(() =>
			{
				List<IObjectColumn> retVal = new List<IObjectColumn>();
				PropertyInfo[] test = GetType().GetProperties().ToArray();
				foreach (PropertyInfo column in test)
				{
					Object property = column.GetValue(this, null);
					if (property != null && column.GetValue(this, null).GetType().GetInterfaces().Contains(typeof(IObjectColumn)))
					{
						retVal.Add((IObjectColumn)property);
					}
				}

				return retVal.OrderBy(x => x.Order).ToArray();
			});
		}

		public async Task<IEnumerable<String>> ExportAsync()
		{
			try
			{
				IEnumerable<IObjectColumn> objectColumns = await GetColumnsAsync();
				return objectColumns.OrderBy(x => x.Order).Select(x => x.Data);
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.ExportColumnDataError, ex);
			}
		}

		public async Task MapObjectColumnsAsync(Dictionary<String, String> metaData)
		{
			await Task.Run(async () =>
			{
				List<IObjectColumn> columns = (await GetColumnsAsync()).ToList();
				foreach (KeyValuePair<String, String> entry in metaData)
				{
					IObjectColumn matchingObjectColumn = columns.FirstOrDefault(x => x.ColumnName == entry.Key);
					if (matchingObjectColumn != null)
					{
						matchingObjectColumn.Data = entry.Value;
					}
				}
			});
		}

		public async Task<Dictionary<String, String>> GetColumnsAsDictionaryAsync()
		{
			Dictionary<String, String> columnsDictionary = new Dictionary<String, String>();

			await Task.Run(async () =>
			{
				IEnumerable<IObjectColumn> columns = (await GetColumnsAsync()).ToList();
				foreach (IObjectColumn currentColumn in columns)
				{
					columnsDictionary.Add(currentColumn.ColumnName, currentColumn.Data);
				}
			});

			return columnsDictionary;
		}
	}
}