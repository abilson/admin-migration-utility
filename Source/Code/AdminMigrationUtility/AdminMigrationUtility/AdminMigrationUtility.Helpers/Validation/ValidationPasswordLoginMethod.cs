using AdminMigrationUtility.Helpers.Models.Interfaces;
using AdminMigrationUtility.Helpers.Validation.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.Validation
{
	public class ValidationPasswordLoginMethod : IValidation
	{
		public IObjectColumn CurrentColumn { get; protected set; }
		public IEnumerable<IObjectColumn> PasswordColumns { get; protected set; }

		public ValidationPasswordLoginMethod(IObjectColumn currentColumn, IEnumerable<IObjectColumn> passwordColumns)
		{
			CurrentColumn = currentColumn;
			PasswordColumns = passwordColumns;
		}

		public async Task<String> ValidateAsync(String input)
		{
			String validationMessage = null;
			if (await ValueExistsInPasswordColumns())
			{
				if (CurrentColumn.ColumnName == Constant.DelmitedFileColumnNames.LoginMethod.TwoFactorInfo)
				{
					validationMessage = await ValidateTwoFactorInfoAsync();
				}
				// Make sure the user entered a TwoFactorMode that exists in the TwoFactor enum
				else if (CurrentColumn.ColumnName == Constant.DelmitedFileColumnNames.LoginMethod.TwoFactorMode
				   && !Enum.IsDefined(typeof(Relativity.Services.Security.Models.TwoFactorMode), CurrentColumn.Data))
				{
					validationMessage = String.Format(Constant.Messages.Violations.InvalidTwoFactorMode, CurrentColumn.ColumnName, String.Join(",", Enum.GetNames(typeof(Relativity.Services.Security.Models.TwoFactorMode))));
				}
			}

			return validationMessage;
		}

		private async Task<String> ValidateTwoFactorInfoAsync()
		{
			String validationMessage = null;
			await Task.Run(() =>
			{
				var twoFactorModeColumn = PasswordColumns.FirstOrDefault(x => x.ColumnName == Constant.DelmitedFileColumnNames.LoginMethod.TwoFactorMode);
				if (twoFactorModeColumn != null && twoFactorModeColumn.Data != null)
				{
					// TwoFactorInfo Should be empty when TwoFactorMode is set to None
					if (twoFactorModeColumn.Data.Trim() == Relativity.Services.Security.Models.TwoFactorMode.None.ToString())
					{
						if (!String.IsNullOrWhiteSpace(CurrentColumn.Data))
						{
							validationMessage = String.Format(Constant.Messages.Violations.TwoFactorInfoShouldBeEmpty, CurrentColumn.ColumnName, Constant.DelmitedFileColumnNames.LoginMethod.TwoFactorMode, Relativity.Services.Security.Models.TwoFactorMode.None.ToString());
						}
					}
					else
					{
						//Two Factor info should contain an email address
						try
						{
							var match = System.Text.RegularExpressions.Regex.Match(CurrentColumn.Data, Constant.RegExPatterns.EmailAddress);
							if (!match.Success)
							{
								throw new Exception();
							}
						}
						catch (Exception)
						{
							validationMessage = String.Format(Constant.Messages.Violations.InvalidTwoFactorInfoEmailBadFormat, Constant.DelmitedFileColumnNames.LoginMethod.TwoFactorInfo, Relativity.Services.Security.Models.TwoFactorMode.None.ToString());
						}
					}
				}
			});
			return validationMessage;
		}

		// Check to see if any of the fields in the password field collection contain a value. When one is set, they all must contain a value
		private async Task<Boolean> ValueExistsInPasswordColumns()
		{
			var retVal = false;
			await Task.Run(() =>
			{
				foreach (var column in PasswordColumns)
				{
					if (!String.IsNullOrWhiteSpace(column.Data))
					{
						retVal = true;
					}
				}
			});
			return retVal;
		}
	}
}