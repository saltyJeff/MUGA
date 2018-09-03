# MAKE UNET GREAT AGAIN

Provides a latency compensation/entity interpolation/input prediction solution
for building ~~battle royale~~ networked games on Unity3d.

## Features

* Plugs into HLAPI, Use NetworkManager with a few adjustments and keep all your
SyncVars and ClientRPCs
* Latency compensation, use the LCPhysics to restore all colliders to a previous state
* Entity Interpolation, render GameObjects where they were a few 100 ms in the past
* Input Prediction, by abusing Physics.Simulate you can predict where the player is on the 
server

## Install/Setup

1. Download the files in the `Assets\Scripts\MUGA` folder
2. Download [MessagePack](https://github.com/neuecc/MessagePack-CSharp) with the Unity3d 
extensions. We use MessagePack over Unity's native serializer for its support for many more 
and its extensibility.
3. Wait 15 years for VS 2017 to boot.
4. Start creating your game.

## Docs/Tutorial Project

This project's features are inspired by Valve's implementation for network games. Read 
[this](https://developer.valvesoftware.com/wiki/Source_Multiplayer_Networking) and 
[this](https://developer.valvesoftware.com/wiki/Latency_Compensating_Methods_in_Client/Server_In-game_Protocol_Design_and_Optimization) 
before starting this library to understand all the words.

Doxygen files are live [here](https://saltyJeff.github.io/MUGA)

The Tutorial project walks you through creating a basic top-down shooter. I will put up
a code walkthrough on the Github wiki, but for now this entire repo is the finished demo.

## Licensing

This project is licensed under the MIT License, so feel free to use it in your game (but I 
wouldn't mind a name drop in the credits)

This project also relies on MessagePack, which is as of this writing is also on the MIT license.
