// Fram3d — Feature Toggle System
// Each feature can be toggled on/off to show UI growth over milestones.

const FEATURES = {
  // 1.1 Virtual Camera
  'hud':                { name: 'Camera Info HUD',                milestone: '1.1', section: '1.2.3', enabled: true },

  // 1.2 Camera Overlays
  'aspect-ratio':       { name: 'Aspect Ratio Masks',            milestone: '1.2', section: '1.2.1', enabled: true },
  'frame-guides':       { name: 'Frame Guides',                  milestone: '1.2', section: '1.2.2', enabled: true },

  // 1.3 Scene Management
  'tool-mode':          { name: 'Tool Mode Indicator (QWER)',    milestone: '1.3', section: '1.3.2', enabled: true },
  'director-view':      { name: 'Director View Toggle',          milestone: '1.3', section: '1.3.5', enabled: true },

  // 1.4 Shot Sequencer
  'shot-management':    { name: 'Shot Add / Delete',             milestone: '1.4', section: '1.4.2', enabled: true },
  'shot-reorder':       { name: 'Shot Drag Reorder',             milestone: '1.4', section: '1.4.2', enabled: true },
  'duration-edit':      { name: 'Duration Editing',              milestone: '1.4', section: '1.4.2', enabled: true },
  'aggregate-duration': { name: 'Aggregate Duration',            milestone: '1.4', section: '1.4.2', enabled: true },
  'hover-thumbnails':   { name: 'Shot Hover Thumbnails',         milestone: '1.4', section: '1.4.2', enabled: true },
  'scenes':             { name: 'Scenes Switcher',               milestone: '1.4', section: '1.4.4', enabled: true },

  // 1.5 Keyframe Animation
  'transport-bar':      { name: 'Transport Bar',                 milestone: '1.5', section: '1.5.1', enabled: true },
  'status-bar':         { name: 'Status Bar (Keyboard Hints)',   milestone: '1.5', section: '1.5.1', enabled: true },
  'track-expand':       { name: 'Track Collapse / Expand',       milestone: '1.5', section: '1.5.2', enabled: true },
  'keyframe-drag':      { name: 'Keyframe Drag',                 milestone: '1.5', section: '1.5.3', enabled: true },
  'camera-path':        { name: 'Camera Path Toggle',            milestone: '1.5', section: '1.5.6', enabled: true },

  // 1.6 Viewport Panel System
  'full-shortcuts':     { name: 'Panel Layout Switcher',         milestone: '1.6', section: '1.6.1', enabled: true },

  // 3.6 Object Linking & Grouping
  'link-indicators':    { name: 'Link Indicators in Panel',      milestone: '3.6', section: '3.6.3', enabled: true },
  'lights-section':     { name: 'Separate Lights Section',       milestone: '3.6', section: '3.6.3', enabled: true },

  // 3.8 Selection Refinements
  'interp-curves':      { name: 'Interpolation Curve Presets',   milestone: '3.8', section: '3.8.3', enabled: true },

  // 3.9 Multi-camera
  'active-cam-keys':    { name: 'Active Camera Shortcuts',       milestone: '3.9', section: '3.9.3', enabled: true },
  'coverage-track':     { name: 'Coverage Splitting Track',      milestone: '3.9', section: '3.9.4', enabled: true },
  'multi-split':        { name: 'Multi-split',                   milestone: '3.9', section: '3.9.5', enabled: true },
};

function feat(id) {
  return FEATURES[id] && FEATURES[id].enabled;
}

function setFeature(id, enabled) {
  if (FEATURES[id]) {
    FEATURES[id].enabled = enabled;
    if (typeof applyFeatureVisibility === 'function') applyFeatureVisibility();
    if (typeof render === 'function') render();
  }
}

function setMilestone(milestone, enabled) {
  Object.values(FEATURES).forEach(f => {
    if (f.milestone === milestone) f.enabled = enabled;
  });
  if (typeof applyFeatureVisibility === 'function') applyFeatureVisibility();
  if (typeof render === 'function') render();
}

function getMilestones() {
  const milestones = {};
  Object.entries(FEATURES).forEach(([id, f]) => {
    if (!milestones[f.milestone]) milestones[f.milestone] = [];
    milestones[f.milestone].push({ id, ...f });
  });
  return milestones;
}
