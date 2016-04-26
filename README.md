# DiRTTelemetryErrorFix
A hook/detour written in C# for the Windows ws2_32 lib to fix the 10038 socket error when telemetry is enabled in DiRT 2 and DiRT Showdown.

![](https://raw.githubusercontent.com/RoccoC/DiRTTelemetryErrorFix/master/error.png "DiRT Telemetry Error")

**_Thanks to SimXperience forum member Carsten Gottschling for creating the original detour to fix this issue. The idea for this project is based on his work._**

Background
----------
If you've tried running DiRT Showdown or DiRT 2/3 with telemetry output enabled (e.g. for motion, SimVibe, etc), chances are that you have seen the game crash with a Windows Socket Error 10038 at race end. This bug was introduced with a release in 2011, and according to multiple forum posts, it doesn't look like it's something that Codemasters will ever fix (see [HERE](http://forums.codemasters.com/discussion/183/dirt-3-telemetry-and-motion-simulator-bug)). My understanding is the error is caused by the game trying send motion data to a closed socket, hence the unhandled exception. This bug essentially makes the game unplayable with telemetry output enabled, and is a shame that Codies doesn't plan on creating what I think would be a very simple patch to fix this.

The Fix
-------
This project is based on the ability to hook or ["detour"](http://research.microsoft.com/en-us/projects/detours/) native Windows libraries in order to intercept function calls. DiRTTelemetryErrorFix uses the [EasyHook](https://github.com/EasyHook/EasyHook) package to intercept calls to the WinSock sendto function. Essentially, the hook calls the original sendto function, checks the return code to see if it is error 10038, and if so, spoof a return code of 0 to the calling process.

This fix is packaged in the form of a simple Windows System Tray app which checks for the existence of a running DiRT process and intercepts any calls from the active process to the WinSock sendto function. The app will automatically release the hook and self-close when it has detected that the game is no longer running.

Installation
------------
1. Download the ZIP file using [this link](https://github.com/RoccoC/DiRTTelemetryErrorFix/blob/master/bin/DiRTTelemetryErrorFix_Release.zip?raw=true).
2. Extract the ZIP file to a folder of your choosing.
3. Open the folder you created in the previous step, and then run DiRTTelemetryErrorFix.exe.
4. Within 10 seconds of starting DiRTTelemetryErrorFix.exe, start DiRT Showdown, DiRT 2, or DiRT 3.
5. DiRTTelemetryErrorFix.exe will automatically hook into the game process and will suppress the WinSock error.

Notes
-----
* Ideally this app should be automatically started via a pre or post-launch script if possible.
* DiRTTelemetryErrorFix.exe is configured to self-close if a running DiRT process is not found within ten seconds of starting.
* Valid process names are configurable and can be set in the application's ([config file](https://github.com/RoccoC/DiRTTelemetryErrorFix/blob/master/src/DiRTTelemetryErrorFix/App.config)).
* The fix will only hook calls made from the active DiRT process, and so any other calls to WinSock sendto from other applications are unaffected.
* Fix has been tested on Windows 8.1 and Windows 10.
* The application will self-close within 30 seconds of exiting the DiRT game.
