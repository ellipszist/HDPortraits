﻿using AeroCore.Generics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;

namespace HDPortraits.Models
{
	public class MetadataModel
	{
		public int Size { set; get; } = 64;
		public AnimationModel Animation { get; set; } = null;
		public string Portrait { 
			get => portraitPath; 
			set {
				portraitPath = value;
				overrideTexture.Reload();
			}
		}

		public readonly LazyAsset<Texture2D> overrideTexture;
		private string portraitPath = null;
		public readonly LazyAsset<Texture2D> originalTexture;
		internal string originalPath = null;

		public MetadataModel()
		{
			overrideTexture = new(ModEntry.helper, () => portraitPath)
			{
				CatchErrors = true
			};
			originalTexture = new(ModEntry.helper, () => originalPath);
		}

		public bool TryGetTexture(out Texture2D texture)
		{
			if (portraitPath is null)
			{
				texture = originalTexture.Value;
				return true;
			}
			texture = overrideTexture.Value;
			if (overrideTexture.LastError is not null)
			{
				ModEntry.monitor.Log($"An error occurred attempting to load portrait override @ '{portraitPath}':\n{overrideTexture.LastError}", 
					LogLevel.Error);
				return false;
			}
			return true;
		}
		public Rectangle GetRegion(int which, int millis = -1)
		{
			var missing = !TryGetTexture(out var tex);
			int size = missing ? 64 : Size;
			return Animation is null ? Game1.getSourceRectForStandardTileSheet(tex, which, size, size) : 
				Animation.GetSourceRegion(tex, size, which, millis);
		}
	}
}
