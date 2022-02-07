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
        public bool AlwaysUse { get; set; } = false;
        public string Portrait { 
            get { return portraitPath; }
            set {
                portraitPath = value;
                stored = null;
                if(portraitPath != null)
                {
                    try
                    {
                        stored = ModEntry.helper.Content.Load<Texture2D>(value, ContentSource.GameContent);
                    } catch(ContentLoadException e)
                    {
                        ModEntry.monitor.Log("Could not find image at game asset path: '" + value + "' .", LogLevel.Warn);
                        ModEntry.monitor.Log(e.StackTrace, LogLevel.Warn);
                    }
                }
            }
        }
        public Texture2D overrideTexture
        {
            get {
                return stored;
            }
        }
        private Texture2D stored = null;
        private string portraitPath = null;
    }
}
