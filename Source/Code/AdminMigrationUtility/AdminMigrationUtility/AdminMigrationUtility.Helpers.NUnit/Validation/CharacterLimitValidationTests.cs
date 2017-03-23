using AdminMigrationUtility.Helpers.Validation;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.NUnit.Validation
{
	[TestFixture]
	public class CharacterLimitValidationTests
	{
		[Test]
		[TestCase("123456")]
		public async Task VerifyMessageReturnedWhenMaxLengthExceeded(String input)
		{
			var maxLength = 5;
			var expectedMessage = String.Format(Constant.Messages.Violations.MaxLengthExceeded, maxLength);
			var notNullValidation = new ValidationCharacterLimit(5);
			var result = await notNullValidation.ValidateAsync(input);
			Assert.AreEqual(expectedMessage, result);
		}

		[Test]
		[TestCase("")]
		[TestCase("1234")]
		[TestCase("12345")]
		public async Task VerifyMessageNullWhenInputWithinMaxLength(String input)
		{
			var maxLength = 5;
			var notNullValidation = new ValidationCharacterLimit(maxLength);
			var result = await notNullValidation.ValidateAsync(input);
			Assert.AreEqual(null, result);
		}
	}
}