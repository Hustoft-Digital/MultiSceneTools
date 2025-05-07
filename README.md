[![Codacy Badge](https://app.codacy.com/project/badge/Grade/ab255c26e8694143944b4ce292fee11b)](https://app.codacy.com/gh/Hustoft-Digital/MultiSceneTools/dashboard?utm_source=gh&utm_medium=referral&utm_content=&utm_campaign=Badge_grade)

# Multi Scene Tools

<img src="Images/MultiSceneTools%20Icon.png" alt="MultiSceneToolsIcon" width="200"/>

## Features

- Scene Collection ScriptableObjects
    - Tracks which scenes are used together in runtime.
    - Double Click to load collection.
    - Set which scene is the active scene in the collection. (gets set automatically)
    - Track and add collection scenes to build settings.
    - (Coming) Tracks cross scene references.
 
- Hierarchy Style
    - Icon and tooltip on scenes that belong to a scene collection
    - User defined scene collection color.
    - Checkmark on the target active scene for the collection
    - Icons for displaying which collection scenes are in the build settings

- Multi Scene Management Window
    - Display currently loaded scene collection
    - Load scene collections
    - Load scenes additively
    - Unload scenes
    - Save and override loaded scene collection
    - Create scene collections from loaded scenes
    - Create new scenes and load it additively
    - Add all open scenes to build settings
    - See the current settings set in the config scriptable object

- Multi Scene Loader
    - Load scene collections with this static class
    - OnSceneCollectionLoaded & OnSceneCollectionLoadDebug<SceneCollection, collectionLoadMode> events triggered on successful loading
    - Loading modes
        - Additive
            - Loads all scenes in a collection additively
        - Replace
            - Unloads all scenes other than the boot scene, then loads all scenes additively.
        - DifferenceReplace
            - Unloads all scenes the collections do not share, then load the missing scenes.
        - DifferenceAdditive
            - Load all scenes in the collection that is not already loaded
        - Subtractive [experimental] (not implemented for async)
            - unload all matching scenes
    - Async loading methods
        - preload scenes and activate when ready
        - defer unloading scenes and trigger unloading when ready
        - returns an AsyncCollection
            - track progression of the async operations
            - trigger activation of scenes
            - trigger unloading of scenes 
    - (coming) Scene Node view, Connect scenes together with nodes to visualize adjacent scenes and automate when scenes should be loaded. 

- Multi Scene Tools Config
    - See and set current singleton instance
    - See and set current loaded scene collection
    - Toggle for allowing cross scene referencing (Cross scene referencing is not implemented)
    - Toggle for logging scene collection loading
    - Target path for boot scene or manager scene
    - Target path for loading scene collections

## Demo

Contains several scenes to simulate the outside and inside environment of a building, and a player moving in and out of the building. The demo demonstrates how one could structure two scene collections to contain all the relevant scenes for the interior and exterior, and how to load between them using async. 

To start the demo, open the "DemoAwake" unity scene. This scene demonstrates how to start using scene collections from not having any loaded, and loading into the main demo.

## Setup

- Refer to the installation guide

## Author

- [Henrik Hustoft](https://linktr.ee/henryhouse)

## License

Extension Asset
One license required for each individual user.
For more information, check the EULA and FAQ.

https://unity.com/legal/as-terms    
https://assetstore.unity.com/browse/eula-faq    
https://assetstore.unity.com/packages/tools/utilities/multi-scene-tools-lite-304636