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
		private const string GROUP_SECTION_NAME = "Gas Storage";
		private const string GROUP_NAME_KEY = "groupName";
		private const string FILL_PERCENT_KEY = "fillPercent";

		private RAPI _api;

		private List<GasStorageArray> _storageArrays;

		public Program()
		{
			_api = new RAPI(this);
			Runtime.UpdateFrequency = UpdateFrequency.Update10;

			_storageArrays = new List<GasStorageArray>();

			List<IMyShipConnector> valves = new List<IMyShipConnector>();
			GridTerminalSystem.GetBlocksOfType(valves, valve => MyIni.HasSection(valve.CustomData, GROUP_SECTION_NAME));

			foreach (IMyShipConnector valve in valves)
			{
				_storageArrays.Add(new GasStorageArray(_api, valve));
			}
		}

		public void Main(string argument, UpdateType updateSource)
		{
			foreach (GasStorageArray storageArray in _storageArrays)
			{
				storageArray.Process();
			}
		}
	}
}
