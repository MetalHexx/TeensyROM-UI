# USB Video Capture Card Integration

**Project Overview**: Enable real-time video capture and display from retro computing devices (C64, Atari, etc.) using USB video capture cards. Users will be able to view live video output from their TeensyROM-connected devices directly in the web application.

**Standards Documentation**:

- **Coding Standards**: [CODING_STANDARDS.md](../../CODING_STANDARDS.md)
- **Testing Standards**: [TESTING_STANDARDS.md](../../TESTING_STANDARDS.md)
- **State Standards**: [STATE_STANDARDS.md](../../STATE_STANDARDS.md)
- **Domain Standards**: [DOMAIN_STANDARDS.md](../../DOMAIN_STANDARDS.md)
- **Style Guide**: [STYLE_GUIDE.md](../../STYLE_GUIDE.md)
- **Component Library**: [COMPONENT_LIBRARY.md](../../COMPONENT_LIBRARY.md)

## 🎯 Project Objective

Provide seamless video capture integration that allows users to view real-time output from their retro devices alongside file browsing and playback controls. This creates a unified experience where users can launch a game/program and immediately see the visual output without switching windows or applications.

**Key Value Propositions**:
- **Unified Interface**: Control device and view output in single browser window
- **Cross-Platform**: Works on Windows, macOS, Linux without additional drivers
- **Remote Access**: View device output over network (LAN/WAN)
- **Recording**: Capture screenshots and video clips for sharing
- **Multi-Device**: Support multiple capture cards for multi-device setups

## 📋 Implementation Phases

## Phase 1: Browser-Based Video Capture (Proof of Concept)

### Objective

Validate that modern browser MediaDevices API can access USB capture cards and stream video with acceptable latency and quality.

### Key Deliverables

- [ ] Video device enumeration working in Angular
- [ ] Live video stream displaying in browser
- [ ] Permission handling and error states
- [ ] Basic video controls (play/pause/fullscreen)

### High-Level Tasks

1. **Research & Validation**: Test MediaDevices API with actual USB capture cards to confirm browser support
2. **Create Video Capture Component**: New Angular component in `libs/features/video-capture/`
3. **Device Selection UI**: Dropdown/list to choose from available video input devices
4. **Video Display**: HTML5 `<video>` element with proper sizing and controls
5. **Error Handling**: User-friendly messages for permission denial, device busy, etc.

### Technical Notes

```typescript
// MediaDevices API approach
const devices = await navigator.mediaDevices.enumerateDevices();
const videoDevices = devices.filter(d => d.kind === 'videoinput');

const stream = await navigator.mediaDevices.getUserMedia({
  video: {
    deviceId: { exact: selectedDeviceId },
    width: { ideal: 1920 },
    height: { ideal: 1080 },
    frameRate: { ideal: 60 }
  }
});

videoElement.srcObject = stream;
```

---

## Phase 2: UI Integration & Configuration

### Objective

Integrate video capture seamlessly into existing player UI with device configuration and layout options.

### Key Deliverables

- [ ] Video viewer integrated into player view
- [ ] Device configuration panel
- [ ] Layout options (side-by-side, picture-in-picture, fullscreen)
- [ ] Video quality/resolution settings
- [ ] Persistent user preferences

### High-Level Tasks

1. **Player Layout Updates**: Add video display area to player-device-container
2. **Configuration UI**: Settings panel for device selection, resolution, layout
3. **State Management**: Video capture state in player context (device ID, stream status, settings)
4. **Responsive Design**: Video viewer adapts to different screen sizes
5. **Persistence**: Save user's capture card preferences in local storage

### Layout Options

- **Side-by-Side**: File browser on left, video on right
- **Picture-in-Picture**: Small video overlay on file browser
- **Fullscreen**: Video fills entire player area with overlay controls
- **Tabbed**: Switch between file browser and video view

---

## Phase 3: .NET Backend Streaming (Advanced Features)

### Objective

Implement backend video capture service for scenarios where browser API is insufficient (older browsers, additional processing, recording, etc.)

### Key Deliverables

- [ ] .NET video capture service using DirectShow/FFmpeg
- [ ] SignalR video streaming hub (`/videoHub`)
- [ ] Server-side video encoding and compression
- [ ] Multi-client streaming support
- [ ] Fallback mechanism when browser capture unavailable

### High-Level Tasks

1. **Video Capture Service**: .NET service using DirectShow.NET or FFmpeg to access capture cards
2. **SignalR Hub**: Real-time streaming hub for video frame transmission
3. **Encoding Pipeline**: H.264/VP8 encoding for efficient streaming
4. **Client Component**: Angular component to receive and decode SignalR video stream
5. **Auto-Detection**: Automatically use backend streaming when browser API fails

### Technical Architecture

```
┌─────────────────────┐
│  USB Capture Card   │
└──────────┬──────────┘
           │
┌──────────▼──────────┐
│ .NET Capture Service│
│ (DirectShow/FFmpeg) │
└──────────┬──────────┘
           │
┌──────────▼──────────┐
│   Encoding Layer    │
│   (H.264/VP8/MJPEG) │
└──────────┬──────────┘
           │
┌──────────▼──────────┐
│   SignalR Hub       │
│   (/videoHub)       │
└──────────┬──────────┘
           │
     ┌─────▼─────┐
     │  Browser  │
     │  Client   │
     └───────────┘
```

---

## Phase 4: Advanced Features & Optimization

### Objective

Add value-added features like recording, screenshots, performance optimization, and multi-device support.

### Key Deliverables

- [ ] Screenshot capture with automatic naming/storage
- [ ] Video recording with start/stop controls
- [ ] Performance monitoring and adaptive quality
- [ ] Multi-device capture card support
- [ ] Video filters and overlays (scanlines, CRT effects)

### High-Level Tasks

1. **Recording Features**: MediaRecorder API or backend recording to file
2. **Screenshot Tool**: Canvas-based screenshot capture with download
3. **Performance Monitor**: FPS counter, dropped frames detection, adaptive quality
4. **Multi-Device UI**: Support for multiple capture cards with device switching
5. **Visual Effects**: Optional CRT shader effects, aspect ratio correction, scanlines
6. **Metadata Integration**: Sync screenshots/recordings with currently playing file

---

## 🏗️ Architecture Overview

### Key Design Decisions

- **Browser-First Approach**: Use MediaDevices API as primary method (Phase 1-2), fallback to backend streaming only when necessary
- **Component Modularity**: Video capture as standalone feature in `libs/features/video-capture/` that integrates with player
- **State Management**: Video capture state managed through player context using NgRx Signal Store pattern
- **Progressive Enhancement**: Basic video viewing first, advanced features (recording, effects) added incrementally

### Integration Points

- **Player Context**: Video capture state and controls integrated into existing PLAYER_CONTEXT
- **Player Device Container**: Video viewer as new section alongside file browser and controls
- **Device State**: Capture card configuration tied to TeensyROM device (per-device settings)
- **SignalR Infrastructure**: Reuse existing SignalR connection management, add new `/videoHub` for streaming

### Component Structure

```
libs/features/video-capture/
├── src/
│   ├── lib/
│   │   ├── video-capture-view/
│   │   │   ├── video-capture-view.component.ts       # Main video viewer
│   │   │   ├── video-capture-view.component.html
│   │   │   └── video-capture-view.component.scss
│   │   ├── video-device-selector/
│   │   │   ├── video-device-selector.component.ts    # Device dropdown/list
│   │   │   └── ...
│   │   ├── video-controls/
│   │   │   ├── video-controls.component.ts           # Play/pause/fullscreen/record
│   │   │   └── ...
│   │   └── video-settings/
│   │       ├── video-settings.component.ts           # Resolution, quality, layout
│   │       └── ...
│   └── index.ts

libs/domain/video-capture/                            # Domain layer
├── services/
│   └── video-capture.service.ts                      # MediaDevices API wrapper
└── state/
    └── video-capture.store.ts                        # NgRx Signal Store
```

### Backend Architecture (Phase 3)

```
TeensyRom.Api/
├── Endpoints/
│   └── VideoCapture/
│       ├── GetDevices/                               # List available capture cards
│       ├── GetDeviceCapabilities/                    # Supported resolutions/formats
│       └── StartCapture/                             # Initialize capture session
├── Hubs/
│   └── VideoHub.cs                                   # SignalR streaming hub
└── Services/
    └── VideoCaptureService.cs                        # DirectShow/FFmpeg integration

TeensyRom.Core/
└── VideoCapture/
    ├── IVideoCaptureDevice.cs                        # Abstraction for capture cards
    ├── DirectShowCaptureDevice.cs                    # Windows DirectShow implementation
    └── VideoEncodingService.cs                       # H.264/VP8 encoding
```

---

## 🧪 Testing Strategy

### Unit Tests

- [ ] Video device enumeration and filtering
- [ ] Stream state management (connecting, connected, error, disconnected)
- [ ] User settings persistence and retrieval
- [ ] Error handling for various failure scenarios
- [ ] Component render tests (device selector, controls, settings)

### Integration Tests

- [ ] MediaDevices API permission flow
- [ ] Video stream initialization and teardown
- [ ] Device switching without memory leaks
- [ ] SignalR hub connection and streaming (Phase 3)
- [ ] Recording start/stop without stream interruption (Phase 4)

### E2E Tests

- [ ] User grants camera permission and sees video
- [ ] User switches between multiple capture cards
- [ ] User changes video layout and preferences persist
- [ ] User captures screenshot of current video
- [ ] User records video clip and downloads file

### Manual Testing Scenarios

- [ ] Test with various USB capture card brands/models
- [ ] Verify different resolutions (480p, 720p, 1080p)
- [ ] Check performance with different frame rates
- [ ] Test on different browsers (Chrome, Firefox, Edge, Safari)
- [ ] Validate cross-platform (Windows, macOS, Linux)

---

## ✅ Success Criteria

- [ ] Users can view real-time video from USB capture cards in browser
- [ ] Video latency under 200ms for local connections
- [ ] Smooth 60fps playback for supported devices
- [ ] Works on major browsers (Chrome, Firefox, Edge) without plugins
- [ ] Graceful fallback when capture card unavailable
- [ ] Intuitive UI integrated with existing player
- [ ] No memory leaks during long capture sessions
- [ ] Screenshot and recording features working reliably

---

## 📚 Related Documentation

- **Architecture Overview**: [OVERVIEW_CONTEXT.md](../../OVERVIEW_CONTEXT.md)
- **Player Domain**: [PLAYER_DOMAIN.md](../player-state/PLAYER_DOMAIN.md)
- **Component Library**: [COMPONENT_LIBRARY.md](../../COMPONENT_LIBRARY.md)
- **API Client Generation**: [API_CLIENT_GENERATION.md](../../API_CLIENT_GENERATION.md)

---

## 📝 Notes & Considerations

### Browser Compatibility

- **Chrome/Edge**: Excellent MediaDevices API support, H.264 hardware decoding
- **Firefox**: Good support, may need codec configuration
- **Safari**: Requires WebRTC for some features, test thoroughly on macOS/iOS

### Performance Considerations

- **Local USB Capture**: Very low latency (< 50ms) with MediaDevices API
- **Remote Streaming**: Expect 100-500ms latency depending on network
- **Encoding Overhead**: H.264 hardware encoding preferred for smooth performance
- **Multiple Streams**: May need quality reduction for multi-device scenarios

### Privacy & Security

- **User Permission**: Always request explicit permission for camera access
- **HTTPS Required**: MediaDevices API only works over HTTPS (except localhost)
- **Stream Privacy**: Ensure streams are not accessible to unauthorized users
- **Recording Storage**: Consider privacy implications of server-side recording

### Hardware Compatibility

- **USB 2.0 vs 3.0**: USB 3.0 recommended for 1080p60 capture
- **HDMI Capture Cards**: Most common, widely supported
- **Composite/Component**: Older devices, lower resolution but compatible
- **Latency**: Some cheap capture cards have high latency (> 100ms)

### Future Enhancements

- **WebRTC Peer-to-Peer**: Direct browser-to-browser streaming for remote viewing
- **Cloud Recording**: Upload recordings to cloud storage (Azure Blob, S3)
- **Stream Overlays**: Add custom overlays (game info, controls, branding)
- **AI Enhancement**: Upscaling, deinterlacing, artifact removal
- **Multi-View**: Picture-in-picture for multiple devices simultaneously
- **VR Integration**: View retro devices in virtual arcade/room environment

### Dependencies

- **Browser APIs**: MediaDevices, MediaRecorder, Canvas (for screenshots)
- **.NET Libraries** (Phase 3): DirectShow.NET or FFmpeg.AutoGen
- **SignalR**: Already in use for device logs
- **Angular Material**: Video controls UI components

### Open Questions

- [ ] Which USB capture card brands/models to officially support?
- [ ] Should recording be client-side (MediaRecorder) or server-side?
- [ ] What video encoding format for maximum compatibility?
- [ ] How to handle audio from capture card (if available)?
- [ ] Should we support streaming to external services (Twitch, YouTube)?

---

## 🔄 Implementation Timeline (Estimated)

- **Phase 1**: 1-2 weeks (browser proof of concept)
- **Phase 2**: 2-3 weeks (UI integration and configuration)
- **Phase 3**: 3-4 weeks (backend streaming infrastructure)
- **Phase 4**: 2-3 weeks (advanced features and polish)

**Total Estimated Time**: 8-12 weeks for full implementation

---

## 🎯 Next Steps

1. **Validate Hardware**: Test MediaDevices API with actual USB capture cards
2. **Create Phase 1 Branch**: `feature/video-capture-phase1`
3. **Implement POC Component**: Basic video capture viewer
4. **User Testing**: Get feedback on latency, quality, usability
5. **Decide on Phases 2-4**: Based on POC results and user feedback
