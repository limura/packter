* つかいかた

このプログラムは XNA Framework を利用して作られています。
ですので、以下にあげる三つのもの

* Microsoft .NET Framework 2.0 Service Pack 1 (x86)
* DirectX End-User Runtime
* Microsoft XNA Framework Redistributable 2.0

のインストールが必要かもしれません。
それらのランタイムがインストールされたWindowsであれば、
.exe ファイルをダブルクリックすることで起動します。
(/size 数字、という形で飛んでいくパケットの大きさ(標準値: 8)を変えることができます)

起動した後の操作は以下のような感じです。

Alt-F4: プログラムを終了します
カーソルキー: カメラの目線を動かします
"a" キー: ダミーのパケットを作成します
スペースキー: 内部情報の一部を表示します

"s"キー: 表示している時間を止めます。時間が止まっているときはトグルで時間を進めます。
"Backspace"キー: 5分前の表示に戻します
Shift+"Backspace"キー: 5分後の表示に進めます
"b"キー: 押し続けている間少し前の表示に戻し続けます
Shift+"b"キー: 押し続けている間もう少し前の表示に戻し続けます
"f"キー: 押し続けている間少し後の表示に戻し続けます
Shift+"f"キー: 押し続けている間もう少し後の表示に戻し続けます
"c"キー: 現在のパケットを表示するように時間調整を元に戻します。
パケットは最新の2048個まで記録され、それ以降のパケットは捨てられます

* 設定

標準で添付されている画像を、
自分で用意した好きな画像に置き換えて表示することもできます。
packter0.png, packter1.png ...
(0からの連続した packter[数字].png をメモリの許す限り) と、
packter_sender.png, packter_receiver.png というファイル名で、
.exe ファイルと同じディレクトリに設置することで、

packter[数字].png: 飛んでいくパケットの絵
packter_sender.png: "sender" と書かれている板
packter_receiver.png: "receiver" と書かれている板

を、それぞれ変えることができます。
また、png のアルファチャンネルが設定されている場合はそのように透けます。
(packter_sender.png と packter_receiver.png は最初から透けた状態になります)


* アンインストール

レジストリなどは何もいじらないので、作成されたフォルダを削除するだけでOKです。:D


* 外部からパケットを追加する

Packter_v4.exe は UDP の 11300 を開いてまちうけています。
そのポートに向けて、以下のフォーマットのデータを投げることで、
表示されるパケットを作成することができます。

---begin---
PACKTER\n
[start X position],[end X position],[start Y position],[end Y position],imageNumber\n
[start X position],[end X position],[start Y position],[end Y position],imageNumber\n
[start X position],[end X position],[start Y position],[end Y position],imageNumber\n
...
----end----

"start X position" と "end X position" は、以下のフォーマットを受け付けます。
IPv4 のアドレス表記、例: 127.0.0.1
  (これらはアドレスを 255.255.255.255 で割った値で、0.0 ～ 1.0 の値に正規化されます)
IPv6 のアドレス表記、例: fe80::1
  (これらのアドレスも 0.0 ～ 1.0 の値にに正規化されます)
0.0 ～ 1.0 の値、例: 0.5 
  (そのままの値が利用されます)

"start Y position" と "end Y position" は、以下のフォーマットを受け付けます。
0 ～ 65536 までの数字: 例、12345 (65536で割ることで 0.0 ～ 1.0 の値に正規化されます)
0.0 ～ 1.0 の値、例: 0.5 (そのままの値が利用されます)

X軸は、Senderの板の左から右に向かって、Y軸はSenderの板の下から上に向かって
それぞれ 0.0 から 1.0 の値を持ちます。

