//
//  CustomSplashViewController.m
//  UnitySplashLaunch
//
//  Created by xiabob on 2020/3/6.
//  Copyright © 2020 xiabob. All rights reserved.
//

#import "CustomSplashViewController.h"

@interface CustomSplashViewController ()

@end

@implementation CustomSplashViewController

- (void)viewDidLoad {
    [super viewDidLoad];
    // Do any additional setup after loading the view.
}

/*
#pragma mark - Navigation

// In a storyboard-based application, you will often want to do a little preparation before navigation
- (void)prepareForSegue:(UIStoryboardSegue *)segue sender:(id)sender {
    // Get the new view controller using [segue destinationViewController].
    // Pass the selected object to the new view controller.
}
*/

- (UIInterfaceOrientationMask)supportedInterfaceOrientations {
    UIInterfaceOrientationMask ret = 0;
    
    ret |= (1 << UIInterfaceOrientationLandscapeRight);

    ret |= (1 << UIInterfaceOrientationLandscapeLeft);

    return ret;
}

@end
