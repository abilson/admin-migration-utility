using AdminMigrationUtility.Helpers;
using AdminMigrationUtility.Helpers.Exceptions;
using AdminMigrationUtility.Helpers.Rsapi.Interfaces;
using AdminMigrationUtility.Helpers.SQL;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace AdminMigrationUtility.Agents
{
	public abstract class AgentJobBase
	{
		public Int32 AgentId { get; set; }
		public IAgentHelper AgentHelper { get; set; }
		public ISqlQueryHelper SqlQueryHelper { get; set; }
		public String QueueTable { get; set; }
		public Int32 WorkspaceArtifactId { get; set; }
		public Int32 TableRowId { get; set; }
		public Int32 Priority { get; set; }
		public String OffHoursStartTime { get; set; }
		public String OffHoursEndTime { get; set; }
		public DateTime ProcessedOnDateTime { get; set; }
		public IEnumerable<Int32> AgentResourceGroupIds { get; set; }
		public IAPILog Logger { get; set; }
		public IArtifactQueries ArtifactQueries { get; set; }

		public delegate void RaiseMessageEventHandler(object sender, string message);

		public event RaiseMessageEventHandler OnMessage;

		protected virtual void RaiseAndLogDebugMessage(String message)
		{
			RaiseMessageEventHandler handler = OnMessage;
			if (handler != null)
			{
				handler(this, message);
				Logger.LogDebug($"{Constant.Names.ApplicationName} - {message}");
			}
		}

		private async Task GetOffHoursTimesAsync()
		{
			try
			{
				RaiseAndLogDebugMessage("Retrieving off hours.");

				DataTable dataTable = await SqlQueryHelper.RetrieveOffHoursAsync(AgentHelper.GetDBContext(-1));

				if (dataTable?.Rows == null
					|| dataTable.Rows.Count == 0
					|| String.IsNullOrEmpty(dataTable.Rows[0]["AgentOffHourStartTime"].ToString())
					|| String.IsNullOrEmpty(dataTable.Rows[0]["AgentOffHourEndTime"].ToString())
					|| dataTable.Rows[0]["AgentOffHourStartTime"] == null
					|| dataTable.Rows[0]["AgentOffHourEndTime"] == null)
				{
					throw new AdminMigrationUtilityException(Constant.Messages.Agent.AGENT_OFF_HOURS_NOT_FOUND);
				}

				OffHoursStartTime = dataTable.Rows[0]["AgentOffHourStartTime"].ToString();
				OffHoursEndTime = dataTable.Rows[0]["AgentOffHourEndTime"].ToString();

				RaiseAndLogDebugMessage("Retrieved off hours.");
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.RetrieveOffHoursError, ex);
			}
		}

		public async Task<Boolean> IsOffHoursAsync(DateTime? currentTime = null)
		{
			try
			{
				RaiseAndLogDebugMessage("Checking if the agent is in off hours.");

				DateTime now = currentTime.GetValueOrDefault(DateTime.Now);
				Boolean isOffHours = false;

				await GetOffHoursTimesAsync();
				DateTime todayOffHourStart = DateTime.Parse(now.ToString("d") + " " + OffHoursStartTime);
				DateTime todayOffHourEnd = DateTime.Parse(now.ToString("d") + " " + OffHoursEndTime);

				if (now.Ticks >= todayOffHourStart.Ticks && now.Ticks <= todayOffHourEnd.Ticks)
				{
					isOffHours = true;
				}

				RaiseAndLogDebugMessage("Checked if the agent is in off hours.");

				return isOffHours;
			}
			catch (FormatException ex)
			{
				String errorMessage = Constant.Messages.Agent.AGENT_OFF_HOUR_TIMEFORMAT_INCORRECT;
				RaiseAndLogDebugMessage(errorMessage);
				throw new AdminMigrationUtilityException(errorMessage, ex);
			}
			catch (Exception ex)
			{
				throw new AdminMigrationUtilityException(Constant.ErrorMessages.IsAgentOffHoursError, ex);
			}
		}

		public String GetCommaDelimitedListOfResourceIds(IEnumerable<Int32> agentResourceGroupIds)
		{
			return String.Join(",", agentResourceGroupIds);
		}

		public virtual async Task ResetUnfinishedJobsAsync(IDBContext eddsDbContext)
		{
			await SqlQueryHelper.ResetUnfishedJobsAsync(eddsDbContext, AgentId, QueueTable);
		}

		public abstract Task ExecuteAsync();
	}
}