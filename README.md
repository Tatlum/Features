# Ermine Games Features

Feature system for Unity projects.

## Overview

This package provides a modular feature system for Unity projects, allowing you to enable/disable game features at runtime and communicate between them using a message-based architecture.

## Features

- **Feature Management**: Enable/disable features at runtime with ScriptableObject-based configuration
- **Message System**: Type-safe message passing between features using `FeatureMessage` and `FeatureRequest`
- **Debug Tools**: Built-in message debugger window for tracking feature communication
- **Settings System**: Per-feature settings with editor integration
- **Runtime Data**: Shared data container for cross-feature communication

## Installation

Add this package to your Unity project via Package Manager:

1. Open Package Manager (Window > Package Manager)
2. Click the "+" button and select "Add package from git URL"
3. Enter: `https://github.com/Tatlum/Features.git`

Or add it to your `Packages/manifest.json`:

```json
{
  "dependencies": {
    "com.ermine.features": "https://github.com/Tatlum/Features.git"
  }
}
```

## Quick Start

### 1. Create a Feature

```csharp
using ErmineGames.Features;
using UnityEngine;

public class MyFeature : Feature
{
    protected override void OnEnabled()
    {
        Debug.Log("Feature enabled!");
    }

    protected override void OnDisabled()
    {
        Debug.Log("Feature disabled!");
    }

    public override void Update()
    {
        // Your update logic
    }
}
```

### 2. Create Feature Settings (Optional)

```csharp
using ErmineGames.Features;
using UnityEngine;

[CreateAssetMenu(menuName = "Features/My Feature Settings")]
public class MyFeatureSettings : FeatureSettings
{
    public float someParameter;
    public int anotherParameter;
}
```

### 3. Configure Features

1. Create a `FeaturesSettings` asset (Right-click > Create > Features > Features settings)
2. Add your features to the list
3. Configure settings if needed
4. Enable/disable features in the inspector

### 4. Use Messages

```csharp
// Send a message
public class PlayerFeature : Feature
{
    public void SendPlayerDamage(int damage)
    {
        sharedData.Message.SendMessage(new PlayerDamageMessage { Damage = damage });
    }
}

// Receive messages
public class HealthFeature : Feature
{
    protected override void OnEnabled()
    {
        sharedData.Message.ProcessMessages<PlayerDamageMessage>(message =>
        {
            Debug.Log($"Player took {message.Damage} damage");
        });
    }
}
```

## Architecture

### Core Components

- **Feature**: Base class for all features. Override `OnEnabled()`, `OnDisabled()`, `Update()`, and `FixedUpdate()`
- **FeatureSettings**: Base class for feature-specific settings
- **FeaturesSettings**: ScriptableObject that manages all features and their settings
- **FeatureMessageManager**: Handles message passing between features
- **FeaturesRuntimeSharedData**: Shared data container accessible to all features

### Message System

The package provides two types of communication:

- **FeatureMessage**: One-way messages for events and notifications
- **FeatureRequest**: Request-response pattern with status tracking

```csharp
public class MyMessage : FeatureMessage
{
    public string Data;
}

public class MyRequest : FeatureRequest
{
    public string Query;
    public string Result;
    
    public override void Process()
    {
        // Process the request
        Status = FeatureRequestStatus.Completed;
    }
}
```

### Debug Tools

Enable the `DebugMessageFeature` to track all message traffic:

1. Add `DebugMessageFeature` to your features list
2. Enable it in the inspector
3. Open the Message Debugger window (Window > Ermine Games > Message Debugger)
4. View all messages, requests, and their processing status

## Requirements

- Unity 2022.2 or later
- com.ermine.utils package (dependency)

## License

MIT License - see LICENSE file for details

## Repository

https://github.com/Tatlum/Features.git
