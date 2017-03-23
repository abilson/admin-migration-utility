using AdminMigrationUtility.Helpers.Models;
using AdminMigrationUtility.Helpers.Models.AdminObject;
using AdminMigrationUtility.Helpers.Models.Interfaces;
using AdminMigrationUtility.Helpers.Rsapi.Interfaces;
using AdminMigrationUtility.Helpers.SQL;
using kCura.Relativity.Client;
using NUnit.Framework;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.NUnit.Models.AdminObject
{
	[TestFixture]
	public class ImportExportModelBaseTests
	{
		private class TestModel : AdminObjectBase
		{
			public IObjectColumn FirstName { get; set; } = new ObjectColumn<String>("FirstName");
			public IObjectColumn LastName { get; set; } = new ObjectColumn<String>("LastName");

			public override async Task<IEnumerable<String>> ImportAsync(APIOptions apiOptions, IRsapiRepositoryGroup repositoryGroup, IArtifactQueries artifactQueryHelper, IHelper helper, IDBContext eddsDbContext, ISqlQueryHelper sqlQueryHelper)
			{
				return await Task.Run(() => { return new List<string>(); });
			}

			public override async Task<IEnumerable<String>> ValidateAsync(APIOptions apiOptions, IRsapiRepositoryGroup rsapiRepositoryGroup, IArtifactQueries artifactQueryHelper, IDBContext eddsDbContext, ISqlQueryHelper sqlQueryHelper)
			{
				return await Task.Run(() => { return new List<string>(); });
			}
		}

		[Test]
		public async Task GetColumnsOnlyReturnsIObjectColumns()
		{
			var testObj = new TestModel();
			var objectColumns = await testObj.GetColumnsAsync();
			Assert.IsTrue(objectColumns.Count() == 2);
		}

		[Test]
		public async Task DictionaryCorrectlyMapsToObjectColumns()
		{
			var firstNameValue = "John";
			var lastNameValue = "Doe";
			var input = new Dictionary<String, String>()
			{
				{ "FirstName", "John"},
				{ "LastName", "Doe"},
				{ "NonExistingColumnName", "Fake"}
			};

			var model = new TestModel();
			await model.MapObjectColumnsAsync(input);
			Assert.AreEqual(firstNameValue, model.FirstName.Data);
			Assert.AreEqual(lastNameValue, model.LastName.Data);
		}
	}
}