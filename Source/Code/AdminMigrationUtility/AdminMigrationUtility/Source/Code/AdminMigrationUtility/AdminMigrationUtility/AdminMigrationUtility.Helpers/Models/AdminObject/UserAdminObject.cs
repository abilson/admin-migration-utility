using AdminMigrationUtility.Helpers.Models.Interfaces;
using AdminMigrationUtility.Helpers.Rsapi.Interfaces;
using AdminMigrationUtility.Helpers.SQL;
using AdminMigrationUtility.Helpers.Validation;
using AdminMigrationUtility.Helpers.Validation.Interfaces;
using kCura.Relativity.Client;
using kCura.Relativity.Client.Repositories;
using Relativity.API;
using Relativity.Services.Security;
using Relativity.Services.Security.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.Models.AdminObject
{
	public class UserAdminObject : AdminObjectBase
	{
		public Boolean Validated { get; protected set; }
		public Int32 ArtifactId { get; set; }

		#region PrivateImportVariables

		private kCura.Relativity.Client.DTOs.Client _client;
		private kCura.Relativity.Client.DTOs.FieldValueList<kCura.Relativity.Client.DTOs.Group> _groupList;
		private kCura.Relativity.Client.DTOs.Choice _userType;
		private kCura.Relativity.Client.DTOs.Choice _documentSkip;
		private kCura.Relativity.Client.DTOs.Choice _defaultSelectedFileType;
		private kCura.Relativity.Client.DTOs.Choice _skipDefaultPreference;
		private kCura.Relativity.Client.DTOs.Choice _documentViewer;

		#endregion PrivateImportVariables

		#region UserFields

		public IObjectColumn FirstName { get; set; } = new ObjectColumn<String>(Constant.DelmitedFileColumnNames.UserAdminObject.FirstName)
		{
			ObjectFieldType = Constant.Enums.ObjectFieldType.Standard,
			Validations = new List<IValidation>() {
				new ValidationNotNull(),
				new ValidationCharacterLimit(50)}
		};

		public IObjectColumn LastName { get; set; } = new ObjectColumn<String>(Constant.DelmitedFileColumnNames.UserAdminObject.LastName)
		{
			ObjectFieldType = Constant.Enums.ObjectFieldType.Standard,
			Validations = new List<IValidation>() {
				new ValidationNotNull(),
				new ValidationCharacterLimit(50)}
		};

		public IObjectColumn EmailAddress { get; set; } = new ObjectColumn<String>(Constant.DelmitedFileColumnNames.UserAdminObject.EmailAddress)
		{
			ObjectFieldType = Constant.Enums.ObjectFieldType.Standard,
			Validations = new List<IValidation>() {
				new ValidationNotNull(),
				new ValidationRegEx(Helpers.Constant.RegExPatterns.EmailAddress, "email address")}
		};

		public IObjectColumn Keywords { get; set; } = new ObjectColumn<String>(Constant.DelmitedFileColumnNames.UserAdminObject.Keywords)
		{
			ObjectFieldType = Constant.Enums.ObjectFieldType.Standard,
			Validations = new List<IValidation>() {
				new ValidationCharacterLimit(50)}
		};

		public IObjectColumn Notes { get; set; } = new ObjectColumn<String>(Constant.DelmitedFileColumnNames.UserAdminObject.Notes)
		{
			ObjectFieldType = Constant.Enums.ObjectFieldType.Standard
		};

		public IObjectColumn Groups { get; set; } = new ObjectColumn<String>(Constant.DelmitedFileColumnNames.UserAdminObject.Groups)
		{
			ObjectFieldType = Constant.Enums.ObjectFieldType.Standard
		};

		public IObjectColumn Type { get; set; } = new ObjectColumn<String>(Constant.DelmitedFileColumnNames.UserAdminObject.Type) //Int32
		{
			ObjectFieldType = Constant.Enums.ObjectFieldType.Standard,
			Validations = new List<IValidation>() {
				new ValidationNotNull() }
		};

		public IObjectColumn Client { get; set; } = new ObjectColumn<String>(Constant.DelmitedFileColumnNames.UserAdminObject.Client) //DTO.Client
		{
			ObjectFieldType = Constant.Enums.ObjectFieldType.Standard,
			Validations = new List<IValidation>() {
				new ValidationNotNull() }
		};

		public IObjectColumn RelativityAccess { get; set; } = new ObjectColumn<Boolean?>(Constant.DelmitedFileColumnNames.UserAdminObject.RelativityAccess)
		{
			ObjectFieldType = Constant.Enums.ObjectFieldType.Standard,
			Validations = new List<IValidation>() {
				new ValidationNotNull() }
		};

		public IObjectColumn DocumentSkip { get; set; } = new ObjectColumn<String>(Constant.DelmitedFileColumnNames.UserAdminObject.DocumentSkip) //Int32
		{
			ObjectFieldType = Constant.Enums.ObjectFieldType.Standard,
			Validations = new List<IValidation>() {
				new ValidationNotNull() }
		};

		public IObjectColumn BetaUser { get; set; } = new ObjectColumn<Boolean?>(Constant.DelmitedFileColumnNames.UserAdminObject.BetaUser)
		{
			ObjectFieldType = Constant.Enums.ObjectFieldType.Standard,
			Validations = new List<IValidation>() {
				new ValidationNotNull() }
		};

		public IObjectColumn ChangeSettings { get; set; } = new ObjectColumn<Boolean?>(Constant.DelmitedFileColumnNames.UserAdminObject.ChangeSettings)
		{
			ObjectFieldType = Constant.Enums.ObjectFieldType.Standard,
			Validations = new List<IValidation>() {
				new ValidationNotNull() }
		};

		public IObjectColumn KeyboardShortcuts { get; set; } = new ObjectColumn<Boolean?>(Constant.DelmitedFileColumnNames.UserAdminObject.KeyboardShortcuts)
		{
			ObjectFieldType = Constant.Enums.ObjectFieldType.Standard,
			Validations = new List<IValidation>() {
				new ValidationNotNull() }
		};

		public IObjectColumn ItemListPageLength { get; set; } = new ObjectColumn<Int32?>(Constant.DelmitedFileColumnNames.UserAdminObject.ItemListPageLength)
		{
			ObjectFieldType = Constant.Enums.ObjectFieldType.Standard,
			Validations = new List<IValidation>() {
				new ValidationNotNull(),
				new ValidationNumericRange(1,200, true)}
		};

		public IObjectColumn DefaultSelectedFileType { get; set; } = new ObjectColumn<String>(Constant.DelmitedFileColumnNames.UserAdminObject.DefaultSelectedFileType) //Int32
		{
			ObjectFieldType = Constant.Enums.ObjectFieldType.Standard,
			Validations = new List<IValidation>() {
				new ValidationNotNull() }
		};

		public IObjectColumn SkipDefaultPreference { get; set; } = new ObjectColumn<String>(Constant.DelmitedFileColumnNames.UserAdminObject.SkipDefaultPreference) //Int32
		{
			ObjectFieldType = Constant.Enums.ObjectFieldType.Standard,
			Validations = new List<IValidation>() {
				new ValidationNotNull() }
		};

		public IObjectColumn EnforceViewerCompatibility { get; set; } = new ObjectColumn<Boolean?>(Constant.DelmitedFileColumnNames.UserAdminObject.EnforceViewerCompatibility)
		{
			ObjectFieldType = Constant.Enums.ObjectFieldType.Standard,
			Validations = new List<IValidation>() {
				new ValidationNotNull() }
		};

		public IObjectColumn AdvancedSearchPublicByDefault { get; set; } = new ObjectColumn<Boolean?>(Constant.DelmitedFileColumnNames.UserAdminObject.AdvancedSearchPublicByDefault)
		{
			ObjectFieldType = Constant.Enums.ObjectFieldType.Standard,
			Validations = new List<IValidation>() {
				new ValidationNotNull() }
		};

		public IObjectColumn NativeViewerCacheAhead { get; set; } = new ObjectColumn<Boolean?>(Constant.DelmitedFileColumnNames.UserAdminObject.NativeViewerCacheAhead)
		{
			ObjectFieldType = Constant.Enums.ObjectFieldType.Standard,
			Validations = new List<IValidation>() {
				new ValidationNotNull() }
		};

		public IObjectColumn ChangeDocumentViewer { get; set; } = new ObjectColumn<Boolean?>(Constant.DelmitedFileColumnNames.UserAdminObject.ChangeDocumentViewer)
		{
			ObjectFieldType = Constant.Enums.ObjectFieldType.Standard
		};

		public IObjectColumn DocumentViewer { get; set; } = new ObjectColumn<String>(Constant.DelmitedFileColumnNames.UserAdminObject.DocumentViewer) //DTO.choice
		{
			ObjectFieldType = Constant.Enums.ObjectFieldType.Standard
		};

		public IObjectColumn WindowsAccount { get; set; } = new ObjectColumn<String>(Constant.DelmitedFileColumnNames.LoginMethod.WindowsAccount) //IntergratedAuthentication_WindowsAccount
		{
			ObjectFieldType = Constant.Enums.ObjectFieldType.IntegratedLoginMethod
		};

		public IObjectColumn UserMustChangePasswordOnNextLogin { get; set; } = new ObjectColumn<Boolean?>(Constant.DelmitedFileColumnNames.UserAdminObject.UserMustChangePasswordOnNextLogin) //PasswordAuthentication_UserMustChangePasswordOnNextLogin
		{
			ObjectFieldType = Constant.Enums.ObjectFieldType.Standard,
			Validations = new List<IValidation>()
			{
				new ValidationNotNull()
			}
		};

		public IObjectColumn CanChangePassword { get; set; } = new ObjectColumn<Boolean?>(Constant.DelmitedFileColumnNames.UserAdminObject.CanChangePassword) //PasswordAuthentication_CanChangePassword
		{
			ObjectFieldType = Constant.Enums.ObjectFieldType.Standard,
			Validations = new List<IValidation>()
			{
				new ValidationNotNull()
			}
		};

		public IObjectColumn MaximumPasswordAgeInDays { get; set; } = new ObjectColumn<Int32?>(Constant.DelmitedFileColumnNames.UserAdminObject.MaximumPasswordAgeInDays) //PasswordAuthentication_MaximumPasswordAgeInDays
		{
			ObjectFieldType = Constant.Enums.ObjectFieldType.Standard,
			Validations = new List<IValidation>()
			{
				new ValidationNotNull(),
				new ValidationNumericRange(0, 1000, true)
			}
		};

		public IObjectColumn TwoFactorMode { get; set; } = new ObjectColumn<String>(Constant.DelmitedFileColumnNames.LoginMethod.TwoFactorMode) //PasswordAuthentication_TwoFactorMode
		{
			ObjectFieldType = Constant.Enums.ObjectFieldType.PasswordLoginMethod,
			Validations = new List<IValidation>()
			{
			}
		};

		public IObjectColumn TwoFactorInfo { get; set; } = new ObjectColumn<String>(Constant.DelmitedFileColumnNames.LoginMethod.TwoFactorInfo) //PasswordAuthentication_TwoFactorInfo
		{
			ObjectFieldType = Constant.Enums.ObjectFieldType.PasswordLoginMethod,
			Validations = new List<IValidation>()
			{
			}
		};

		#endregion UserFields

		public UserAdminObject()
		{
			Validated = false;
		}

		#region PublicMethods

		public override async Task<IEnumerable<String>> ImportAsync(APIOptions apiOptions, IRsapiRepositoryGroup repositoryGroup, IArtifactQueries artifactQueryHelper, IHelper helper, IDBContext eddsDbContext, ISqlQueryHelper sqlQueryHelper)
		{
			var violations = new List<string>();

			try
			{
				// Skip import if user already exists
				if (await UserExistsInRelativity(apiOptions, repositoryGroup.UserRepository, artifactQueryHelper, (String)(await EmailAddress.GetDataValueAsync() ?? "")))
				{
					var msg = String.Format(Constant.ErrorMessages.UserAlreadyExists, (String)await EmailAddress.GetDataValueAsync());
					violations.Add(msg);
				}
				else
				{
					// Make sure user is validated
					if (Validated == false)
					{
						violations.AddRange(await ValidateAsync(apiOptions, repositoryGroup, artifactQueryHelper, eddsDbContext, sqlQueryHelper));
					}

					// Import user object
					if (!violations.Any())
					{
						violations.AddRange(await ImportUserAsync(apiOptions, repositoryGroup.UserRepository));
					}

					// Save the User's keywords and notes
					if (!violations.Any())
					{
						violations.AddRange(await SaveUserKeywordAndNotesAsync(sqlQueryHelper, eddsDbContext));
					}

					// Import the Login Method Object
					if (!violations.Any())
					{
						violations.AddRange(await ImportLoginMethodAsync(helper));
					}

					if (violations.Any())
					{
						// Check to see if user was partially imported
						if (await UserExistsInRelativity(apiOptions, repositoryGroup.UserRepository, artifactQueryHelper, (String)(await EmailAddress.GetDataValueAsync() ?? "")))
						{
							// Only prepend msg and present to user if the user didn't already exists
							var userAlreadyExistsMsg = String.Format(Constant.ErrorMessages.UserAlreadyExists, (String)await EmailAddress.GetDataValueAsync());
							if(!violations.Any(x => x.Contains(userAlreadyExistsMsg)))
							{
								violations = violations.Select(x => Constant.ErrorMessages.UserPartiallyImportedPrepend + x).ToList();
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				violations.Add(ex.ToString());
			}

			return violations;
		}

		public override async Task<IEnumerable<String>> ValidateAsync(APIOptions apiOptions, IRsapiRepositoryGroup rsapiRepositoryGroup, IArtifactQueries artifactQueryHelper, IDBContext eddsDbContext, ISqlQueryHelper sqlQueryHelper)
		{
			var violations = new List<String>();
			await AddPasswordValidationToPasswordFieldsAsync();

			// Add Client Validator
			await AddValidationToColumnAsync(Client, new ValidationClient(apiOptions, rsapiRepositoryGroup.ClientRepository, x => _client = x));

			// Add Validators for userType, documentSkip, defaultSelectedFileType, skipDefaultPreference & documentViewer
			await AddValidationToColumnAsync(Type, new ValidationCodeName(sqlQueryHelper, eddsDbContext, Constant.CodeTypeName.UserType, x => _userType = x));
			await AddValidationToColumnAsync(DocumentSkip, new ValidationCodeName(sqlQueryHelper, eddsDbContext, Constant.CodeTypeName.DocumentSkip, x => _documentSkip = x));
			await AddValidationToColumnAsync(DefaultSelectedFileType, new ValidationCodeName(sqlQueryHelper, eddsDbContext, Constant.CodeTypeName.DefaultSelectedFileType, x => _defaultSelectedFileType = x));
			await AddValidationToColumnAsync(SkipDefaultPreference, new ValidationCodeName(sqlQueryHelper, eddsDbContext, Constant.CodeTypeName.SkipDefaultPreference, x => _skipDefaultPreference = x));
			await AddValidationToColumnAsync(DocumentViewer, new ValidationCodeName(sqlQueryHelper, eddsDbContext, Constant.CodeTypeName.DocumentViewer, x => _documentViewer = x));

			// Add Validator for Groups
			await AddValidationToColumnAsync(Groups, new ValidationGroup(apiOptions, rsapiRepositoryGroup.GroupRepository, x => _groupList = x));

			// Call Validation on each ObjectColumn. Note performance does NOT increase by turning this into a parallel loop
			foreach (var column in (await GetColumnsAsync()))
			{
				var validationResults = await column.ValidateAsync();
				if (validationResults != null && validationResults.Any())
				{
					var columnViolations = String.Join(Constant.ViolationDelimiter, validationResults);
					var finalViolationMessage = String.Format(Constant.Messages.Violations.GeneralColumnViolation, column.ColumnName, columnViolations);
					violations.Add(finalViolationMessage);
				}
			}

			// Update this user's validation status
			Validated = !violations.Any();

			return violations;
		}

		#endregion PublicMethods

		#region privateMethods

		private async Task<Boolean> UserExistsInRelativity(APIOptions apiOptions, IGenericRepository<kCura.Relativity.Client.DTOs.User> userRepository, IArtifactQueries artifactQueryHelper, String emailAddress)
		{
			var retVal = false;
			var userArtifactID = await artifactQueryHelper.FindUserByEmailAddressAsync(apiOptions, userRepository, emailAddress);
			if (userArtifactID != null)
			{
				retVal = true;
			}
			return retVal;
		}

		private async Task<IEnumerable<String>> ImportUserAsync(APIOptions apiOptions, IGenericRepository<kCura.Relativity.Client.DTOs.User> userRepository)
		{
			List<String> violations = new List<string>();
			kCura.Relativity.Client.DTOs.User user = null;
			try
			{
				apiOptions.WorkspaceID = -1;
				user = new kCura.Relativity.Client.DTOs.User()
				{
					FirstName = (String) await FirstName.GetDataValueAsync(),
					LastName = (String) await LastName.GetDataValueAsync(),
					EmailAddress = (String) await EmailAddress.GetDataValueAsync(),
					Groups = _groupList,
					Type = _userType,
					Client = _client,
					RelativityAccess = ((Boolean?) await RelativityAccess.GetDataValueAsync()).GetValueOrDefault(),
					DocumentSkip = _documentSkip,
					BetaUser = ((Boolean?) await BetaUser.GetDataValueAsync()).GetValueOrDefault(),
					ChangeSettings = ((Boolean?) await ChangeSettings.GetDataValueAsync()).GetValueOrDefault(),
					KeyboardShortcuts = ((Boolean?) await KeyboardShortcuts.GetDataValueAsync()).GetValueOrDefault(),
					ItemListPageLength = ((Int32?) await ItemListPageLength.GetDataValueAsync()).GetValueOrDefault(),
					DefaultSelectedFileType = _defaultSelectedFileType,
					SkipDefaultPreference = _skipDefaultPreference,
					EnforceViewerCompatibility = ((Boolean?) await EnforceViewerCompatibility.GetDataValueAsync()).GetValueOrDefault(),
					AdvancedSearchPublicByDefault = ((Boolean?) await AdvancedSearchPublicByDefault.GetDataValueAsync()).GetValueOrDefault(),
					NativeViewerCacheAhead = ((Boolean?) await NativeViewerCacheAhead.GetDataValueAsync()).GetValueOrDefault(),
					CanChangeDocumentViewer = ((Boolean?) await ChangeDocumentViewer.GetDataValueAsync()).GetValueOrDefault(),
					DocumentViewer = _documentViewer,
					TrustedIPs = String.Empty,
					ChangePassword = ((Boolean?) await CanChangePassword.GetDataValueAsync()).GetValueOrDefault(),
					MaximumPasswordAge = ((Int32?) await MaximumPasswordAgeInDays.GetDataValueAsync()).GetValueOrDefault(),
					DataFocus = 1, // This field is no longer utilized in Relativity, however it's hardcoded because it is required
					ChangePasswordNextLogin = ((Boolean?) await UserMustChangePasswordOnNextLogin.GetDataValueAsync()).GetValueOrDefault()
				};

				ArtifactId = userRepository.CreateSingle(user);
			}
			catch (Exception ex)
			{
				String msg = null;
				if (ex.ToString().Contains("The entered E-Mail Address is already associated with a user") && user != null && !String.IsNullOrWhiteSpace(user.EmailAddress))
				{
					msg = String.Format(Constant.ErrorMessages.UserAlreadyExists, user.EmailAddress);
				}
				else
				{
					msg = ex.ToString();
				}
				violations.Add(msg);
			}
			return violations;
	}

		private async Task<IEnumerable<String>> ImportLoginMethodAsync(IHelper helper)
		{
			List<String> violations = new List<string>();

			try
			{
				using (var loginManager = helper.GetServicesManager().CreateProxy<ILoginProfileManager>(ExecutionIdentity.CurrentUser))
				{
					var userLoginProfile = await loginManager.GetLoginProfileAsync(ArtifactId);

					if (!String.IsNullOrWhiteSpace(TwoFactorMode.Data) &&
						!String.IsNullOrWhiteSpace(TwoFactorInfo.Data))
					{
						userLoginProfile.Password = new PasswordMethod()
						{
							IsEnabled = true,
							MustResetPasswordOnNextLogin = ((Boolean?)(await UserMustChangePasswordOnNextLogin.GetDataValueAsync())).GetValueOrDefault(),
							UserCanChangePassword = ((Boolean?)(await CanChangePassword.GetDataValueAsync())).GetValueOrDefault(),
							PasswordExpirationInDays = ((Int32?)(await MaximumPasswordAgeInDays.GetDataValueAsync())).GetValueOrDefault(),
							TwoFactorMode = (TwoFactorMode)Enum.Parse(typeof(TwoFactorMode), (String)(await TwoFactorMode.GetDataValueAsync())),
							TwoFactorInfo = ((String)(await TwoFactorInfo.GetDataValueAsync()))
						};
					}

					if (!String.IsNullOrWhiteSpace(WindowsAccount.Data))
					{
						userLoginProfile.IntegratedAuthentication = new IntegratedAuthenticationMethod()
						{
							IsEnabled = true,
							Account = (String)(await WindowsAccount.GetDataValueAsync())
						};
					}

					await loginManager.SaveLoginProfileAsync(userLoginProfile);
				}
			}
			catch (Exception ex)
			{
				violations.Add(ex.ToString());
			}

			return violations;
		}

		private async Task<IEnumerable<String>> SaveUserKeywordAndNotesAsync(ISqlQueryHelper sqlQueryHelper, IDBContext eddsDbContext)
		{
			List<String> violations = new List<string>();
			try
			{
				var keywords = (String)(await Keywords.GetDataValueAsync());
				var notes = (String)(await Notes.GetDataValueAsync());
				if (!String.IsNullOrWhiteSpace(keywords) || !String.IsNullOrWhiteSpace(notes))
				{
					await sqlQueryHelper.UpdateKeywordAndNotes(eddsDbContext, ArtifactId, keywords, notes);
				}
			}
			catch (Exception ex)
			{
				violations.Add(ex.ToString());
			}

			return violations;
		}

		private async Task AddPasswordValidationToPasswordFieldsAsync()
		{
			var passwordColumns = (await GetColumnsAsync()).Where(x => x.ObjectFieldType == Constant.Enums.ObjectFieldType.PasswordLoginMethod);
			foreach (var column in passwordColumns)
			{
				await AddValidationToColumnAsync(column, new ValidationPasswordLoginMethod(column, passwordColumns));
			}
		}

		private async Task AddValidationToColumnAsync(IObjectColumn column, IValidation newValidation)
		{
			await Task.Run(() =>
			{
				column.Validations = column.Validations ?? new List<IValidation>();
				if (column.Validations.Count(x => x.GetType() == newValidation.GetType()) < 1)
				{
					var updateValidations = new List<IValidation>();
					updateValidations.AddRange(column.Validations);
					updateValidations.Add(newValidation);
					column.Validations = updateValidations;
				}
			});
		}

		#endregion privateMethods
	}
}