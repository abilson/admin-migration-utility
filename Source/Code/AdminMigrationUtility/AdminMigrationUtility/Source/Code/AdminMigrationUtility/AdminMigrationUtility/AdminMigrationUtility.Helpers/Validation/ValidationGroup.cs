using AdminMigrationUtility.Helpers.Rsapi;
using AdminMigrationUtility.Helpers.Validation.Interfaces;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using kCura.Relativity.Client.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.Validation
{
	public class ValidationGroup : IValidation
	{
		private APIOptions _apiOptions;
		private IGenericRepository<Group> _groupRepository;
		private GroupDelegate _callback;

		public ValidationGroup(APIOptions apiOptions, IGenericRepository<Group> groupRepository, GroupDelegate callback)
		{
			_apiOptions = apiOptions;
			_groupRepository = groupRepository;
			_callback = callback;
		}

		public async Task<String> ValidateAsync(String input)
		{
			String validationMessage = null;
			var returnGroups = new FieldValueList<Group>();
			if (!String.IsNullOrWhiteSpace(input))
			{
				try
				{
					input = input.Trim(Constant.ViolationDelimiter.ToCharArray()[0]);
					var groupList = input.Split(Constant.ViolationDelimiter.ToCharArray()[0]);
					var missingGroupList = new List<String>();
					var query = new ArtifactQueries();
					var groupQueryResult = await query.QueryGroupsAsync(_apiOptions, _groupRepository, groupList);
					if (groupQueryResult.Success && groupQueryResult.Results.Count > 0)
					{
						var results = groupQueryResult.Results;
						foreach (var group in groupList)
						{
							var matchingGroup = results.FirstOrDefault(x => x.Artifact.Name == group.Trim());
							if (matchingGroup == null)
							{
								missingGroupList.Add(group);
							}
							else
							{
								returnGroups.Add(new Group(matchingGroup.Artifact.ArtifactID));
							}
						}

						if (missingGroupList.Any())
						{
							validationMessage = String.Format(Constant.Messages.Violations.GroupDoesNotExist, String.Join(Constant.ViolationDelimiter, missingGroupList));
						}
					}
					else if (groupQueryResult.Success)
					{
						validationMessage = String.Format(Constant.Messages.Violations.GroupDoesNotExist, input);
					}
					else
					{
						validationMessage = groupQueryResult.Message;
					}
				}
				catch (Exception ex)
				{
					validationMessage = String.Format(Constant.Messages.Violations.Exception, $"querying the following querying the groups related to this user: {input}", ex);
				}
			}
			_callback(returnGroups);

			return validationMessage;
		}
	}
}