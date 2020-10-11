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
using VRage.Game.ModAPI.Ingame;
using VRage.Game.ModAPI.Ingame.Utilities;
using VRage.Game.ObjectBuilders.Definitions;
using VRage.Game;
using VRageMath;
using System.Runtime.CompilerServices;

namespace IngameScript
{
	partial class Program
	{
		public class RAPI
		{
			public MyCommandLine CommandLine { get; set; }
			public MyIni Ini { get; set; }
			public Program Program { get; private set; }

			private static IMyTextSurface programmingBlockLcd = null;

			private bool debug = false;

			public RAPI(Program program, bool debug = false)
			{
				CommandLine = new MyCommandLine();
				Ini = new MyIni();
				Program = program;

				this.debug = debug;

				if (Program.Me.SurfaceCount > 0)
				{
					programmingBlockLcd = Program.Me.GetSurface(0);
					programmingBlockLcd.ContentType = ContentType.TEXT_AND_IMAGE;
					programmingBlockLcd.WriteText(String.Empty);
				}
			}

			public void Debug(string message, bool append = true)
			{
				if (!debug)
					return;

				if (programmingBlockLcd != null)
					programmingBlockLcd.WriteText($"{message}\n", append);

				Program.Echo(message);
			}
		}
	}
}
