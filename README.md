# osu!Presence
A more detailed Discord Rich Presence integration for osu!

![Example](https://user-images.githubusercontent.com/46385879/133864672-840a149b-6115-4121-9c23-f7d6697ebd38.gif)

# Details
osu!Presence can display almost every possible game state in osu! This works by reading patterns and retrieving data via. memory. This is **NOT** a cheat and does not put you at risk of getting banned.

## Installation
Make sure to go the "Integration" options in the "Online" tab in osu! and **disable** "Discord Rich Presence" in order to not overrite the data from this program. As of now, I have **NOT** Implemented a way to patch your game so this runs on launch, in the meantime, you can download the executable over in the [Released Binaries](https://github.com/InsaneSlay/osuPresence/releases). After, just open osu!Presence.exe while or before opening osu!. A game patch will be released soon in order to avoid going through the extra hassle.

## Game States
  * Main Menu
    > Will display "Idle" when on the Main Menu
  * Solo
    > Will display "Selecting a song" when searching for a song to play
  * Solo (Playing)
    ###### Displays:
      * Mods selected *(ex. +HDHR)*, star rating, song title, and song difficulity name
      * SS *(if applicable)*, combo, accuracy, and pp count
      * Time remaining on map **(changes if a speed modifier is present ex. DT/NC)**
      * Download button which directs to the beatmap set currently playing
      * **(star rating and pp counter slightly inaccurate due to 7/27 changes)**
  * Solo (After Game Report)
    ###### Displays:
      * Map name, star rating and difficulity
      * PP Count and accuracy
      * **(star rating and pp counter slightly inaccurate due to 7/27 changes)**
  * Replay
    ###### Displays:
      * Who the replay is by
      * Map name, star rating, and difficulity
  * Replay (After Game Report)
    > Will display "Finished watching a replay, as well as the map name, star rating, and difficulity"
  * Multi Menu
    > Will display "Searching for lobby"
  * osu!Direct
    > Will display "Downloading Songs" while in the osu!Direct menu
  * Editor
    > Will display "Editing a map" while editing  

# Requirements
 * A desktop and/or laptop running Windows 8.1 or 10 with [.NET Framework 4.7.2](https://dotnet.microsoft.com/download/dotnet-framework/thank-you/net472-web-installer) Installed.
 * [osu!](https://osu.ppy.sh/home/download) running version **20190816** or higher
 * For working with the source of osu!Presence [Visual Studio 2019](https://visualstudio.microsoft.com/thank-you-downloading-visual-studio/?sku=Community&rel=16) or higher is required.

# Libraries
osu!Presence requires some libraries to function. The names and respected owners are listed below.
 * [DiscordRichPresence by Lachee](https://github.com/Lachee/discord-rpc-csharp)
 * [OsuMemoryDataProvider by Piotrekol](https://github.com/Piotrekol/ProcessMemoryDataFinder/tree/master/OsuMemoryDataProvider)
 * [ProcessMemoryDataFinder by Piotrekol](https://github.com/Piotrekol/ProcessMemoryDataFinder/tree/master/ProcessMemoryDataFinder)
 * [OppaiSharp by HoLLy-HaCKeR](https://github.com/HoLLy-HaCKeR/OppaiSharp)

# Contact
If peppy or anyone over at the osu! team does not like this program, you may contact me for takedown at
 > fnbrleaks@gmail.com

If there is an issue with this program and you need to notify me, DO NOT CONTACT ME THIS WAY. Open a new issue [here](https://github.com/InsaneSlay/osuPresence/issues) and I will get to it.
