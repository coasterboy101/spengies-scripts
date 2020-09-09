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
		void Configuration()
		{
			/* LIGHT COLOR SETTINGS 
			 *  
			 * 1. number is red color from 0.0 (black) to 1.0 (red) 
			 * 2. number is green color from 0.0 (black) to 1.0 (green) 
			 * 3. number is blue color from 0.0 (black) to 1.0 (blue) 
			 *  
			 * Examples: 
			 *   Color(0.5f, 0.0f, 0.0f) is dark red 
			 *   Color(0.0f, 0.5f, 0.0f) is dark green 
			 *   Color(1.0f, 1.0f, 1.0f) is white color 
			 *   Color(1.0f, 1.0f, 0.0f) is yellow 
			 *   Color(1.0f, 0.5f, 0.0f) is orange 
			 *   Color(0.0f, 1.0f, 1.0f) is cyan 
			 *  
			 * change only the numbers (leave f at the end of each number) */

			// color when doors are locked 
			MMConfig.LOCK_COLOR = new Color(1.0f, 0.0f, 0.0f);
			// color when pressure is wrong but doors are unlocked 
			MMConfig.WARN_COLOR = new Color(1.0f, 0.5f, 0.0f);
			// color when doors are open 
			MMConfig.OPEN_COLOR = new Color(0.0f, 1.0f, 0.0f);

			// Tags must be surrounded by square brackets, e.g. [AI] for Airlock Inner
			// Group name must start with this to be considered by this script (case insensitive)
			MMConfig.GROUP_TAG = "a";

			MMConfig.INNER_TAG = "i";
			MMConfig.OUTER_TAG = "o";
			MMConfig.CONTROL_TAG = "c";

			// Should we completely open the doors even if pressure is not ok?
			MMConfig.OPEN_ANYWAY = false;
		}
		#endregion

		// Enable debug to antenna or LCD marked with [DEBUG] 
		public static bool EnableDebug = false;

		public Program()
		{
			Runtime.UpdateFrequency = UpdateFrequency.Update10;
		}

		public void Main(string argument)
		{
			Configuration();

			// Init MMAPI and debug panels marked with [DEBUG] 
			MM.Init(GridTerminalSystem, EnableDebug, this);

			AirlockControlProgram prog = new AirlockControlProgram();
			prog.Run(argument);
		}
	}
}
