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
	partial class Program
	{
		public class GasStorage
		{
			public string Key { get; set; }

			private int targetFill;
			private IMyShipConnector valve;
			private List<IMyGasTank> tanks;

			public GasStorage(string key, IMyShipConnector valve, List<IMyGasTank> tanks, int targetFill)
			{
				this.Key = key;

				this.targetFill = targetFill;
				this.valve = valve;
				this.tanks = tanks;
			}

			public void Process()
			{
				bool tanksFilled = !tanks.Any(tank => tank.FilledRatio * 100 <= targetFill);

				if (!tanksFilled)
				{
					foreach (IMyGasTank tank in tanks)
					{
						bool tankFilled = tank.FilledRatio * 100 > targetFill;
						if (tank.Enabled == tankFilled)
							tank.Enabled = !tankFilled;
					}

					if (valve.Status == MyShipConnectorStatus.Connectable)
						valve.Connect();
				}	
				else if (tanksFilled && valve.Status == MyShipConnectorStatus.Connected)
				{
					valve.Disconnect();

					foreach (IMyGasTank tank in tanks)
					{
						if (!tank.Enabled)
							tank.Enabled = true;
					}
				}
			}
		}
	}
}
