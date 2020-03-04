# IntelligentCopyAndPaste

## 概要
UnityEditor上で、インスペクタでの主に配列やListへ設定の補助をするエディタ拡張です。

## 導入方法
IntelligentCopyAndPasteフォルダをそのままプロジェクトに追加してください。
または、IntelligentCopyAndPaste.unitypackage をインポートしてください。

## 使い方
### コピー
コピーしたいAssetsまたはGameObjectを選択した状態でメニューバーから `Assets-Copy Assets Paths` を選択する事で、情報がコピーがされ**Copy元管理ウインドウ**が開きます。
※右クリックメニューから`Copy Assets Paths` を選ぶ事も出来ます。
※メニューバーから`Assets-AppendCopy Assets Paths` または 右クリックメニューから`AppendCopy Assets Paths` を選択することで**追加コピー**になります。

![image](https://user-images.githubusercontent.com/2524278/75860497-42822f00-5e3f-11ea-8e2e-c5bf9deeca01.png)

### ペースト
コピーされた状態で、Inspectorで配列などを設定したいGameObjectを選択し、メニューバーから `Assets-Paste Assets Paths` を選択することで、**Paste先確認ウインドウ**が開きます。
※右クリックメニューから`Paste Assets Paths` を選ぶ事も出来ます。
※メニューバーから`Assets-AppendPaste Assets Paths` または 右クリックメニューから`AppendPaste Assets Paths` を選択することで **追加ペースト(もともと設定されているデータの後ろに追加)** になります。

## Copy元管理ウインドウ

![image](https://user-images.githubusercontent.com/2524278/75859270-09e15600-5e3d-11ea-9dc7-73b792b1d2ca.png)

選択をしてコピーをすると

![image](https://user-images.githubusercontent.com/2524278/75859367-39905e00-5e3d-11ea-9a4e-3dcd9e17eea0.png)

Copy元管理ウインドウが表示されます。

## Paste先確認ウインドウ
例として、このようなScriptを書きます。
```cs
using UnityEngine;
using UnityEngine.UI;

public class ImageClick : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Sprite[] spritesArray;
    [SerializeField] private Button button;

    private int spriteIndex;

    void Start()
    {
        spriteIndex = 0;
        button.onClick.AddListener(() =>
        {
            image.sprite = spritesArray[spriteIndex];
            spriteIndex = (spriteIndex + 1) % spritesArray.Length;
        });
    }
}
```

これは**ボタンを押すたびに、インスペクタで指定したSprite配列を一つずつ表示**するScriptになります。
インスペクタ上ではこのような表示になります。

![image](https://user-images.githubusercontent.com/2524278/75859931-4ceff900-5e3e-11ea-80f1-23bf119748c1.png)

この **ImageClickスクリプトがアタッチされたGameObject** を選択した状態でペースト(`Assets-Paste Assets Paths`)を行うと

![image](https://user-images.githubusercontent.com/2524278/75859991-698c3100-5e3e-11ea-90e9-1584bbc31438.png)

このように、Paste先確認ウインドウが表示され、}**ペースト先の候補と、それに対するコピー元の候補の組み合わせが（可能な限り）全て洗い出され**表示されます。

（今回の例だと、コピーされているのは複数の`Sprite`なので、`Sprite`が設定できる可能性があるものが全て表示されることになり、`Imageコンポーネント` のSprite(配列、Listではない）と、`ImageClickスクリプト` の Sprite配列 の2つが候補として表示されています。

実際にペーストをしたい対象を選択し、"OK"を押すことで、実際にペーストされます。

※なおIsOverride にチェックが入っている場合は上書き、チェックが外れている場合は追加(Ctrl+Shift+C同等)になります。

## ショートカット
|  ショートカット | 機能 |
| --- | --- |
|  Ctrl+c | Copy Assets Paths |
|  Ctrl+Shift+c | AppendCopy Assets Paths |
|  Ctrl+v | Paste Assets Paths |
|  Ctrl+Shift+v | AppendPaste Assets Paths |

## 注意
既存の Ctrl-c,Ctrl-Shift-c および Ctrl-v,Ctrl-Shift-v のショートカットが使用できなくなります。
何かと競合してしまってどうしても修正したい場合は `\IntelligentCopyAndPaste\Editor\ArrayCopyPasteComponent.cs` にショートカットの登録が`MenuItem` **Attribute** にて指定されてますので、変更する手もあります（責任は負えません）
