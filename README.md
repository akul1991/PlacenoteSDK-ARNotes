# PlacenoteSDK-ARNotes

**Place and save notes in augmented reality, using PlacenoteSDK!**

Placenote is an easy and convenient method to add mapping and precise persistent content placement to your ARKit app. This project shows you how to build an AR Notes app with ARKit and Placenote.

## Getting Started

To install ARNotes, follow these instructions.

1. Ensure Git LFS (large file storage) is installed on your Mac. Several critical library files are stored using this mechanism.
  - Install Git LFS either using HomeBrew: `brew install git-lfs`
  - Or MacPorts: `port install git-lfs`
  - To install, run: `git lfs install`
2. Clone this repository.
  - SSH: `git clone git@github.com:Placenote/PlacenoteSDK-ARNotes.git`
  - HTTPS: `git clone https://github.com/Placenote/PlacenoteSDK-ARNotes.git`
3. Download files stored with Git LFS
  - `git lfs pull`
4. Open the project as a new project in Unity (Recommended: Unity 2018.3.6)
5. Make sure you have your Placenote API key. [Get your API key here!](https://developer.placenote.com)
6. Load the ARNotes scene from the project window (`Placenote/ARNotes/ARNotes`) and add your API key under `PlacenoteCameraManager`.
7. Follow the [Unity Build Instructions](https://placenote.com/docs/unity/build-instructions/) to build and run the project on your iOS device.

## Using the app
1. Select _New Map_, and start moving your phone to map the area.
2. Tap the screen to add notes!
3. To edit a note, tap the note and select the green edit icon.
4. To delete a note, tap the note and select the red delete icon.
5. Once you are happy with the scene, select _Save Map_. Keep note of the name your map was saved with!
6. To view the notes, select _Load Map_, and find the map name. Make sure you are near the same area in order for the app to relocalize!
7. Voila! Your notes should show up, exactly where you left them.
