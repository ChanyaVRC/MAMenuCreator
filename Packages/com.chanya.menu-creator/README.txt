## できること
1. アバター内の変更できる BlendShape の一覧を確認
2. アバターの BlendShape の変更
3. MAによるメニューの作成
4. AnimationClip の作成

## 前提条件
Modular Avatar (MA) がインストールされていること

## 使用方法
1. アバターに「Utils/Blend Shape Editor」コンポーネントをつける

### BlendShapes を編集する
1. 該当の BlendShape の値を変更する

### MA Menu を作成する (Toggle)
1. 該当の BlendShape の右の「MA」ボタンをクリック
2. クリックすると、アバターの Menu にメニューが追加される

#### 例
Avatar
 > Armature
 > Body
 > Clothes
 > Menu    <- 追加される (MA Menu Group)
  > Blink  <- 追加される (MA Menu Item)

### MA Menu を作成する (Radial Puppet)
1. 該当の BlendShape の名前を右クリック
2. 「Create menu for Radial Puppet」をクリック
3. クリックすると、アバターの Menu にメニューが追加される
※ AnimationClip などが生成されます。Git などでバージョン管理している場合は、コミット対象にしてください。

### AnimationClip を作成する
1. 該当の BlendShape の名前を右クリック
2. 「Create AnimationClip (*)」をクリック
3. クリックすると、AnimationClip が生成される

## 設計思想
### Ideal Menu on BlendShapeEditor
* アバター配下の Menu(GameObject) に集約される
* Toggle は ON 時に active、OFF 時に inactive になるべき
* Runtime のデフォルトと Editor 上の表示は一致すべき
* 同じ Menu Item を複数作成できるべき
 * ただし、同じ Menu Group に同じ機能の Menu Item を複数作成できるべきではない

### UI表示の統一
* 0 → 100 と 100 → 0 を両方できるようにする
* 0 → 100 は「→」で表し、100 → 0 は「←」で表す
* 視認性のため、(意味が極端に変わらない限り) 冠詞は使用しない
* 他のライブラリの語を指す場合は、表記を他のUIと同じにする
 * Object の class は UpperCamel で記載する

## ライセンス
MIT License (https://opensource.org/license/mit)

## 更新履歴
- v0.3 2025/08/24:
 - コンテキストメニューの追加・修正
  - 「Create menu for Toggle」と「Create menu for Radial Puppet」をそれぞれ 0 → 100 と 100 → 0 の両方を作成できるようにした
  - 上記に伴い、メニュー名を修正
 - 「Create AnimationClip」で生成される AnimationClip の命名規則を変更
  - アバター名が入らないようにした
 - Asset の作成時に、既に同じ名前の Asset が存在する場合は再作成をしないようにした
  - 既存のメニューが Missing になることを防ぐためである
 
- v0.2 2025/08/23:
 - コンテキストメニューの実装
  - 「Create menu for Toggle」を追加
  - 「Create menu for Radial Puppet」を追加
  - 「Create AnimationClip (Weight: 0)」を追加
  - 「Create AnimationClip (Weight: 100)」を追加
  - 「Create AnimationClip (Weight: Current Value)」を追加
 - 動作に関係ない部分を修正
- v0.1 2025/08/22: 初版リリース