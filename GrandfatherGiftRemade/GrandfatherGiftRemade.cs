using System.Collections.Generic;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewModdingAPI.Utilities;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;
using StardewValley.Objects;
using pepoHelper;

namespace GrandfatherGiftRemade
{
    class ModConfig
    {
        public int triggerDay { get; set; }
        public bool traceLogging { get; set; }
        public string weaponStats { get; set; }

        public ModConfig()
        {
            this.triggerDay = 2;
            this.traceLogging = true;
            this.weaponStats = "3/5/.5/0/5/0/1/-1/-1/0/.20/3";
        }
    }

    /// <summary>Mod entry point</summary>
    public class GrandfatherGiftRemade : Mod, IAssetEditor
    {
        /***** Constants *****/
        const int WEAP_ID = 20;

        /***** Properteze *****/
        private ModConfig Config;
        private SDate triggerDate;
        private bool abortMod = false;

        private string letterGrandpaMessage;
        private string narrationMessage1;
        private string narrationMessage2;

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
            string wName = this.Helper.Translation.Get("weapon.name");
            string wDesc = this.Helper.Translation.Get("weapon.desc");
            string wStat = this.Config.weaponStats;
            string wData = $"{wName}/{wDesc}/{wStat}";
            data[WEAP_ID] = wData;
            this.Log($"weapon {WEAP_ID} set to {wData}");
        }

        public override void Entry(IModHelper helper)
        {
            this.Config = helper.ReadConfig<ModConfig>();
            if (!this.Config.traceLogging)
                this.Monitor.Log("WARNING: Trace logging disabled via config.json", LogLevel.Warn);
            else
                pepoCommon.monitor = this.Monitor;

            this.PrepTrigger();
            this.PrepTranslations();
            this.RegisterEvents("Mod Startup");
        }


        /***** Private Methodes *****/

        private void PrepTrigger()
        {
            int triggerDateDay = this.Config.triggerDay;
            if (triggerDateDay < 2) triggerDateDay = 2;
            else if (triggerDateDay > 28) triggerDateDay = 28;
            SDate tD = new SDate(triggerDateDay, "spring", 1);
            //SDate tD = new SDate(25, "spring", 3);
            this.Log($"triggerDate set to {tD.Day} {tD.Season} {tD.Year}", LogLevel.Info);
            this.triggerDate = tD;
        }

        private void PrepTranslations()
        {
            string s;

            s = this.Helper.Translation.Get("letter");
            this.letterGrandpaMessage = s;
            this.Log($"loaded Grandpa's Letter from i18n, {s.Length} chars");

            s = this.Helper.Translation.Get("narration1");
            this.narrationMessage1 = s;
            this.Log($"loaded Narration1 from i18n, {s.Length} chars");

            s = this.Helper.Translation.Get("narration2");
            this.narrationMessage2 = s;
            this.Log($"loaded Narration2 from i18n, {s.Length} chars");
        }

        private void Log(string message, LogLevel level=LogLevel.Debug)
        {
            if (!this.Config.traceLogging && level == LogLevel.Trace) return;
            this.Monitor.Log(message, level);
        }

        private void RegisterEvents(string reason)
        {
            var evtLoop = this.Helper.Events.GameLoop;
            evtLoop.OneSecondUpdateTicked += this.Supervisor;
            evtLoop.DayStarted += this.OnDayStarted;
            this.Log($"Events registered: {reason}");
        }

        private void DeregisterEvents(string reason)
        {
            var evtLoop = this.Helper.Events.GameLoop;
            evtLoop.OneSecondUpdateTicked -= this.Supervisor;
            evtLoop.DayStarted -= this.OnDayStarted;
            this.Log($"Events deregistered: {reason}");
        }

        private void Supervisor(object sender, OneSecondUpdateTickedEventArgs e)
        {
            if (this.abortMod) this.DeregisterEvents("ABORTING MOD");
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            var curDate = SDate.Now();
            if (curDate > triggerDate)
            {
                this.DeregisterEvents("passed triggerDate");
            }
            if (curDate != triggerDate)
            {
                this.Log("new day, but not our day", LogLevel.Trace);
                return;
            }

            this.DeregisterEvents("triggered");

            Farmer farmer = Game1.player;
            MeleeWeapon weapon = new MeleeWeapon(WEAP_ID);

            IDisplayEvents dispEvents = this.Helper.Events.Display;

            IClickableMenu narrationDayStart1 =
                new pepoHelper.DialogOnBlack(this.narrationMessage1);
            IClickableMenu narrationDayStart2 =
                new pepoHelper.DialogOnBlack(this.narrationMessage2);
            IClickableMenu letterGrandpa =
                new LetterViewerMenu(this.letterGrandpaMessage.Replace("@", farmer.Name));
            this.Log("built the menus", LogLevel.Trace);

            // Make some chains
            narrationDayStart1.exitFunction = () => Game1.activeClickableMenu = narrationDayStart2;
            narrationDayStart2.exitFunction = () => Game1.activeClickableMenu = letterGrandpa;
            letterGrandpa.exitFunction = () => {
                dispEvents.MenuChanged -= pepoHandler.ClosedMenu;
                this.Log("deregistered from MenuChanged events");
                farmer.holdUpItemThenMessage(weapon);
                };
            this.Log("chained the menus", LogLevel.Trace);

            // Activate the ClosedMenu handler that actually do the chaining
            dispEvents.MenuChanged += pepoHandler.ClosedMenu;
            this.Log("registered for MenuChanged events");

            // Narration about what happened last night segues to Letter from Grandpa
            Game1.activeClickableMenu = narrationDayStart1;
            this.Log("displayed DayStart narration1, continuing doing things in the background");

            // Shift farmer to the left to leave the bed
            farmer.moveRelTiles(h: -2);
            farmer.faceDirection(3);  // face left
            this.Log("moved farmer out of bed", LogLevel.Trace);

            // Drop a chest containing weapon (this will be hidden by DialogOnBlack)
            Chest chest = new Chest(true);
            this.Log("created Chest(true)", LogLevel.Trace);
            chest.addItem(weapon);
            this.Log($"inserted weapon into chest", LogLevel.Trace);
            Game1.getLocationFromName(farmer.currentLocation.Name).dropObject(
                obj: chest,
                dropLocation: farmer.relTiles(h: -1) * Game1.tileSize,
                viewport: Game1.viewport,
                initialPlacement: true);
            this.Log("dropped chest in front of farmer", LogLevel.Trace);

            // TODO: Instead of a simple chest, make a chest with interaction
            // TODO: Interaction with chest = Farmer lift weapon above head + chimes

        }
    }
}
