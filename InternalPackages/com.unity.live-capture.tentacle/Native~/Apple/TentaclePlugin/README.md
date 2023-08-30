# How to build Tentacle Sync plugin for iOS and macOS
 
1. Download `Tentacle.xcframework` from the vendor.
2. Open `TentaclePlugin.xcodeproj` in Xcode.
3. Select the `TentaclePlugin` target. Navigate to the "Build Phases" tab.
4. Expand "Embed frameworks" and click on the `+` icon.
5. Click "Add Other...". Browse to the location of `Tentacle.xcframework`.
6. Build the `TentaclePlugin` target. If you get a codesigning error, try checking the "Copy only when installing" box, building, unchecking the box, and rebuilding again.
7. Select the `TentaclePluginStatic` target. Navigate to the "Build Phases" tab.
8. Expand "Link Binary With Libraries" and click the + icon.
9. Select `Tentacle.xcframework` under `TentaclePlugin Project` and click "Add".
10. Archive the `TentaclePluginStatic` target and distribute the build contents.
11. Navigate to the output directory for the `TentaclePlugin.bundle` product. In the Project Navigator, right-click on `TentaclePlugin.bundle` under "Products" and select "Show in Finder".
12. Copy `TentaclePlugin.bundle` to `Packages/com.unity.live-capture/TimecodeSources/Tentacle/Plugins/OSX`.
13. Navigate to the output directory for the archived `libTentaclePluginStatc.a` product.
14. Copy `libTentaclePluginStatc.a` and `Tentacle.framework` to `Packages/com.unity.live-capture/TimecodeSources/Tentacle/Plugins/iOS`.

## TODO
Automate this with a script.
