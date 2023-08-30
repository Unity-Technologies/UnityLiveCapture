#pragma once

#include "Unity/IUnityGraphics.h"

namespace NvencPlugin {

///////////////////////////////////////////////////////////////////////////////
//
// FUNCTION NAME:  OnGraphicsDeviceEvent
//
//! DESCRIPTION:   Overrided callbacks to handle the Device related events.
//!
//! WHEN TO USE:   Automatically called and used when the system is initialized or destroyed.
//!
//  SUPPORTED GFX: Direct3D 11, Direct3D 12
//!
//! \param [in]    eventType      Either specify that the Device has been initialized or destroyed.
///////////////////////////////////////////////////////////////////////////////
static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType);



///////////////////////////////////////////////////////////////////////////////
//
// FUNCTION NAME:  OnRenderEvent
//
//! DESCRIPTION:   Overrided callbacks to handle the Quadro Sync related events.
//!
//! WHEN TO USE:   Called from C# to use a specific Quadro Sync functionality.
//!
//  SUPPORTED GFX: Direct3D 11, Direct3D 12
//!
//! \param [in]    eventID      EQuadroSyncRenderEvent corresponding to the event.
//! \param [in]    data         Buffer containing the data related to the event.
///////////////////////////////////////////////////////////////////////////////
static void UNITY_INTERFACE_API OnRenderEvent(int eventID, void* data);
}
