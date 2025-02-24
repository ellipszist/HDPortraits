﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using System.Collections.Generic;
using HDPortraits.Patches;
using HDPortraits.Models;
using System;
using AeroCore.Utils;

namespace HDPortraits
{
	public class API : IHDPortraitsAPI
	{
		public string OverrideName { 
			get => PortraitDrawPatch.overrideName.Value; 
			set => PortraitDrawPatch.overrideName.Value = value; 
		}
		public void DrawPortrait(SpriteBatch b, NPC npc, int index, Rectangle region, Color? color = null, bool reset = false)
		{
			(Rectangle source, Texture2D tex) = GetTextureAndRegion(npc, index, Game1.currentGameTime.ElapsedGameTime.Milliseconds, reset);
			b.Draw(tex, region, source, color ?? Color.White);
		}
		public void DrawPortrait(SpriteBatch b, NPC npc, int index, Point position, Color? color = null, bool reset = false)
			=> DrawPortrait(b, npc, index, new Rectangle(position, new(256, 256)), color, reset);
		public void DrawPortrait(SpriteBatch b, string name, string suffix, int index, Rectangle region, Color? color = null, bool reset = false)
		{
			(Rectangle source, Texture2D tex) = GetTextureAndRegion(name, suffix, index, Game1.currentGameTime.ElapsedGameTime.Milliseconds, reset);
			b.Draw(tex, region, source, color ?? Color.White);
		}
		public void DrawPortrait(SpriteBatch b, string name, string suffix, int index, Point position, Color? color = null, bool reset = false)
			=> DrawPortrait(b, name, suffix, index, new Rectangle(position, new(256, 256)), color, reset);
		public void DrawPortraitOrOverride(SpriteBatch b, NPC npc, int index, Rectangle region, Color? color = null, bool reset = false)
		{
			if (npc is not null)
				DrawPortrait(b, npc, index, region, color, reset);
			else if(PortraitDrawPatch.overrideName.Value is not null)
				DrawPortrait(b, PortraitDrawPatch.overrideName.Value, null, index, region, color, reset);
		}
		public void DrawPortraitOrOverride(SpriteBatch b, NPC npc, int index, Point position, Color? color = null, bool reset = false)
			=> DrawPortraitOrOverride(b, npc, index, new Rectangle(position, new(256, 256)), color, reset);
		public string GetEventPortraitFor(NPC npc)
			=> npc.uniquePortraitActive ? PortraitDrawPatch.NpcEventSuffixes.Value.GetValueOrDefault(npc, null) : null;
		public (Rectangle, Texture2D) GetTextureAndRegion(NPC npc, int index, int elapsed = -1, bool reset = false)
		{
			if (!ModEntry.TryGetMetadata(npc.Name, PortraitDrawPatch.GetSuffix(npc), out var data))
				return (Game1.getSourceRectForStandardTileSheet(npc.Portrait, index, 64, 64), npc.Portrait);
			if (!data.TryGetTexture(out var texture))
				texture = npc.Portrait;
			if (reset)
				data.Animation?.Reset();
			return (data.GetRegion(index, elapsed).Clamp(texture.Bounds), texture);
		}
		public (Rectangle, Texture2D) GetTextureAndRegion(string name, string suffix, int index, int elapsed = -1, bool reset = false)
		{
			var path = $"Portraits/{name}{(suffix is not null ? '_' + suffix : null)}";
			if (!Misc.TryLoadAsset<Texture2D>(ModEntry.monitor, ModEntry.helper, path, out var tex))
			{
				ModEntry.monitor.Log(
					$"Default portrait '{path}' does not exist or could not be loaded! Do you have a typo or missing asset?",
					LogLevel.Warn);
				return (default, null);
			}

			if (!ModEntry.TryGetMetadata(name, suffix, out var metadata))
				return (Game1.getSourceRectForStandardTileSheet(tex, index, 64, 64), tex);

			if (reset)
				metadata.Animation?.Reset();

			Texture2D texture = metadata.overrideTexture.Value ?? tex;
			return (metadata.GetRegion(index, elapsed).Clamp(texture.Bounds), texture);
		}
		public void ReloadData()
			=> ModEntry.monitor.Log("ReloadData() is deprecated! Invalidate the relevant assets instead, and they will be automatically reloaded.", 
				LogLevel.Debug);

		public bool TryGetPortrait(string name, string suffix, int index, out Texture2D texture, out Rectangle region, int millis = -1, bool forceSuffix = false)
		{
			region = new(0, 0, 64, 64);
			texture = null;
			if(!ModEntry.TryGetMetadata(name, suffix, out var data, forceSuffix))
				return false;
			if(!data.TryGetTexture(out texture))
			{
				ModEntry.monitor.Log($"Could not load portrait for '{name}_{suffix}'! No texture found.", LogLevel.Warn);
				return false;
			}
			region = data.GetRegion(index, millis);
			return true;
		}
		public bool TryGetPortrait(NPC character, int index, out Texture2D texture, out Rectangle region, int millis = -1)
		{
			texture = null;
			region = default;
			if (character is null)
				return false;
			if (ModEntry.TryGetMetadata(character.Name, PortraitDrawPatch.GetSuffix(character), out var data))
			{
				if (!data.TryGetTexture(out texture))
					texture = character.Portrait;
				region = data.GetRegion(index, millis);
				return true;
			}
			texture = character.Portrait;
			region = Game1.getSourceRectForStandardTileSheet(texture, index, 64, 64);
			return false;
		}
	}
}
