# User Task 01: Create Background VFX Graph

## 概要
VFX Graph を使用して、背景となるパーティクルエフェクトを作成してください。

## 手順

1. **VFX Asset の作成**
   - `Assets/VFX` フォルダを作成してください（なければ）。
   - その中に `Background.vfx` という名前で VFX Graph Asset を作成してください。

2. **VFX Graph の編集**
   - `Background.vfx` を開き、以下の仕様でグラフを構築してください。
     - **Particle Output**: Point Output (または Quad Output)
     - **Capacity**: 10,000 ~ 50,000 程度
     - **Spawn**: Constant Spawn (Rate: 1000/s など)
     - **Initialize**: 
       - Box Position (大きな範囲に配置, size: 500, 500, 500)
       - Lifetime (10秒以上、または無限)
   - **イベント受信**
     - 以下の Event Name を受信してリアクションするようにしてください。
       - `OnBeat`: 瞬間的に Emissive Color を明るくする、サイズを変えるなど
       - `OnMeasure`: 違う色に変える、動きを変えるなど

3. **シーンへの配置**
   - シーンに `VisualEffect` Game Object を作成し、`Background.vfx` を割り当ててください。
   - 作成したオブジェクトに、実装済みの `SpaceManager` コンポーネントをアタッチしてください。
   - `SpaceManager` の `Background VFX` フィールドに、その Visual Effect コンポーネントを割り当ててください。
