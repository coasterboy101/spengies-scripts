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
		//public class AirlockControlProgram
		//{
		//	public static MMAirlockCollection airlocks = new MMAirlockCollection();
		//	public static Dictionary<string, DRPanel> panels = new Dictionary<string, DRPanel>();
		//	public List<DRPanel> panelList = new List<DRPanel>();

		//	public DRAirlock GetAirlock(string name)
		//	{
		//		DRAirlock airlock = null;
		//		if (airlocks.ContainsKey(name))
		//			airlock = airlocks.GetItem(name);
		//		else
		//		{
		//			airlock = new DRAirlock();
		//			airlock.name = name;
		//			airlocks.AddItem(name, airlock);
		//		}

		//		return airlock;
		//	}

		//	public DRPanel GetPanel(IMyTextPanel lcd)
		//	{
		//		DRPanel panel = null;
		//		string key = lcd.CustomName + lcd.GetPosition().ToString("F0") + lcd.NumberInGrid.ToString();
		//		if (panels.ContainsKey(key))
		//			panel = panels[key];
		//		else
		//		{
		//			panel = new DRPanel();
		//			panels.Add(key, panel);
		//		}

		//		if (!panel.panels.ContainsKey(key))
		//			panel.panels.AddItem(key, lcd);

		//		if (!panelList.Contains(panel))
		//			panelList.Add(panel);

		//		return panel;
		//	}

		//	public void AddGroup(IMyBlockGroup group)
		//	{
		//		List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();

		//		DR.Debug("Group: '" + group.Name + "'");
		//		int spos = group.Name.IndexOf('[');
		//		int epos = group.Name.IndexOf(']');
		//		if (spos < 0 || epos < 0)
		//			return;

		//		string tag = group.Name.Substring(spos + 1, epos - 1).ToLower();
		//		string name = group.Name.Length > epos + 1 ? group.Name.Substring(epos + 1).Trim() : "";
		//		group.GetBlocks(blocks);

		//		if (tag == MMConfig.GROUP_TAG + MMConfig.CONTROL_TAG)
		//		{
		//			DRAirlock airlock = GetAirlock(name);

		//			for (int i = 0; i < blocks.Count; i++)
		//			{
		//				IMyTerminalBlock block = blocks[i];
		//				IMyAirVent airvent = block as IMyAirVent;
		//				if (airvent != null)
		//				{
		//					airlock.airVents.Add(airvent);
		//					if (airvent.CanPressurize)
		//					{
		//						double oxyLevel = airvent.GetOxygenLevel();
		//						if (oxyLevel < airlock.lowestPressure)
		//							airlock.lowestPressure = oxyLevel;
		//					}
		//					else
		//						airlock.lowestPressure = -1f;
		//					continue;
		//				}

		//				IMyLightingBlock light = block as IMyLightingBlock;
		//				if (light != null)
		//					airlock.control = light;

		//				IMyTextPanel lcd = block as IMyTextPanel;
		//				if (lcd != null)
		//				{
		//					DRPanel panel = GetPanel(lcd);
		//					airlock.lcds.Add(panel);
		//				}
		//			}

		//			DR.Debug("LowestPressure: " + airlock.lowestPressure.ToString("F0"));

		//			if (airlock.lowestPressure > 0.01f && airlock.lowestPressure >= Math.Max(airlock.InnerPressure, airlock.OuterPressure))
		//			{
		//				if (airlock.FullTime < DRAirlock.FullTimeThreshold)
		//					airlock.FullTime++;
		//				airlock.EmptyTime = 0;
		//			}
		//			else
		//			{
		//				airlock.FullTime = 0;

		//				if (airlock.lowestPressure <= 0.01f)
		//				{
		//					if (airlock.EmptyTime < DRAirlock.EmptyTimeThreshold)
		//						airlock.EmptyTime++;
		//				}
		//				else
		//					airlock.EmptyTime = 0;
		//			}

		//			return;
		//		}

		//		if (tag == MMConfig.GROUP_TAG + MMConfig.INNER_TAG)
		//		{
		//			DRAirlock airlock = GetAirlock(name);

		//			for (int i = 0; i < blocks.Count; i++)
		//			{
		//				IMyDoor door = blocks[i] as IMyDoor;
		//				if (door != null)
		//				{
		//					if (door.Status == DoorStatus.Open)
		//						airlock.InnerOpen = true;
		//					airlock.innerDoors.Add(door);
		//					continue;
		//				}

		//				IMyLightingBlock light = blocks[i] as IMyLightingBlock;
		//				if (light != null)
		//				{
		//					airlock.innerLights.Add(light);
		//					continue;
		//				}

		//				IMySoundBlock sound = blocks[i] as IMySoundBlock;
		//				if (sound != null)
		//				{
		//					airlock.innerSound.Add(sound);
		//					continue;
		//				}

		//				IMyAirVent airvent = blocks[i] as IMyAirVent;
		//				if (airvent != null)
		//				{
		//					float pres = (airvent.CanPressurize ? airvent.GetOxygenLevel() : -1f);
		//					if (pres < airlock.InnerPressure)
		//						airlock.InnerPressure = pres;
		//					continue;
		//				}
		//			}
		//			return;
		//		}

		//		if (tag == MMConfig.GROUP_TAG + MMConfig.OUTER_TAG)
		//		{
		//			DRAirlock airlock = GetAirlock(name);

		//			for (int i = 0; i < blocks.Count; i++)
		//			{
		//				IMyDoor door = blocks[i] as IMyDoor;
		//				if (door != null)
		//				{
		//					if (door.Status == DoorStatus.Open)
		//						airlock.OuterOpen = true;
		//					airlock.outerDoors.Add(door);
		//					continue;
		//				}

		//				IMyLightingBlock light = blocks[i] as IMyLightingBlock;
		//				if (light != null)
		//				{
		//					airlock.outerLights.Add(light);
		//					continue;
		//				}

		//				IMySoundBlock sound = blocks[i] as IMySoundBlock;
		//				if (sound != null)
		//				{
		//					airlock.outerSound.Add(sound);
		//				}

		//				IMyAirVent airvent = blocks[i] as IMyAirVent;
		//				if (airvent != null)
		//				{
		//					float pres = (airvent.CanPressurize ? airvent.GetOxygenLevel() : -1f);
		//					if (pres > airlock.OuterPressure)
		//						airlock.OuterPressure = pres;
		//					continue;
		//				}
		//			}
		//			return;
		//		}
		//	}

		//	public void Run(string argument)
		//	{
		//		int tmpIdx = argument.Trim().LastIndexOf(" ");
		//		string cmd_airlock;
		//		string cmd_command;
		//		if (tmpIdx >= 0)
		//		{
		//			cmd_airlock = argument.Substring(0, tmpIdx);
		//			cmd_command = (tmpIdx + 1 < argument.Length ? argument.Substring(tmpIdx + 1) : "toggle");
		//		}
		//		else
		//		{
		//			cmd_airlock = argument;
		//			cmd_command = "toggle";
		//		}

		//		for (int i = 0; i < airlocks.CountAll(); i++)
		//			airlocks.GetItemAt(i).Reset();

		//		List<IMyBlockGroup> controlGroups = new List<IMyBlockGroup>();
		//		DR.Debug("Processing inner and outer groups");
		//		List<IMyBlockGroup> BlockGroups = new List<IMyBlockGroup>();
		//		DR.gridTerminalSystem.GetBlockGroups(BlockGroups);
		//		for (int gid = 0; gid < BlockGroups.Count; gid++)
		//		{
		//			IMyBlockGroup group = BlockGroups[gid];
		//			string name = group.Name.ToLower();

		//			int spos = name.IndexOf('[');
		//			int epos = name.IndexOf(']');
		//			if (spos < 0 || epos < 0)
		//				continue;

		//			string tag = name.Substring(spos + 1, epos - 1).ToLower();
		//			if (!tag.StartsWith(MMConfig.GROUP_TAG))
		//				continue;

		//			if (tag.StartsWith(MMConfig.GROUP_TAG + MMConfig.CONTROL_TAG))
		//			{
		//				controlGroups.Add(group);
		//				continue;
		//			}

		//			AddGroup(group);
		//		}
		//		DR.Debug("Processing control group");
		//		for (int gid = 0; gid < controlGroups.Count; gid++)
		//		{
		//			AddGroup(controlGroups[gid]);
		//		}
		//		DR.Debug("Processing LCD panels");
		//		for (int i = 0; i < panelList.Count; i++)
		//		{
		//			panelList[i].SortPanels();
		//			DRLcdTextManager.SetupLCDText(panelList[i]);
		//			DRLcdTextManager.ClearText(panelList[i]);
		//		}

		//		for (int i = 0; i < airlocks.CountAll(); i++)
		//		{
		//			DR.Debug(cmd_airlock);
		//			DRAirlock airlock = airlocks.GetItemAt(i);
		//			if (cmd_airlock == airlock.name)
		//				airlock.command = cmd_command;
		//			airlock.Process();
		//		}
		//		DR.Debug("Updating panels");
		//		for (int i = 0; i < panelList.Count; i++)
		//			panelList[i].Update();
		//	}
		//}

		///// <summary>
		///// IMyTerminalBlock collection with useful methods.
		///// </summary>
		//public class DRBlockCollection
		//{
		//	/// <summary>
		//	/// A list containing all blocks in this collection.
		//	/// </summary>
		//	public List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();

		//	// add Blocks with name containing nameLike   
		//	public void AddBlocksOfNameLike(string nameLike)
		//	{
		//		if (nameLike == "" || nameLike == "*")
		//		{
		//			List<IMyTerminalBlock> lBlocks = new List<IMyTerminalBlock>();
		//			DR.gridTerminalSystem.GetBlocks(lBlocks);
		//			Blocks.AddList(lBlocks);
		//			return;
		//		}

		//		string group = (nameLike.StartsWith("G:") ? nameLike.Substring(2).Trim().ToLower() : "");
		//		if (group != "")
		//		{
		//			List<IMyBlockGroup> BlockGroups = new List<IMyBlockGroup>();
		//			DR.gridTerminalSystem.GetBlockGroups(BlockGroups);

		//			for (int i = 0; i < BlockGroups.Count; i++)
		//			{
		//				IMyBlockGroup g = BlockGroups[i];
		//				if (g.Name.ToLower() == group)
		//					g.GetBlocks(Blocks);
		//			}
		//			return;
		//		}

		//		DR.gridTerminalSystem.SearchBlocksOfName(nameLike, Blocks);
		//	}

		//	// add Blocks of type (optional: with name containing nameLike)   
		//	public void AddBlocksOfType(string type, string nameLike = "")
		//	{
		//		if (nameLike == "" || nameLike == "*")
		//		{
		//			List<IMyTerminalBlock> blocksOfType = new List<IMyTerminalBlock>();
		//			DR.GetBlocksOfType(ref blocksOfType, type);
		//			Blocks.AddList(blocksOfType);
		//		}
		//		else
		//		{
		//			string group = (nameLike.StartsWith("G:") ? nameLike.Substring(2).Trim().ToLower() : "");
		//			if (group != "")
		//			{
		//				List<IMyBlockGroup> BlockGroups = new List<IMyBlockGroup>();
		//				DR.gridTerminalSystem.GetBlockGroups(BlockGroups);

		//				for (int i = 0; i < BlockGroups.Count; i++)
		//				{
		//					IMyBlockGroup g = BlockGroups[i];
		//					if (g.Name.ToLower() == group)
		//					{
		//						List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
		//						g.GetBlocks(blocks);
		//						for (int j = 0; j < blocks.Count; j++)
		//							if (DR.IsBlockOfType(blocks[j], type))
		//								Blocks.Add(blocks[j]);
		//						return;
		//					}
		//				}
		//				return;
		//			}
		//			List<IMyTerminalBlock> blocksOfType = new List<IMyTerminalBlock>();
		//			DR.GetBlocksOfType(ref blocksOfType, type);

		//			for (int i = 0; i < blocksOfType.Count; i++)
		//				if (blocksOfType[i].CustomName.Contains(nameLike))
		//					Blocks.Add(blocksOfType[i]);
		//		}
		//	}

		//	// add all Blocks from collection col to this collection   
		//	public void AddFromCollection(DRBlockCollection col)
		//	{
		//		Blocks.AddList(col.Blocks);
		//	}

		//	// clear all blocks from this collection   
		//	public void Clear()
		//	{
		//		Blocks.Clear();
		//	}

		//	// number of blocks in collection   
		//	public int Count()
		//	{
		//		return Blocks.Count;
		//	}
		//}

		//public class DRPanel
		//{
		//	// full line of progress bar 
		//	public const int DFULL_PROGRESS_CHARS = 116;
		//	public int FULL_PROGRESS_CHARS = 0;
		//	// approximate width of LCD panel line 
		//	public const float LCD_LINE_WIDTH = 730;

		//	public MMTextPanelCollection panels = new MMTextPanelCollection();
		//	public DRLcdTextManager.MMLCDText text = null;
		//	public IMyTextPanel first = null;

		//	public void SetFontSize(float size)
		//	{
		//		for (int i = 0; i < panels.CountAll(); i++)
		//			panels.GetItemAt(i).SetValueFloat("FontSize", size);
		//	}

		//	public void SortPanels()
		//	{
		//		panels.SortAll();
		//		first = panels.GetItemAt(0);
		//	}

		//	public bool IsWide()
		//	{
		//		return (first.DefinitionDisplayNameText.Contains("Wide")
		//			|| first.DefinitionDisplayNameText == "Computer Monitor");
		//	}

		//	public void Update()
		//	{
		//		if (text == null)
		//			return;

		//		int cnt = panels.CountAll();

		//		if (cnt > 1)
		//			SetFontSize(first.GetValueFloat("FontSize"));

		//		for (int i = 0; i < panels.CountAll(); i++)
		//		{
		//			IMyTextPanel panel = panels.GetItemAt(i);
		//			panel.WriteText(text.GetDisplayString(i));
		//			panel.ContentType = VRage.Game.GUI.TextPanel.ContentType.TEXT_AND_IMAGE;
		//		}
		//	}

		//}

		//public static class DRLcdTextManager
		//{
		//	private static Dictionary<IMyTextPanel, MMLCDText> panelTexts = new Dictionary<IMyTextPanel, MMLCDText>();
		//	public static void SetupLCDText(DRPanel p)
		//	{
		//		MMLCDText lcdText = GetLCDText(p);
		//		lcdText.SetTextFontSize(p.first.GetValueFloat("FontSize"));
		//		lcdText.SetNumberOfScreens(p.panels.CountAll());
		//		p.FULL_PROGRESS_CHARS = (int)(DRPanel.DFULL_PROGRESS_CHARS * lcdText.widthMod);
		//		lcdText.widthMod = (p.IsWide() ? 2.0f : 1.0f) * (0.8f / lcdText.fontSize);
		//	}

		//	public static MMLCDText GetLCDText(DRPanel p)
		//	{
		//		MMLCDText lcdText = null;
		//		IMyTextPanel panel = p.first;

		//		if (!panelTexts.TryGetValue(panel, out lcdText))
		//		{
		//			lcdText = new MMLCDText();
		//			p.text = lcdText;
		//			panelTexts.Add(panel, lcdText);
		//		}

		//		p.text = lcdText;
		//		return lcdText;
		//	}

		//	public static void AddLine(DRPanel panel, string line)
		//	{
		//		MMLCDText lcd = GetLCDText(panel);
		//		lcd.AddLine(line);
		//	}

		//	public static void Add(DRPanel panel, string text)
		//	{
		//		MMLCDText lcd = GetLCDText(panel);
		//		lcd.AddFast(text);
		//		lcd.current_width += MMStringFunc.GetStringSize(text);
		//	}

		//	public static void AddRightAlign(DRPanel panel, string text, float end_screen_x)
		//	{
		//		MMLCDText lcd = GetLCDText(panel);

		//		float text_width = MMStringFunc.GetStringSize(text);
		//		end_screen_x *= lcd.widthMod;
		//		end_screen_x -= lcd.current_width;

		//		if (end_screen_x < text_width)
		//		{
		//			lcd.AddFast(text);
		//			lcd.current_width += text_width;
		//			return;
		//		}

		//		end_screen_x -= text_width;
		//		int fillchars = (int)Math.Round(end_screen_x / MMStringFunc.WHITESPACE_WIDTH, MidpointRounding.AwayFromZero);
		//		float fill_width = fillchars * MMStringFunc.WHITESPACE_WIDTH;

		//		string filler = new String(' ', fillchars);
		//		lcd.AddFast(filler + text);
		//		lcd.current_width += fill_width + text_width;
		//	}

		//	public static void AddCenter(DRPanel panel, string text, float screen_x)
		//	{
		//		MMLCDText lcd = GetLCDText(panel);
		//		float text_width = MMStringFunc.GetStringSize(text);
		//		screen_x *= lcd.widthMod;
		//		screen_x -= lcd.current_width;

		//		if (screen_x < text_width / 2)
		//		{
		//			lcd.AddFast(text);
		//			lcd.current_width += text_width;
		//			return;
		//		}

		//		screen_x -= text_width / 2;
		//		int fillchars = (int)Math.Round(screen_x / MMStringFunc.WHITESPACE_WIDTH, MidpointRounding.AwayFromZero);
		//		float fill_width = fillchars * MMStringFunc.WHITESPACE_WIDTH;

		//		string filler = new String(' ', fillchars);
		//		lcd.AddFast(filler + text);
		//		lcd.current_width += fill_width + text_width;
		//	}

		//	public static void AddProgressBar(DRPanel panel, double percent, int width = 22)
		//	{
		//		MMLCDText lcd = GetLCDText(panel);
		//		int totalBars = width - 2;
		//		int fill = (int)(percent * totalBars) / 100;
		//		if (fill > totalBars)
		//			fill = totalBars;
		//		string progress = "[" + new String('|', fill) + new String('\'', totalBars - fill) + "]";

		//		lcd.AddFast(progress);
		//		lcd.current_width += MMStringFunc.PROGRESSCHAR_WIDTH * width;
		//	}

		//	public static void ClearText(DRPanel panel)
		//	{
		//		GetLCDText(panel).ClearText();
		//	}

		//	public static void UpdatePanel(DRPanel panel)
		//	{
		//		panel.Update();
		//		GetLCDText(panel).ScrollNextLine();
		//	}

		//	public class MMLCDText
		//	{
		//		public int SCROLL_LINES = 5;
		//		public float fontSize = 0.8f;
		//		public float widthMod = 1.0f;
		//		public int scrollPosition = 0;
		//		public int scrollDirection = 1;
		//		public int DisplayLines = 22; // 22 for font size 0.8 
		//		public int screens = 1;

		//		public List<string> lines = new List<string>();
		//		public int current_line = 0;
		//		public float current_width = 0;

		//		public MMLCDText(float _fontSize = 0.8f)
		//		{
		//			SetTextFontSize(_fontSize);
		//			lines.Add("");
		//		}

		//		public void SetTextFontSize(float _fontSize)
		//		{
		//			fontSize = _fontSize;
		//			DisplayLines = (int)Math.Round(22 * (0.8 / fontSize) * screens);
		//		}

		//		public void SetNumberOfScreens(int _screens)
		//		{
		//			screens = _screens;
		//			DisplayLines = (int)Math.Round(22 * (0.8 / fontSize) * screens);
		//		}

		//		public void AddFast(string text)
		//		{
		//			lines[current_line] += text;
		//		}

		//		public void AddLine(string line)
		//		{
		//			lines[current_line] += line;
		//			lines.Add("");
		//			current_line++;
		//			current_width = 0;
		//		}

		//		public void ClearText()
		//		{
		//			lines.Clear();
		//			lines.Add("");
		//			current_width = 0;
		//			current_line = 0;
		//		}

		//		public string GetFullString()
		//		{
		//			return String.Join("\n", lines);
		//		}

		//		// Display only X lines from scrollPos 
		//		public string GetDisplayString(int screenidx = 0)
		//		{
		//			if (lines.Count < DisplayLines / screens)
		//			{
		//				if (screenidx == 0)
		//				{
		//					scrollPosition = 0;
		//					scrollDirection = 1;
		//					return GetFullString();
		//				}
		//				return "";
		//			}


		//			int scrollPos = scrollPosition + screenidx * (DisplayLines / screens);
		//			if (scrollPos > lines.Count)
		//				scrollPos = lines.Count;

		//			List<string> display =
		//				lines.GetRange(scrollPos,
		//					Math.Min(lines.Count - scrollPos, DisplayLines / screens));

		//			return String.Join("\n", display);
		//		}

		//		public void ScrollNextLine()
		//		{
		//			int lines_cnt = lines.Count - 1;
		//			if (lines_cnt <= DisplayLines)
		//			{
		//				scrollPosition = 0;
		//				scrollDirection = 1;
		//				return;
		//			}

		//			if (scrollDirection > 0)
		//			{
		//				if (scrollPosition + SCROLL_LINES + DisplayLines > lines_cnt)
		//				{
		//					scrollDirection = -1;
		//					scrollPosition = Math.Max(lines_cnt - DisplayLines, 0);
		//					return;
		//				}

		//				scrollPosition += SCROLL_LINES;
		//			}
		//			else
		//			{
		//				if (scrollPosition - SCROLL_LINES < 0)
		//				{
		//					scrollPosition = 0;
		//					scrollDirection = 1;
		//					return;
		//				}

		//				scrollPosition -= SCROLL_LINES;
		//			}
		//		}
		//	}
		//}

		//public static class MMStringFunc
		//{
		//	private static Dictionary<char, float> charSize = new Dictionary<char, float>();

		//	public const float WHITESPACE_WIDTH = 8f;
		//	public const float PROGRESSCHAR_WIDTH = 6f;

		//	public static void InitCharSizes()
		//	{
		//		if (charSize.Count > 0)
		//			return;

		//		AddCharsSize("3FKTabdeghknopqsuy", 17f);
		//		AddCharsSize("#0245689CXZ", 19f);
		//		AddCharsSize("$&GHPUVY", 20f);
		//		AddCharsSize("ABDNOQRS", 21f);
		//		AddCharsSize("(),.1:;[]ft{}", 9f);
		//		AddCharsSize("+<=>E^~", 18f);
		//		AddCharsSize(" !I`ijl", 8f);
		//		AddCharsSize("7?Jcz", 16f);
		//		AddCharsSize("L_vx", 15f);
		//		AddCharsSize("\"-r", 10f);
		//		AddCharsSize("mw", 27f);
		//		AddCharsSize("M", 26f);
		//		AddCharsSize("W", 31f);
		//		AddCharsSize("'|", 6f);
		//		AddCharsSize("*", 11f);
		//		AddCharsSize("\\", 12f);
		//		AddCharsSize("/", 14f);
		//		AddCharsSize("%", 24f);
		//		AddCharsSize("@", 25f);
		//		AddCharsSize("\n", 0f);
		//	}

		//	private static void AddCharsSize(string chars, float size)
		//	{
		//		for (int i = 0; i < chars.Length; i++)
		//			charSize.Add(chars[i], size);
		//	}

		//	public static float GetCharSize(char c)
		//	{
		//		float width = 17f;
		//		charSize.TryGetValue(c, out width);

		//		return width;
		//	}

		//	public static float GetStringSize(string str)
		//	{
		//		float sum = 0;
		//		for (int i = 0; i < str.Length; i++)
		//			sum += GetCharSize(str[i]);

		//		return sum;
		//	}

		//	public static string GetStringTrimmed(string text, float pixel_width)
		//	{
		//		int trimlen = Math.Min((int)pixel_width / 14, text.Length - 2);
		//		float stringSize = GetStringSize(text);
		//		if (stringSize <= pixel_width)
		//			return text;

		//		while (stringSize > pixel_width - 20)
		//		{
		//			text = text.Substring(0, trimlen);
		//			stringSize = GetStringSize(text);
		//			trimlen -= 2;
		//		}
		//		return text + "..";
		//	}
		//}

		//public class MMTextPanelCollection
		//{
		//	public Dictionary<string, IMyTextPanel> dict = new Dictionary<string, IMyTextPanel>();
		//	public List<string> keys = new List<string>();

		//	public void AddItem(string key, IMyTextPanel item) { if (!dict.ContainsKey(key)) { keys.Add(key); dict.Add(key, item); } }
		//	public int CountAll() { return dict.Count; }
		//	public bool ContainsKey(string k) { return dict.ContainsKey(k); }
		//	public bool ContainsItem(IMyTextPanel item) { return dict.ContainsValue(item); }
		//	public IMyTextPanel GetItem(string key) { if (dict.ContainsKey(key)) return dict[key]; return null; }
		//	public IMyTextPanel GetItemAt(int index) { return dict[keys[index]]; }
		//	public void ClearAll() { keys.Clear(); dict.Clear(); }
		//	public void SortAll() { keys.Sort(); }
		//}
		//public class MMAirlockCollection
		//{
		//	public Dictionary<string, DRAirlock> dict = new Dictionary<string, DRAirlock>();
		//	public List<string> keys = new List<string>();

		//	public void AddItem(string key, DRAirlock item) { if (!dict.ContainsKey(key)) { keys.Add(key); dict.Add(key, item); } }
		//	public int CountAll() { return dict.Count; }
		//	public bool ContainsKey(string k) { return dict.ContainsKey(k); }
		//	public bool ContainsItem(DRAirlock item) { return dict.ContainsValue(item); }
		//	public DRAirlock GetItem(string key) { if (dict.ContainsKey(key)) return dict[key]; return null; }
		//	public DRAirlock GetItemAt(int index) { return dict[keys[index]]; }
		//	public void ClearAll() { keys.Clear(); dict.Clear(); }
		//	public void SortAll()
		//	{
		//		keys.Sort();
		//	}
		//}
	}
}
