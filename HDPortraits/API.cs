using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;

namespace HDPortraits
{
    class API : IHDPortraitsAPI
    {
        public void DrawPortrait(SpriteBatch b, NPC npc, int index, Rectangle region, Color? color = null, bool reset = false)
        {
            (Rectangle source, Texture2D tex) = GetTextureAndRegion(npc, index, Game1.currentGameTime.ElapsedGameTime.Milliseconds, reset);
            b.Draw(tex, region, source, color ?? Color.White);
        }
        public void DrawPortrait(SpriteBatch b, NPC npc, int index, Point position, Color? color = null, bool reset = false)
        {
            DrawPortrait(b, npc, index, new Rectangle(position, new(256, 256)), color, reset);
        }
        public (Rectangle, Texture2D) GetTextureAndRegion(NPC npc, int index, int elapsed = -1, bool reset = false)
        {
            if (!ModEntry.portraitSizes.TryGetValue(npc.name, out var metadata))
                return (Game1.getSourceRectForStandardTileSheet(npc.Portrait, index, 64, 64), npc.Portrait);

            if (reset)
                metadata.Animation?.Reset();

            Texture2D texture = metadata.overrideTexture ?? npc.Portrait;
            Rectangle rect = (metadata.Animation != null) ?
                metadata.Animation.GetSourceRegion(texture, metadata.Size, index, elapsed) :
                Game1.getSourceRectForStandardTileSheet(texture, index, metadata.Size, metadata.Size);
            return (rect, texture);
        }
        public void ReloadData()
        {
            ModEntry.ReloadData();
        }
    }
}
