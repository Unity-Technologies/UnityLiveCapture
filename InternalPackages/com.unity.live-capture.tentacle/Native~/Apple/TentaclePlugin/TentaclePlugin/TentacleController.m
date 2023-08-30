#import "TentacleController.h"

@interface TentacleController()
@property (nonatomic, strong) TentacleBluetoothController* m_BluetoothController;
@end

@implementation TentacleController

- (instancetype)init
{
    self = [super init];
    if (self) {
        self.m_BluetoothController = [[TentacleBluetoothController alloc] init];
        self.m_BluetoothController.delegate = self;
    }
    return self;
}

- (void)didStartScanning
{
    NSLog(@"Did start scanning.");
}

- (void)didUpdateToState:(enum TentacleBluetoothState)state
{
    NSString *stateString = @"Unknown";

    switch (state)
    {
        case TentacleBluetoothStateResetting:
            stateString = @"Resetting";
            break;
        case TentacleBluetoothStateUnsupported:
            stateString = @"Unsupported";
            break;
        case TentacleBluetoothStateUnauthorized:
            stateString = @"Unauthorized";
            break;
        case TentacleBluetoothStatePoweredOff:
            stateString = @"PoweredOff";
            break;
        case TentacleBluetoothStatePoweredOn:
            stateString = @"PoweredOn";
            [self.m_BluetoothController startScanning];
            break;
        default:
            break;
    }

    NSLog(@"Did update to state: %@", stateString);
}

- (void)didReceiveAdvertisementForDeviceIndex:(NSInteger)deviceIndex
{
}

- (void)stopScanning
{
    [self.m_BluetoothController stopScanning];
}

@end
