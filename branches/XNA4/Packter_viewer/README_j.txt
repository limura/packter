* ��������

���̃v���O������ XNA Framework �𗘗p���č���Ă��܂��B
�ł��̂ŁA�ȉ��ɂ�����O�̂���

* Microsoft .NET Framework 2.0 Service Pack 1 (x86)
* DirectX End-User Runtime
* Microsoft XNA Framework Redistributable 2.0

�̃C���X�g�[�����K�v��������܂���B
�����̃����^�C�����C���X�g�[�����ꂽWindows�ł���΁A
.exe �t�@�C�����_�u���N���b�N���邱�ƂŋN�����܂��B
(/size �����A�Ƃ����`�Ŕ��ł����p�P�b�g�̑傫��(�W���l: 8)��ς��邱�Ƃ��ł��܂�)

�N��������̑���͈ȉ��̂悤�Ȋ����ł��B

Alt-F4: �v���O�������I�����܂�
�J�[�\���L�[: �J�����̖ڐ��𓮂����܂�
"a" �L�[: �_�~�[�̃p�P�b�g���쐬���܂�
�X�y�[�X�L�[: �������̈ꕔ��\�����܂�

"s"�L�[: �\�����Ă��鎞�Ԃ��~�߂܂��B���Ԃ��~�܂��Ă���Ƃ��̓g�O���Ŏ��Ԃ�i�߂܂��B
"Backspace"�L�[: 5���O�̕\���ɖ߂��܂�
Shift+"Backspace"�L�[: 5����̕\���ɐi�߂܂�
"b"�L�[: ���������Ă���ԏ����O�̕\���ɖ߂������܂�
Shift+"b"�L�[: ���������Ă���Ԃ��������O�̕\���ɖ߂������܂�
"f"�L�[: ���������Ă���ԏ�����̕\���ɖ߂������܂�
Shift+"f"�L�[: ���������Ă���Ԃ���������̕\���ɖ߂������܂�
"c"�L�[: ���݂̃p�P�b�g��\������悤�Ɏ��Ԓ��������ɖ߂��܂��B
�p�P�b�g�͍ŐV��2048�܂ŋL�^����A����ȍ~�̃p�P�b�g�͎̂Ă��܂�

* �ݒ�

�W���œY�t����Ă���摜���A
�����ŗp�ӂ����D���ȉ摜�ɒu�������ĕ\�����邱�Ƃ��ł��܂��B
packter0.png, packter1.png ...
(0����̘A������ packter[����].png ���������̋�������) �ƁA
packter_sender.png, packter_receiver.png �Ƃ����t�@�C�����ŁA
.exe �t�@�C���Ɠ����f�B���N�g���ɐݒu���邱�ƂŁA

packter[����].png: ���ł����p�P�b�g�̊G
packter_sender.png: "sender" �Ə�����Ă����
packter_receiver.png: "receiver" �Ə�����Ă����

���A���ꂼ��ς��邱�Ƃ��ł��܂��B
�܂��Apng �̃A���t�@�`�����l�����ݒ肳��Ă���ꍇ�͂��̂悤�ɓ����܂��B
(packter_sender.png �� packter_receiver.png �͍ŏ����瓧������ԂɂȂ�܂�)


* �A���C���X�g�[��

���W�X�g���Ȃǂ͉���������Ȃ��̂ŁA�쐬���ꂽ�t�H���_���폜���邾����OK�ł��B:D


* �O������p�P�b�g��ǉ�����

Packter_v4.exe �� UDP �� 11300 ���J���Ă܂������Ă��܂��B
���̃|�[�g�Ɍ����āA�ȉ��̃t�H�[�}�b�g�̃f�[�^�𓊂��邱�ƂŁA
�\�������p�P�b�g���쐬���邱�Ƃ��ł��܂��B

---begin---
PACKTER\n
[start X position],[end X position],[start Y position],[end Y position],imageNumber\n
[start X position],[end X position],[start Y position],[end Y position],imageNumber\n
[start X position],[end X position],[start Y position],[end Y position],imageNumber\n
...
----end----

"start X position" �� "end X position" �́A�ȉ��̃t�H�[�}�b�g���󂯕t���܂��B
IPv4 �̃A�h���X�\�L�A��: 127.0.0.1
  (�����̓A�h���X�� 255.255.255.255 �Ŋ������l�ŁA0.0 �` 1.0 �̒l�ɐ��K������܂�)
IPv6 �̃A�h���X�\�L�A��: fe80::1
  (�����̃A�h���X�� 0.0 �` 1.0 �̒l�ɂɐ��K������܂�)
0.0 �` 1.0 �̒l�A��: 0.5 
  (���̂܂܂̒l�����p����܂�)

"start Y position" �� "end Y position" �́A�ȉ��̃t�H�[�}�b�g���󂯕t���܂��B
0 �` 65536 �܂ł̐���: ��A12345 (65536�Ŋ��邱�Ƃ� 0.0 �` 1.0 �̒l�ɐ��K������܂�)
0.0 �` 1.0 �̒l�A��: 0.5 (���̂܂܂̒l�����p����܂�)

X���́ASender�̔̍�����E�Ɍ������āAY����Sender�̔̉������Ɍ�������
���ꂼ�� 0.0 ���� 1.0 �̒l�������܂��B

