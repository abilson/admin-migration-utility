using AdminMigrationUtility.Helpers.Rsapi.Interfaces;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using kCura.Relativity.Client.Repositories;

namespace AdminMigrationUtility.Helpers.Rsapi
{
	public class RSAPIiRepositoryGroup : IRsapiRepositoryGroup
	{
		public IGenericRepository<Batch> BatchRepository { get; set; }
		public IGenericRepository<BatchSet> BatchSetRepository { get; set; }
		public IGenericRepository<kCura.Relativity.Client.DTOs.Choice> ChoiceRepository { get; set; }
		public IGenericRepository<Client> ClientRepository { get; set; }
		public IGenericRepository<Document> DocumentRepository { get; set; }
		public IGenericRepository<Error> ErrorRepository { get; set; }
		public IGenericRepository<kCura.Relativity.Client.DTOs.Field> FieldRepository { get; set; }
		public IGenericRepository<Group> GroupRepository { get; set; }
		public IGenericRepository<Layout> LayoutRepository { get; set; }
		public IGenericRepository<MarkupSet> MarkupSetRepository { get; set; }
		public IGenericRepository<ObjectType> ObjectTypeRepository { get; set; }
		public IGenericRepository<RDO> RdoRepository { get; set; }
		public IGenericRepository<kCura.Relativity.Client.DTOs.RelativityApplication> RelativityApplicationRepository { get; set; }
		public IGenericRepository<RelativityScript> RelativityScriptRepository { get; set; }
		public IGenericRepository<Tab> TabRepository { get; set; }
		public IGenericRepository<kCura.Relativity.Client.DTOs.User> UserRepository { get; set; }
		public IGenericRepository<View> ViewRepository { get; set; }
		public IGenericRepository<Workspace> WorkspaceRepository { get; set; }

		public RSAPIiRepositoryGroup(IRSAPIClient client)
		{
			BatchRepository = client.Repositories.Batch;
			BatchSetRepository = client.Repositories.BatchSet;
			ChoiceRepository = client.Repositories.Choice;
			ClientRepository = client.Repositories.Client;
			DocumentRepository = client.Repositories.Document;
			ErrorRepository = client.Repositories.Error;
			FieldRepository = client.Repositories.Field;
			GroupRepository = client.Repositories.Group;
			LayoutRepository = client.Repositories.Layout;
			MarkupSetRepository = client.Repositories.MarkupSet;
			ObjectTypeRepository = client.Repositories.ObjectType;
			RdoRepository = client.Repositories.RDO;
			RelativityApplicationRepository = client.Repositories.RelativityApplication;
			RelativityScriptRepository = client.Repositories.RelativityScript;
			TabRepository = client.Repositories.Tab;
			UserRepository = client.Repositories.User;
			ViewRepository = client.Repositories.View;
			WorkspaceRepository = client.Repositories.Workspace;
		}
	}
}