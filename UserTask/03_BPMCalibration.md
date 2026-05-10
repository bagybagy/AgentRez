# User Task 03: BPM Calibration & Music Data Setup

BPM解析とMusicData作成のためのツールを実装しました。
以下の手順で、楽曲のBPMを確定させ、ゲームに設定してください。

## 1. Calibration シーンの作成とセットアップ

1. Unityで新しいシーンを作成し、`BPMCalibration` という名前で保存する。
2. シーンに `Canvas` を作成する。
3. `Canvas` の中に空オブジェクト `Tool` を作成し、`BPMCalibrationTool` コンポーネントをアタッチする。
4. `Tool` コンポーネントのプロパティに、必要なUI要素をアタッチする。
   - UI要素がない場合は、簡単に作成してください:
     - **Status Text** (Text): 状態表示用
     - **Analyze Button** (Button): 解析開始用
     - **BPM Dropdown** (Dropdown): 候補選択用
     - **Play Button** (Button): 試聴用
     - **Save Button** (Button): 保存用
     - **Metronome Visual** (Image): ビート確認用のパネルなど
     - **Preview Source** (AudioSource): `Tool` オブジェクトに `AudioSource` を追加してアタッチ。

5. **Target Clip** に `Assets/AudioSouse/BGM.mp3` をセットする。

## 2. 解析と保存

1. シーンを再生する (Play Mode)。
2. **Analyze Button** を押す。
   - `BPM Dropdown` に候補が表示されます。
3. **Play Button** を押して試聴する。
   - 曲のリズムに合わせて `Metronome Visual` が点滅するか確認する。
   - ズレている場合は Dropdown で別の候補を選ぶ。
4. 最も適切な BPM を選んだ状態で **Save Button** を押す。
   - `Assets/Scripts/Audio/` フォルダの下に `MusicData_BGM.asset` が生成される。
5. Play Mode を終了する。

## 3. シーンへの適用

1. メインのゲームシーン (`SampleScene`) に戻る。
2. Hierarchy の `Managers/MusicManager` を選択。
3. `Music Manager` コンポーネントの `Music Data` プロパティに、先ほど生成した `MusicData_BGM.asset` をドラッグ&ドロップする。
4. ゲームを再生し、BPMが正しく適用されているか確認する。
