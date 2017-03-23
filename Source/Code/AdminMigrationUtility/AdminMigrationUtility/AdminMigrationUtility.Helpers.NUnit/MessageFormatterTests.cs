using AdminMigrationUtility.Helpers.Rsapi;
using NUnit.Framework;
using System;
using System.Collections.Generic;

namespace AdminMigrationUtility.Helpers.NUnit
{
	[TestFixture]
	public class MessageFormatterTests
	{
		[Description("When the results were successful, should return an empty string")]
		[Test]
		public void FormatMessage_ReceivesSuccessfulResults_ReturnsEmpty()
		{
			// Arrange
			var success = true;

			// Act
			String actual = MessageFormatter.FormatMessage(new List<string>(), String.Empty, success);

			// Assert
			Assert.AreEqual(String.Empty, actual);
		}

		[Description("When the results were unsuccessful, should return the message with each result appended")]
		[Test]
		public void FormatMessage_RecievesFailedResults_ReturnsFormattedString()
		{
			// Arrange
			var results = new List<String>()
											{
												"First result",
												"Second result",
												"Third result"
											};
			var message = "This is my test message";
			var success = false;

			// Act
			String actual = MessageFormatter.FormatMessage(results, message, success);

			// Assert
			Assert.AreEqual("This is my test messageFirst resultSecond resultThird result", actual);
		}
	}
}