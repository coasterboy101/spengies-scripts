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
		public class GasStorageArray
		{
			public string Key { get; private set; }
			public int TargetFill { get; private set; }

			public IMyShipConnector Valve { get; private set; }
			public List<IMyGasTank> Tanks { get; private set; }

			public GasStorageArray(IMyShipConnector valve)
			{
				Valve = valve;
				api.Ini.TryParse(Valve.CustomData);

				Key = api.Ini.Get(GROUP_SECTION_NAME, GROUP_ID_KEY).ToString();
				TargetFill = api.Ini.Get(GROUP_SECTION_NAME, FILL_PERCENT_KEY).ToInt32();

				Tanks = new List<IMyGasTank>();
				api.Program.GridTerminalSystem.GetBlocksOfType(Tanks, tank => MyIni.HasSection(tank.CustomData, Key));
			}

			public void Process()
			{
				try
				{
					bool tanksFilled = !Tanks.Any(tank => tank.FilledRatio * 100 <= TargetFill);

					if (!tanksFilled)
					{
						foreach (IMyGasTank tank in Tanks)
						{
							bool tankFilled = tank.FilledRatio * 100 > TargetFill;
							if (tank.Enabled == tankFilled)
								tank.Enabled = !tankFilled;
						}

						if (Valve.Status == MyShipConnectorStatus.Connectable)
							Valve.Connect();
					}
					else if (tanksFilled && Valve.Status == MyShipConnectorStatus.Connected)
					{
						Valve.Disconnect();

						foreach (IMyGasTank tank in Tanks)
						{
							if (!tank.Enabled)
								tank.Enabled = true;
						}
					}
				}
				catch
				{
					return;
				}
			}
		}
	}
}
