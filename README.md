# Multi Scene Tools

<img src="Gizmos/MultiSceneTools%20Icon.png" alt="MultiSceneToolsIcon" width="200"/>

## Features

- Scene Collection ScriptableObjects
    - Tracks which scenes are used together in runtime.
    - Double Click to load collection
    - (Comming) Tracks cross scene references

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
    - OnSceneCollectionLoaded & OnSceneCollectionLoadDebug<SceneCollection, collectionLoadMode> events triggered on sucessful loading
    - Loading modes
        - Additive
            - Loads all scenes in a collection additive
        - Difference
            - Unloads all scenes the collections do not share, then load the missing scenes.
        - Replace
            - Unloads all scenes other than the boot scene, then loads all scenes additively.
    - (comming) Scene Node view, Connect scenes together with nodes to visualise adjacent scenes and automate when scenes should be loaded. 

- Multi Scene Toools Config
    - See and set current singleton instance
    - See and set current loaded scene collection
    - Toggle for allowing cross scene referencing (Cross scene referencing is not implemented)
    - Toggle for logging scene collection loading
    - Target path for boot scene or manager scene
    - Target path for loading scene collections

## Examples

- Boot loader
    - Gets current collection when entering play in editor
    - loads the main menu when the _Boot scene is started

- Scene Transitioner
    - Waits for the transition animation to finish before loading the next scene collection.
    - Tracks the in and out transition state

## Setup

- Install via Package Manager → Add package via git URL: 
    - https://github.com/HenrysHouses/MultiSceneTools.git
- Alternatively, download and put the folder in your Assets
- Create the config asset with the window popup

## Author

- [Henrik Hustoft](https://www.linkedin.com/in/henrik-hustoft-2366ab220/)

## License

- GNU General Public License. Refer to the [LICENSE](./LICENSE) file
