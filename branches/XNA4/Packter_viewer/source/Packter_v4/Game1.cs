// ������ɂȂ����Ƃ���
// http://sorceryforce.com/xna/

/*
 * XNA ������Ă��ꂽ�Q�[���̖{�́B
 * 
 * �������Ńp�P�b�g�p�̃e�N�X�`���C���[�W��ǂݍ��ނ��́A
 * packter sender ����̃f�[�^��ǂݍ��ނ��̂Ȃ񂾂̂Ƃ������A
 * ������ main���� �̂��Ƃ����
 */

using System;
using System.Collections.Generic;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;
using System.Net;
using System.Net.Sockets;


namespace Packter_viewer
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        const int MaxPacketNum = 1024*1024; // 1024*1024�܂Ńp�P�b�g�͊o����
        const int flyMillisecond = 3000; // �p�P�b�g�����ł����̂ɂ����鎞��
        
        double addMilliseconds = 0.0; // �p�P�b�g�\�����鎞�ɂ��炳��鎞��
        
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;

        Board senderBoard = null;
        Board receiverBoard = null;
        //ArrayList packetList = new ArrayList();
        RingBuffer<PacketBoard> packetList = new RingBuffer<PacketBoard>(MaxPacketNum);
        // ArrayList drawPacketList = new ArrayList();
        
        Model packetModel;

        Vector3 cameraPosition = new Vector3(-128, 250, 480);
        Vector3 cameraTarget = new Vector3(0, 0, 0);
        Vector3 cameraRotation = new Vector3(0, 0, 0);
        MouseState mouseState;
        KeyboardState keyboardState;

        PacketReader packetReader_v4 = null;
        PacketReader packetReader_v6 = null;

        ArrayList packetImages = new ArrayList();
        string[] args = null;
        float defaultScale = 8;

        bool statusDraw = false;

        int defaultWidth = 0;
        int defaultHeight = 0;

        Texture2D v4Texture = null;
        
        Board myship = null;
        Board myship_wake = null;
        bool showMyShip = false;
        bool disableShowV4 = false;

        /// <summary>
        /// �����~�܂����Ƃ��̎���
        /// </summary>
        GameTime stopedGameTime = null;

        PacketBoard hilightPacketBoard = null;

        public Game1(string[] Args)
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            args = Args;
        }

        public void writeString(string[] Args)
        {
            return;
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        /// <summary>
        /// FlyPacket ����A���ۂɔ��ł��� PacketBoard ���쐬���A�\��queue�ɒǉ����܂��B
        /// FlyPacket �ł� 0.0�`1.0 �܂łŋL�^����Ă���p�P�b�g���W�����A
        /// ��ʂ̍��W���ɕϊ����� PacketBoard �ɓo�^���Ă��܂��B
        /// </summary>
        /// <param name="packet">���ł����p�P�b�g</param>
        void AddPacketBoard(FlyPacket packet)
        {
            float srcX = packet.SrcAddress;
            float srcY = packet.SrcPort;
            float dstX = packet.DstAddress;
            float dstY = packet.DstPort;
            byte imageNumber = packet.PacketImageNumber;
            GameTime nowGameTime = packet.CreatedGameTime;
            
            Vector3 startPoint = new Vector3((srcX - 0.5f) * senderBoard.Scale * 2 - defaultScale / 4.0f,
                ((srcY - 0.5f) * senderBoard.Scale * 2) - defaultScale / 4.0f, senderBoard.Position.Z);
            Vector3 endPoint = new Vector3((dstX - 0.5f) * receiverBoard.Scale * 2 - defaultScale / 4.0f,
                ((dstY - 0.5f) * receiverBoard.Scale * 2) - defaultScale / 4.0f, receiverBoard.Position.Z);
            
            //System.Diagnostics.Debug.WriteLine("new Packet: " + srcIP + ", " + srcPort + " -> " + endPoint.X + ", " + endPoint.Y + " end: " + endPoint.X + ", " + endPoint.Y);

            PacketBoard pb = new PacketBoard(packetModel, (Texture2D)packetImages[imageNumber % packetImages.Count]
                , startPoint, endPoint, nowGameTime, flyMillisecond, packet.OriginalString);
            pb.Scale = defaultScale;
            packetList.Add(pb);
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            this.IsMouseVisible = true;
            defaultWidth = GraphicsDevice.DisplayMode.Width;
            defaultHeight = GraphicsDevice.DisplayMode.Height;
            if (args != null && args.Length >= 2)
            {
                float f;
                if (args[0] == "/size" && float.TryParse(args[1], out f))
                {
                    if (f > 0)
                        defaultScale = f;
                }
            }

            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("font");
            v4Texture = Content.Load<Texture2D>("v4");

            packetModel = Content.Load<Model>("boad");

            senderBoard = new Board(Content.Load<Model>("boad"));
            senderBoard.Position = new Vector3(0, 0, -150);
            senderBoard.Scale = 100;
            senderBoard.RotationY = (float)Math.PI;
            senderBoard.Alpha = 0.3f;
            receiverBoard = new Board(Content.Load<Model>("boad"));
            receiverBoard.Position = new Vector3(0, 0, 150);
            receiverBoard.RotationY = (float)Math.PI;
            receiverBoard.Scale = 100;
            receiverBoard.Alpha = 0.3f;

            myship = new Board(Content.Load<Model>("myship"));
            myship.Position = new Vector3(0, 0, 0);
            myship.Scale = 20;
            myship.RotationY = (float)Math.PI;
            myship_wake = new Board(Content.Load<Model>("boad"));
            myship_wake.Position = new Vector3(0, 0, myship.Scale);
            myship_wake.Scale = 6;
            myship_wake.Texture = Content.Load<Texture2D>("Belt_of_light");
            myship_wake.RotationY = (float)Math.PI / -2;

            {
                Texture2D t = null;
                try
                {
                    t = Texture2D.FromFile(GraphicsDevice, "packter_sender.png");
                }
                catch
                {
                    t = Content.Load<Texture2D>("packter_sender");
                }
                senderBoard.Texture = t;
                try
                {
                    t = Texture2D.FromFile(GraphicsDevice, "packter_receiver.png");
                }
                catch
                {
                    t = Content.Load<Texture2D>("packter_receiver");
                }
                receiverBoard.Texture = t;
            }

            float aspectRaito = (float)GraphicsDevice.Viewport.Width / GraphicsDevice.Viewport.Height;

            /// packter[n].png �Ƃ����t�@�C����ǂݍ���ŁA�e�N�X�`���Ƃ��ēo�^����
            {
                int i = 0;
                while(true)
                {
                    Texture2D t = null;
                    try
                    {
                        t = Texture2D.FromFile(GraphicsDevice, "packter" + i + ".png");
                    }
                    catch
                    {
                        try
                        {
                            t = Content.Load<Texture2D>("packter" + i);
                        }
                        catch { break; }
                    }
                    if (t != null)
                        packetImages.Add(t);
                    else
                        break;

                    i++;
                }
            }

            try
            {
                packetReader_v6 = new PacketReader(new IPEndPoint(IPAddress.IPv6Any, 11300));
            }
            catch
            {
                packetReader_v6 = null;
            }
            try
            {
                packetReader_v4 = new PacketReader(new IPEndPoint(IPAddress.Any, 11300));
            }
            catch
            {
                packetReader_v4 = null;
            }
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            if(packetReader_v4 != null)
                packetReader_v4.Dispose();
            if(packetReader_v6 != null)
                packetReader_v6.Dispose();
        }

        /// <summary>
        /// ���A���̏u�Ԃ� key �Ŏw�����ꂽ�L�[�������ꂽ�̂��ǂ����𔻒肷��
        /// </summary>
        /// <param name="key">�����ꂽ���ǂ����𔻒肷��L�[</param>
        /// <returns>������Ă��̂Ȃ� true</returns>
        private bool JustNowKeyDown(KeyboardState nowKeyState, Keys key)
        {
            return nowKeyState.IsKeyDown(key) == true && keyboardState.IsKeyDown(key) == false;
        }

        UInt32 n = 0;

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// ����Update()������I�ɌĂ΂��̂ŁA����method���ŃQ�[�����Ԃ�i�߂�B
        /// packter sender ����̓��̓f�[�^��ǂݍ���ł���̂������B
        /// �\������̂� Draw()�B
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            
            // TODO: Add your update logic here
            MouseState nowMouseState = Mouse.GetState();
            KeyboardState nowKeyState = Keyboard.GetState();

            if ((JustNowKeyDown(nowKeyState, Keys.Enter)
                && (nowKeyState.IsKeyDown(Keys.LeftAlt) == true || nowKeyState.IsKeyDown(Keys.RightAlt) == true)))
            {
                if (graphics.IsFullScreen == true)
                {
                    graphics.PreferredBackBufferHeight = defaultHeight;
                    graphics.PreferredBackBufferWidth = defaultWidth;
                    graphics.ToggleFullScreen();
                }
                else
                {
                    int width = 0;
                    int height = 0;
                    foreach (DisplayMode mode in GraphicsAdapter.DefaultAdapter.SupportedDisplayModes)
                    {
                        if (mode.Height > height)
                            height = mode.Height;
                        if (mode.Width > width)
                            width = mode.Width;
                    }
                    graphics.PreferredBackBufferHeight = height;
                    graphics.PreferredBackBufferWidth = width;
                    graphics.ToggleFullScreen();
                }
            }

            if (JustNowKeyDown(nowKeyState, Keys.Space))
            {
                if (statusDraw == true)
                    statusDraw = false;
                else
                    statusDraw = true;
            }

            if (nowKeyState.IsKeyDown(Keys.T) && nowKeyState.IsKeyDown(Keys.S) && nowKeyState.IsKeyDown(Keys.E))
                showMyShip = true;
            else
                showMyShip = false;
            if (nowKeyState.IsKeyDown(Keys.V) && nowKeyState.IsKeyDown(Keys.NumPad4))
                disableShowV4 = true;
            else
                disableShowV4 = false;
            
            float xDelta = 0;
            float yDelta = 0;
#if false
            if (nowMouseState.LeftButton == ButtonState.Pressed)
            {
                float size = Math.Max(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
                xDelta = (float)(mouseState.X - nowMouseState.X) / size;
                yDelta = (float)(mouseState.Y - nowMouseState.Y) / size;

                //cameraPosition *= Matrix.CreateRotationZ(xDelta);
                //cameraPosition *= Matrix.CreateRotationX(yDelta);
            }
            else
#endif
            if (nowKeyState.IsKeyDown(Keys.Left))
            {
                xDelta = 0.08f;
            }else if(nowKeyState.IsKeyDown(Keys.Right))
            {
                xDelta = -0.08f;
            }
            else if(nowKeyState.IsKeyDown(Keys.Up))
            {
                yDelta = -0.05f;
            }
            else if (nowKeyState.IsKeyDown(Keys.Down))
            {
                yDelta = 0.05f;
            }
            //cameraRotation.Z = (cameraRotation.Z + xDelta) % (float)(2 * Math.PI);
#if true
            cameraPosition = Vector3.Transform(cameraPosition, Matrix.CreateFromYawPitchRoll(xDelta, 0, 0));
            Vector3 qv3 = cameraPosition;
            qv3.Y = 0;
            qv3 = Vector3.Transform(qv3, Matrix.CreateFromYawPitchRoll((float)(Math.PI / 2), 0, 0));
            qv3.Normalize();
            Quaternion qc = new Quaternion(cameraPosition.X, cameraPosition.Y, cameraPosition.Z, 0);
            Quaternion q = Quaternion.CreateFromAxisAngle(qv3, yDelta);
            Quaternion r = Quaternion.CreateFromAxisAngle(-qv3, yDelta);
            Quaternion ans = r * qc * q;
            cameraPosition.X = ans.X;
            cameraPosition.Y = ans.Y;
            cameraPosition.Z = ans.Z;
#else
            cameraPosition = Vector3.Transform(cameraPosition, Matrix.CreateFromYawPitchRoll(xDelta, 0, 0));
#endif
            //System.Diagnostics.Debug.WriteLine("cameraPosition " + cameraPosition.X + ", " + cameraPosition.Y);

            mouseState = nowMouseState;

            n++;
            if(JustNowKeyDown(nowKeyState, Keys.A))
            {
                FlyPacket packet = new FlyPacket((float)((n & 0xff) * 0x10000000) / 4294967296.0f
                    , (float)((n & 0xff) * 0x100) / 65536.0f, 0.5f, 0.5f, (byte)(n / 10), "Test Packet", gameTime);
                AddPacketBoard(packet);
            }

            if (JustNowKeyDown(nowKeyState, Keys.Back))
            {
                if (nowKeyState.IsKeyDown(Keys.RightShift) == true || nowKeyState.IsKeyDown(Keys.LeftShift))
                {
                    addMilliseconds += 1000.0 * 60 * 5; // 5����ɐi�߂�
                }
                else
                {
                    addMilliseconds -= 1000.0 * 60 * 5; // 5���O�ɖ߂�
                }
            }
            if (nowKeyState.IsKeyDown(Keys.B))
                if (nowKeyState.IsKeyDown(Keys.LeftShift) || nowKeyState.IsKeyDown(Keys.RightShift))
                    addMilliseconds -= flyMillisecond / 3; // �����O�ɖ߂�
                else
                    addMilliseconds -= flyMillisecond / 30; // �����O�ɖ߂�
            if (nowKeyState.IsKeyDown(Keys.F))
                if (nowKeyState.IsKeyDown(Keys.LeftShift) || nowKeyState.IsKeyDown(Keys.RightShift))
                    addMilliseconds += flyMillisecond / 3; // ������ɐi�߂�
                else
                    addMilliseconds += flyMillisecond / 30; // ������ɐi�߂�
            if (JustNowKeyDown(nowKeyState, Keys.C))
                addMilliseconds = 0.0; // C �L�[�������ꂽ�猳�ɖ߂�
            if (JustNowKeyDown(nowKeyState, Keys.T))
            {
                // TraceBack �J�n
                if (hilightPacketBoard != null)
                {
                    // Traceback �T�[�o�[�擾
                    char[] delimiter = { '-' };
                    string[] words = hilightPacketBoard.Description.Split('-');
                    if (words.Length == 2)
                    {
                        System.Text.Encoding enc = System.Text.Encoding.UTF8;
                        UdpClient udp = new UdpClient();
                        byte[] traceback = enc.GetBytes(words[0]);
                        udp.Send(traceback, traceback.Length, words[1], 11301);
                    }
                }
            }
            if (JustNowKeyDown(nowKeyState, Keys.S))
            {
                // S �L�[�������ꂽ�玞���~�߂�̂��g�O������
                if (stopedGameTime == null)
                {
                    stopedGameTime = gameTime;
                }
                else
                {
                    stopedGameTime = null;
                }
            }
            if (stopedGameTime != null)
            {
                addMilliseconds -= gameTime.ElapsedGameTime.TotalMilliseconds;
            }

            // �L�[�{�[�h�̏�Ԃ�����ꍇ�͂��̍s���O�Ō��Ȃ��Ƒʖ�
            keyboardState = nowKeyState;
            /// packter sender ����p�P�b�g��ǂݍ���
            ReadPackets(gameTime);
            
#if false
            /// PacketBoard �̃��X�g���Q�[�����ԂōX�V���A
            /// �\���������̂����� drawPacketList �ɓ����B
            drawPacketList.Clear();
            bool drawEnabled = false;
            foreach (PacketBoard pb in packetList)
            {
                if (pb.Update(gameTime.TotalGameTime.TotalMilliseconds + addMilliseconds) == true)
                {
                    drawPacketList.Add(pb);
                    drawEnabled = true;
                }
                else if (drawEnabled == true)
                {
                    break; // ��x�\�������͈͂̂��̂��o�Ă�����A�\������Ȃ��͈͂̂��̂��o�Ă����Ƃ������Ƃ́A�\�������͈͂͏I�����
                }
            }
            /// �����Ȃ肷�����p�P�b�g�͎̂Ă�
            if (packetList.Count > MaxPacketNum)
            {
                packetList.RemoveRange(0, packetList.Count - MaxPacketNum);
            }
#endif
            /// �}�E�X�̏���ǂݍ���ŁAPacketBoard ��I������Ă��邩�ǂ����𔻒肷��B
            UpdateForMouse(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// Update���Ƀ}�E�X�̏���ǂݍ���łȂɂ�炷�镔��
        /// </summary>
        private void UpdateForMouse(GameTime gameTime)
        {
            double nowTime = gameTime.TotalGameTime.TotalMilliseconds + addMilliseconds;

            // http://msdn.microsoft.com/ja-jp/library/bb203905.aspx
            // �̃R�s�y�B:P
            MouseState mouseState = Mouse.GetState();

            Vector3 nearSource = new Vector3((float)mouseState.X, (float)mouseState.Y, 0.0f);
            Vector3 farSource =  new Vector3((float)mouseState.X, (float)mouseState.Y, 1.0f);

            Matrix view = Matrix.CreateLookAt(
                    this.cameraPosition,
                    this.cameraTarget,
                    Vector3.Transform(Vector3.Up,
                                      Matrix.CreateRotationZ(this.cameraRotation.Z))
                );
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.PiOver4,
                    (float)this.GraphicsDevice.Viewport.Width /
                        (float)this.GraphicsDevice.Viewport.Height,
                    1.0f,
                    1000.0f
                );
            Matrix world = Matrix.CreateTranslation(0, 0, 0);
            Vector3 nearPoint = graphics.GraphicsDevice.Viewport.Unproject(nearSource, projection, view, world);
            Vector3 farPoint =  graphics.GraphicsDevice.Viewport.Unproject(farSource,  projection, view, world);

            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();
            Ray pickRay = new Ray(nearPoint, direction);

            Nullable<float> nearest = null;
            PacketBoard nearPacketBoard = null;
            int first = BinarySearchPacketBoard(gameTime);
            for (int i = 0; i < packetList.Length; i++)
            {
                PacketBoard pb = packetList[i + first];
                if (pb != null)
                {
                    if (pb.Update(nowTime) != 0)
                    {
                        break; // �\�����ׂ����̂��Ȃ��Ȃ���
                    }
                    else
                    {
                        Nullable<float> result = pickRay.Intersects(pb.BoundingSphere);
                        if (result != null)
                        {
                            if (nearest == null || nearest > result)
                            {
                                nearest = result;
                                nearPacketBoard = pb;
                            }
                        }
                    }
                }
                else
                {
                    break;
                }
            }
            if (nearPacketBoard != null)
            {
                hilightPacketBoard = nearPacketBoard;
            }
            else
            {
                hilightPacketBoard = null;
            }
        }

        /// packter sender ���瑗�M���ꂽ�p�P�b�g�̃��X�g��ǂݍ���ł���Ƃ���B
        private void ReadPackets(GameTime gameTime)
        {
            FlyPacket[] newPackets = null;
            if (packetReader_v4 != null)
            {
                newPackets = packetReader_v4.GetAvailablePackets();
                if (newPackets != null)
                {
                    foreach (FlyPacket packet in newPackets)
                    {
                        packet.CreatedGameTime = gameTime;
                        AddPacketBoard(packet);
                    }
                }
            }
            if (packetReader_v6 != null)
            {
                newPackets = packetReader_v6.GetAvailablePackets();
                if (newPackets != null)
                {
                    foreach (FlyPacket packet in newPackets)
                    {
                        packet.CreatedGameTime = gameTime;
                        AddPacketBoard(packet);
                    }
                }
            }
        }

        private int BinarySearchPacketBoard(GameTime gameTime)
        {
            double nowTime = gameTime.TotalGameTime.TotalMilliseconds + addMilliseconds;

            // �\�������ׂ����̂̂�����ԌÂ��̂��̂� index ��T��
            int div = packetList.Length / 2;
            int nowChecking = div - 1;
            while (div > 0)
            {
                PacketBoard pb = packetList[nowChecking];
                div /= 2;
                if (pb != null)
                {
                    int ret = pb.Update(nowTime);
                    if (ret > 0)
                    {
                        // �\������鎞�Ԃ��炷��Ɗ��ɒʂ�߂��Ă��܂�������
                        nowChecking += div;
                    }
                    else if (ret < 0)
                    {
                        // �\������鎞�Ԃ��炷��ƁA�܂��\���̈�ɓ����Ă��Ȃ�����
                        nowChecking -= div;
                    }
                    else
                    {
                        // �\�������ׂ�����
                        // ��O���\�������ׂ��łȂ����̂ł���΁A��������\������΂悢
                        if (div > 0)
                        {
                            pb = packetList[nowChecking - 1];
                            if (pb != null && pb.Update(nowTime) > 0)
                            {
                                div = 0;
                            }
                        }
                        if (div == 0)
                        {
                            // ���� nowChecking ���ŏ��̂��́B
                            return nowChecking;
                        }
                        // �܂��ŏ��̂��̂��݂��Ă��Ȃ���������Ȃ�
                        nowChecking -= div;
                    }
                }
                else
                {
                    nowChecking -= div;
                }
            }
            return 0;
        }

        /// <summary>
        /// packetList �ɕۑ�����Ă���p�P�b�g�̒�����\�����ׂ��p�P�b�g��񕪒T���Ō������A�\������
        /// </summary>
        /// <param name="gameTime">Draw() �ɓn���ꂽ GameTime</param>
        /// <param name="view">�\���Ɏg���r���[�s��</param>
        /// <param name="viewport">�\���Ɏg���r���[�|�[�g</param>
        /// <param name="projection">�\���Ɏg�������s��</param>
        private void DrawPacketBoards(GameTime gameTime, Matrix view, Viewport viewport, Matrix projection)
        {
            double nowTime = gameTime.TotalGameTime.TotalMilliseconds + addMilliseconds;
#if false
            // �\�������ׂ����̂̂�����ԌÂ��̂��̂� index ��T��
            int div = packetList.Length / 2;
            int nowChecking = div - 1;
            while (div > 0)
            {
                PacketBoard pb = packetList[nowChecking];
                div /= 2;
                if (pb != null)
                {
                    int ret = pb.Update(nowTime);
                    if (ret > 0)
                    {
                        // �\������鎞�Ԃ��炷��Ɗ��ɒʂ�߂��Ă��܂�������
                        nowChecking += div;
                    }
                    else if (ret < 0)
                    {
                        // �\������鎞�Ԃ��炷��ƁA�܂��\���̈�ɓ����Ă��Ȃ�����
                        nowChecking -= div;
                    }
                    else
                    {
                        // �\�������ׂ�����
                        // ��O���\�������ׂ��łȂ����̂ł���΁A��������\������΂悢
                        if (div > 0)
                        {
                            pb = packetList[nowChecking - 1];
                            if (pb != null && pb.Update(nowTime) > 0)
                            {
                                div = 0;
                            }
                        }
                        if (div == 0)
                        {
                            // ���� nowChecking ���ŏ��̂��́B
                            for (int i = 0; i < packetList.Length; i++)
                            {
                                pb = packetList[i + nowChecking];
                                if (pb == null || pb.Update(nowTime) != 0)
                                    return; // �\�����ׂ����̂��Ȃ��Ȃ���
                                pb.Draw(view, viewport, projection);
                            }
                            return;
                        }
                        // �܂��ŏ��̂��̂��݂��Ă��Ȃ���������Ȃ�
                        nowChecking -= div;
                    }
                }
                else
                {
                    nowChecking -= div;
                }
                System.Diagnostics.Debug.WriteLine("nowChecking: " + nowChecking);
            }
#else
            int first = BinarySearchPacketBoard(gameTime);
            for (int i = 0; i < packetList.Length; i++)
            {
                PacketBoard pb = packetList[i + first];
                if (pb == null || pb.Update(nowTime) != 0)
                    return; // �\�����ׂ����̂��Ȃ��Ȃ���
                pb.Draw(view, viewport, projection);
            }
#endif
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// ���̂Ƃ��̃Q�[�����Ԃ̂��̂�\������B
        /// �Ԃ����Ⴏ�����̉�
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);
            //graphics.GraphicsDevice.Clear(Color.Bisque);

            // �[�x�o�b�t�@��L���ɂ���
            this.GraphicsDevice.RenderState.DepthBufferEnable = true;

            // �r���[�}�g���b�N�X�쐬
            Matrix view = Matrix.CreateLookAt(
                    this.cameraPosition,
                    this.cameraTarget,
                    Vector3.Transform(Vector3.Up,
                                      Matrix.CreateRotationZ(this.cameraRotation.Z))
                );

            // �v���W�F�N�V�����}�g���b�N�X�쐬
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(
                    MathHelper.PiOver4,
                    (float)this.GraphicsDevice.Viewport.Width /
                        (float)this.GraphicsDevice.Viewport.Height,
                    1.0f,
                    1000.0f
                );

            Matrix transform = Matrix.Identity;
            transform *= Matrix.CreateScale(1.0f);
            transform *= Matrix.CreateRotationY(0.0f);
            transform *= Matrix.CreateTranslation(new Vector3(2.0f, 0.0f, 0.0f));


            if (showMyShip)
            {
                myship.Draw(view, GraphicsDevice.Viewport, projection);
                myship_wake.Draw(view, GraphicsDevice.Viewport, projection);
            }
            if (cameraPosition.Z > 0)
            {
                senderBoard.Draw(view, GraphicsDevice.Viewport, projection);
                DrawPacketBoards(gameTime, view, GraphicsDevice.Viewport, projection);
                receiverBoard.Draw(view, GraphicsDevice.Viewport, projection);
            }
            else
            {
                receiverBoard.Draw(view, GraphicsDevice.Viewport, projection);
                DrawPacketBoards(gameTime, view, GraphicsDevice.Viewport, projection);
                senderBoard.Draw(view, GraphicsDevice.Viewport, projection);
            }

            this.spriteBatch.Begin();
            if (packetReader_v4 == null && packetReader_v6 == null)
            {
                this.spriteBatch.DrawString(this.font, "can not bind port 11300", new Vector2(50, 50), Color.White);
            }
            else if (packetReader_v6 == null && disableShowV4 == false)
            {
                this.spriteBatch.Draw(v4Texture, new Rectangle((int)(graphics.PreferredBackBufferWidth * 0.87)
                    , (int)(graphics.PreferredBackBufferHeight * 0.02), (int)(graphics.PreferredBackBufferHeight * 0.15)
                , (int)(graphics.PreferredBackBufferHeight * 0.0375)), Color.White);
                //this.spriteBatch.DrawString(this.font, "IPv4", new Vector2(graphics.PreferredBackBufferWidth - 70, 30), Color.White);
            }
            if (statusDraw == true)
            {
                // �J�����̏���\��
                this.spriteBatch.DrawString(this.font,
                    "CameraPosition : " + cameraPosition,
                    new Vector2(5, graphics.PreferredBackBufferHeight - 75), Color.White);
            }

            // �ǂ̒��x���Ԃ������߂��Ă��邩��\������
            TimeSpan time = TimeSpan.FromMilliseconds(addMilliseconds);
            this.spriteBatch.DrawString(this.font, "Display Time : " + time.ToString()
                    , new Vector2(5, graphics.PreferredBackBufferHeight - 50), Color.White);
            
            // �p�P�b�g�̏���\������
            if (hilightPacketBoard != null)
            {
                String line = hilightPacketBoard.Description;
                char[] delimiters = { ',', '\n' };
                string[] words = line.Split(delimiters);
                if (words.Length >= 5)
                {
                    String line2 = words[5];
                    char [] delimiters2 = {'-', '\n' };
                    string[] words2 = line2.Split(delimiters2);
                    if (words2.Length == 2)
                    {
                        String hash = String.Format("hash:{0}", words2[0]);
                        this.spriteBatch.DrawString(this.font, hash
                                , new Vector2(5, graphics.PreferredBackBufferHeight - 25), Color.White);
                    }
                    else
                    {
                        this.spriteBatch.DrawString(this.font, line2
                                , new Vector2(5, graphics.PreferredBackBufferHeight - 25), Color.White);
                    }
                }

            }

            this.spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
