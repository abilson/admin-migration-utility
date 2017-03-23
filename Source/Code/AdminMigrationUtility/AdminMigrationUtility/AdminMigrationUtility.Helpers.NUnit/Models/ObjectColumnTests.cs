using AdminMigrationUtility.Helpers.Models;
using AdminMigrationUtility.Helpers.Models.Interfaces;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.NUnit.Models
{
	[TestFixture]
	public class ObjectColumnTests
	{
		[Test]
		[TestCase(typeof(Int32), "", 0)]
		[TestCase(typeof(Int32), null, 0)]
		[TestCase(typeof(Int32?), "", null)]
		[TestCase(typeof(Int32?), null, null)]
		[TestCase(typeof(String), "", "")]
		[TestCase(typeof(String), null, "")]
		[TestCase(typeof(Boolean), "", false)]
		[TestCase(typeof(Boolean), null, false)]
		[TestCase(typeof(Boolean?), "", null)]
		[TestCase(typeof(Boolean?), null, null)]
		[TestCase(typeof(Boolean?), "true", true)]
		public async Task VerifyNullValuesReturnAsExpected(Type returnType, String inputData, Object expectedResult)
		{
			Type type = typeof(ObjectColumn<>).MakeGenericType(returnType);
			IObjectColumn myObject = (IObjectColumn)Activator.CreateInstance(type, "TestColumn");
			myObject.Data = inputData;

			var result = await myObject.GetDataValueAsync();
			Assert.AreEqual(expectedResult, result);
		}
	}
}