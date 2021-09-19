using DiscordRPC;
using DiscordRPC.Logging;
using OppaiSharp;
using OsuMemoryDataProvider;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using static OppaiSharp.Helpers;
using static osu_Presence.Win.osu_;
namespace osu_Presence
{
    class Program
    {
        public static DiscordRpcClient client;
        public static OsuMemoryReader memoryReader;
        public static PPv2 ppCount;


        public static ulong cTime;
        public static ulong nTime;
        public static string username;

        static async Task Main(string[] args)
        {
            Console.Title = "osu!Presence";

            memoryReader = new OsuMemoryReader();
            client = new DiscordRpcClient("887247757046325248");
            client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };
            client.Initialize();
            username = File.ReadAllLines($"{GetOsuRunningDirectory()}//osu!.{Environment.UserName}.cfg")[0].Split(' ')[4];
            UpdatePresence();
            while (true)
            {
                UpdatePresence();
                Thread.Sleep(500); //to avoid discord rate limiting
            }
        }

        private static void UpdatePresence()
        {
            int status;
            memoryReader.GetCurrentStatus(out status);

            byte[] data = new WebClient().DownloadData("https://osu.ppy.sh/osu/" + memoryReader.GetMapId());
            var stream = new MemoryStream(data, false);
            var reader = new StreamReader(new MemoryStream(data, false));
            var beatmap = Beatmap.Read(reader);
            Mods mods = (Mods)memoryReader.GetMods();
            string Smods = "";
            if (!string.IsNullOrEmpty(ModsToString(mods)))
                Smods = "+" + ModsToString(mods);
            var diff = new DiffCalc().Calc(beatmap, mods);
            switch (status)
            {
                case 2:                
                    if (!memoryReader.IsReplay())
                    {
                        if (Smods.Contains("DT") || Smods.Contains("NC"))
                        {
                            cTime = Convert.ToUInt64(DateTimeOffset.Now.ToUnixTimeMilliseconds());
                            double rTime = beatmap.TimingPoints.Last().Time / 1.5 - memoryReader.ReadPlayTime();
                            nTime = cTime + (ulong)rTime;
                        }
                        else
                        {
                            cTime = Convert.ToUInt64(DateTimeOffset.Now.ToUnixTimeMilliseconds());
                            double rTime = beatmap.TimingPoints.Last().Time - memoryReader.ReadPlayTime();
                            nTime = cTime + (ulong)rTime;
                        }                       

                        ppCount = new PPv2(new PPv2Parameters(beatmap, diff, c300: memoryReader.ReadHit300(), c100: memoryReader.ReadHit100(), c50: memoryReader.ReadHit50(), cMiss: memoryReader.ReadHitMiss(), mods: mods));
                        if (double.IsNaN(ppCount.ComputedAccuracy.Value()))
                            UpdateStatus($"{Smods} [{diff.Total:F2}*] {beatmap.Title} [{beatmap.Version}]", 
                                $"{memoryReader.ReadCombo()}x | 0.00% | 0pp", 
                                getGameMode(memoryReader.ReadSongSelectGameMode()),
                                true,
                                true,
                                true);
                        else if (double.IsNaN(ppCount.Total))
                        {                            
                            ppCount = new PPv2(new PPv2Parameters(beatmap, diff, c100: 0, combo: memoryReader.ReadCombo(), mods: mods));
                            UpdateStatus($"{Smods} [{diff.Total:F2}*] {beatmap.Title} [{beatmap.Version}]"
                                , $"SS | {memoryReader.ReadCombo()}x | {ppCount.ComputedAccuracy.Value() * 100:F2}% | {ppCount.Total:F2}pp",
                                getGameMode(memoryReader.ReadSongSelectGameMode()),
                                true,
                                true,
                                true);
                        }
                        else
                            UpdateStatus($"{Smods} [{diff.Total:F2}*] {beatmap.Title} [{beatmap.Version}]",
                                $"{memoryReader.ReadCombo()}x | {ppCount.ComputedAccuracy.Value() * 100:F2}% | {ppCount.Total:F2}pp",
                                getGameMode(memoryReader.ReadSongSelectGameMode()),
                                true,
                                true,
                                true);
                    }
                    else
                        UpdateStatus($"Watching A Replay By {memoryReader.PlayerName()}", $"[{diff.Total:F2}*] {beatmap.Title} [{beatmap.Version}]", getGameMode(memoryReader.ReadSongSelectGameMode()), false, true, true);
                    break;
                case 0:
                    UpdateStatus("Main Menu", "Idle", getGameMode(memoryReader.ReadSongSelectGameMode()), false, false,true);
                    break;
                case 5:
                    UpdateStatus("Playing Solo", "Selecting A Song", getGameMode(memoryReader.ReadSongSelectGameMode()), false, false, true);
                    break;
                case 15:
                    UpdateStatus("osu!Direct", "Downloading Songs", getGameMode(memoryReader.ReadSongSelectGameMode()), false, false, true);
                    break;
                case 4:
                    UpdateStatus("Editor", "Editing A Map", getGameMode(memoryReader.ReadSongSelectGameMode()), false, false, true);
                    break;
                case 11:
                    UpdateStatus("Playing Multiplayer", "Searching for a lobby", getGameMode(memoryReader.ReadSongSelectGameMode()), false, false, true);
                    break;
                case 7:
                    try
                    {
                        if (!double.IsNaN(ppCount.Total))
                            UpdateStatus($"Finished: [{diff.Total:F2}*] {beatmap.Title} [{beatmap.Version}]",$" {ppCount.ComputedAccuracy.Value() * 100:F2}% | {ppCount.Total:F2}pp", getGameMode(memoryReader.ReadSongSelectGameMode()), false, true, true);
                        else
                            UpdateStatus($"Finished Watching A Replay", $"[{diff.Total:F2}*] {beatmap.Title} [{beatmap.Version}]", getGameMode(memoryReader.ReadSongSelectGameMode()), false, true, true);
                    }
                    catch { UpdateStatus($"Finished Watching A Replay", $"[{diff.Total:F2}*] {beatmap.Title} [{beatmap.Version}]", getGameMode(memoryReader.ReadSongSelectGameMode()), false, true, true); }
                    break;
            }
        }    

        public static void UpdateStatus(string details, string state, string gamemode, bool hasTime, bool hasMapBtn, bool hasAccBtn)
        {

            List<Button> buttons = new List<Button>();


            RichPresence presence = new RichPresence();
            presence.Details = details;
            presence.State = state;
            presence.Assets = new Assets()
            {
                LargeImageKey = "osulogo",
                SmallImageText = gamemode,
                SmallImageKey = gamemode.Replace("!", ""),
                LargeImageText = "osu!Presence v1.0",

            };
            if (hasTime)
                presence.Timestamps = new Timestamps()
                {
                    StartUnixMilliseconds = cTime,
                    EndUnixMilliseconds = nTime
                };

            if (hasAccBtn)
                buttons.Add(new Button() { Label = "View Profile", Url = "https://osu.ppy.sh/u/" + username });

            if (hasMapBtn)
                buttons.Add(new Button() { Label = "Download Map", Url = "https://osu.ppy.sh/beatmapsets/" + memoryReader.GetMapSetId() });

            presence.Buttons = buttons.ToArray();

            client.SetPresence(presence);
        }
        
        public static string getGameMode(int val)
        {
            switch (val)
            {
                case 0:
                    return "osu!";
                case 1:
                    return "osu!taiko";
                case 2:
                    return "osu!catch";
                case 3:
                    return "osu!mania";
                default:
                    return "";
            }
        }

    }
}
