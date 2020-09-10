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
		#region mdk preserve
		public void Configuration()
		{
			// Lignting color when doors are locked 
			Config.LOCK_COLOR = new Color(1.0f, 0.0f, 0.0f);
			// Lighting color when pressure is wrong but doors are unlocked 
			Config.WARN_COLOR = new Color(1.0f, 0.5f, 0.0f);
			// Lighting color when doors are open 
			Config.OPEN_COLOR = new Color(0.0f, 1.0f, 0.0f);

			// Tags must be surrounded by square brackets, e.g. [AI] for Airlock Inner
			// Group name must start with this to be considered by this script (case insensitive)
			Config.GROUP_TAG = "A";

			Config.INNER_TAG = "I";
			Config.OUTER_TAG = "E";
			Config.CONTROL_TAG = "C";

			// Should we completely open the doors even if pressure is not ok?
			Config.OPEN_ANYWAY = false;
		}
		#endregion

		// Enable debug to antenna or LCD marked with [DEBUG] 
		//public static bool EnableDebug = false;

		public Program()
		{
			Runtime.UpdateFrequency = UpdateFrequency.Update10;
		}

		public void Main(string argument)
		{
			Configuration();

			// Init MMAPI and debug panels marked with [DEBUG] 
			DR.Init(GridTerminalSystem, EnableDebug, this);

			AirlockControlProgram prog = new AirlockControlProgram();
			prog.Run(argument);
		}
	}
}
