﻿using AdminMigrationUtility.Helpers.Rsapi;
using kCura.Relativity.Client.DTOs;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AdminMigrationUtility.Helpers.NUnit
{
	[TestFixture]
	public class ResponseTests
	{
		[Test]
		public void CompileWriteResult_ReceivesSuccessfulSet_ReturnsParsedResults()
		{
			// Arrange
			var results = new WriteResultSet<RDO>()
			{
				Message = "This is my global message",
				Success = true,
				Results = new List<Result<RDO>>()
																		{
																			new Result<RDO>()
																				{
																					Artifact = new RDO(),
																					Message = "This is a test message",
																					Success = true
																				}
																		}
			};

			// Act
			var actual = Response<Int32>.CompileWriteResults(results);

			// Assert
			Assert.IsTrue(actual.Success);
			Assert.AreEqual(String.Empty, actual.Message);
			Assert.Greater(actual.Results.Count(), 0);
		}

		[Test]
		public void CompileWriteResult_ReceivesFailedSet_ReturnsFormattedMessageWithResults()
		{
			// Arrange
			var results = new WriteResultSet<RDO>()
			{
				Message = "This is my global message",
				Success = false,
				Results = new List<Result<RDO>>()
																		{
																			new Result<RDO>()
																				{
																					Artifact = new RDO(),
																					Message = "This is a test message",
																					Success = true
																				},
																			new Result<RDO>()
																				{
																					Artifact = new RDO(),
																					Message = "This is another test message",
																					Success = false
																				}
																		}
			};

			// Act
			var actual = Response<Int32>.CompileWriteResults(results);

			// Assert
			Assert.IsFalse(actual.Success);
			Assert.AreEqual("This is my global messageThis is a test messageThis is another test message", actual.Message);
			Assert.Greater(actual.Results.Count(), 0);
		}
	}
}