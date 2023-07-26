# Set up an Actor

Set up a GameObject of the Scene as an [Actor](data-capture-process.md#actor) to be able to drive it from a connected [Client Device](data-capture-process.md#client-device) through a [Capture Device](data-capture-process.md#capture-device).

## Each Actor is different

The way to set up an Actor highly depends on the Client Device and the type of data you're using to drive it. For this reason, there is no common setup procedure.

To ease your Actor setup process, the Live Capture package provides:
* A GameObject menu item that automatically creates a functional Virtual Camera Actor for the Virtual Camera app.
* A sample character head already rigged and set up as a functional Actor for the Face Capture app.

To get instructions about how to use them, follow the [additional reference links](#additional-references).

## Actor setup general guidelines

By principle, to set up a Live Capture Actor:

1. In the Hierarchy, select or create a GameObject that conceptually and technically corresponds to the Client Device.  

   For example:

     * If the Client Device outputs camera data, the GameObject must at least include a component that makes it a Camera.
     * If the Client Device outputs face or body animation data, the GameObject hierarchy must at least include a rigged character model.


2. Add the proper Live Capture components to the GameObject to make it an Actor compatible with the Client Device.  

   For example:

     * If you're using the Virtual Camera app, you have at least to add a [Virtual Camera Actor](ref-component-virtual-camera-actor.md) component to the GameObject, along with other components: [Physical Camera Driver](ref-component-physical-camera-driver.md) and [Frame Lines](ref-component-frame-lines.md).
     * If you're using the Face Capture app, you have at least to add an [ARKit Face Actor](ref-component-arkit-face-actor.md) component to the GameObject and link this component to a [Face Mapper](ref-component-arkit-default-face-mapper.md) asset.

## Additional references

* [Get started with Virtual Camera setup](virtual-camera-getting-started.md)
* [Get started with Face Capture setup](face-capture-getting-started.md)
