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
using System.Runtime.InteropServices;

namespace IngameScript
{
	partial class Program : MyGridProgram
	{
		private const string ID_BLOCK_SECTION_NAME = "Airlock";
		private const string ID_BLOCK_AIRLOCK_ID_SETTING_KEY = "id";

		private const string CONTROL_GROUP_SECTION_NAME = "AirlockC";
		private const string A_GROUP_SECTION_NAME = "AirlockA";
		private const string B_GROUP_SECTION_NAME = "AirlockB";

		private static RAPI api;

		private List<Airlock> airlocks;

		public Program()
		{
			Runtime.UpdateFrequency = UpdateFrequency.Update10;

			api = new RAPI(this, false);
			airlocks = new List<Airlock>();

			List<IMyButtonPanel> panels = new List<IMyButtonPanel>();
			GridTerminalSystem.GetBlocksOfType(panels, p => MyIni.HasSection(p.CustomData, ID_BLOCK_SECTION_NAME));

			foreach (IMyButtonPanel panel in panels)
			{
				airlocks.Add(new Airlock(panel));
			}
		}

		public void Main(string argument, UpdateType updateSource)
		{
			api.Debug(Runtime.LastRunTimeMs.ToString("n2"));
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

					airlock.Cycle(targetState);
				}
			}

			foreach (Airlock airlock in airlocks)
			{
				airlock.Process();
			}
		}
	}
}
