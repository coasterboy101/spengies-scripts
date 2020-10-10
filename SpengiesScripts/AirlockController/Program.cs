using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI.Ingame;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System;
using VRage.Collections;
using VRage.Game.Components;
using VRage.Game.GUI.TextPanel;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRage;
using VRageMath;
using Sandbox.Game.Screens.Helpers;

namespace IngameScript
{
	partial class Program : MyGridProgram
	{
		private const string CONFIG_GROUP_SECTION_NAME = "Airlock";
		private const string CONFIG_KEY_NAME_AIRLOCK_ID = "airlockID";

		private const string CONTROL_GROUP_SECTION_NAME = "Airlock Control";
		private const string A_GROUP_SECTION_NAME = "Airlock A";
		private const string B_GROUP_SECTION_NAME = "Airlock B";

		private RAPI api;

		private List<Airlock> airlocks;

		public Program()
		{
			api = new RAPI(this);
			Runtime.UpdateFrequency = UpdateFrequency.Update10;

			airlocks = new List<Airlock>();

			List<IMyControlPanel> panels = new List<IMyControlPanel>();
			GridTerminalSystem.GetBlocksOfType(panels, panel => MyIni.HasSection(panel.CustomData, CONFIG_GROUP_SECTION_NAME));

			foreach (IMyControlPanel panel in panels)
			{
				airlocks.Add(new Airlock(api, panel));
			}
		}

		public void Main(string argument, UpdateType updateSource)
		{
			if (!String.IsNullOrWhiteSpace(argument) && api.CommandLine.TryParse(argument))
			{
				string key = api.CommandLine.Argument(0).Trim();
				
				Airlock airlock = airlocks.Where(a => a.ID == key).FirstOrDefault();
				if (airlock != null)
				{
					AirlockState targetState;
					if (api.CommandLine.Switch("a"))
						targetState = AirlockState.ASideOpen;
					else if (api.CommandLine.Switch("b"))
						targetState = AirlockState.BSideOpen;
					else
						targetState = airlock.CurrentState == AirlockState.ASideOpen ? AirlockState.BSideOpen : AirlockState.ASideOpen;
				}
			}
		}
	}
}
