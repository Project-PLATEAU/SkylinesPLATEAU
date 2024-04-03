## 本MODのビルド方法

-------

### 1. ビルドに必要なツール

Microsoft Visual Studio（以下「Visual Studio」という。開発時はVisual Studio Professional 2022を使用します。）

### 2. ビルド手順

① ソースファイルをダウンロードし、展開します。

&emsp;ソースファイルは[こちら](https://github.com/Project-PLATEAU/SkylinesPLATEAU)からダウンロード可能です。

<br>

② SkylinesPLATEAU-mainフォルダに保存されているソリューションファイル（SkylinesPlateau.sln）をVisual Studioで開きます。

![](../resources/userMan/2024-01-29_22h14_18.jpg)

<br>

③ソリューション構成を【Release】に、ソリューションプラットフォームを【Any CPU】に設定します。

![](../resources/userMan/2024-01-29_22h15_28.jpg)

<br>

④ 「ビルド」メニュー＞「ソリューションのビルド」を選択し、ソリューション全体をビルドします。

![](../resources/userMan/2024-01-29_22h15_38.jpg)

<br>

⑤ ビルドが正常に終了すると、SkylinesPlateau\bin\\ReleaseフォルダにMODファイルが生成されます。（本MODの動作に必要なライブラリも同じフォルダにコピーされます。）