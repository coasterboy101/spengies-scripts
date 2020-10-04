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

			private Program program = null;
			private IMyTextSurface progammingBlockLcd = null;

			private bool debug = false;

			public RAPI(Program program, bool debug = false)
			{
				CommandLine = new MyCommandLine();
				Ini = new MyIni();

				this.program = program;
				this.debug = debug;

				if (this.program.Me.SurfaceCount > 0)
					progammingBlockLcd = this.program.Me.GetSurface(0);
			}

			public void Debug(string message, bool append = true)
			{
				if (!debug)
					return;

				if (progammingBlockLcd != null)
					progammingBlockLcd.WriteText(message, append);

				program.Echo(message);
			}
		}
	}
}
