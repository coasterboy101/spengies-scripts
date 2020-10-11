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

namespace IngameScript
{
	partial class Program : MyGridProgram
	{
		private const string GROUP_SECTION_NAME = "Gas Storage Array";
		private const string GROUP_ID_KEY = "groupID";
		private const string FILL_PERCENT_KEY = "fillPercent";

		private static RAPI api;

		private List<GasStorageArray> storageArrays;

		public Program()
		{
			api = new RAPI(this);
			Runtime.UpdateFrequency = UpdateFrequency.Update10;

			storageArrays = new List<GasStorageArray>();

			List<IMyShipConnector> valves = new List<IMyShipConnector>();
			GridTerminalSystem.GetBlocksOfType(valves, valve => MyIni.HasSection(valve.CustomData, GROUP_SECTION_NAME));

			foreach (IMyShipConnector valve in valves)
			{
				storageArrays.Add(new GasStorageArray(valve));
			}
		}

		public void Main(string argument, UpdateType updateSource)
		{
			foreach (GasStorageArray storageArray in storageArrays)
			{
				storageArray.Process();
			}
		}
	}
}
