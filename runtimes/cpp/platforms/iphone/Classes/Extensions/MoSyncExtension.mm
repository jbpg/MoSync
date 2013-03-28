#include <objc/runtime.h>
#include "MoSyncExtension.h"
#include <helpers/cpp_defs.h>

static NSMutableDictionary* sExtensions;
static NSMutableDictionary* sFunctions;

void initExtensions(MoSync* mosync)
{
	sExtensions = [[NSMutableDictionary alloc] init ];
	int numClasses = objc_getClassList(NULL, 0);
	Class *classes = NULL;
	classes = (Class*)malloc(sizeof(Class) * numClasses);
	numClasses = objc_getClassList(classes, numClasses);

    id object = [[NSClassFromString(extensionImpl) alloc] init];
	for(int i = 0; i < numClasses; i++)
	{
		if(classes[i] && class_conformsToProtocol(classes[i], @protocol(MoSyncExtensionProtocol)))
		{
			id extension = [[classes[i] alloc] initWithMoSync:mosync];
			[sExtensions setObject:extension forKey:extensionName];
		}
	}

	free((void*) classes);
}

int invokeExtensionFunction(MAExtensionFunction fn, int numargs, int ptrs) {
    FunctionInvocation* fn = [sFunctions objectForKey:fn]
    if (fn) {
        fn->invoke(numargs, ptrs);
        return 0;
    }
    maPanic(40035, "Cannot find function");
    return 1;
}

@interface ExtensionManager : NSObject {
}

-(void) load;
-(int) maExtensionModuleLoad:NSString name :int hash;
-(int) maExtensionFunctionLoad:int module :int fn;
-(int) invoke:(int)fn ;

@end

@implementation ExtensionManager
-(int) maExtensionModuleLoad:NSString name, int hash
{
}
-(int) maExtensionFunctionLoad:int module,int fn
{

}

@end

@implementation FunctionInvocation
-(id) init
{
    return self;
}

-(void) invoke:(int)numargs (int) ptrs
{
    (int*) castPtrs = (int*)ptrs;
}
@end

#if 0
@interface TestExtension : NSObject<MoSyncExtensionProtocol>
{
}

-(long long) maScreenGrabberGetScreen:(int)image;

@end

@implementation TestExtension
-(id) initWithMoSync:(MoSync*)mosync
{
	return self;
}

-(void) close
{
}

-(NSString*) getName
{
	return @"ScreenGrabber";
}

-(int) getHash
{
	return 0x1234124;
}

-(long long) invokeExtensionWithIndex:(int)_index andArg1:(int)a andArg2:(int)b andArg3:(int)c
{
	switch(_index)
    {
		case 1:
			return [self maScreenGrabberGetScreen:a];
	}
	return MA_EXTENSION_FUNCTION_UNAVAILABLE;
}

-(long long) maScreenGrabberGetScreen:(int)image
{
	return 0;
}
@end

#endif // if 0
