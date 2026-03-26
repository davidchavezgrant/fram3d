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
static BOOL             sOverlayInstalled = NO;
static NSWindow*        sInstalledWindow;
static NSWindow*        sRememberedWindow;

@class Fram3dCursorOverlayView;
static Fram3dCursorOverlayView* sOverlayView;

// ── Helpers ─────────────────────────────────────────────────────────

static BOOL Fram3dIsUsableWindow(NSWindow* window)
{
    return window != nil
        && window.contentView != nil
        && !window.isMiniaturized
        && (window.isVisible || window == sInstalledWindow || window == sRememberedWindow);
}

static NSWindow* Fram3dTargetWindow(void)
{
    NSApplication* app = NSApplication.sharedApplication;

    NSWindow* overlayWindow = sOverlayView ? ((NSView*)sOverlayView).window : nil;

    if (Fram3dIsUsableWindow(overlayWindow))
        return overlayWindow;

    if (Fram3dIsUsableWindow(sInstalledWindow))
        return sInstalledWindow;

    if (Fram3dIsUsableWindow(sRememberedWindow))
        return sRememberedWindow;

    if (Fram3dIsUsableWindow(app.keyWindow))
        return app.keyWindow;

    if (Fram3dIsUsableWindow(app.mainWindow))
        return app.mainWindow;

    for (NSWindow* window in app.orderedWindows)
    {
        if (Fram3dIsUsableWindow(window))
            return window;
    }

    return nil;
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

// ── Overlay NSView ──────────────────────────────────────────────────

@interface Fram3dCursorOverlayView : NSView
@property(nonatomic, strong) NSTrackingArea* fram3dTrackingArea;
@end

@implementation Fram3dCursorOverlayView

- (BOOL)isOpaque              { return NO; }
- (BOOL)acceptsFirstResponder { return NO; }
- (BOOL)acceptsFirstMouse:(NSEvent*)event { return NO; }
- (NSView*)hitTest:(NSPoint)point { return nil; }

- (void)cursorUpdate:(NSEvent*)event
{
    // This is the ONLY place macOS officially supports cursor changes.
    // By setting the cursor here, we work WITH AppKit instead of against it.
    [Fram3dCursorForKind(sActiveCursor) set];
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
        NSTrackingActiveInKeyWindow |
        NSTrackingInVisibleRect |
        NSTrackingAssumeInside;

    self.fram3dTrackingArea =
        [[NSTrackingArea alloc] initWithRect:NSZeroRect
                                     options:options
                                       owner:self
                                    userInfo:nil];
    [self addTrackingArea:self.fram3dTrackingArea];
    [super updateTrackingAreas];
}

- (void)resetCursorRects
{
    [super resetCursorRects];

    // Register a cursor rect covering the entire view. macOS uses this
    // to know which cursor to show — no polling, no per-frame invalidation.
    NSRect cursorRect = NSIntersectionRect(self.bounds, self.visibleRect);
    if (!NSIsEmptyRect(cursorRect))
        [self addCursorRect:cursorRect cursor:Fram3dCursorForKind(sActiveCursor)];
}

@end

// ── Overlay lifecycle ───────────────────────────────────────────────

static void Fram3dInstallOverlay(void)
{
    if (sOverlayInstalled)
        return;

    NSWindow* window = Fram3dTargetWindow();
    if (window == nil)
        return;

    NSView* contentView = window.contentView;
    if (contentView == nil)
        return;

    sInstalledWindow  = window;
    sRememberedWindow = window;

    if (sOverlayView == nil)
    {
        sOverlayView = [[Fram3dCursorOverlayView alloc] initWithFrame:contentView.bounds];
        sOverlayView.autoresizingMask = NSViewWidthSizable | NSViewHeightSizable;
    }

    if (sOverlayView.superview != contentView)
    {
        [sOverlayView removeFromSuperview];
        [contentView addSubview:sOverlayView
                     positioned:NSWindowAbove
                     relativeTo:nil];
    }
    else
    {
        // Ensure we're still the topmost subview
        if (contentView.subviews.lastObject != sOverlayView)
        {
            [sOverlayView removeFromSuperviewWithoutNeedingDisplay];
            [contentView addSubview:sOverlayView
                         positioned:NSWindowAbove
                         relativeTo:nil];
        }
    }

    sOverlayView.frame = contentView.bounds;
    sOverlayInstalled  = YES;
}

/// Called ONLY when the cursor kind changes — triggers macOS to call
/// resetCursorRects and cursorUpdate: with the new cursor.
static void Fram3dInvalidateOnce(void)
{
    if (sOverlayView == nil)
        return;

    NSWindow* window = sOverlayView.window;
    if (window == nil)
        return;

    [window invalidateCursorRectsForView:sOverlayView];
}

// ── Public API ──────────────────────────────────────────────────────

extern "C" {

    /// Called once per frame from C#. Installs the overlay if not yet
    /// installed. Does NOT invalidate cursor rects — that only happens
    /// when the cursor kind changes.
    __attribute__((visibility("default"))) void Fram3dEnsureOverlay(void)
    {
        @autoreleasepool
        {
            Fram3dInstallOverlay();
        }
    }

    /// Sets the active cursor kind. Invalidates cursor rects so macOS
    /// picks up the change. Only call when the cursor actually changes.
    __attribute__((visibility("default"))) void Fram3dSetCursor(int kind)
    {
        @autoreleasepool
        {
            Fram3dCursorKind newKind = (Fram3dCursorKind)kind;

            if (newKind == sActiveCursor)
                return;

            sActiveCursor = newKind;
            Fram3dInstallOverlay();
            Fram3dInvalidateOnce();
            [Fram3dCursorForKind(newKind) set];
        }
    }

    // ── Legacy entry points (kept for compatibility) ────────────────

    __attribute__((visibility("default"))) void RefreshActiveCursor(void)
    {
        @autoreleasepool { Fram3dInstallOverlay(); }
    }

    __attribute__((visibility("default"))) void SetCursorToArrow(void)
    {
        @autoreleasepool { Fram3dSetCursor(Fram3dCursorKindDefault); }
    }

    __attribute__((visibility("default"))) void SetCursorToPointingHand(void)
    {
        @autoreleasepool { Fram3dSetCursor(Fram3dCursorKindPointingHand); }
    }

    __attribute__((visibility("default"))) void SetCursorToIBeam(void)
    {
        @autoreleasepool { Fram3dSetCursor(Fram3dCursorKindIBeam); }
    }

    __attribute__((visibility("default"))) void SetCursorToCrosshair(void)
    {
        @autoreleasepool { Fram3dSetCursor(Fram3dCursorKindCrosshair); }
    }

    __attribute__((visibility("default"))) void SetCursorToOpenHand(void)
    {
        @autoreleasepool { Fram3dSetCursor(Fram3dCursorKindOpenHand); }
    }

    __attribute__((visibility("default"))) void SetCursorToClosedHand(void)
    {
        @autoreleasepool { Fram3dSetCursor(Fram3dCursorKindClosedHand); }
    }

    __attribute__((visibility("default"))) void SetCursorToResizeLeftRight(void)
    {
        @autoreleasepool { Fram3dSetCursor(Fram3dCursorKindResizeLeftRight); }
    }

    __attribute__((visibility("default"))) void SetCursorToResizeUp(void)
    {
        @autoreleasepool { Fram3dSetCursor(Fram3dCursorKindResizeUp); }
    }

    __attribute__((visibility("default"))) void SetCursorToResizeDown(void)
    {
        @autoreleasepool { Fram3dSetCursor(Fram3dCursorKindResizeDown); }
    }

    __attribute__((visibility("default"))) void SetCursorToResizeUpDown(void)
    {
        @autoreleasepool { Fram3dSetCursor(Fram3dCursorKindResizeUpDown); }
    }

    __attribute__((visibility("default"))) void SetCursorToOperationNotAllowed(void)
    {
        @autoreleasepool { Fram3dSetCursor(Fram3dCursorKindOperationNotAllowed); }
    }

    __attribute__((visibility("default"))) void SetCursorToBusy(void)
    {
        @autoreleasepool { Fram3dSetCursor(Fram3dCursorKindBusy); }
    }
}
