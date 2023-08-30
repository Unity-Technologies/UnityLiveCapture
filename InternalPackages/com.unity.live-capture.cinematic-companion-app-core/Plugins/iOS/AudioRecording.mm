#import <Foundation/Foundation.h>
#import <AVFoundation/AVFoundation.h>

AVAudioRecorder* audioRecorder;
NSURL* audioOutputPath;
bool audioIsRecording;

#ifdef __cplusplus
extern "C" {
#endif

void UnityAudioCaptureSetPath(const char* path)
{
    audioOutputPath = [[NSURL alloc] initFileURLWithPath:[NSString stringWithUTF8String:path] isDirectory:false];
}

bool UnityAudioCaptureIsRecording()
{
    return audioIsRecording;
}

void UnityAudioCaptureToggleRecording()
{
    if (!audioIsRecording)
    {
        // get the audio session
        AVAudioSession *session = [AVAudioSession sharedInstance];
        [session setCategory:AVAudioSessionCategoryPlayAndRecord error:nil];

        // configure the audio recording options
        NSMutableDictionary *audioSettings = [[NSMutableDictionary alloc] init];
        [audioSettings setValue:[NSNumber numberWithInteger:kAudioFormatLinearPCM] forKey:AVFormatIDKey];
        [audioSettings setValue:[NSNumber numberWithFloat:44100] forKey:AVSampleRateKey];
        [audioSettings setValue:[NSNumber numberWithInteger:1] forKey:AVNumberOfChannelsKey];

        // create an audio recorder and begin recording
        audioRecorder = [[AVAudioRecorder alloc] initWithURL:audioOutputPath settings:audioSettings error:nil];
        [audioRecorder prepareToRecord];
        [audioRecorder record];

        audioIsRecording = true;
    }
    else
    {
        [audioRecorder stop];
        audioRecorder = nil;

        audioIsRecording = false;
    }
}

#ifdef __cplusplus
}
#endif
