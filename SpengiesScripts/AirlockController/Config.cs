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
		public static class Config
		{
			public static string GROUP_TAG = "A";
			public static string INNER_TAG = "I";
			public static string OUTER_TAG = "E";
			public static string CONTROL_TAG = "C";
			
			public static bool OPEN_ANYWAY = false;

			public static Color LOCK_COLOR;
			public static Color WARN_COLOR;
			public static Color OPEN_COLOR;
		}
	}
}
