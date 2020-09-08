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
		IMyShipConnector valve;
		List<IMyGasTank> tanks;

		public Program()
		{
			tanks = new List<IMyGasTank>();
			valve = GridTerminalSystem.GetBlockWithName("testingConnector") as IMyShipConnector;
			IMyBlockGroup group = GridTerminalSystem.GetBlockGroupWithName("testGroup");
			List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
			group.GetBlocks(blocks);
			foreach(IMyTerminalBlock block in blocks)
			{
				tanks.Add(block as IMyGasTank);
			}

			Runtime.UpdateFrequency = UpdateFrequency.Update10;
		}

		public void Main(string argument, UpdateType updateSource)
		{
			double total = 0.0;
			foreach (IMyGasTank tank in tanks)
			{
				total += tank.FilledRatio;
			}

			double average = total / tanks.Count;
			if (average >= 0.1 && valve.Status == MyShipConnectorStatus.Connected)
				valve.Disconnect();
			else if (average < 0.1 && valve.Status == MyShipConnectorStatus.Connectable)
				valve.Connect();
		}
	}
}
