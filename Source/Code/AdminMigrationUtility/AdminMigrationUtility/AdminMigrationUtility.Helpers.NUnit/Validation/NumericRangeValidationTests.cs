using AdminMigrationUtility.Helpers.Validation;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.NUnit.Validation
{
	[TestFixture]
	public class NumericRangeValidationTests
	{
		[Test]
		[TestCase(5, 15, "14")]
		public async Task VerifyNoMessageReturnedWhenInputInsideRange(Int32 min, Int32 max, String input)
		{
			var validator = new ValidationNumericRange(min, max, true);

			var result = await validator.ValidateAsync(input);
			Assert.AreEqual(null, result);
		}

		[Test]
		[TestCase(5, 15, "16")]
		public async Task VerifyMessageReturnedWhenInputOutOfRange(Int32 min, Int32 max, String input)
		{
			var validator = new ValidationNumericRange(min, max, true);

			var expectedMessage = String.Format(Constant.Messages.Violations.OutOfNumericRange, min, max);

			var result = await validator.ValidateAsync(input);
			Assert.AreEqual(expectedMessage, result);
		}

		[Test]
		[TestCase(5, 15, "Hello")]
		public async Task VerifyMessageReturnedWhenInputNotANumber(Int32 min, Int32 max, String input)
		{
			var validator = new ValidationNumericRange(min, max, true);

			var expectedMessage = String.Format(Constant.Messages.Violations.NotANumber);

			var result = await validator.ValidateAsync(input);
			Assert.AreEqual(expectedMessage, result);
		}

		[Test]
		[TestCase(5, 15, "5")]
		[TestCase(5, 15, "15")]
		public async Task VerifyInclusiveFunctionality(Int32 min, Int32 max, String input)
		{
			var validator = new ValidationNumericRange(min, max, true);

			var result = await validator.ValidateAsync(input);
			Assert.AreEqual(null, result);
		}

		[Test]
		[TestCase(5, 15, "5")]
		[TestCase(5, 15, "15")]
		public async Task VerifyExclusiveFunctionality(Int32 min, Int32 max, String input)
		{
			var validator = new ValidationNumericRange(min, max, false);

			var expectedMessage = String.Format(Constant.Messages.Violations.OutOfNumericRange, min, max);

			var result = await validator.ValidateAsync(input);
			Assert.AreEqual(expectedMessage, result);
		}
	}
}