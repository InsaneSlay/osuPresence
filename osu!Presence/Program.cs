using DiscordRPC;
using DiscordRPC.Logging;
using OppaiSharp;
using OsuMemoryDataProvider;
using OsuRTDataProvider;
using OsuRTDataProvider.Listen;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace osu_Presence
{
    class Program
    {
        public static DiscordRpcClient client;
        public static OsuMemoryReader memoryReader;
        public static PPv2 ppCount;
        static async Task Main(string[] args)
        {

            memoryReader = new OsuMemoryReader();


            client = new DiscordRpcClient("887247757046325248");
            client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };
            client.OnReady += (sender, e) =>
            {
                //Console.WriteLine("Received Ready from user {0}", e.User.Username);
            };
            client.OnPresenceUpdate += (sender, e) =>
            {
                //Console.WriteLine("Received Update! {0}", e.Presence);
            };
            client.Initialize();

            UpdatePresence();
            while (true)
            {
                UpdatePresence();
                Thread.Sleep(500);
            }
        }

        private static void UpdatePresence()
        {
            int status;
            memoryReader.GetCurrentStatus(out status);

            Console.WriteLine(status); 
            byte[] data = new WebClient().DownloadData("https://osu.ppy.sh/osu/" + memoryReader.GetMapId());
            var stream = new MemoryStream(data, false);
            var reader = new StreamReader(stream);
            var beatmap = Beatmap.Read(reader);
            Mods mods = (Mods)memoryReader.GetMods();
            string Smods = ModsToString(mods.ToString());
            var diff = new DiffCalc().Calc(beatmap, mods);
            switch (status)
            {
                case 2:                
                    if (!memoryReader.IsReplay())
                    {
                        ulong cTime, nTime;
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
                            UpdateStatusPlaying($"{Smods} [{diff.Total:F2}*] {beatmap.Title} [{beatmap.Version}]", 
                                $"{memoryReader.ReadCombo()}x | 0.00% | 0pp", 
                                getGameMode(memoryReader.ReadSongSelectGameMode()),
                                cTime,
                                nTime);
                        else if (double.IsNaN(ppCount.Total))
                        {                            
                            ppCount = new PPv2(new PPv2Parameters(beatmap, diff, c100: 0, combo: memoryReader.ReadCombo(), mods: mods));
                            UpdateStatusPlaying($"{Smods} [{diff.Total:F2}*] {beatmap.Title} [{beatmap.Version}]"
                                , $"SS | {memoryReader.ReadCombo()}x | {ppCount.ComputedAccuracy.Value() * 100:F2}% | {ppCount.Total:F2}pp",
                                getGameMode(memoryReader.ReadSongSelectGameMode()),
                                cTime,
                                nTime);
                        }
                        else
                            UpdateStatusPlaying($"{Smods} [{diff.Total:F2}*] {beatmap.Title} [{beatmap.Version}]",
                                $"{memoryReader.ReadCombo()}x | {ppCount.ComputedAccuracy.Value() * 100:F2}% | {ppCount.Total:F2}pp",
                                getGameMode(memoryReader.ReadSongSelectGameMode()),
                                cTime,
                                nTime);
                    }
                    else
                        UpdateStatusButton($"Watching A Replay By {memoryReader.PlayerName()}", $"[{diff.Total:F2}*] {beatmap.Title} [{beatmap.Version}]", getGameMode(memoryReader.ReadSongSelectGameMode()));
                    break;
                case 0:
                    UpdateStatus("Main Menu", "Idle", getGameMode(memoryReader.ReadSongSelectGameMode()));
                  break;
                case 5:
                    UpdateStatus("Playing Solo", "Selecting A Song", getGameMode(memoryReader.ReadSongSelectGameMode()));
                    break;
                case 15:
                    UpdateStatus("osu!Direct", "Downloading Songs", getGameMode(memoryReader.ReadSongSelectGameMode()));
                    break;
                case 4:
                    UpdateStatus("Editor", "Editing A Map", getGameMode(memoryReader.ReadSongSelectGameMode()));
                    break;
                case 11:
                    UpdateStatus("Playing Multiplayer", "Searching for a lobby", getGameMode(memoryReader.ReadSongSelectGameMode()));
                    break;
                case 7:

                    try
                    {
                        if (!double.IsNaN(ppCount.Total))
                            UpdateStatusButton($"Finished: [{diff.Total:F2}*] {beatmap.Title} [{beatmap.Version}]",$" {ppCount.ComputedAccuracy.Value() * 100:F2}% | {ppCount.Total:F2}pp", getGameMode(memoryReader.ReadSongSelectGameMode()));
                        else
                            UpdateStatusButton($"Finished Watching A Replay", $"[{diff.Total:F2}*] {beatmap.Title} [{beatmap.Version}]", getGameMode(memoryReader.ReadSongSelectGameMode()));
                    }
                    catch { UpdateStatusButton($"Finished Watching A Replay", $"[{diff.Total:F2}*] {beatmap.Title} [{beatmap.Version}]", getGameMode(memoryReader.ReadSongSelectGameMode())); }
                    break;
            }
        }

        public static void UpdateStatusButton(string details, string state, string gamemode)
        {
            client.SetPresence(new RichPresence()
            {
                Details = details,
                State = state,
                Assets = new Assets()
                {
                    LargeImageKey = "osulogo",
                    SmallImageText = gamemode,
                    SmallImageKey = gamemode.Replace("!", ""),
                    LargeImageText = $"{memoryReader.PlayerName()} ",

                },
                Buttons = new Button[]
                {
                    new Button() { Label = "Download Map", Url = "https://osu.ppy.sh/beatmapsets/" + memoryReader.GetMapSetId() }
                }
            });
        }

        public static void UpdateStatusPlaying(string details, string state, string gamemode, ulong start, ulong end)
        {
            client.SetPresence(new RichPresence()
            {
                Details = details,
                State = state,
                Timestamps = new Timestamps()
                {
                    StartUnixMilliseconds = start,
                    EndUnixMilliseconds = end
                },
                Assets = new Assets()
                {
                    LargeImageKey = "osulogo",
                    SmallImageText = gamemode,
                    SmallImageKey = gamemode.Replace("!", ""),
                    LargeImageText = $"{memoryReader.PlayerName()} ",

                },
                Buttons = new Button[]
                {
                    new Button() { Label = "Download Map", Url = "https://osu.ppy.sh/beatmapsets/" + memoryReader.GetMapSetId() }
                }
            });
        }
        public static void UpdateStatus(string details, string state, string gamemode)
        {
            client.SetPresence(new RichPresence()
            {
                Details = details,
                State = state,
                Assets = new Assets()
                {
                    LargeImageKey = "osulogo",
                    SmallImageText = gamemode,
                    SmallImageKey = gamemode.Replace("!", ""),
                    LargeImageText = $"{memoryReader.PlayerName()} ",

                }
            });
        }

        public static string ModsToString(string mods)
        {
            string re = "+";
            foreach(var item in mods.Split(','))
            {
                string nMod = item.Replace(" ", "");
                switch (nMod)
                {
                    case "NoMod":
                        re = "";
                        break;
                    case "NoFail":
                        re += "NF";
                        break;
                    case "Easy":
                        re += "EZ";
                        break;
                    case "TouchDevice":
                        re += "TD";
                        break;
                    case "Hidden":
                        re += "HD";
                        break;
                    case "Nightcore":
                        re += "NC";
                        break;
                    case "DoubleTime":
                        re += "DT";                 
                        break;
                    case "Hardrock":
                        re += "HR";
                        break;
                    case "HalfTime":
                        re += "HT";
                        break;
                    case "SpeedChanging":
                        break;
                    case "MapChanging":
                        break;
                    case "Flashlight":
                        re += "FL";
                        break;
                    case "SpunOut":
                        re += "SO";
                        break;
                }
            }
            if (re.Contains("NC"))
                re = re.Replace("DT", "");
            return re;
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
