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
			private const int STEP_WAIT_TIME_IN_MS = 1000;
			private const int MAX_VENT_TIME_IN_MS = 30000;

			private const string AIRLOCK_READY_TEXT = "READY";
			private const string AIRLOCK_CYCLE_TEXT = "CYCLING";

			public string ID { get; private set; }

			public AirlockStatus Status { get; private set; }
			public AirlockState CurrentState { get; private set; }
			public AirlockState TargetState { get; private set; }

			private int stepMillesecondsCount = 0;
			private int ventMillisecondsCount = 0;

			private List<IMyAirVent> vents = new List<IMyAirVent>();
			private List<IMyLightingBlock> lights = new List<IMyLightingBlock>();
			private List<IMyTextSurface> lcds = new List<IMyTextSurface>();

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
				IMyTextSurfaceProvider panelLcd = panel as IMyTextSurfaceProvider;
				if (panelLcd != null && panelLcd.SurfaceCount > 0)
					lcds.Add(panelLcd.GetSurface(0));

				List<IMyAirVent> allVents = new List<IMyAirVent>();
				api.Program.GridTerminalSystem.GetBlocksOfType(allVents, v => MyIni.HasSection(v.CustomData, ID));

				vents = allVents.Where(v => MyIni.HasSection(v.CustomData, CONTROL_GROUP_SECTION_NAME)).ToList();
				api.Program.GridTerminalSystem.GetBlocksOfType(lights,
					l => MyIni.HasSection(l.CustomData, ID) && MyIni.HasSection(l.CustomData, CONTROL_GROUP_SECTION_NAME));

				List<IMyTerminalBlock> screens = new List<IMyTerminalBlock>();
				api.Program.GridTerminalSystem.GetBlocksOfType(screens,
					l => MyIni.HasSection(l.CustomData, ID) && MyIni.HasSection(l.CustomData, CONTROL_GROUP_SECTION_NAME));
				foreach (IMyTerminalBlock screen in screens)
				{
					IMyTextSurface screenPanel = screen as IMyTextSurface;
					if (screenPanel != null)
					{
						lcds.Add(screenPanel);
						continue;
					}

					IMyTextSurfaceProvider screenProvider = screen as IMyTextSurfaceProvider;
					if (screenProvider != null && screenProvider.SurfaceCount > 0)
					{
						lcds.Add(screenProvider.GetSurface(0));
						continue;
					}
				}

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
				if (Status != AirlockStatus.Standby)
					return;

				if (targetState == CurrentState)
				{

				}

				TargetState = targetState;
				Status = AirlockStatus.DoorsClosing;

				activeDoors = null;
				stepMillesecondsCount = 0;

				foreach (IMyTextSurface lcd in lcds)
				{
					if (lcd != null)
						lcd.WriteText(AIRLOCK_CYCLE_TEXT);
				}
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
							stepMillesecondsCount += api.Program.Runtime.TimeSinceLastRun.Milliseconds;
							if (stepMillesecondsCount < STEP_WAIT_TIME_IN_MS)
								return;

							foreach (IMyDoor door in activeDoors)
							{
								if (door != null && door.Enabled)
									door.Enabled = false;
							}

							activeDoors = null;
							stepMillesecondsCount = 0;
							ventMillisecondsCount = 0;

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
							stepMillesecondsCount += api.Program.Runtime.TimeSinceLastRun.Milliseconds;
							if (stepMillesecondsCount < STEP_WAIT_TIME_IN_MS)
								return;
							
							stepMillesecondsCount = 0;

							Status = AirlockStatus.DoorsOpening;
						}
						break;
					case AirlockStatus.Depressurizing:
						if (Depressurize(vents))
						{
							stepMillesecondsCount += api.Program.Runtime.TimeSinceLastRun.Milliseconds;
							if (stepMillesecondsCount < STEP_WAIT_TIME_IN_MS)
								return;
							
							stepMillesecondsCount = 0;

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
							stepMillesecondsCount += api.Program.Runtime.TimeSinceLastRun.Milliseconds;
							if (stepMillesecondsCount < STEP_WAIT_TIME_IN_MS)
								return;

							Status = AirlockStatus.Standby;
							CurrentState = TargetState;

							foreach (IMyTextSurface lcd in lcds)
							{
								if (lcd != null)
									lcd.WriteText(AIRLOCK_READY_TEXT);
							}
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
						continue;

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
						continue;

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
				bool pressurized = ventMillisecondsCount >= MAX_VENT_TIME_IN_MS || vents.Min(v => v.GetOxygenLevel()) >= 1.00f;
				foreach (IMyAirVent vent in vents)
				{
					if (vent == null || pressurized)
						continue;

					if (!vent.Enabled)
						vent.Enabled = true;
					if (vent.Depressurize)
						vent.Depressurize = false;
				}

				ventMillisecondsCount += api.Program.Runtime.TimeSinceLastRun.Milliseconds;
				return pressurized;
			}

			private bool Depressurize(List<IMyAirVent> vents)
			{
				bool depressurized = ventMillisecondsCount >= MAX_VENT_TIME_IN_MS || vents.Max(v => v.GetOxygenLevel()) <= 0.00f;
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

				ventMillisecondsCount += api.Program.Runtime.TimeSinceLastRun.Milliseconds;
				return depressurized;
			}
		}
	}
}
