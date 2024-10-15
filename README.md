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

## Examples

- Scene Transitioner
    - Waits for the transition animation to finish before loading the next scene collection.
    - Tracks the in and out transition state

## Setup

- Install via Package Manager → Add package via git URL: 
    - https://github.com/HenrysHouses/MultiSceneTools.git
- Alternatively, download and put the folder in your Assets
- Create the config asset with the window popup

## Author

- [Hustoft Digital](https://www.linkedin.com/in/henrik-hustoft-2366ab220/)

## License

- Apache License 2.0 License. Refer to the [LICENSE](./LICENSE) file
