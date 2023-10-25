# Video Streaming Client package

This package is an internal package that adds support for receiving and decoding a video stream over RTSP.

It is required by the companion app development projects: [Virtual Camera Client](../../CompanionApps/VirtualCamera/VirtualCameraClient) and [Face Capture Client](../../CompanionApps/FaceCapture/FaceCaptureClient).

To contribute to the maintenance and development of this package, refer to the [Contribution and maintenance](../../CONTRIBUTING.md) page.

Review the specific [license terms](LICENSE.md) about this package.

## A Video Streaming client built using RTSP and H.264 encoding.

The network layer is based on [RtspClientSharp](https://github.com/BogdanovKirill/RtspClientSharp)

H.264 Decoding relies on a custom native plugin supporting OSX and iOS,
see `Native~` directory for plugin source

## Usage

Usage is meant to be as simple as possible, and revolves around the `VideoStreamingClient` class.

This class will be passed an URL, whose typical format is:
`rtsp://127.0.0.1:9000` that is `rtsp://{ip}:{port}`

```
m_VideoStreamingClient.Connect("rtsp://127.0.0.1:9000");
```

The decoded video is rendered on an exposed texture,
displaying the video stream then boils down to (for example):

* drawing said texture on `Update`: `Graphics.Blit(m_VideoStreamingClient.texture)`,

* assigning it to a UI component: `m_RawImage.texture = m_VideoStreamingClient.texture;`
