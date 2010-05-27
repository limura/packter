﻿// おせわになったところ
// http://sorceryforce.com/xna/

/*
 * XNA が作ってくれたゲームの本体。
 * 
 * 初期化でパケット用のテクスチャイメージを読み込むだの、
 * packter sender からのデータを読み込むだのなんだのといった、
 * いわゆる main相当 のことをやる
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

using System.Linq;
using System.Text;
using System.Diagnostics;
//using System.Windows.Forms;

using WinFormsGraphicsDevice;
using Packter_viewer;

namespace Packter_viewer2
{
    class PackterDisplayControl : GraphicsDeviceControl
    {
        Stopwatch timer;
        ContentBuilder contentBuilder;
        ContentManager contentManager;
        ContentManager Content;
        HashSet<System.Windows.Forms.Keys> EnableKeySet = new HashSet<System.Windows.Forms.Keys>();

        const int MaxPacketNum = 1024*1024; // 1024*1024個までパケットは覚える
        const int flyMillisecond = 3000; // パケットが飛んでいくのにかかる時間
        
        double addMilliseconds = 0.0; // パケット表示する時にずらされる時間
        
        //GraphicsDeviceManager graphics;
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

        Packter_viewer.PacketReader packetReader_v4 = null;
        Packter_viewer.PacketReader packetReader_v6 = null;

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
        /// 時が止まったときの時間
        /// </summary>
        GameTime stopedGameTime = null;

        PacketBoard hilightPacketBoard = null;
        DateTime startDateTime;
        DateTime prevDateTime;
        GameTime currentGameTime;

        protected override void Initialize()
        {
            timer = Stopwatch.StartNew();
            timer.Start();
            prevDateTime = startDateTime = DateTime.Now;
            System.Windows.Forms.Application.Idle += delegate
            {
                Invalidate();
                DateTime nowTime = DateTime.Now;
                GameTime time = new GameTime(new TimeSpan((nowTime.Ticks - startDateTime.Ticks) * 1)
                    , new TimeSpan((nowTime.Ticks - prevDateTime.Ticks) * 1)
                    , new TimeSpan((nowTime.Ticks - startDateTime.Ticks) * 1)
                    , new TimeSpan((nowTime.Ticks - prevDateTime.Ticks) * 1));
                currentGameTime = time;
                prevDateTime = nowTime;
                this.UpdateSene(time);
            };

            //graphics = new GraphicsDeviceManager(this);
            //Content.RootDirectory = "Content";
            args = null;

            Content = new ContentManager(Services, "Content");
            //LoadContent();
        }

        public void writeString(string[] Args)
        {
            return;
        }

        /// <summary>
        /// FlyPacket から、実際に飛んでいく PacketBoard を作成し、表示queueに追加します。
        /// FlyPacket では 0.0～1.0 までで記録されているパケット座標情報を、
        /// 画面の座標情報に変換して PacketBoard に登録しています。
        /// </summary>
        /// <param name="packet">飛んでいくパケット</param>
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

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
        }

        public void RegisterContentBuilder(ContentBuilder builder, ContentManager manager)
        {
            contentBuilder = builder;
            contentManager = manager;

            contentManager.RootDirectory = "Content";
            LoadContent();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        public void LoadContent()
        {
            contentManager.Unload();
            contentBuilder.Clear();

            defaultWidth = GraphicsDevice.DisplayMode.Width;
            defaultHeight = GraphicsDevice.DisplayMode.Height;

            //this.IsMouseVisible = true; // XXX
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

            //font = Content.Load<SpriteFont>("font");
            font = contentManager.Load<SpriteFont>("font");
            //v4Texture = Content.Load<Texture2D>("v4");
            //contentBuilder.Add("v4.png", "v4Texture", null, "TextureProcessor");
            v4Texture = contentManager.Load<Texture2D>("v4");

            //contentBuilder.Add(System.IO.Directory.GetCurrentDirectory() + "\\Content\\board.x", "Board", null, "ModelProcessor");
            contentBuilder.Add(@"Content\board.x", "Board", null, "ModelProcessor");
            string buildError = contentBuilder.Build();
            if (string.IsNullOrEmpty(buildError))
            {
                packetModel = contentManager.Load<Model>("Board");

                senderBoard = new Board(Content.Load<Model>("Board"));
                senderBoard.Position = new Vector3(0, 0, -150);
                senderBoard.Scale = 100;
                senderBoard.RotationY = (float)Math.PI;
                senderBoard.Alpha = 0.3f;
                receiverBoard = new Board(Content.Load<Model>("Board"));
                receiverBoard.Position = new Vector3(0, 0, 150);
                receiverBoard.RotationY = (float)Math.PI;
                receiverBoard.Scale = 100;
                receiverBoard.Alpha = 0.3f;
            }
            else
            {
                packetModel = contentManager.Load<Model>("board");

                senderBoard = new Board(Content.Load<Model>("board"));
                senderBoard.Position = new Vector3(0, 0, -150);
                senderBoard.Scale = 100;
                senderBoard.RotationY = (float)Math.PI;
                senderBoard.Alpha = 0.3f;
                receiverBoard = new Board(Content.Load<Model>("board"));
                receiverBoard.Position = new Vector3(0, 0, 150);
                receiverBoard.RotationY = (float)Math.PI;
                receiverBoard.Scale = 100;
                receiverBoard.Alpha = 0.3f;
            }

#if false
            myship = new Board(Content.Load<Model>("myship"));
            myship.Position = new Vector3(0, 0, 0);
            myship.Scale = 20;
            myship.RotationY = (float)Math.PI;
            myship_wake = new Board(Content.Load<Model>("boad"));
            myship_wake.Position = new Vector3(0, 0, myship.Scale);
            myship_wake.Scale = 6;
            myship_wake.Texture = Content.Load<Texture2D>("Belt_of_light");
            myship_wake.RotationY = (float)Math.PI / -2;
#endif

            {
                Texture2D t = null;
                try
                {
                    t = Texture2D.FromFile(GraphicsDevice, "packter_sender.png");
                }
                catch
                {
                    t = contentManager.Load<Texture2D>("packter_sender");
                }
                senderBoard.Texture = t;
                try
                {
                    t = Texture2D.FromFile(GraphicsDevice, "packter_receiver.png");
                }
                catch
                {
                    t = contentManager.Load<Texture2D>("packter_receiver");
                }
                receiverBoard.Texture = t;
            }

            float aspectRaito = (float)GraphicsDevice.Viewport.Width / GraphicsDevice.Viewport.Height;

            /// packter[n].png というファイルを読み込んで、テクスチャとして登録する
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
                            t = contentManager.Load<Texture2D>("packter" + i);
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
                packetReader_v6 = new Packter_viewer.PacketReader(new IPEndPoint(IPAddress.IPv6Any, 11300));
            }
            catch
            {
                packetReader_v6 = null;
            }
            try
            {
                packetReader_v4 = new Packter_viewer.PacketReader(new IPEndPoint(IPAddress.Any, 11300));
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
        /*protected override*/ void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
            if(packetReader_v4 != null)
                packetReader_v4.Dispose();
            if(packetReader_v6 != null)
                packetReader_v6.Dispose();
        }

        /// <summary>
        /// 今、この瞬間に key で指示されたキーが押されたのかどうかを判定する
        /// </summary>
        /// <param name="key">押されたかどうかを判定するキー</param>
        /// <returns>押されてたのなら true</returns>
        private bool JustNowKeyDown(KeyboardState nowKeyState, Keys key)
        {
            return nowKeyState.IsKeyDown(key) == true && keyboardState.IsKeyDown(key) == false;
        }

        UInt32 n = 0;

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// このUpdate()が定期的に呼ばれるので、このmethod内でゲーム時間を進める。
        /// packter sender からの入力データを読み込んでいるのもここ。
        /// 表示するのは Draw()。
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public void UpdateSene(GameTime gameTime) // XXXX
        {
            // Allows the game to exit
            //if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
            //    this.Exit();
            
            // TODO: Add your update logic here
            MouseState nowMouseState = Mouse.GetState();
            KeyboardState nowKeyState = Keyboard.GetState();

#if false // 全画面モードあたりはちょっとお蔵入り
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
#endif

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
                    addMilliseconds += 1000.0 * 60 * 5; // 5分後に進める
                }
                else
                {
                    addMilliseconds -= 1000.0 * 60 * 5; // 5分前に戻す
                }
            }
            if (nowKeyState.IsKeyDown(Keys.B))
                if (nowKeyState.IsKeyDown(Keys.LeftShift) || nowKeyState.IsKeyDown(Keys.RightShift))
                    addMilliseconds -= flyMillisecond / 3; // 少し前に戻す
                else
                    addMilliseconds -= flyMillisecond / 30; // 少し前に戻す
            if (nowKeyState.IsKeyDown(Keys.F))
                if (nowKeyState.IsKeyDown(Keys.LeftShift) || nowKeyState.IsKeyDown(Keys.RightShift))
                    addMilliseconds += flyMillisecond / 3; // 少し後に進める
                else
                    addMilliseconds += flyMillisecond / 30; // 少し後に進める
            if (JustNowKeyDown(nowKeyState, Keys.C))
                addMilliseconds = 0.0; // C キーが押されたら元に戻す
            if (JustNowKeyDown(nowKeyState, Keys.T))
            {
                // TraceBack 開始
                if (hilightPacketBoard != null)
                {
                    // Traceback サーバー取得
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
                // S キーが押されたら時を止めるのをトグルする
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

            // キーボードの状態を見る場合はこの行より前で見ないと駄目
            keyboardState = nowKeyState;
            /// packter sender からパケットを読み込む
            ReadPackets(gameTime);
            
#if false
            /// PacketBoard のリストをゲーム時間で更新しつつ、
            /// 表示されるものだけを drawPacketList に入れる。
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
                    break; // 一度表示される範囲のものが出てきた後、表示されない範囲のものが出てきたということは、表示される範囲は終わった
                }
            }
            /// 多くなりすぎたパケットは捨てる
            if (packetList.Count > MaxPacketNum)
            {
                packetList.RemoveRange(0, packetList.Count - MaxPacketNum);
            }
#endif
            /// マウスの情報を読み込んで、PacketBoard を選択されているかどうかを判定する。
            UpdateForMouse(gameTime);

            //base.Update(gameTime);
        }

        /// <summary>
        /// Update時にマウスの情報を読み込んでなにやらする部分
        /// </summary>
        private void UpdateForMouse(GameTime gameTime)
        {
            double nowTime = gameTime.TotalGameTime.TotalMilliseconds + addMilliseconds;

            // http://msdn.microsoft.com/ja-jp/library/bb203905.aspx
            // のコピペ。:P
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
            Vector3 nearPoint = GraphicsDevice.Viewport.Unproject(nearSource, projection, view, world);
            Vector3 farPoint =  GraphicsDevice.Viewport.Unproject(farSource,  projection, view, world);

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
                        break; // 表示すべきものがなくなった
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

        /// packter sender から送信されたパケットのリストを読み込んでいるところ。
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

            // 表示されるべきもののうち一番古いのものの index を探す
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
                        // 表示される時間からすると既に通り過ぎてしまったもの
                        nowChecking += div;
                    }
                    else if (ret < 0)
                    {
                        // 表示される時間からすると、まだ表示領域に入っていないもの
                        nowChecking -= div;
                    }
                    else
                    {
                        // 表示されるべきもの
                        // 一つ前が表示されるべきでないものであれば、ここから表示すればよい
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
                            // この nowChecking が最初のもの。
                            return nowChecking;
                        }
                        // まだ最初のものをみつけていないかもしれない
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
        /// packetList に保存されているパケットの中から表示すべきパケットを二分探索で検索しつつ、表示する
        /// </summary>
        /// <param name="gameTime">Draw() に渡された GameTime</param>
        /// <param name="view">表示に使うビュー行列</param>
        /// <param name="viewport">表示に使うビューポート</param>
        /// <param name="projection">表示に使う視線行列</param>
        private void DrawPacketBoards(GameTime gameTime, Matrix view, Viewport viewport, Matrix projection)
        {
            double nowTime = gameTime.TotalGameTime.TotalMilliseconds + addMilliseconds;
#if false
            // 表示されるべきもののうち一番古いのものの index を探す
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
                        // 表示される時間からすると既に通り過ぎてしまったもの
                        nowChecking += div;
                    }
                    else if (ret < 0)
                    {
                        // 表示される時間からすると、まだ表示領域に入っていないもの
                        nowChecking -= div;
                    }
                    else
                    {
                        // 表示されるべきもの
                        // 一つ前が表示されるべきでないものであれば、ここから表示すればよい
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
                            // この nowChecking が最初のもの。
                            for (int i = 0; i < packetList.Length; i++)
                            {
                                pb = packetList[i + nowChecking];
                                if (pb == null || pb.Update(nowTime) != 0)
                                    return; // 表示すべきものがなくなった
                                pb.Draw(view, viewport, projection);
                            }
                            return;
                        }
                        // まだ最初のものをみつけていないかもしれない
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
                    return; // 表示すべきものがなくなった
                pb.Draw(view, viewport, projection);
            }
#endif
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// そのときのゲーム時間のものを表示する。
        /// ぶっちゃけ呪文の塊
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw()
        {
            GameTime gameTime = currentGameTime;
            if (gameTime == null)
            {
                return;
            }
            GraphicsDevice.Clear(Color.CornflowerBlue);
            //GraphicsDevice.Clear(Color.Bisque);

            // 深度バッファを有効にする
            this.GraphicsDevice.RenderState.DepthBufferEnable = true;

            // ビューマトリックス作成
            Matrix view = Matrix.CreateLookAt(
                    this.cameraPosition,
                    this.cameraTarget,
                    Vector3.Transform(Vector3.Up,
                                      Matrix.CreateRotationZ(this.cameraRotation.Z))
                );

            // プロジェクションマトリックス作成
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
#if true
                this.spriteBatch.Draw(v4Texture, new Rectangle((int)(this.Width * 0.87)
                    , (int)(this.Height * 0.02), (int)(this.Height * 0.15)
                , (int)(this.Height * 0.0375)), Color.White);
#else
                this.spriteBatch.Draw(v4Texture, new Rectangle((int)(graphics.PreferredBackBufferWidth * 0.87)
                    , (int)(graphics.PreferredBackBufferHeight * 0.02), (int)(graphics.PreferredBackBufferHeight * 0.15)
                , (int)(graphics.PreferredBackBufferHeight * 0.0375)), Color.White);
                //this.spriteBatch.DrawString(this.font, "IPv4", new Vector2(graphics.PreferredBackBufferWidth - 70, 30), Color.White);
#endif
            }
            if (statusDraw == true)
            {
                // カメラの情報を表示
                this.spriteBatch.DrawString(this.font,
                    "CameraPosition : " + cameraPosition,
                    new Vector2(5, this.Height - 75), Color.White);
                    //new Vector2(5, graphics.PreferredBackBufferHeight - 75), Color.White);
            }

            // どの程度時間を巻き戻しているかを表示する
            TimeSpan time = TimeSpan.FromMilliseconds(addMilliseconds);
            this.spriteBatch.DrawString(this.font, "Display Time : " + time.ToString()
                    , new Vector2(5, this.Height - 50), Color.White);
                    //, new Vector2(5, graphics.PreferredBackBufferHeight - 50), Color.White);
            
            // パケットの情報を表示する
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
                                , new Vector2(5, this.Height - 25), Color.White);
                                //, new Vector2(5, graphics.PreferredBackBufferHeight - 25), Color.White);
                    }
                    else
                    {
                        this.spriteBatch.DrawString(this.font, line2
                                , new Vector2(5, this.Height - 25), Color.White);
                    }
                }

            }

            this.spriteBatch.End();

            //base.Draw(gameTime);
        }

#if false
        public void KeyDown(System.Windows.Forms.Keys key)
        {
            EnableKeySet.Add(key);
        }
        public void KeyUp(System.Windows.Forms.Keys key)
        {
            EnableKeySet.Remove(key);
        }
#endif
    }
}
