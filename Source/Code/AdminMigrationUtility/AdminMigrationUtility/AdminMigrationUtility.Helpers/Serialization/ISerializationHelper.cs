using AdminMigrationUtility.Helpers.Models.Interfaces;
using System;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Helpers.Serialization
{
	public interface ISerializationHelper
	{
		Task<String> SerializeAdminObjectAsync(IAdminObject adminObject);

		Task<IAdminObject> DeserializeToAdminObjectAsync(String metaData);
	}
}