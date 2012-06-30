HOW TO USE

The human whose name is written in this note shall die.

...oh, it's mistake.


* How to use

This program use XNA Framework.
Maybe, You need install XNA framework runtime.
It's needs are:
* Microsoft .NET Framework 2.0 Service Pack 1 (x86)
* DirectX End-User Runtime
* Microsoft XNA Framework Redistributable 2.0

If you installed these runtimes,
Just double click .exe file for run.
(You can use "/size #" option for change flying object size. (default 8))

Alt-F4: Exit this program.
Allow key: move camera position.
"a" key: Create dummy packet.
space key: Toggle internal information.

"s" key: toggle stop time.
"Backspace" key: viewing time back 5 minutes.
Shift+"Backspace" key: viewing time forward 5 minutes.
"b" key: viewing time back to littile second.
Shift+"b" key: viewing time back to some second.
"f" key: viewing time forward to little second.
Shift+"f" key: viewing time forward to some second.
"c" key: viewing time reset to now.
only newer 1024*1024 packets are memoryed.




* Configuration.

You can prepare packter0.png, packter1.png, packter2.png ... packter[number].png
and packter_sender.png, packter_receiver.png.
Thease texture file put on same .exe directory.
Thease texture can use Alpha channel.

packter[0-9].png: change flying object Texture.
packter_sender.png: change sender board Texture.
packter_receiver.png: change receiver board Texture.


* Uninstall

Just remove Packter_v4 directory only. :D


* How to add packet

Packter_v4.exe open UDP port 11300.

Packet format are:

---begin---
PACKTER\n
[start X position],[end X position],[start Y position],[end Y position],imageNumber\n
[start X position],[end X position],[start Y position],[end Y position],imageNumber\n
[start X position],[end X position],[start Y position],[end Y position],imageNumber\n
...
----end----

"start X position" and "end X position" can use these format.
IPv4 addressing: i.e. 127.0.0.1
  (convert to Address / 255.255.255.255)
IPv6 addressing: i.e. fe80::1
  (convert to Address / ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff:ff)
float 0 < x < 1.0 format: i.e. 0.5 (just use x)

"start Y position" and "end Y position" can use these format.
int 0 < x < 65536 format: i.e. 12345 (convert to x / 65536)
float 0 < x < 1 format: i.e. (just use x)

Have fun!
