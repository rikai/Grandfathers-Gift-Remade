using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.Menus;
using StardewModdingAPI;
using StardewModdingAPI.Events;

namespace pepoHelper
{
    static class pepoCommon
    {
        public static IMonitor monitor = null;
        public static SpriteBatch spriteBatch;

        public static void LogTr(string message)
        {
            pepoCommon.monitor?.Log(message, LogLevel.Trace);
        }
    }

    static class pepoDrawer
    {
        public static void MultilineString(
            SpriteFont font, string message, Rectangle area, SpriteBatch b = null)
        {
            if (b != null) pepoCommon.spriteBatch = b;
            b = pepoCommon.spriteBatch;
            String todoStr = message;
            float lineHeight = Game1.dialogueFont.MeasureString("M").Y;

            Vector2 curPos = new Vector2(area.X, area.Y);
            String workStr = "";
            String prevStr = "";
            for (int i = 0; i < todoStr.Length; i++)
            {
                prevStr = workStr;
                workStr += todoStr[i];
                if (Game1.dialogueFont.MeasureString(workStr).X > area.Width)
                {
                    b.DrawString(font, prevStr, curPos, Color.Black);
                    curPos.Y += lineHeight;
                    workStr = todoStr.Substring(i, 1);
                }
            }
            b.DrawString(font, workStr, curPos, Color.Black);
            return;
        }

        public static void MultilineStringWithWordWrap(
            SpriteFont font, string message, Rectangle area,
            int lineHeightPercent = 110, SpriteBatch b = null)
        {
            if (b != null) pepoCommon.spriteBatch = b;
            b = pepoCommon.spriteBatch;
            string[] toDoStr = message.Split();
            float factor = ((float)lineHeightPercent / (float)100.0);
            // "M" is the boxiest letter: tallest and widest.
            float lineHeight = font.MeasureString("M").Y * factor;

            Vector2 curPos = new Vector2(area.X, area.Y);
            String workStr = "";
            String prevStr = "";
            for (int i = 0; i < toDoStr.Length; i++)
            {
                prevStr = workStr;
                workStr += " " + toDoStr[i];
                if (font.MeasureString(workStr).X > area.Width)
                {
                    b.DrawString(font, prevStr, curPos, Color.Black);
                    curPos.Y += lineHeight;
                    workStr = toDoStr[i];
                }
            }
            b.DrawString(font, workStr, curPos, Color.Black);
            return;
        }

        public static void BlackScreen(SpriteBatch b = null)
        {
            if (b != null) pepoCommon.spriteBatch = b;
            b = pepoCommon.spriteBatch;
            // Too small, and the drawing engine takes too much CPU time. But too big, the drawing engine
            // probably will push the graphics driver too heavily. Need to find a right balance here...
            const int TEXTURE_SIZE = 16;
            const uint TEXTURE_DATA_ARGB = 0xffffffff;
            Rectangle target = new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height);
            Texture2D flatblank = new Texture2D(Game1.graphics.GraphicsDevice, TEXTURE_SIZE, TEXTURE_SIZE);
            uint[] data = new uint[TEXTURE_SIZE * TEXTURE_SIZE];
            for (int i = 0; i < data.Length; i++)
            {
                data[i] = TEXTURE_DATA_ARGB;
            }
            flatblank.SetData<uint>(data);
            Color black = new Color(0, 0, 0);
            b.Draw(flatblank, target, black);
        }
    }

    internal class DialogOnBlack : DialogueBox
    {
        public DialogOnBlack(string dialog) : base(dialog)
        {
        }

        public override void draw(SpriteBatch b)
        {
            pepoDrawer.BlackScreen(b);
            base.draw(b);
        }
    }

    public static class FarmerExtension
    {
        public static void moveRelTiles(this Farmer f, int h=0, int v=0)
        {
            pepoCommon.LogTr($"farmer was at ({f.getTileX()},{f.getTileY()})");
            f.setTileLocation(f.relTiles(h: h, v: v));
            pepoCommon.LogTr($"farmer now at ({f.getTileX()},{f.getTileY()})");
        }

        public static Vector2 relTiles(this Farmer f, int h=0, int v=0)
        {
            return new Vector2(f.getTileX() + h, f.getTileY() + v);
        }
    }


    public static class pepoHandler
    {
        public static void ClosedMenu(object sender, MenuChangedEventArgs e)
        {
            pepoCommon.LogTr($"ClosedMenu triggered by {sender}");
            if (e.OldMenu == null)
            {
                pepoCommon.LogTr($"new menu {e.NewMenu}");
                return;
            }
            pepoCommon.LogTr($"old menu {e.OldMenu}");
            pepoCommon.LogTr("calling exitFunction");
            e.OldMenu.exitFunction();
        }
    }

}
