# Update: PoGo currently experiences API access problems. 

Please don't ask why Pokestops and Pokemon aren't showing on the map. This is a known issue that almost all third party Pokemon GO tools are experiencing. Please avoid opening new issues, we'll update you when things change. _Update_: A new API has been finalized and is currently in the process of being implemented. We should see a new build soon, with an updated API and some UI changes. Worth the wait

# PoGo for Windows 10

Check [Wiki](https://github.com/ST-Apps/PoGo-UWP/wiki) for information about the project, installing instructions and more.

# Social

[Reddit](https://www.reddit.com/r/PoGoUWP/)

We have an official reddit made to discuss about PoGo. Make sure to follow the subreddit rules and you're good to go.

We decided to have 3 social chat groups to make sure everyone can use their favorite service to disscuss. Feel free to join any of them, or all at once. You're not limited to just one.

[Skype](https://join.skype.com/hOeCHq2oEyhA)

The skype group is used for Text/Voice chat and general disscussion, as well as support.

[Telegram](https://telegram.me/PoGoUWP)

For telegram users, there is also a Telegram superchat that is also for general disscussion, ideas and support.

[Discord](https://discord.gg/4GMbEWH)

The Discord is used mostly for live voice chat and is about the same subjects above groups are.

# Questions & Answers

Q: What is PoGo?

A: PoGo is an UWP (Universal Windows Platform) client for Niantic's Pokemon™ Go Android/iOS game. Being a client, this means that it gives you the ability to play in the same game-world as your friends that are playing with an Android or iOS device.

Q: Why PoGo?

A: Because learning new things is always cool. Because it could be done. Because Microsoft rejected my job application saying that I wasn't showing enough "passion", and this proves them wrong :)
 
Q: Will this app feature 3D graphics and AR?

A: No, for both of them, it just takes too much work. If you feel that you could do this, clone the repo, add the changes and submit a pull request.

Q: Will this work on Windows Phone 8.1?

A: Not officialy. This is an open-source project, so people might fork it and port it to Windows Phone 8.1 later on.

Q: Can I play with the same account that I'm using on my Android/iOS device?

A: Yes, but **not at the same time or you may end up having a duplicated account** so, please, logout from the Android app before logging in PoGo.

Q: Will it run on low-end devices?

A: Yes, but the performance may not be perfect.

Q: Why is the Device portal returning error 0x80073cf6?

A: Change storage settings from SD card to phone storage. More infos [here](github.com/ST-Apps/PoGo-UWP/issues/11)
If you already had the app installed, probably a reboot is required.

Q: How can I logout?

A: Press the Pokeball and hit the "LOGOUT" button in the top right corner.

# Changelog

## (01/08/2016) [v 1.0.12.0-beta] - Bug fixes and improvements
* Fixed crash when tapping on Pokebal menu
* Fixed crash when loading the map
* Added back the progress ring to notify that we're still waiting for GPS signal, avoiding people being shown in Central Africa
* Fixed [#25](https://github.com/ST-Apps/PoGo-UWP/issues/25) 

## (01/08/2016) [v 1.0.8.0-beta] - Bug fixes and improvements
* App renamed to PoGo
* Fixed a problem that caused you to get Candy and XP when a Pokemon escapes.
* Background on Catching screen replaced with original.
* Fixed PokeStop floating over map
* Added app version on both Login Screen and Game Page
* Did huge refactoring on view models
* Trying to fix problem that is causing Pokestops/Pokemon not to show when in fast motion.
* Working on making maintenance easier(this can cause more issues)


## (01/08/2016) [v 1.0.7.0-beta] - Bug fixes and improvements 
* Fixed Pokestop dissappear while moving [#15](https://github.com/ST-Apps/PoGo-UWP/issues/15)
* Fixed Pokemon dissappear issue [#27](https://github.com/ST-Apps/PoGo-UWP/issues/27)

## (31/07/2016) [v 1.0.6.0-beta] - Bug fixes and improvements 
* Fixed crash on loading [#3](https://github.com/ST-Apps/PoGo-UWP/issues/3)

## (31/07/2016) [v 1.0.5.0-beta] - Bug fixes and improvements
* Removed perfect shot everytime player throws the ball [#12](https://github.com/ST-Apps/PoGo-UWP/issues/12)
* Changed Pokestop icons to purple on already used Pokestops [#26](https://github.com/ST-Apps/PoGo-UWP/issues/26)

## (31/07/2016) [v 1.0.4.0-beta] - Bug fixes and improvements
* Added update notification [#9](https://github.com/ST-Apps/PoGo-UWP/issues/9)
* Prevented lockscreen while playing [#16](https://github.com/ST-Apps/PoGo-UWP/issues/16)
* Fixed crash when running on desktop [#7](https://github.com/ST-Apps/PoGo-UWP/issues/7)

## (31/07/2016) [v 1.0.3.0] - First public beta release
* PTC Login
* Map browsing
* Pokemon catching
* Pokestop visiting

# Download

Download the latest official release [here](https://github.com/ST-Apps/PoGo-UWP/releases)
