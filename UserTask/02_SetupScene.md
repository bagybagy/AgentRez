# User Task 02: Scene Setup & Prefab Creation

スクリプトの実装が完了しました。以下の手順でシーン内のオブジェクトを作成・設定してください。

## 1. プレハブの作成

### 1.1 Projectile Prefab (攻撃弾)
1. Hierarchy で `Sphere` (Small, Scale 0.2) を作成し、名前を `Projectile` とする。
2. `Projectile` コンポーネントをアタッチする。
3. `TrailRenderer` コンポーネントをアタッチする。
   - `Time` を 0.5 程度に設定。
   - `Width` を 0.1 -> 0.0 のカーブに設定すると見栄えが良い。
   - `Material` に適当な Default-Line 等をセットする。
4. Prefab 化して、シーン上のインスタンスは削除する。

### 1.2 Target Prefab (ターゲット)
1. Hierarchy で `Cube` (Scale 1.0) を作成し、名前を `Target` とする。
2. `Target` コンポーネントをアタッチする。
3. `Collider` がついていることを確認する。
4. Prefab 化して、シーン上のインスタンスは削除する。

### 1.3 Lock Marker Prefab (UI用)
1. Hierarchy で UI > Image を作成し、適当な赤い円形などのSpriteを割り当てる。
2. サイズを 50x50 程度にする。
3. 名前を `LockMarker` とする。
4. Prefab 化する。

---

## 2. シーンオブジェクトの構成

### 2.1 Managers
空の Game Object `Managers` を作成し、以下の子オブジェクトを作成・設定する。

1. **MusicManager**
   - オブジェクト名: `MusicManager`
   - コンポーネント: `MusicManager`, `AudioSource`
   - AudioSource に `Assets/AudioSouse/BGM.mp3` をセットする。
   - `MusicManager` コンポーネントの `BPM` に 120 (または BGM に合った値) をセットする。

2. **BeatManager**
   - オブジェクト名: `BeatManager`
   - コンポーネント: `BeatManager`
   - (BPMの設定は不要です。MusicManagerから上書きされます)

3. **StageManager**
   - オブジェクト名: `StageManager`
   - コンポーネント: `StageManager`

4. **HitSoundSynthesizer**
   - オブジェクト名: `HitSoundSynthesizer`
   - コンポーネント: `HitSoundSynthesizer`, `AudioSource`
   - AudioSource: Play On Awake OFF
   - Script の `Hit Clip` に `Assets/AudioSouse/Hit_SE.mp3` をセットする。

5. **ImpactScheduler**
   - オブジェクト名: `ImpactScheduler`
   - コンポーネント: `ImpactScheduler`

### 2.2 Environment
以前作成した `Background VFX` オブジェクトがあるはずです。
1. **SpaceManager**
   - コンポーネント: `SpaceManager` がついていることを確認。
   - `Background VFX` に VFX オブジェクトをセットする。

### 2.3 Targets
1. **TargetSpawner**
   - 空のオブジェクト `TargetSpawner` を作成。
   - コンポーネント: `TargetSpawner`
   - `Target Prefab` に手順 1.2 で作成した Prefab をセット。
   - `Context Menu` (コンポーネントの右クリック) から "Spawn Targets" を実行すると、エディタ上で配置テストが可能です。

### 2.4 Player (Main Camera)
シーンにある `Main Camera` オブジェクトを、プレイヤーとして扱います。分かりやすくするために名前を `Player` に変更しても構いませんが、ここでは `Main Camera` のままで説明します。

`Main Camera` オブジェクトを選択し、以下のコンポーネントを追加してください。

1. **基本コンポーネント**:
   - `PlayerController`
   - `LockOnSystem`:
     - `Lock Sound` に `Assets/AudioSouse/Target_SE.mp3` をセット。
   - `PlayerInput` (Input System):
     - `Actions` に `Assets/InputSystem_Actions` をセットする。
     - `Default Map` を `Player` にする。
   - `AudioSource` (LockOnSystem用) を追加。

2. **Combat**:
   - 引き続き `Main Camera` に `ProjectileManager` コンポーネントを追加してください。
   - `ProjectileManager` 設定:
     - `Lock On System`: 自分自身 (`Main Camera`) をドラッグ&ドロップ。
     - `Scheduler`: シーンの `ImpactScheduler` オブジェクトをドラッグ&ドロップ。
     - `Projectile Prefab`: 手順 1.1 で作成した Prefab をセット。
     - `Fire Point`: 
       1. `Main Camera` の子オブジェクトとして空の GameObject を作成し、名前を `FirePoint` とする。
       2. `FirePoint` の位置を、カメラの少し前方（例: Z = 0.5, Y = -0.2）に移動させる。
       3. この `FirePoint` オブジェクトを `Fire Point` プロパティにセットする。

### 2.5 UI
1. Canvas を作成 (Render Mode: Screen Space - Overlay)。
2. Canvas の子に空オブジェクト `LookUI` を作成し、画面中央に配置。
   - コンポーネント: `CrosshairUI`
   - `Lock On System`: Player を参照。
   - `Lock Marker Prefab`: 手順 1.3 の Prefab。
   - `Marker Container`: `LookUI` 自身またはその子オブジェクトを参照。
3. 中央に照準となる Image (Crosshair) を配置する。

---

## 3. 実行確認
Play ボタンを押して以下を確認してください。
- BGM が流れる
- マウス/スティックで視点が回る
- ターゲットにカーソルを合わせると LockMarker が出る (+音が鳴る)
- クリック/ボタン解放で Projectile が飛ぶ
- Projectile がリズムに合わせてヒットし、音が鳴る
