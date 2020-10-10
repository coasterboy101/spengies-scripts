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
		public enum AirlockState
		{
			ASideOpen,
			BSideOpen
		}

		public class Airlock
		{
			public const int ClosingTimeDefault = 3;
			public const int FullTimeThreshold = 3;
			public const int EmptyTimeThreshold = 3;
			public const int DepressurizeMaxTime = 20;
			public const int PressurizeMaxTime = 20;

			public string ID { get; private set; }
			public bool Working { get; private set; }

			public AirlockState CurrentState { get; private set; }
			public AirlockState TargetState { get; private set; }

			public IMyControlPanel ControlPanel { get; private set; }

			private List<IMyAirVent> controlVents = new List<IMyAirVent>();
			private List<IMyLightingBlock> controlLights = new List<IMyLightingBlock>();
			private List<IMyTextPanel> controlLcds = new List<IMyTextPanel>();

			private IMyAirVent aSideVent = null;
			private List<IMyDoor> aSideDoors = new List<IMyDoor>();
			private List<IMyLightingBlock> aSideLights = new List<IMyLightingBlock>();

			private IMyAirVent bSideVent = null;
			private List<IMyDoor> bSideDoors = new List<IMyDoor>();
			private List<IMyLightingBlock> bSideLights = new List<IMyLightingBlock>();

			private float aSidePressure = 0f;
			private float bSidePressure = 0f;

			private RAPI api;

			//public int WantedState = 0;
			//public int CurrentState = 1;
			//public bool WasDepressurizing = false;

			//public int ClosingTimer = -1;
			//public int FullTime = 0;
			//public int EmptyTime = 0;
			//public int ChangingTime = 0;
			//public int TimeSinceChange = 0;

			//public bool OuterOpen = false;
			//public bool InnerOpen = false;

			//public double lowestPressure = 100.0f;

			//public string name = "Airlock";

			public Airlock(RAPI api, IMyControlPanel panel)
			{
				this.api = api;

				ControlPanel = panel;
				api.Ini.TryParse(panel.CustomData);

				ID = api.Ini.Get(CONFIG_GROUP_SECTION_NAME, CONFIG_KEY_NAME_AIRLOCK_ID).ToString();

				List<IMyAirVent> allVents = new List<IMyAirVent>();
				api.Program.GridTerminalSystem.GetBlocksOfType(allVents, v => MyIni.HasSection(v.CustomData, ID));

				controlVents = allVents.Where(v => MyIni.HasSection(v.CustomData, CONTROL_GROUP_SECTION_NAME)).ToList();
				api.Program.GridTerminalSystem.GetBlocksOfType(controlLights,
					l => MyIni.HasSection(l.CustomData, ID) && MyIni.HasSection(l.CustomData, CONTROL_GROUP_SECTION_NAME));
				api.Program.GridTerminalSystem.GetBlocksOfType(controlLcds,
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

				DoorStatus aSideDoorStatus = aSideDoors.FirstOrDefault()?.Status ?? DoorStatus.Open;
				bool aSideDoorsAreOpen = aSideDoorStatus == DoorStatus.Open || aSideDoorStatus == DoorStatus.Opening;

				DoorStatus bSideDoorStatus = bSideDoors.FirstOrDefault()?.Status ?? DoorStatus.Open;
				bool bSideDoorsAreOpen = bSideDoorStatus == DoorStatus.Open || bSideDoorStatus == DoorStatus.Opening;

				if (bSideDoorsAreOpen)
				{
					CurrentState = AirlockState.BSideOpen;

					if (aSideDoorsAreOpen)
					{
						foreach (IMyDoor door in aSideDoors)
							door.CloseDoor();
					}
					
					foreach (IMyAirVent vent in controlVents)
						vent.Depressurize = !(bSideVent?.CanPressurize ?? false);
				}
				else
				{
					CurrentState = AirlockState.ASideOpen;

					if (bSideDoorsAreOpen)
					{
						foreach (IMyDoor door in bSideDoors)
							door.CloseDoor();
					}

					foreach (IMyAirVent vent in controlVents)
						vent.Depressurize = !(aSideVent?.CanPressurize ?? false);
				}
			}

			public void Cycle(AirlockState targetState)
			{
				if (Working || targetState == CurrentState)
					return;

				TargetState = targetState;
				Working = true;
			}

			public void Process()
			{
				if (!Working)
					return;

				if (IsDep != WasDepressurizing
					|| (TimeSinceChange > 40 && !Stuck &&
						((InnerOpen && !IsPressureOk(InnerPressure)) ||
						 (OuterOpen && !IsPressureOk(OuterPressure)))))
				{
					if (!IsDep)
					{
						if (InnerPressure > 0.01f)
							WantedState = 1;
						else
							WantedState = 2;
					}
					else
					{
						if (OuterPressure <= 0.01f)
							WantedState = 2;
						else
							WantedState = 1;
					}
					if (CurrentState == WantedState && IsDep != WasDepressurizing)
						WantedState = (CurrentState == 1 ? 2 : 1);
					CurrentState = 0;
				}

				if (control != null)
				{
					// buttons 
					if (control.BlinkLength > 50f)
						WantedState = 1;
					else
						if (control.BlinkLength < 50f)
						WantedState = 2;

					control.SetValueFloat("Blink Lenght", 50f);

					// switch 
					if (control.BlinkOffset > 50f)
						WantedState = (WantedState == 2 ? 1 : 2);

					control.SetValueFloat("Blink Offset", 50f);
				}

				if (command != "")
				{
					switch (command)
					{
						case "in":
							WantedState = 1;
							break;
						case "out":
							WantedState = 2;
							break;
						case "toggle":
							WantedState = (WantedState == 2 ? 1 : 2);
							break;
					}
				}

				if (WantedState == 0)
					WantedState = (IsDep ? 2 : 1);

				if (WantedState != CurrentState)
				{
					CurrentState = 0;
					TimeSinceChange = 0;
				}

				if (ClosingTimer >= 0)
					ClosingTimer--;

				DR.Debug("WantSt: " + WantedState.ToString() + " CurSt: " + CurrentState.ToString());
				DR.Debug("Closing Timer: " + ClosingTimer.ToString());
				DR.Debug("FullTime: " + FullTime + " EmptyTime: " + EmptyTime);
				DR.Debug("ChangingTime: " + ChangingTime);
				DR.Debug("Inner: " + InnerPressure.ToString("F2") + " Outer: " + OuterPressure.ToString("F2"));
				DR.Debug("Opening: " + Opening.ToString() + " Stuck: " + Stuck.ToString());
				DR.Debug("LastChange: " + TimeSinceChange.ToString());

				if (CurrentState == 0 && ChangingTime > 0)
				{
					if (WantedState == 1 && IsPressureOk(InnerPressure))
					{
						CurrentState = 1;
						Opening = true;
						Stuck = false;
					}
					else
						if (WantedState == 2 && IsPressureOk(OuterPressure))
					{
						CurrentState = 2;
						Opening = true;
						Stuck = false;
					}
					else
							if (WantedState == 2 && ChangingTime >= DepressurizeMaxTime * 6)
					{
						CurrentState = 2;
						Opening = true;
						Stuck = true;
					}
					else
								if (WantedState == 1 && ChangingTime >= PressurizeMaxTime * 6)
					{
						CurrentState = 1;
						Opening = true;
						Stuck = true;
					}
				}

				switch (CurrentState)
				{
					case 0:
						ChangingTime++;
						TimeSinceChange = 0;
						SetClose(internalDoors, true);
						SetClose(externalDoors, true);
						SetLights(0);
						break;
					case 1:
						ChangingTime = 0;
						SetClose(externalDoors, true);
						if (!InnerOpen && Opening)
						{
							if (IsPressureOk(InnerPressure) || MMConfig.OPEN_ANYWAY)
								SetClose(internalDoors, false);
							else
								SetLock(internalDoors, false);
						}
						else
						{
							SetLock(internalDoors, false);
							if (Stuck && IsPressureOk(InnerPressure))
								Stuck = false;
							Opening = false;
						}
						SetLights(1);
						break;
					case 2:
						ChangingTime = 0;
						SetClose(internalDoors, true);
						if (!OuterOpen && Opening)
						{
							if (IsPressureOk(OuterPressure) || MMConfig.OPEN_ANYWAY)
								SetClose(externalDoors, false);
							else
								SetLock(externalDoors, false);
						}
						else
						{
							SetLock(externalDoors, false);
							if (Stuck && IsPressureOk(OuterPressure))
								Stuck = false;
							Opening = false;
						}
						SetLights(2);
						break;
				}

				for (int lid = 0; lid < controlLcds.Count; lid++)
					DRLcdTextManager.Add(controlLcds[lid], name);

				if (lowestPressure >= 100f)
					lowestPressure = -1f;

				string pressure = (lowestPressure < 0 ? "[N/A]" : "[" + (lowestPressure * 100f).ToString("F0") + "%]");
				switch (WantedState)
				{
					case 0: // no change 
						for (int lid = 0; lid < controlLcds.Count; lid++)
							DRLcdTextManager.AddRightAlign(controlLcds[lid], "!! " + pressure, DRPanel.LCD_LINE_WIDTH);
						break;
					case 1: // in (pressurize) 
						if (ClosingTimer <= 0)
						{
							if (InnerPressure > 0.01f)
							{
								DR.Debug("Pressurize");
								Depressurize(false);
								IsDep = false;
							}
							else
							{
								DR.Debug("Depressurize");
								Depressurize(true);
								IsDep = true;
							}
						}

						if (ClosingTimer > 0 || CurrentState == 0 || Opening)
							if (Stuck)
								for (int lid = 0; lid < controlLcds.Count; lid++)
									DRLcdTextManager.AddRightAlign(controlLcds[lid], "!! IN " + pressure, DRPanel.LCD_LINE_WIDTH);
							else
								for (int lid = 0; lid < controlLcds.Count; lid++)
									DRLcdTextManager.AddRightAlign(controlLcds[lid], ">> IN " + pressure, DRPanel.LCD_LINE_WIDTH);
						else
							for (int lid = 0; lid < controlLcds.Count; lid++)
								DRLcdTextManager.AddRightAlign(controlLcds[lid], "IN " + pressure, DRPanel.LCD_LINE_WIDTH);
						break;
					case 2: // out (decompress) 
						if (ClosingTimer <= 0)
						{
							if (OuterPressure > 0.01f)
							{
								Depressurize(false);
								DR.Debug("Pressurize");
								IsDep = false;
							}
							else
							{
								Depressurize(true);
								DR.Debug("Depressurize");
								IsDep = true;
							}
						}

						if (ClosingTimer > 0 || CurrentState == 0 || Opening)
							if (Stuck)
								for (int lid = 0; lid < controlLcds.Count; lid++)
									DRLcdTextManager.AddRightAlign(controlLcds[lid], "!! OUT " + pressure, DRPanel.LCD_LINE_WIDTH);
							else
								for (int lid = 0; lid < controlLcds.Count; lid++)
									DRLcdTextManager.AddRightAlign(controlLcds[lid], ">> OUT " + pressure, DRPanel.LCD_LINE_WIDTH);
						else
							for (int lid = 0; lid < controlLcds.Count; lid++)
								DRLcdTextManager.AddRightAlign(controlLcds[lid], "OUT " + pressure, DRPanel.LCD_LINE_WIDTH);
						break;
				}

				for (int lid = 0; lid < controlLcds.Count; lid++)
				{
					DRLcdTextManager.AddLine(controlLcds[lid], "");
					DRLcdTextManager.AddProgressBar(controlLcds[lid], Math.Max(0f, lowestPressure * 100f), controlLcds[lid].FULL_PROGRESS_CHARS);
					DRLcdTextManager.AddLine(controlLcds[lid], "");
				}


				if (ClosingTimer == 0)
				{
					ChangingTime = 0;

					if (CurrentState == 0)
						switch (WantedState)
						{
							case 1:
								PlaySound(internalSound);
								break;
							case 2:
								PlaySound(externalSound);
								break;
						}
				}

				if (!Stuck)
					TimeSinceChange++;
				WasDepressurizing = IsDep;
				DR.EnableDebug = false;
			}

			private void SetLight(List<IMyLightingBlock> lights, Color c, float BlinkInt = 0f, float BlinkLen = 100f, float BlinkOff = 0f)
			{
				for (int i = 0; i < lights.Count; i++)
				{
					if (lights[i].GetProperty("Color").AsColor().GetValue(lights[i]) != c
						|| BlinkInt != lights[i].BlinkIntervalSeconds
						|| !lights[i].IsWorking)
					{
						if (!lights[i].IsWorking)
						{
							lights[i].SetValue("Blink Interval", BlinkInt);
							lights[i].SetValue("Blink Lenght", BlinkLen);
							lights[i].SetValue("Blink Offset", BlinkOff);
							lights[i].SetValue("Color", c);
							lights[i].ApplyAction("OnOff_On");
						}
						else
							lights[i].ApplyAction("OnOff_Off");
					}
				}
			}

			private void SetClose(List<IMyDoor> doors, bool closed)
			{
				DR.Debug(closed ? "Want to close" : "Want to open");
				for (int i = 0; i < doors.Count; i++)
				{
					DR.Debug("Door '" + doors[i].CustomName + "' status: " + doors[i].Status.ToString());

					if (closed == (doors[i].Status == DoorStatus.Closed) && closed == !doors[i].IsWorking)
					{
						DR.Debug("Correct state - continue (isworking = " + doors[i].IsWorking + ")");
						continue;
					}

					if (!doors[i].IsWorking)
					{
						doors[i].ApplyAction("OnOff_On");
						continue;
					}

					if (closed && (doors[i].Status == DoorStatus.Open))
					{
						DR.Debug("Closing");

						SetupClosingTimer(doors[i]);

						doors[i].ApplyAction("Open_Off");
						continue;
					}

					if (!closed && (doors[i].Status == DoorStatus.Closed))
					{
						DR.Debug("Opening");
						SetupClosingTimer(doors[i]);

						doors[i].ApplyAction("Open_On");
						continue;
					}

					if (closed)
					{
						if ((!doors[i].CustomName.Contains(" CT:") && doors[i].Status == DoorStatus.Closed) ||
							(doors[i].CustomName.Contains(" CT:") && ClosingTimer <= 0))
						{
							doors[i].ApplyAction("OnOff_Off");
						}
						else
						{
							if (!doors[i].CustomName.Contains(" CT:"))
								ClosingTimer = (int)Math.Ceiling((float)ClosingTimeDefault * doors[i].OpenRatio) + 1;
						}
					}
				}
			}

			private void SetupClosingTimer(IMyTerminalBlock doors)
			{
				int default_time = (int)Math.Ceiling(ClosingTimeDefault * (doors as IMyDoor).OpenRatio) * 6 + 1;
				string ct = doors.CustomName;
				int idx = ct.IndexOf(" CT:");
				if (idx >= 0)
				{
					int val = -1;
					ct = ct.Substring(idx + 4).TrimStart(' ');
					idx = ct.IndexOf(' ');
					if (idx >= 0)
						ct = ct.Substring(0, idx);

					if (int.TryParse(ct, out val))
					{
						val *= 6;
						if (val > ClosingTimer)
							ClosingTimer = val;
					}
					else
						if (default_time > ClosingTimer)
						ClosingTimer = default_time;
				}
				else
					if (default_time > ClosingTimer)
					ClosingTimer = default_time;
			}

			private void PlaySound(List<IMySoundBlock> sounds)
			{
				for (int i = 0; i < sounds.Count; i++)
				{
					sounds[i].ApplyAction("PlaySound");
				}
			}

			private void SetLock(List<IMyDoor> doors, bool locked)
			{
				string action = (locked ? "OnOff_Off" : "OnOff_On");
				for (int i = 0; i < doors.Count; i++)
					doors[i].ApplyAction(action);
			}

			private bool IsPressureOk(float wantedPressure)
			{
				if (wantedPressure <= 0.01f)
					return (EmptyTime >= EmptyTimeThreshold);
				else
					return (FullTime >= FullTimeThreshold);
			}

			private void SetLights(int state)
			{
				if ((Opening && !Stuck) || state == 0)
				{
					if (WantedState == 1)
						SetLight(internalLights, MMConfig.LOCK_COLOR, 2f, 50f);
					else
						SetLight(internalLights, MMConfig.LOCK_COLOR);
					if (WantedState == 2)
						SetLight(externalLights, MMConfig.LOCK_COLOR, 2f, 50f);
					else
						SetLight(externalLights, MMConfig.LOCK_COLOR);
					return;
				}

				switch (state)
				{
					case 1:
						if (!IsPressureOk(InnerPressure) && !InnerOpen)
							SetLight(internalLights, MMConfig.WARN_COLOR);
						else
							SetLight(internalLights, MMConfig.OPEN_COLOR);

						SetLight(externalLights, MMConfig.LOCK_COLOR);
						break;
					case 2:
						SetLight(internalLights, MMConfig.LOCK_COLOR);

						if (!IsPressureOk(OuterPressure) && !OuterOpen)
							SetLight(externalLights, MMConfig.WARN_COLOR);
						else
							SetLight(externalLights, MMConfig.OPEN_COLOR);
						break;
				}
			}

			private bool IsAVDepressurizing(IMyAirVent airvent)
			{
				return airvent.Depressurize;
			}

			private bool IsDepressurizing()
			{
				for (int i = 0; i < controlVents.Count; i++)
					if (IsAVDepressurizing(controlVents[i]))
						return true;

				return false;
			}

			private void Depressurize(bool on)
			{
				for (int i = 0; i < controlVents.Count; i++)
				{
					if (on)
						controlVents[i].ApplyAction("Depressurize_On");
					else
						controlVents[i].ApplyAction("Depressurize_Off");
				}
			}

			private bool Opening = false;
			private bool Stuck = false;
		}
	}
}
