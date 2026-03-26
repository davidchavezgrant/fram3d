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
    /// installed, and re-applies [cursor set] to counteract Unity's
    /// render loop resetting the cursor. Does NOT invalidate cursor
    /// rects — that only happens when the cursor kind changes.
    __attribute__((visibility("default"))) void Fram3dEnsureOverlay(void)
    {
        @autoreleasepool
        {
            Fram3dInstallOverlay();
        }
    }

    /// Re-applies the active cursor via [cursor set] without
    /// invalidating cursor rects. Called every frame when a custom
    /// cursor is active to counteract Unity resetting the cursor
    /// during its render loop.
    __attribute__((visibility("default"))) void Fram3dReapplyCursor(void)
    {
        @autoreleasepool
        {
            if (sActiveCursor != Fram3dCursorKindDefault)
                [Fram3dCursorForKind(sActiveCursor) set];
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

    /// Extracts the native cursor image as RGBA pixel data so C# can
    /// create a Texture2D for use with Unity's Cursor.SetCursor API.
    /// Returns pixel dimensions, hotspot, and a malloc'd RGBA buffer
    /// that the caller must free via Fram3dFreeCursorPixels.
    __attribute__((visibility("default"))) int Fram3dExtractCursorImage(
        int kind,
        int* outWidth, int* outHeight,
        float* outHotspotX, float* outHotspotY,
        unsigned char** outPixels)
    {
        @autoreleasepool
        {
            NSCursor* cursor = Fram3dCursorForKind((Fram3dCursorKind)kind);
            if (cursor == nil)
                return 0;

            NSImage* image = cursor.image;
            if (image == nil)
                return 0;

            NSPoint hotspot = cursor.hotSpot;

            // Get the best representation (highest resolution for Retina)
            NSArray<NSImageRep*>* reps = image.representations;
            NSBitmapImageRep* bestRep = nil;
            NSInteger bestWidth = 0;

            for (NSImageRep* rep in reps)
            {
                if ([rep isKindOfClass:[NSBitmapImageRep class]])
                {
                    NSBitmapImageRep* bmp = (NSBitmapImageRep*)rep;
                    if (bmp.pixelsWide > bestWidth)
                    {
                        bestRep = bmp;
                        bestWidth = bmp.pixelsWide;
                    }
                }
            }

            if (bestRep == nil)
            {
                // Fallback: render the image to a bitmap
                NSSize size = image.size;
                NSInteger w = (NSInteger)(size.width * 2); // 2x for Retina
                NSInteger h = (NSInteger)(size.height * 2);

                bestRep = [[NSBitmapImageRep alloc]
                    initWithBitmapDataPlanes:nil
                                 pixelsWide:w
                                 pixelsHigh:h
                              bitsPerSample:8
                            samplesPerPixel:4
                                   hasAlpha:YES
                                   isPlanar:NO
                             colorSpaceName:NSDeviceRGBColorSpace
                               bitmapFormat:NSBitmapFormatAlphaNonpremultiplied
                                bytesPerRow:w * 4
                               bitsPerPixel:32];

                [NSGraphicsContext saveGraphicsState];
                NSGraphicsContext* ctx = [NSGraphicsContext graphicsContextWithBitmapImageRep:bestRep];
                [NSGraphicsContext setCurrentContext:ctx];
                [image drawInRect:NSMakeRect(0, 0, w, h)
                         fromRect:NSZeroRect
                        operation:NSCompositingOperationCopy
                         fraction:1.0];
                [NSGraphicsContext restoreGraphicsState];
            }

            NSInteger w = bestRep.pixelsWide;
            NSInteger h = bestRep.pixelsHigh;

            *outWidth    = (int)w;
            *outHeight   = (int)h;

            // Scale hotspot to pixel coordinates (hotspot is in points)
            float scale  = (float)w / (float)image.size.width;
            *outHotspotX = hotspot.x * scale;
            *outHotspotY = hotspot.y * scale;

            size_t dataSize = w * h * 4;
            unsigned char* pixels = (unsigned char*)malloc(dataSize);

            // Convert to RGBA32 regardless of source format
            for (NSInteger y = 0; y < h; y++)
            {
                for (NSInteger x = 0; x < w; x++)
                {
                    NSColor* color = [bestRep colorAtX:x y:y];
                    NSColor* rgbColor = [color colorUsingColorSpace:[NSColorSpace sRGBColorSpace]];

                    CGFloat r = 0, g = 0, b = 0, a = 0;
                    if (rgbColor != nil)
                        [rgbColor getRed:&r green:&g blue:&b alpha:&a];

                    // Unity Texture2D expects bottom-up rows, NSImage is top-down
                    NSInteger flippedY = h - 1 - y;
                    size_t idx = (flippedY * w + x) * 4;
                    pixels[idx + 0] = (unsigned char)(r * 255);
                    pixels[idx + 1] = (unsigned char)(g * 255);
                    pixels[idx + 2] = (unsigned char)(b * 255);
                    pixels[idx + 3] = (unsigned char)(a * 255);
                }
            }

            *outPixels = pixels;
            return 1;
        }
    }

    __attribute__((visibility("default"))) void Fram3dFreeCursorPixels(unsigned char* pixels)
    {
        if (pixels != NULL)
            free(pixels);
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
