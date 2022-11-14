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
    - Load scene collections additively from a list
    - Unload scene collections
    - Save and override loaded scene collection
    - Create scene collections from loaded scenes
    - Create new scenes and load it additively
    - Add all open scenes to build settings

- Multi Scene Loader
    - Load scene collections with this static class
    - Loading modes
        - Additive
            - Loads all scenes in a collection additive
        - Difference
            - Unloads all scenes the collections do not share, then load the missing scenes.
        - Replace
            - Unloads all scenes other than the boot scene, then loads all scenes additively.

## Examples

- Boot loader and scene transitions

## Setup

- Install via Package Manager → Add package via git URL: 
    - https://github.com/HenrysHouses/MultiSceneTools.git
- Alternatively, download and put the folder in your Assets

## Author

- [Henrik Hustoft](https://www.linkedin.com/in/henrik-hustoft-2366ab220/)
