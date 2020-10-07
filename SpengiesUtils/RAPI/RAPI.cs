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
			public Program Program { get; private set; }

			private IMyTextSurface _progammingBlockLcd = null;

			private bool _debug = false;

			public RAPI(Program program, bool debug = false)
			{
				CommandLine = new MyCommandLine();
				Ini = new MyIni();
				Program = program;

				_debug = debug;

				if (Program.Me.SurfaceCount > 0)
					_progammingBlockLcd = Program.Me.GetSurface(0);
			}

			public void Debug(string message, bool append = true)
			{
				if (!_debug)
					return;

				if (_progammingBlockLcd != null)
					_progammingBlockLcd.WriteText(message, append);

				Program.Echo(message);
			}
		}
	}
}
