# Video call Demo

This demo demonstrates basic video call functionality of the Voximplant Unity SDK.
It is possible to make video calls with any application (mobile or web) that have integrated Voximplant SDKs.

## Features

The application is able to:

- log in to the Voximplant Cloud
- make a video call
- receive an incoming call

## Getting started

To get started, you'll need to register a free Voximplant developer account.

You'll need the following:

- Voximplant application
- two Voximplant users
- VoxEngine scenario
- routing setup

### VoxEngine scenario example

```js
VoxEngine.addEventListener(AppEvents.CallAlerting, (e) => {
const newCall = VoxEngine.callUserDirect(
  e.call,
  e.destination,
  e.callerid,
  e.displayName,
  null
);
VoxEngine.easyProcess(e.call, newCall, ()=>{}, true);
});
```

## Installing

1. Clone this repo
2. Install Voximplant Unity SDK from [GitHub Releases](https://github.com/voximplant/unity_sdk/releases)
