using AdminMigrationUtility.Helpers.Exceptions;
using AdminMigrationUtility.Helpers.Models;
using AdminMigrationUtility.Helpers.Models.AdminObject;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.NUnit
{
	[TestFixture]
	public class UtilityTests
	{
		#region Tests

		[Test]
		public async Task ExceptionThrownWhenUnsupportedImportObjectSelected()
		{
			//Arrage
			var selectedObjectType = "FakeUnsupportedObject";

			//Act
			var resultingException = await Task.Run(() => Assert.Throws<AdminMigrationUtilityException>(async () => await Utility.GetImportObjectSelectionAsync(selectedObjectType)));

			//Assert
			Assert.AreEqual(typeof(AdminMigrationUtilityException), resultingException.GetType());
		}

		[Test]
		public async Task CorrectObjectTypeIsReturned()
		{
			//Arrage
			var selectedObjectType = "User";
			var expectedReturnType = typeof(UserAdminObject);

			//Act
			var resultingObject = await Utility.GetImportObjectSelectionAsync(selectedObjectType);

			//Assert
			Assert.AreEqual(expectedReturnType, resultingObject.GetType());
		}

		[Test]
		[TestCase("Hel\"lo", "\"Hel\"\"lo\"")]
		public async Task QuotesAreEscapedInCsvCells(String input, String expected)
		{
			//Act
			var resultingString = await Utility.FormatCsvCellAsync(input);

			//Assert
			Assert.AreEqual(expected, resultingString);
		}

		[Test]
		[TestCase("Hel,lo", "\"Hel,lo\"")]
		[TestCase("Hel\"lo", "\"Hel\"\"lo\"")]
		public async Task CellsAreSurroundedInQuotesWhenTheyContainReservedCharacters(String input, String expected)
		{
			//Act
			var resultingString = await Utility.FormatCsvCellAsync(input);

			//Assert
			Assert.AreEqual(expected, resultingString);
		}

		[Test]
		[TestCase(new[] { "column1", "column," }, @"column1,""column,""")]
		public async Task CsvLineIsFormattedCorrectly(IEnumerable<String> input, String expected)
		{
			//Act
			var resultingCsvLine = await Utility.FormatCsvLineAsync(input);

			//Assert
			Assert.AreEqual(expected, resultingCsvLine);
		}

		[Test]
		[TestCase(new Int32[] { 1 }, 5, 1, 3)]
		[TestCase(new Int32[] { 1, 1 }, 5, 1, 3)]
		[TestCase(new Int32[] { 1, 1, 2 }, 5, 1, 2)]
		[TestCase(new Int32[] { }, 5, 1, 4)]
		[TestCase(new Int32[] { }, 5, 0, 5)]
		[TestCase(new Int32[] { 1, 1 }, 5, 0, 4)]
		[TestCase(new Int32[] { 1, 2 }, 5, 0, 3)]
		public void NumberOfImportsCorrectlyCalculated(IEnumerable<Int32> errorLineNumbers, Int32 expectedNumberOfImports, Int32 numberOfWorkerRecords, Int32 expectedResult)
		{
			//Arrange
			var errors = new List<ImportJobError>();
			foreach (var lineNumber in errorLineNumbers)
			{
				errors.Add(
					new ImportJobError() { LineNumber = lineNumber, Type = Constant.ImportUtilityJob.ErrorType.DataLevel, Message = "Random Message" }
					);
			}

			//Act
			var resultingCalculatedImports = Utility.CalculateImportJobImports(errors, expectedNumberOfImports, numberOfWorkerRecords);

			//Assert
			Assert.AreEqual(expectedResult, resultingCalculatedImports);
		}

		[Test]
		[TestCase(new Int32[] { 1 }, 5, 1, 2)]
		[TestCase(new Int32[] { 1, 1 }, 5, 1, 2)]
		[TestCase(new Int32[] { 1, 1, 2 }, 5, 1, 3)]
		[TestCase(new Int32[] { }, 5, 1, 0)]
		[TestCase(new Int32[] { }, 5, 0, 0)]
		[TestCase(new Int32[] { 1, 1 }, 5, 0, 1)]
		[TestCase(new Int32[] { 1, 2 }, 5, 0, 2)]
		public void NumberOfObjectsNotImportsCorrectlyCalculated(IEnumerable<Int32> errorLineNumbers, Int32 expectedNumberOfImports, Int32 numberOfWorkerRecords, Int32 expectedResult)
		{
			//Arrange
			var errors = new List<ImportJobError>();
			foreach (var lineNumber in errorLineNumbers)
			{
				errors.Add(
					new ImportJobError() { LineNumber = lineNumber, Type = Constant.ImportUtilityJob.ErrorType.DataLevel, Message = "Random Message" }
					);
			}

			//Act
			var resultingCalculatedImports = Utility.CalculateImportJobObjectsThatWereNotImported(errors, expectedNumberOfImports, numberOfWorkerRecords);

			//Assert
			Assert.AreEqual(expectedResult, resultingCalculatedImports);
		}

		[Test]
		[TestCase(new String[] { "[Column1]" }, "ORDER BY [Column1]")]
		[TestCase(new String[] { "Column1" }, "ORDER BY [Column1]")]
		[TestCase(new String[] { "Column1", "Column2" }, "ORDER BY [Column1],[Column2]")]
		[TestCase(new String[] {  }, "")]
		public void OrderByColumnsAreFormattedCorrectly(IEnumerable<String> input, String expected)
		{
			//Act
			var resultingString = Utility.FormatOrderByColumns(input);

			//Assert
			Assert.AreEqual(expected, resultingString);
		}

		#endregion Tests
	}
}