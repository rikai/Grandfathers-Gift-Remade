using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;
using SObject = StardewValley.Object;

namespace GrandfatherGiftRemade
{
    class ModConfig
    {
        public int triggerDate { get; set; }
        public bool giveChest { get; set; }
        public bool traceLogging { get; set; }

        public ModConfig()
        {
            this.triggerDate = 2;
            this.giveChest = true;
            this.traceLogging = true;
        }
    }

    /// <summary>Mod entry point</summary>
    public class GrandfatherGiftRemade : Mod, IAssetEditor
    {
        /***** Constants *****/
        const int WEAP_ID = 20;
        const string WEAP_DATA = "Elf Blade/Only the nimble hands of an elf could craft this./3/5/.5/0/5/0/1/-1/-1/0/.20/3";

        /***** Properteze *****/
        private ModConfig Config;
        private SDate triggerDate;
        private bool abortMod = false;
        private IModHelper helper;
        private IModEvents events;

        /***** Publique Methodes *****/
        public bool CanEdit<T>(IAssetInfo asset)
        {
            if (asset.AssetNameEquals("Data/weapons")) return true;
            return false;
        }

        public void Edit<T>(IAssetData asset)
        {
            if (!asset.AssetNameEquals("Data/weapons")) return;
            IDictionary<int, string> data = asset.AsDictionary<int, string>().Data;
            data[WEAP_ID] = WEAP_DATA;
            this.Monitor.Log($"weapon {WEAP_ID} set to {WEAP_DATA}");
        }

        public override void Entry(IModHelper helper)
        {
            this.helper = helper;
            this.events = helper.Events;

            this.Config = helper.ReadConfig<ModConfig>();

            this.PrepTrigger();
            this.PrepGoodies();
            this.RegisterEvents("Mod Startup");
        }

        private void PrepTrigger()
        {
            int triggerDateDay = this.Config.triggerDate;
            if (triggerDateDay < 2) triggerDateDay = 2;
            else if (triggerDateDay > 28) triggerDateDay = 28;
            // this.triggerDate = new SDate(triggerDateDay, "spring", 1);
            SDate tD = new SDate(25, "spring", 3);
            this.Monitor.Log($"triggerDate set to {tD.Day} {tD.Season} {tD.Year}");
            this.triggerDate = tD;
        }


        /***** Private Methodes *****/

        private void PrepGoodies()
        {
            // TODO: Create custom weapon here
        }

        private void traceLoggingIf(string message)
        {
            if (this.Config.traceLogging) this.Monitor.Log(message, LogLevel.Trace);
        }

        private void RegisterEvents(string reason)
        {
            this.events.GameLoop.OneSecondUpdateTicked += this.Supervisor;
            this.events.GameLoop.DayStarted += this.OnDayStarted;
            this.Monitor.Log($"Events registered: {reason}");
        }

        private void DeregisterEvents(string reason)
        {
            this.events.GameLoop.OneSecondUpdateTicked -= this.Supervisor;
            this.events.GameLoop.DayStarted -= this.OnDayStarted;
            this.Monitor.Log($"Events deregistered: {reason}");
        }

        private void Supervisor(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (this.abortMod) this.DeregisterEvents("ABORTING MOD");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            var curDate = SDate.Now();
            if (curDate > triggerDate)
            {
                this.DeregisterEvents("passed triggerDate");
            }
            if (curDate != triggerDate) return;

            this.DeregisterEvents("triggered");

            string message = "Before you went to bed last night, you noticed a loose board.^^" +
                "Too tired to pull it up you went to bed with the intention of checking " +
                "what's underneath first thing in the morning.^^As you wake up, you pry open the loose " +
                "board and found a wooden chest...^";
            pepoHelper.DialogOnBlack newDayMessage = new pepoHelper.DialogOnBlack(message);
            Game1.activeClickableMenu = newDayMessage;

            // TODO: Shift farmer 1 tile to the left
            Farmer farmer = Game1.player;
            int cur_x = farmer.getTileX();
            int cur_y = farmer.getTileY();
            this.Monitor.Log($"farmer originally at ({cur_x}, {cur_y})");
            int new_x = cur_x - 2;
            farmer.setTileLocation(new Vector2(new_x, cur_y));
            farmer.faceDirection(3);
            this.Monitor.Log($"farmer now at ({new_x}, {cur_y}) facing 3 (left)");

            // TODO: Add a package object with interaction event

            // TODO: Remove package from map, add weapon & chest to inventory

        }

        private void OnPackageOpen(object sender)
        {
            string message = "Grandfather's message";
            LetterViewerMenu letter = new LetterViewerMenu(message);
            Game1.activeClickableMenu = letter;

            // TODO: Wait until menu is gone before adding:
            Game1.player.addItemByMenuIfNecessary(new MeleeWeapon(WEAP_ID));
            // TODO: Add chest
        }

    }
}
