#import <Foundation/Foundation.h>
#import <AVFoundation/AVFoundation.h>
#import <ARKit/ARKit.h>

typedef struct UnityXRNativeSessionPtr
{
    int version;
    void* session;
} UnityXRNativeSessionPtr;

@interface CaptureSession : NSObject<AVCaptureAudioDataOutputSampleBufferDelegate>

@property (strong, nonatomic) AVCaptureSession* captureSession;
@property (strong, nonatomic) AVCaptureDevice* inputMicrophone;
@property (strong, nonatomic) AVCaptureDeviceInput* audioInput;
@property (strong, nonatomic) AVCaptureAudioDataOutput* audioCaptureOutput;
@property (strong, nonatomic) AVAssetWriter* videoWriter;
@property (strong, nonatomic) AVAssetWriterInputPixelBufferAdaptor* pixelBufferAdaptor;
@property (strong, nonatomic) AVAssetWriterInput* videoOutput;
@property (strong, nonatomic) AVAssetWriterInput* audioOutput;
@property (weak, nonatomic) ARSession* arSession;
@property (nonatomic) CMTime frameInterval;
@property (nonatomic) CMTime recordingTime;
@property (nonatomic) bool isRecording;
@property (nonatomic) bool includeAudio;

-(instancetype) init:(const void*)sessionPtr withFps:(int)fps;
-(void) start:(NSURL*)path withIncludeAudio:(bool)includeAudio;
-(void) stop;
-(void) captureVideoFrame;

@end

@implementation CaptureSession

-(instancetype) init:(const void*)sessionPtr withFps:(int)fps
{
    if (self = [super init])
    {
        UnityXRNativeSessionPtr* nativePtr = (UnityXRNativeSessionPtr*)sessionPtr;
        self.arSession = (__bridge ARSession*)nativePtr->session;

        self.frameInterval = CMTimeMake(1, fps);

        self.captureSession = [[AVCaptureSession alloc] init];
        [self.captureSession setSessionPreset:AVCaptureSessionPresetMedium];
        [self.captureSession beginConfiguration];

        self.inputMicrophone = [AVCaptureDevice defaultDeviceWithMediaType:AVMediaTypeAudio];

        NSError* error = nil;
        self.audioInput = [[AVCaptureDeviceInput alloc] initWithDevice:self.inputMicrophone error:&error];

        if (error)
        {
            NSLog(@"Microphone error");
        }

        if ([self.captureSession canAddInput:self.audioInput])
        {
            [self.captureSession addInput:self.audioInput];
        }

        dispatch_queue_t audioCaptureQueue = dispatch_get_global_queue(DISPATCH_QUEUE_PRIORITY_HIGH, 0);

        self.audioCaptureOutput = [[AVCaptureAudioDataOutput alloc] init];
        [self.audioCaptureOutput setSampleBufferDelegate:self queue:audioCaptureQueue];

        if ([self.captureSession canAddOutput:self.audioCaptureOutput])
        {
            [self.captureSession addOutput:self.audioCaptureOutput];
        }

        [self.captureSession commitConfiguration];
    }
    return self;
}

-(void) start:(NSURL*)path withIncludeAudio:(bool)includeAudio
{
    if (self.isRecording)
        return;
    
    NSLog(@"Start recording at path %@", [path absoluteString]);

    NSError *error = nil;
    self.videoWriter = [[AVAssetWriter alloc] initWithURL:path fileType:AVFileTypeMPEG4 error:&error];

    if (error)
    {
        NSLog(@"Error creating AssetWriter: %@",[error description]);
        return;
    }

    [self.captureSession startRunning];
    
    // configure picture input
    CVPixelBufferRef capturedImage = self.arSession.currentFrame.capturedImage;
    CGSize frameSize;
    frameSize.width = CVPixelBufferGetWidth(capturedImage);
    frameSize.height = CVPixelBufferGetHeight(capturedImage);

    NSDictionary *videoSettings = [NSDictionary dictionaryWithObjectsAndKeys:
       AVVideoCodecTypeH264, AVVideoCodecKey,
       [NSNumber numberWithInt:frameSize.width], AVVideoWidthKey,
       [NSNumber numberWithInt:frameSize.height], AVVideoHeightKey,
       nil];

    self.videoOutput = [AVAssetWriterInput
        assetWriterInputWithMediaType:AVMediaTypeVideo
        outputSettings:videoSettings];
    
    self.videoOutput.expectsMediaDataInRealTime = YES;

    CGFloat rotationAngle;
    switch (UIDevice.currentDevice.orientation)
    {
        case UIDeviceOrientationPortrait:
            rotationAngle = 90;
            break;
        case UIDeviceOrientationPortraitUpsideDown:
            rotationAngle = -90;
            break;
        default:
            rotationAngle = 0;
            break;
    }

    CGAffineTransform t = CGAffineTransformIdentity;
    t = CGAffineTransformRotate(t, rotationAngle * M_PI / 180);

    self.videoOutput.transform = t;
    
    NSMutableDictionary* attributes = [[NSMutableDictionary alloc] init];
    [attributes setObject:[NSNumber numberWithUnsignedInt:kCVPixelFormatType_32ARGB] forKey:(NSString*)kCVPixelBufferPixelFormatTypeKey];
    [attributes setObject:[NSNumber numberWithUnsignedInt:frameSize.width] forKey:(NSString*)kCVPixelBufferWidthKey];
    [attributes setObject:[NSNumber numberWithUnsignedInt:frameSize.height] forKey:(NSString*)kCVPixelBufferHeightKey];

    self.pixelBufferAdaptor = [AVAssetWriterInputPixelBufferAdaptor
                        assetWriterInputPixelBufferAdaptorWithAssetWriterInput:self.videoOutput
                        sourcePixelBufferAttributes:attributes];

    [self.videoWriter addInput:self.videoOutput];
    
    
    // configure audio input
    self.includeAudio = includeAudio;
    
    if (includeAudio)
    {
        self.captureSession.usesApplicationAudioSession = NO;
        self.captureSession.automaticallyConfiguresApplicationAudioSession = YES;

        NSDictionary* audioSettings = [self.audioCaptureOutput recommendedAudioSettingsForAssetWriterWithOutputFileType:AVFileTypeMPEG4];

        self.audioOutput = [AVAssetWriterInput
            assetWriterInputWithMediaType: AVMediaTypeAudio
            outputSettings: audioSettings];

        self.audioOutput.expectsMediaDataInRealTime = YES;

        [self.videoWriter addInput:self.audioOutput];
    }
    else
    {
        self.captureSession.usesApplicationAudioSession = YES;
        self.captureSession.automaticallyConfiguresApplicationAudioSession = NO;
    }
    
    self.recordingTime = CMTimeMakeWithSeconds(CACurrentMediaTime(), 240);
    
    [self.videoWriter startWriting];
    [self.videoWriter startSessionAtSourceTime:self.recordingTime];
    
    self.isRecording = true;
}

-(void) stop
{
    if (!self.isRecording)
        return;
    
    [self.videoWriter finishWritingWithCompletionHandler:^{}];
    [self.captureSession stopRunning];
    self.isRecording = false;
}

-(void) captureOutput:(AVCaptureOutput*)output didOutputSampleBuffer:(CMSampleBufferRef)sampleBuffer fromConnection:(AVCaptureConnection*)connection
{
    if (self.isRecording && self.includeAudio && self.audioOutput.isReadyForMoreMediaData)
    {
        [self.audioOutput appendSampleBuffer:sampleBuffer];
    }
}

-(void) captureVideoFrame
{
    if (!self.isRecording)
        return;
        
    ARFrame* frame = self.arSession.currentFrame;

    if (frame == nullptr)
    {
        NSLog(@"null frame");
        return;
    }

    if (self.videoOutput.isReadyForMoreMediaData)
    {
        self.recordingTime = CMTimeAdd(self.recordingTime, self.frameInterval);
        [self.pixelBufferAdaptor appendPixelBuffer:[frame capturedImage] withPresentationTime:self.recordingTime];
    }
}

@end

CaptureSession* session;
NSURL* videoOutputPath;

#ifdef __cplusplus
extern "C" {
#endif

void UnityVideoCaptureSetup(const void* sessionPtr, int fps)
{
    session = [[CaptureSession alloc] init:sessionPtr withFps:fps];
}

void UnityVideoCaptureSetPath(const char* path)
{
    videoOutputPath = [[NSURL alloc] initFileURLWithPath:[NSString stringWithUTF8String:path] isDirectory:false];
}

bool UnityVideoCaptureIsRecording()
{
    return session.isRecording;
}

void UnityVideoCaptureStartRecording(bool includeAudio)
{
    [session start:videoOutputPath withIncludeAudio:includeAudio];
}

void UnityVideoCaptureStopRecording()
{
    [session stop];
}

void UnityCaptureVideoFrame()
{
    [session captureVideoFrame];
}

#ifdef __cplusplus
}
#endif
