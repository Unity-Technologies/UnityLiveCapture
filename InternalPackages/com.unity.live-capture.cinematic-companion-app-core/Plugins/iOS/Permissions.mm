#import <AVFoundation/AVFoundation.h>

#ifdef __cplusplus
extern "C" {
#endif

void OpenSettings ()
{
    NSURL * url = [NSURL URLWithString: UIApplicationOpenSettingsURLString];
    [[UIApplication sharedApplication] openURL: url];
}

bool HasVideoPermission()
{
    AVAuthorizationStatus authStatus = [AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeVideo];
    return authStatus == AVAuthorizationStatusAuthorized;
}

bool HasAudioPermission()
{
    AVAuthorizationStatus authStatus = [AVCaptureDevice authorizationStatusForMediaType:AVMediaTypeAudio];
    return authStatus == AVAuthorizationStatusAuthorized;
}

#ifdef __cplusplus
}
#endif
