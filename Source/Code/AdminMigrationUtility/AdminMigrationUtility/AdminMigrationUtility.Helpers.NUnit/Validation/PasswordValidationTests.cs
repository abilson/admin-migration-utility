using AdminMigrationUtility.Helpers.Models;
using AdminMigrationUtility.Helpers.Models.Interfaces;
using AdminMigrationUtility.Helpers.Validation;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.NUnit.Validation
{
	[TestFixture]
	public class PasswordValidationTests
	{
		[Test]
		public async Task NoMessageReturnedWhenTwoFactorModeMatchesEnum()
		{
			var currentColumn = new ObjectColumn<Int32>(Constant.DelmitedFileColumnNames.LoginMethod.TwoFactorMode) { Data = Relativity.Services.Security.Models.TwoFactorMode.Always.ToString() };
			var objectColumns = new List<IObjectColumn>()
			{
				currentColumn,
				new ObjectColumn<String>(Constant.DelmitedFileColumnNames.UserAdminObject.CanChangePassword) { Data = "true"},
				new ObjectColumn<String>(Constant.DelmitedFileColumnNames.UserAdminObject.MaximumPasswordAgeInDays) { Data = "30"},
				new ObjectColumn<String>(Constant.DelmitedFileColumnNames.LoginMethod.TwoFactorInfo) { Data = "test@email.com"},
				new ObjectColumn<String>(Constant.DelmitedFileColumnNames.UserAdminObject.UserMustChangePasswordOnNextLogin) { Data = "false"}
			};
			var validation = new ValidationPasswordLoginMethod(currentColumn, objectColumns);
			var result = await validation.ValidateAsync(null);
			Assert.AreEqual(null, result);
		}

		[Test]
		public async Task MessageReturnedWhenTwoFactorModeDoesNotMatchEnum()
		{
			var columnName = Constant.DelmitedFileColumnNames.LoginMethod.TwoFactorMode;
			var currentColumn = new ObjectColumn<Int32>(columnName) { Data = "Random Non Existing two-factor mode" };
			var objectColumns = new List<IObjectColumn>()
			{
				currentColumn,
				new ObjectColumn<String>(Constant.DelmitedFileColumnNames.UserAdminObject.CanChangePassword) { Data = "true"},
				new ObjectColumn<String>(Constant.DelmitedFileColumnNames.UserAdminObject.MaximumPasswordAgeInDays) { Data = "30"},
				new ObjectColumn<String>(Constant.DelmitedFileColumnNames.LoginMethod.TwoFactorInfo) { Data = "test@email.com"},
				new ObjectColumn<String>(Constant.DelmitedFileColumnNames.UserAdminObject.UserMustChangePasswordOnNextLogin) { Data = "false"}
			};
			var validation = new ValidationPasswordLoginMethod(currentColumn, objectColumns);
			var expectedResult = String.Format(Constant.Messages.Violations.InvalidTwoFactorMode, columnName, String.Join(",", Enum.GetNames(typeof(Relativity.Services.Security.Models.TwoFactorMode))));
			var result = await validation.ValidateAsync(null);
			Assert.AreEqual(expectedResult, result);
		}

		[Test]
		public async Task MessageReturnedWhenTwoFactorInfoNotAnEmailAddress()
		{
			var columnName = Constant.DelmitedFileColumnNames.LoginMethod.TwoFactorInfo;
			var currentColumn = new ObjectColumn<String>(columnName) { Data = "NotAtSymbolNotAnEmailAddress" };
			var objectColumns = new List<IObjectColumn>()
			{
				currentColumn,
				new ObjectColumn<String>(Constant.DelmitedFileColumnNames.LoginMethod.TwoFactorMode) { Data = Relativity.Services.Security.Models.TwoFactorMode.Always.ToString()}
			};
			var validation = new ValidationPasswordLoginMethod(currentColumn, objectColumns);
			var expectedResult = String.Format(Constant.Messages.Violations.InvalidTwoFactorInfoEmailBadFormat, Constant.DelmitedFileColumnNames.LoginMethod.TwoFactorInfo, Relativity.Services.Security.Models.TwoFactorMode.None.ToString());
			var result = await validation.ValidateAsync(null);
			Assert.AreEqual(expectedResult, result);
		}

		[Test]
		public async Task MessageReturnedWhenTwoFactorModeNotNullAndTwoFactorInfoPopulated()
		{
			var columnName = Constant.DelmitedFileColumnNames.LoginMethod.TwoFactorInfo;
			var currentColumn = new ObjectColumn<Int32>(columnName) { Data = "test@email.com" };
			var objectColumns = new List<IObjectColumn>()
			{
				currentColumn,
				new ObjectColumn<String>(Constant.DelmitedFileColumnNames.LoginMethod.TwoFactorMode) { Data = Relativity.Services.Security.Models.TwoFactorMode.None.ToString()}
			};
			var validation = new ValidationPasswordLoginMethod(currentColumn, objectColumns);
			var expectedResult = String.Format(Constant.Messages.Violations.TwoFactorInfoShouldBeEmpty, columnName, Constant.DelmitedFileColumnNames.LoginMethod.TwoFactorMode, Relativity.Services.Security.Models.TwoFactorMode.None.ToString());
			var result = await validation.ValidateAsync(null);
			Assert.AreEqual(expectedResult, result);
		}
	}
}