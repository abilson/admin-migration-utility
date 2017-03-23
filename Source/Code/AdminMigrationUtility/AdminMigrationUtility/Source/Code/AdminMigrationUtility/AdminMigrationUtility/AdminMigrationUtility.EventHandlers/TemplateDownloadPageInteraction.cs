using AdminMigrationUtility.Helpers;
using kCura.EventHandler;
using System;

namespace AdminMigrationUtility.EventHandlers
{
	[kCura.EventHandler.CustomAttributes.Description("This Event Handler allows for the download of the Import Template CSV file.")]
	[System.Runtime.InteropServices.Guid("E3EFB033-5994-45E7-8C3A-BD86656A1F09")]
	public class TemplateDownloadPageInteraction : PageInteractionEventHandler
	{
		public override Response PopulateScriptBlocks()
		{
			var retVal = new Response()
			{
				Message = String.Empty,
				Success = true
			};

			if (PageMode == kCura.EventHandler.Helper.PageMode.Edit)
			{
				var finalJavascriptInsert = Javascript.ButtonTemplate.Replace(Constant.GuidReplacement, Constant.Guids.Application.ApplicationGuid.ToString());
				RegisterClientScriptBlock(new ScriptBlock() { Key = "Object Template Buttons", Script = finalJavascriptInsert });
			}

			return retVal;
		}
	}
}