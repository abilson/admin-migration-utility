using AdminMigrationUtility.Helpers.Validation;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.NUnit.Validation
{
	[TestFixture]
	public class NotNullValidationTests
	{
		[Test]
		[TestCase(null)]
		[TestCase("")]
		[TestCase("      ")]
		public async Task VerifyMessageReturnedWhenNull(String input)
		{
			var notNullValidation = new ValidationNotNull();
			var result = await notNullValidation.ValidateAsync(input);
			Assert.AreEqual(Constant.Messages.Violations.NullValue, result);
		}

		[Test]
		[TestCase("Random Value")]
		public async Task VerifyNullMessageReturnedWhenInputContainsValue(String input)
		{
			var notNullValidation = new ValidationNotNull();
			var result = await notNullValidation.ValidateAsync(input);
			Assert.AreEqual(null, result);
		}
	}
}