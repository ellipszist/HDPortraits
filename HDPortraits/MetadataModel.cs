using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;

namespace HDPortraits
{
    public class MetadataModel
    {
        public int Size { set; get; } = 64;
        public AnimationModel Animation { get; set; } = null;
        public string Portrait { 
            get { return portraitPath; }
            set {
                portraitPath = value;
                Reload();
            }
        }
        public Texture2D overrideTexture
        {
            get {
                return stored;
            }
        }
        internal string defaultPath = null;
        private Texture2D stored = null;
        private Texture2D savedDefault = null;
        private string portraitPath = null;
        public void Reload()
        {
            Animation?.Reset();
            if (portraitPath is not null)
            {
                try
                {
                    stored = ModEntry.helper.Content.Load<Texture2D>(portraitPath, ContentSource.GameContent);
                }
                catch (ContentLoadException)
                {
                    ModEntry.monitor.Log("Could not find image at game asset path: '" + portraitPath + "' .", LogLevel.Warn);
                }
            }
            if (defaultPath is not null)
            {
                try
                {
                    savedDefault = ModEntry.helper.Content.Load<Texture2D>(defaultPath, ContentSource.GameContent);
                }
                catch (ContentLoadException)
                {
                    ModEntry.monitor.Log("Could not find default asset at path: '" + defaultPath + "'! An NPC is missing their portrait!", LogLevel.Error);
                }
            }
        }
        public Texture2D GetDefault()
        {
            return savedDefault;
        }
    }
}
