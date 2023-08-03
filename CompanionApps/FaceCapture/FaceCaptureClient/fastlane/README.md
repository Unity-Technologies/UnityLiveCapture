fastlane documentation
================
# Installation

Make sure you have the latest version of the Xcode command line tools installed:

```
xcode-select --install
```

Install _fastlane_ using
```
[sudo] gem install fastlane -NV
```
or alternatively using `brew install fastlane`

# Available Actions
## iOS
### ios test
```
fastlane ios test
```

### ios local_build
```
fastlane ios local_build
```
Create Local Dev Build
### ios install_local
```
fastlane ios install_local
```
Install on local device
### ios increment
```
fastlane ios increment
```
Update Build Number
### ios beta
```
fastlane ios beta
```
CloudBuild Upload beta build to TestFlight

----

This README.md is auto-generated and will be re-generated every time [fastlane](https://fastlane.tools) is run.
More information about fastlane can be found on [fastlane.tools](https://fastlane.tools).
The documentation of fastlane can be found on [docs.fastlane.tools](https://docs.fastlane.tools).
