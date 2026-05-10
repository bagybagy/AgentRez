# Completed Refinement Tasks

## 1. Player Control
- **Vertical Look Limit**:
  - Implemented clamping logic in `PlayerController.cs`.
  - Pitch range restricted to +/- 70 degrees.

## 2. Projectile Behavior
- **Kinematic Movement**:
  - Updated `Projectile.cs` to use physics-based acceleration instead of linear interpolation.
  - Implemented guaranteed hit logic using `SimpleLaser` reference equations.
- **Visuals**:
  - Added randomized initial velocity for curved trajectory.
  - Added requirement for `TrailRenderer` component.
