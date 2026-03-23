#import <Cocoa/Cocoa.h>

typedef NS_ENUM(NSInteger, Fram3dCursorKind) {
    Fram3dCursorKindDefault = 0,
    Fram3dCursorKindIBeam,
    Fram3dCursorKindCrosshair,
    Fram3dCursorKindOpenHand,
    Fram3dCursorKindClosedHand,
    Fram3dCursorKindResizeLeftRight,
    Fram3dCursorKindResizeUp,
    Fram3dCursorKindResizeDown,
    Fram3dCursorKindResizeUpDown,
    Fram3dCursorKindOperationNotAllowed,
    Fram3dCursorKindPointingHand,
    Fram3dCursorKindBusy
};

static Fram3dCursorKind sActiveCursor = Fram3dCursorKindDefault;
static NSWindow*        sInstalledWindow;
static BOOL             sPreviousAcceptsMouseMovedEvents = NO;
static BOOL             sHasPreviousAcceptsMouseMovedEvents = NO;

@class Fram3dCursorOverlayView;
static Fram3dCursorOverlayView* sOverlayView;

static NSWindow* Fram3dTargetWindow(void)
{
    NSApplication* app = NSApplication.sharedApplication;
    return app.keyWindow ?: app.mainWindow ?: app.windows.firstObject;
}

static NSCursor* Fram3dCursorForKind(Fram3dCursorKind kind)
{
    switch (kind)
    {
        case Fram3dCursorKindIBeam:               return [NSCursor IBeamCursor];
        case Fram3dCursorKindCrosshair:           return [NSCursor crosshairCursor];
        case Fram3dCursorKindOpenHand:            return [NSCursor openHandCursor];
        case Fram3dCursorKindClosedHand:          return [NSCursor closedHandCursor];
        case Fram3dCursorKindResizeLeftRight:     return [NSCursor resizeLeftRightCursor];
        case Fram3dCursorKindResizeUpDown:        return [NSCursor resizeUpDownCursor];
        case Fram3dCursorKindOperationNotAllowed: return [NSCursor operationNotAllowedCursor];
        case Fram3dCursorKindPointingHand:        return [NSCursor pointingHandCursor];
        case Fram3dCursorKindResizeUp:
        case Fram3dCursorKindResizeDown:
        case Fram3dCursorKindBusy:
        case Fram3dCursorKindDefault:
        default:                                  return [NSCursor arrowCursor];
    }
}

static void Fram3dApplyActiveCursor(void)
{
    NSCursor* cursor = Fram3dCursorForKind(sActiveCursor);
    if (cursor == nil)
        cursor = NSCursor.arrowCursor;

    [cursor set];
}

@interface Fram3dCursorOverlayView : NSView
@property(nonatomic, strong) NSTrackingArea* fram3dTrackingArea;
@end

@implementation Fram3dCursorOverlayView

- (BOOL)isOpaque
{
    return NO;
}

- (BOOL)acceptsFirstResponder
{
    return NO;
}

- (BOOL)acceptsFirstMouse:(NSEvent*)event
{
    return NO;
}

- (NSView*)hitTest:(NSPoint)point
{
    return nil;
}

- (void)cursorUpdate:(NSEvent*)event
{
    if (sActiveCursor != Fram3dCursorKindDefault)
        Fram3dApplyActiveCursor();
}

- (void)mouseEntered:(NSEvent*)event
{
    if (sActiveCursor != Fram3dCursorKindDefault)
        Fram3dApplyActiveCursor();
}

- (void)mouseMoved:(NSEvent*)event
{
    if (sActiveCursor != Fram3dCursorKindDefault)
        Fram3dApplyActiveCursor();
}

- (void)updateTrackingAreas
{
    if (self.fram3dTrackingArea != nil)
    {
        [self removeTrackingArea:self.fram3dTrackingArea];
        self.fram3dTrackingArea = nil;
    }

    NSTrackingAreaOptions options =
        NSTrackingCursorUpdate |
        NSTrackingMouseEnteredAndExited |
        NSTrackingMouseMoved |
        NSTrackingActiveInKeyWindow |
        NSTrackingInVisibleRect |
        NSTrackingAssumeInside |
        NSTrackingEnabledDuringMouseDrag;

    self.fram3dTrackingArea =
        [[NSTrackingArea alloc] initWithRect:NSZeroRect
                                     options:options
                                       owner:self
                                    userInfo:nil];
    [self addTrackingArea:self.fram3dTrackingArea];
    [super updateTrackingAreas];
}

@end

static void Fram3dEnableCursorRects(void)
{
    if (sInstalledWindow == nil)
        return;

    if (sHasPreviousAcceptsMouseMovedEvents)
    {
        sInstalledWindow.acceptsMouseMovedEvents = sPreviousAcceptsMouseMovedEvents;
        sHasPreviousAcceptsMouseMovedEvents = NO;
    }

    [sInstalledWindow enableCursorRects];
    sInstalledWindow = nil;
}

static void Fram3dDisableCursorRects(NSWindow* window)
{
    if (window == nil)
        return;

    if (sInstalledWindow == window)
        return;

    if (sInstalledWindow != nil && sInstalledWindow != window)
    {
        if (sHasPreviousAcceptsMouseMovedEvents)
        {
            sInstalledWindow.acceptsMouseMovedEvents = sPreviousAcceptsMouseMovedEvents;
            sHasPreviousAcceptsMouseMovedEvents = NO;
        }

        [sInstalledWindow enableCursorRects];
    }

    sPreviousAcceptsMouseMovedEvents = window.acceptsMouseMovedEvents;
    sHasPreviousAcceptsMouseMovedEvents = YES;
    window.acceptsMouseMovedEvents = YES;
    [window disableCursorRects];
    sInstalledWindow = window;
}

static void Fram3dRemoveOverlayView(void)
{
    if (sOverlayView == nil)
        return;

    [sOverlayView removeFromSuperview];
    sOverlayView = nil;
}

static void Fram3dEnsureOverlayView(void)
{
    NSWindow* window = Fram3dTargetWindow();

    if (window == nil)
        return;

    Fram3dDisableCursorRects(window);

    NSView* contentView = window.contentView;

    if (contentView == nil)
        return;

    if (sOverlayView != nil && sOverlayView.superview == contentView)
        return;

    Fram3dRemoveOverlayView();

    sOverlayView = [[Fram3dCursorOverlayView alloc] initWithFrame:contentView.bounds];
    sOverlayView.autoresizingMask = NSViewWidthSizable | NSViewHeightSizable;

    [contentView addSubview:sOverlayView
                 positioned:NSWindowAbove
                 relativeTo:nil];
}

static void Fram3dActivateCursor(Fram3dCursorKind kind)
{
    sActiveCursor = kind;
    Fram3dEnsureOverlayView();
    Fram3dApplyActiveCursor();
}

static void Fram3dDeactivateCursor(void)
{
    sActiveCursor = Fram3dCursorKindDefault;
    Fram3dRemoveOverlayView();
    Fram3dEnableCursorRects();
    [NSCursor.arrowCursor set];
}

extern "C" {
    __attribute__((visibility("default"))) void RefreshActiveCursor(void)
    {
        @autoreleasepool
        {
            if (sActiveCursor == Fram3dCursorKindDefault)
                return;

            Fram3dEnsureOverlayView();
            Fram3dApplyActiveCursor();
        }
    }

    __attribute__((visibility("default"))) void SetCursorToArrow(void)
    {
        @autoreleasepool
        {
            Fram3dDeactivateCursor();
        }
    }

    __attribute__((visibility("default"))) void SetCursorToIBeam(void)
    {
        @autoreleasepool
        {
            Fram3dActivateCursor(Fram3dCursorKindIBeam);
        }
    }

    __attribute__((visibility("default"))) void SetCursorToCrosshair(void)
    {
        @autoreleasepool
        {
            Fram3dActivateCursor(Fram3dCursorKindCrosshair);
        }
    }

    __attribute__((visibility("default"))) void SetCursorToOpenHand(void)
    {
        @autoreleasepool
        {
            Fram3dActivateCursor(Fram3dCursorKindOpenHand);
        }
    }

    __attribute__((visibility("default"))) void SetCursorToClosedHand(void)
    {
        @autoreleasepool
        {
            Fram3dActivateCursor(Fram3dCursorKindClosedHand);
        }
    }

    __attribute__((visibility("default"))) void SetCursorToResizeLeftRight(void)
    {
        @autoreleasepool
        {
            Fram3dActivateCursor(Fram3dCursorKindResizeLeftRight);
        }
    }

    __attribute__((visibility("default"))) void SetCursorToResizeUp(void)
    {
        @autoreleasepool
        {
            Fram3dActivateCursor(Fram3dCursorKindResizeUp);
        }
    }

    __attribute__((visibility("default"))) void SetCursorToResizeDown(void)
    {
        @autoreleasepool
        {
            Fram3dActivateCursor(Fram3dCursorKindResizeDown);
        }
    }

    __attribute__((visibility("default"))) void SetCursorToResizeUpDown(void)
    {
        @autoreleasepool
        {
            Fram3dActivateCursor(Fram3dCursorKindResizeUpDown);
        }
    }

    __attribute__((visibility("default"))) void SetCursorToOperationNotAllowed(void)
    {
        @autoreleasepool
        {
            Fram3dActivateCursor(Fram3dCursorKindOperationNotAllowed);
        }
    }

    __attribute__((visibility("default"))) void SetCursorToPointingHand(void)
    {
        @autoreleasepool
        {
            Fram3dActivateCursor(Fram3dCursorKindPointingHand);
        }
    }

    __attribute__((visibility("default"))) void SetCursorToBusy(void)
    {
        @autoreleasepool
        {
            Fram3dActivateCursor(Fram3dCursorKindBusy);
        }
    }
}
