using AdminMigrationUtility.Helpers.Validation;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.NUnit.Validation
{
	[TestFixture]
	public class RegExValidationTests
	{
		[Test]
		public async Task MessageReturnedWhenNoRexExMatchFound()
		{
			var input = "RandomNonMatchingPattern";
			var emailPattern = Constant.RegExPatterns.EmailAddress;
			var patternDescription = "email address";
			var expectedResult = String.Format(Constant.Messages.Violations.RegExMisMatch, patternDescription);
			var regExValidation = new ValidationRegEx(emailPattern, patternDescription);
			var result = await regExValidation.ValidateAsync(input);
			Assert.AreEqual(expectedResult, result);
		}

		[Test]
		public async Task NullReturnedWhenRexExPassesValidation()
		{
			var input = "Johndoe@web.com";
			var emailPattern = Constant.RegExPatterns.EmailAddress;
			var patternDescription = "email address";
			var regExValidation = new ValidationRegEx(emailPattern, patternDescription);
			var result = await regExValidation.ValidateAsync(input);
			Assert.AreEqual(null, result);
		}
	}
}