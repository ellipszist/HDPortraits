using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace HDPortraits
{
    public interface IHDPortraitsAPI
    {
        public void DrawPortrait(SpriteBatch b, NPC npc, int index, Rectangle region, Color? color = null, bool reset = false);
        public void DrawPortrait(SpriteBatch b, NPC npc, int index, Point position, Color? color = null, bool reset = false);
        public (Rectangle, Texture2D) GetTextureAndRegion(NPC npc, int index, int elapsed = -1, bool reset = false);
        public void ReloadData();
    }
}
