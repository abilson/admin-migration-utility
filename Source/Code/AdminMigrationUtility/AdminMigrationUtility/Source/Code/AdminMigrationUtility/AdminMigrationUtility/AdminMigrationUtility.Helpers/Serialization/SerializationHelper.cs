using AdminMigrationUtility.Helpers.Exceptions;
using AdminMigrationUtility.Helpers.Models.Interfaces;
using Newtonsoft.Json;
using System;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.Serialization
{
	public class SerializationHelper : ISerializationHelper
	{
		public async Task<String> SerializeAdminObjectAsync(IAdminObject adminObject)
		{
			try
			{
				return await Task.Run(() =>
				{
					String userMetadata = JsonConvert.SerializeObject(
						value: adminObject,
						formatting: Formatting.None,
						settings: new JsonSerializerSettings
						{
							TypeNameHandling = TypeNameHandling.Objects,
							TypeNameAssemblyFormat = System.Runtime.Serialization.Formatters.FormatterAssemblyStyle.Simple
						}
					);
					return userMetadata;
				});
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.AdminObjectSerializationError, ex);
			}
		}

		public async Task<IAdminObject> DeserializeToAdminObjectAsync(String metaData)
		{
			if (metaData == null)
			{
				throw new ArgumentException(nameof(metaData));
			}

			try
			{
				return await Task.Run(() =>
				{
					IAdminObject adminObject = JsonConvert.DeserializeObject<IAdminObject>(
						value: metaData,
						settings: new JsonSerializerSettings
						{
							TypeNameHandling = TypeNameHandling.Objects,
							ObjectCreationHandling = ObjectCreationHandling.Replace
						}
					);
					return adminObject;
				});
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.AdminObjectDeSerializationError, ex);
			}
		}
	}
}