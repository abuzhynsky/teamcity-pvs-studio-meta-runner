#### TeamCity PVS Studio Meta-Runner

Integrate [PVS Studio](http://www.viva64.com/ru/pvs-studio/) inspections with [TeamCity](http://www.jetbrains.com/teamcity/).

This plugin adds new build step type 'PVS Studio'. It analyzes your solution with PVS Studio and imports reports into TeamCity 'Code Inspection' tab. You have to specify your solution file and path to PVS Studio installation folder.

If PVS Studio is not registered you'll see 'TRIAL RESTRICTION' instead of file names. Even though there is information regarding project, warning message and line number. So if your commits are atomic and you run build on every commit this analysis can be quite useful. Anyway it's better to purchase licence.

The plugin has been tested on Team City 9.1.5, but should work with any version of Team City 9.

### Installation

1. Install PVS Studio on build agent
2. Ensure you have default settings file for PVS Studio. It can be found here (_%AppData%/PVS-Studio/Settings.xml_). If TeamCity build agent is running under System account settings file should be here (_C:\Windows\System32\config\systemprofile\AppData\Roaming\PVS-Studio\Settings.xml_)
3. Download 'pvs-studio-build-runner.zip' from the [latest release](https://github.com/abuzhynsky/teamcity-pvs-studio-meta-runner/releases/latest)
4. Copy this file into the _[Team City Data Directory]\plugins_ directory on the TeamCity Server, by default this is _C:\ProgramData\JetBrains\TeamCity\plugins_ or upload it using TeamCity 'Plugins List' tab (_Administration-Plugins List_)
5. Restart the Team City server

'PVS Studio' build step type should now be available.

### Building the plugin

To build the plugin from code, use MSBuild to run _package.msbuild_, this will create the pvs-studio-build-runner.zip file. The [MSBuild Community Tasks](https://github.com/loresoft/msbuildtasks) must be installed to run this.

### Credits
Inspired by [Team City xUnit Meta Runner](https://github.com/rhysgodfrey/team-city-xunit-meta-runner).
