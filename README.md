Native Unity Spine Importer
====

Imports Esoteric Software's Spine data into native Unity animations

This library is still very much a work in progress, and only supports a limited number of Spine's features so far.

Features
----

**Supported**
* Bone hierarchy imported
* Regions imported as Unity sprites
* Meshes
* Skinned meshes
* Multiple skins
* Animations imported as native Unity animations
* Animation events
* Supports native Unity Mechanim state machine blending etc...

**Coming soon (in approximate order)**
* Animation curve types
* Draw order
* Attachment swapping animation

**Known Issues**
* Single frame animations have some issues within Unity. It's harmless, but errors will show in the log window when the animation is opened in the Animator
* Animation clips are leaked upon clicking refresh. This is harmless, but will cause a message when you save
* Animation curves don't exactly match those coming from Spine yet, as the curve type isn't parsed
* Unskinned meshes don't animate correctly when the root is scaled with a negative scale

**Limitations**
* All images are placed in a folder called 'images' next to the spine JSON file
* No atlases are imported (nor are there any plans to do so)
* FFD may be supported via Unity blend shapes, but no promises

**Warnings**
* This code hasn't yet been tested outside my current project, so if there's any issues with compilation or usage please let me know
* Some functionality relies on using internal Unity Editor classes, as until they're officially supported there's no other choice. While this works with my current Unity version (4.5.2) there's always a small possibility it may break in a future release. In this case I'll fix it up as soon as I can, as I always try to use the latest Unity releases where possible

Instructions
----

1. Export a json file from spine, and place it anywhere within your Unity assets directory
2. Place all images that the animation requires in a folder named 'images' alongside it
3. Right click on the json within the project window, and click "Spine/Import" in the context menu
4. Drag the created prefab into scene
5. Open the native Unity Animator window and use that as you would any animation created within Unity
6. Done!

If you want to change the skin, simply change the text in the 'active skin' field and hit Refresh

Refresh will update all animations, bones etc... If done on the prefab it will be saved to the prefab. If done within a scene you will need to remember to hit Apply. All objects you parent to existing bones should remain, but this has not been fully tested.

Support
----
No support is provided, as this library is purely for my personal use, but please do whatever you wish with the code.

That said, pull requests/issues are welcome, I just can't promise I'll look at them in a timely manner.

I can be contacted via:
* **E-mail:** jamie AT pixelballoon DOT com
* **Twitter:** @pixelballoon

Enjoy!
