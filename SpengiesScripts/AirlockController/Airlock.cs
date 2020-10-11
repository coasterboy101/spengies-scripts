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
using System.Diagnostics;

namespace IngameScript
{
	partial class Program
	{
		public enum AirlockState
		{
			ASideOpen,
			BSideOpen
		}

		public enum AirlockStatus
		{ 
			Standby,
			DoorsClosing,
			DoorsOpening,
			Pressurizing,
			Depressurizing			
		}

		public class Airlock
		{
			private const int STEP_WAIT_TIME_IN_UPDATES = 6;
			private const int MAX_VENT_TIME_IN_UPDATES = 180;

			public string ID { get; private set; }

			public AirlockStatus Status { get; private set; }
			public AirlockState CurrentState { get; private set; }
			public AirlockState TargetState { get; private set; }

			private int stepUpdateCount = 0;
			private int ventUpdateCount = 0;

			private List<IMyAirVent> vents = new List<IMyAirVent>();
			private List<IMyLightingBlock> lights = new List<IMyLightingBlock>();
			private List<IMyTextPanel> lcds = new List<IMyTextPanel>();

			private IMyAirVent aSideVent = null;
			private List<IMyDoor> aSideDoors = new List<IMyDoor>();
			private List<IMyLightingBlock> aSideLights = new List<IMyLightingBlock>();

			private IMyAirVent bSideVent = null;
			private List<IMyDoor> bSideDoors = new List<IMyDoor>();
			private List<IMyLightingBlock> bSideLights = new List<IMyLightingBlock>();

			private List<IMyDoor> activeDoors = new List<IMyDoor>();

			public Airlock(IMyButtonPanel panel)
			{
				api.Ini.TryParse(panel.CustomData);
				
				ID = api.Ini.Get(ID_BLOCK_SECTION_NAME, ID_BLOCK_AIRLOCK_ID_SETTING_KEY).ToString();
				api.Debug(ID);

				List<IMyAirVent> allVents = new List<IMyAirVent>();
				api.Program.GridTerminalSystem.GetBlocksOfType(allVents, v => MyIni.HasSection(v.CustomData, ID));

				vents = allVents.Where(v => MyIni.HasSection(v.CustomData, CONTROL_GROUP_SECTION_NAME)).ToList();
				api.Program.GridTerminalSystem.GetBlocksOfType(lights,
					l => MyIni.HasSection(l.CustomData, ID) && MyIni.HasSection(l.CustomData, CONTROL_GROUP_SECTION_NAME));
				api.Program.GridTerminalSystem.GetBlocksOfType(lcds,
					l => MyIni.HasSection(l.CustomData, ID) && MyIni.HasSection(l.CustomData, CONTROL_GROUP_SECTION_NAME));

				aSideVent = allVents.Where(v => MyIni.HasSection(v.CustomData, A_GROUP_SECTION_NAME)).FirstOrDefault();
				api.Program.GridTerminalSystem.GetBlocksOfType(aSideDoors,
					d => MyIni.HasSection(d.CustomData, ID) && MyIni.HasSection(d.CustomData, A_GROUP_SECTION_NAME));
				api.Program.GridTerminalSystem.GetBlocksOfType(aSideLights,
					l => MyIni.HasSection(l.CustomData, ID) && MyIni.HasSection(l.CustomData, A_GROUP_SECTION_NAME));

				bSideVent = allVents.Where(v => MyIni.HasSection(v.CustomData, B_GROUP_SECTION_NAME)).FirstOrDefault();
				api.Program.GridTerminalSystem.GetBlocksOfType(bSideDoors,
					d => MyIni.HasSection(d.CustomData, ID) && MyIni.HasSection(d.CustomData, B_GROUP_SECTION_NAME));
				api.Program.GridTerminalSystem.GetBlocksOfType(bSideLights,
					l => MyIni.HasSection(l.CustomData, ID) && MyIni.HasSection(l.CustomData, B_GROUP_SECTION_NAME));

				CurrentState = bSideDoors.Any(d => d.Status == DoorStatus.Open || d.Status == DoorStatus.Opening) ? AirlockState.BSideOpen : AirlockState.ASideOpen;
			}

			public void Cycle(AirlockState targetState)
			{
				if (Status != AirlockStatus.Standby || targetState == CurrentState)
					return;

				TargetState = targetState;
				Status = AirlockStatus.DoorsClosing;

				activeDoors = null;
				stepUpdateCount = 0;
			}

			public void Process()
			{
				if (Status == AirlockStatus.Standby)
					return;

				switch (Status)
				{
					case AirlockStatus.DoorsClosing:
						if (activeDoors == null || activeDoors.Count <= 0)
							activeDoors = aSideDoors.Union(bSideDoors).ToList();

						if (Close(activeDoors))
						{
							stepUpdateCount++;
							if (stepUpdateCount < STEP_WAIT_TIME_IN_UPDATES)
								return;

							activeDoors = null;
							stepUpdateCount = 0;
							ventUpdateCount = 0;

							switch (TargetState)
							{
								case AirlockState.ASideOpen:
									if (aSideVent != null && aSideVent.CanPressurize)
										Status = AirlockStatus.Pressurizing;
									else
										Status = AirlockStatus.Depressurizing;
									break;
								case AirlockState.BSideOpen:
									if (bSideVent != null && bSideVent.CanPressurize)
										Status = AirlockStatus.Pressurizing;
									else
										Status = AirlockStatus.Depressurizing;
									break;
							}
						}
						break;
					case AirlockStatus.Pressurizing:
						if (Pressurize(vents))
						{
							stepUpdateCount++;
							if (stepUpdateCount < STEP_WAIT_TIME_IN_UPDATES)
								return;
							else
								stepUpdateCount = 0;

							Status = AirlockStatus.DoorsOpening;
						}
						break;
					case AirlockStatus.Depressurizing:
						if (Depressurize(vents))
						{
							stepUpdateCount++;
							if (stepUpdateCount < STEP_WAIT_TIME_IN_UPDATES)
								return;
							else
								stepUpdateCount = 0;

							Status = AirlockStatus.DoorsOpening;
						}
						break;
					case AirlockStatus.DoorsOpening:
						if (activeDoors == null || activeDoors.Count <= 0)
						{
							activeDoors = new List<IMyDoor>();
							switch (TargetState)
							{
								case AirlockState.ASideOpen:
									activeDoors = aSideDoors;
									break;
								case AirlockState.BSideOpen:
									activeDoors = bSideDoors;
									break;
							}
						}

						if (Open(activeDoors))
						{
							stepUpdateCount++;
							if (stepUpdateCount < STEP_WAIT_TIME_IN_UPDATES)
								return;

							Status = AirlockStatus.Standby;
							CurrentState = TargetState;
						}
						break;
				}
			}

			private bool Open(List<IMyDoor> doors)
			{
				bool opened = true;
				foreach (IMyDoor door in doors)
				{
					if (door.Status == DoorStatus.Open)
					{
						if (door.Enabled)
							door.Enabled = false;
						continue;
					}

					if (!door.Enabled)
						door.Enabled = true;
					if (door.Status != DoorStatus.Opening)
						door.OpenDoor();

					opened = false;
				}

				return opened;
			}

			private bool Close(List<IMyDoor> doors)
			{
				bool closed = true;
				foreach (IMyDoor door in doors)
				{
					if (door == null)
						continue;

					if (door.Status == DoorStatus.Closed)
					{
						if (door.Enabled)
							door.Enabled = false;
						continue;
					}

					if (!door.Enabled)
						door.Enabled = true;
					if (door.Status != DoorStatus.Closing)
						door.CloseDoor();

					closed = false;
				}

				return closed;
			}

			private bool Pressurize(List<IMyAirVent> vents)
			{
				bool pressurized = ventUpdateCount >= MAX_VENT_TIME_IN_UPDATES || vents.Average(v => v.GetOxygenLevel()) >= 1.00f;
				foreach (IMyAirVent vent in vents)
				{
					if (vent == null || pressurized)
						continue;

					if (!vent.Enabled)
						vent.Enabled = true;
					if (vent.Depressurize)
						vent.Depressurize = false;
				}

				ventUpdateCount++;
				return pressurized;
			}

			private bool Depressurize(List<IMyAirVent> vents)
			{
				bool depressurized = ventUpdateCount >= MAX_VENT_TIME_IN_UPDATES || vents.Average(v => v.GetOxygenLevel()) <= 0.00f;
				foreach (IMyAirVent vent in vents)
				{
					if (vent == null)
						continue;

					if (depressurized)
					{
						if (vent.Enabled)
							vent.Enabled = false;
						if (vent.Depressurize)
							vent.Depressurize = false;

						continue;
					}
					
					if (!vent.Enabled)
						vent.Enabled = true;
					if (!vent.Depressurize)
						vent.Depressurize = true;
				}

				ventUpdateCount++;
				return depressurized;
			}
		}
	}
}
