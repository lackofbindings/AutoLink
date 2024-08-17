# AutoLink

An unofficial add-on for VRChat's AudioLink that manages AudioLinkController levels automatically. 

Originally created by [Lesse](https://x.com/LesseVR), updated for AudioLink 1.0 by lackofbindings. Original code can be found [in this Twitter post](https://x.com/LesseVR/status/1641723080413446148).

## Setup

### Dependencies

- Unity 2022.3.22f1 (or latest supported version).
- AudioLink 1.4.0 or newer.
- VRC SDK 3.6.1 or newer.

These should be installed automatically by the VCC when you add this package to your project.

### Install

1. Go to the [VPM Listing](https://lackofbindings.github.io/AutoLink/) for this repo and hit "Add to VCC".
   
   - If the "Add to VCC" button does not work you can manually enter the following url into the Packages settings page of the VCC `https://lackofbindings.github.io/AutoLink/index.json` 

   - If you do not have access to the VCC, there are also unitypackage versions available in the [Releases](https://github.com/lackofbindings/AutoLink/releases/latest).

2. Once you have the repo added to your VCC, you can add AutoLink to your project from the Mange Project screen.

### Setup

1. Ensure that AudioLink and an AudioLinkController are set up and working in your project before proceeding.
   
2. Select your AudioLink object and ensure that the "Enable Readback" button has been pressed.
   
3. Drag the AutoLink prefab into your scene.

	* Make it a child of your AudioLinkController's `AudioLinkControllerBody` object and it will sit on top of the controller and move with it. If it looks out of place, reset its position and rotation to `0,0,0`.
	
	* You can also place it anywhere in the scene if you want it to stand on its own (or even be hidden).

	* If using the legacy controller called `AudioLinkControllerV0` then you must use the `AutoLinkV0` prefab instead.
	
4. Fill out the `Audio Link` and `Audio Link Controller` fields on the AutoLink object.

## Credits

Created by [Lesse](https://x.com/LesseVR).

Updated by lackofbindings for AudioLink v1.0 and released as a VPM.
