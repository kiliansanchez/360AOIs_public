# 360AOIs_public
This project is part of a master's thesis at Technische Universität Berlin in fulfillment of the requirements for the Degree Master of Science in Human Factors.

Author: Kilian Sanchez Holguin

Contact: kilian.sanchezholguin@gmail.com

The program allows the creation and animation of dynamic areas of interest (AOIs) in 360° videos. The software can be used to display the video in VR using an HTC Vive Pro Eye VR Headset and record eye-tracking data during playback. After each session, an evaluation of relevant eye-tracking parameters regarding the previously defined AOIs is provided as a CSV file data export.

![main ui](https://github.com/kiliansanchez/360AOIs_public/assets/18534327/f4423a5c-5f8e-493e-ae8c-092adfa3c261)

## Abstract
This master’s thesis presents the development and validation of a software tool for preparing
and conducting eye-tracking experiments in VR with 360° videos. The program allows for the annotation of dynamic Areas of Interest (AOIs) on any 360° video. Subsequently, participants can view the video in VR using an HTC Vive Pro Eye VR headset, and relevant eye-tracking parameters related to the predefined AOIs are analyzed and provided. The software validation was carried out through experiments in which the gaze movements of six participants were recorded, manually coded, and then compared with the data export from the software. Additionally, the software’s fixation detection was compared with an existing software package. The results demonstrate a high level of agreement between the software, manual ratings, and the comparison software package, indicating the software’s potential as an effective data collection tool.

## Installation Guide
#### Steam and SteamVR
In order to use the presented program, the video game distribution platform Steam has to be installed first so that the VR environment SteamVR can be run on the system. The program uses Valve's SteamVR plugin for Unity as an API for communicating with the VR glasses.
-	Download and install Steam https://store.steampowered.com/
-	Installing SteamVR via Steam https://store.steampowered.com/app/250820/SteamVR/
Vive Pro HMD Setup
The HTC Vive Pro Eye used for the project, as well as the associated base stations and controllers, must be correctly connected and installed on the PC used. The Vive Pro HMD Setup provided by HTC helps the user to perform the necessary steps.
-	Download and run HTC Vive Pro HMD Setup https://www.vive.com/us/setup/vive-pro-hmd/

#### SRanipal and Tobii VRU02 Runtime
The SRanipal and Tobii VRU02 runtimes are runtime environments for communication with the eye-tracking system of the VR goggles. They must be installed in order to perform experiments with the program. After the runtimes are installed, the VR glasses' eye tracking can be enabled in the SteamVR settings.
-	Registration in the Vive Developer Portal https://developer.vive.com/eu/
-	Download and installation of the runtime environments at https://hub.vive.com/en-US/download
-	Enabling eye-tracking functionality in SteamVR
-	Launch SteamVR via Steam
-	Pick up a controller and put on VR goggles
-	Press the menu button of the Vive controller.
-	Click on the blue Vive Pro Eye button in VR.
-	Activation of the eye tracking by clicking on Toggle
-	Agreement to terms of use
-	Carry out initial calibration
 
#### Python and libraries
For the creation of the sequence diagram, the program calls a Python script. Accordingly, Python3 must be installed on the system.
-	Download and install Python3 at https://www.python.org/downloads/
The visualization uses the following Python libraries, which must be installed manually.
-	pandas (https://pandas.pydata.org/docs/getting_started/install.html)
-	matplotlib (https://matplotlib.org/stable/users/installing/index.html)
-	argparse (https://pypi.org/project/argparse/)

#### AOI Editor
Lastly, the program can be downloaded from https://github.com/kiliansanchez/360AOIs_public/blob/main/executable.zip The link leads to a .zip file available on GitHub. After the download, the file can be unpacked. It contains a folder that contains all the files needed to run the program. By double-clicking on the file "360AOIs.exe", the program is executed.

### Setup of the Unity project
If adjustments are to be made to the program, the following steps must be taken in addition to the download and installation of the previously mentioned dependencies:
-	Download and install Unity Hub (https://unity.com/download)
-	Download and install Editor version 2021.3.16f1 via Unity Hub

The project can then be cloned or downloaded and opened in Unity Hub. The Unity project repository can be found at https://github.com/kiliansanchez/360AOIs_public. To customize the program code, it is recommended to connect an IDE such as Visual Studio Community or Visual Studio Code to Unity. Instructions for doing so can be found here https://docs.unity3d.com/Manual/ScriptingToolsIDEs.html.

## Code Documentation
https://kiliansanchez.github.io/360AOIs_public/index.html

## Complete Thesis (German Only)
Here's a link to the master's thesis in German: https://github.com/kiliansanchez/360AOIs_public/blob/main/Thesis_Kilian_Sanchez_Holguin.pdf
The thesis attachments contain a guide to the Gameobjects of the unity scene, which can assist in getting started with the project.

## Acknowledgments
This project is inspired by the work of Franziska Schicks (https://gitlab.com/franziska.schicks/360videoassistent).
