# Additional Specifications

## 1. Player Control Refinement
- **Vertical Look Limit**:
    - Restrict vertical camera rotation to ±70 degrees to prevent flipping or awkward angles.

## 2. Projectile Behavior Refinement
- **Curved Trajectory**:
    - Instead of linear linear movement, use physics-based kinematics (Acceleration/Velocity).
    - **Reference**: `SimpleLaser.cs` approach.
        - Set random initial velocity (X/Y/Z) for "dispersal" effect on launch.
        - Calculate required acceleration `a` to reach target `T` at time `period`.
        - `a = 2 * (Destination - (Velocity * time)) / time^2`
    - **Guaranteed Hit**: The physics calculation must ensure the projectile arrives exactly at the `ImpactTime`.
- **Visuals**:
    - Add `TrailRenderer` to visualize the curved path.

## 3. Documentation
- Maintain a record of completed tasks in `Design/03_CompletedTasks.md`.
