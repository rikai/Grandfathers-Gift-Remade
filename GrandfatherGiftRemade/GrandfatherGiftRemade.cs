using System;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;

namespace GrandfatherGiftRemade
{
    class ModConfig
    {
        public int triggerDate { get; set; } = 2;
        public bool giveChest { get; set; } = true;
        public string weaponType { get; set; } = "dagger";
    }

    /// <summary>Mod entry point</summary>
    public class GrandfatherGiftRemade : Mod
    {
        /***** Properteze *****/
        private ModConfig Config;
        private SDate triggerDate;

        /***** Publique Methodes *****/
        public override void Entry(IModHelper helper)
        {
            this.Config = this.Helper.ReadConfig<ModConfig>();

            int triggerDateDay = this.Config.triggerDate;
            if (triggerDateDay < 2) triggerDateDay = 2;
            else if (triggerDateDay > 28) triggerDateDay = 28;
            this.triggerDate = new SDate(triggerDateDay, "spring", 1);

            helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        }

        /***** Private Methodes *****/
        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            var curDate = SDate.Now();
            if (curDate != triggerDate) return;

            // TODO: Things
            // TODO: Pop up message of "last night I saw a xyzzy" before map loaded
            // TODO: Wait until map loaded and add a package with interaction event
            // TODO: Interaction event (show letter)
            // TODO: Remove package from map, add weapon & chest to inventory

            // We have done what we have to do. Exit stage left
            Helper.Events.GameLoop.DayStarted -= this.OnDayStarted;
        }
    }
}
