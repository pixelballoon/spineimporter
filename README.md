Native Unity Spine Importer
====


IMPORTANT: This code hasn't yet been tested outside my current project, so if there's any issues with compilation please let me know.

About
----

Imports Esoteric Software's Spine data into native Unity animations

This library is still very much a work in progress, and only supports a limited number of Spine's features so far.

**Supported**
* Bone hierarchy imported
* Regions imported as Unity sprites
* Multiple skins
* Animations imported as native Unity animations
* Supports native Unity Mechanim state machine blending etc...

**Unsupported (for now)**
* Skinned Meshes (next up!)
* Events
* Animation curve types
* Everything else

**Known Issues**
* Single frame animations have some issues within Unity, causing harmless errors to show in the log window when opened in the Animator
* Animation curves don't exactly match those coming from Spine yet, as the curve type isn't checked

**Limitations**
* All images are placed in a folder called 'images' next to the spine JSON file
* No atlases are imported (nor are there any plans to do so)
* FFD likely won't ever be supported, as it's complex to implement within Unity's animation system, but skinned meshes will be

Instructions
----

1. Place the exported json anywhere within your Unity assets directory
2. Place all images that the animation requires in a folder named 'images' alongside it
3. Right click on the json within the project window
4. Drag prefab into scene
5. Done!

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
