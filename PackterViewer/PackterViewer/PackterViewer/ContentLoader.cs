﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;

namespace PackterViewer
{
    class ContentLoader
    {
        ContentBuilder contentBuilder = null;
        ContentManager contentManager = null;
        ContentManager Content = null;
        GraphicsDevice graphicsDevice = null;

        Dictionary<string, Texture2D> cachedTexture2D = new Dictionary<string, Texture2D>();
        Dictionary<string, Model> cachedModel = new Dictionary<string, Model>();
        Dictionary<string, SoundEffect> cachedSoundEffect = new Dictionary<string, SoundEffect>();

        public void Initialize(ContentBuilder builder, ContentManager manager, IServiceProvider serviceProvider, GraphicsDevice graphics)
        {
            contentBuilder = builder;
            contentManager = manager;
            Content = new ContentManager(serviceProvider, "Content");
            Content.RootDirectory = "Content";
            graphicsDevice = graphics;
        }

        Model TryLoadModelFromContent(string fileName)
        {
            if (fileName == null || Content == null)
            {
                return null;
            }
            Model tmpModel = null;
            try
            {
                tmpModel = Content.Load<Model>(fileName);
            }
            catch(Exception e)
            {
                System.Diagnostics.Debug.Print(e.ToString());
                tmpModel = null;
            }
            return tmpModel;
        }

        Model TryLoadModelFromFile(string fileName)
        {
            if (fileName == null || contentBuilder == null)
            {
                return null;
            }
            Model tmpModel = null;
            if (!System.IO.File.Exists(fileName))
            {
                // ファイルがなかったら一応 .x をつけてみる
                fileName += ".x";
            }
            if (System.IO.File.Exists(fileName))
            {
                string targetFileName = System.IO.Directory.GetCurrentDirectory() + "\\" + fileName;
                contentBuilder.Add(targetFileName, fileName, null, "ModelProcessor");
                string buildError = contentBuilder.Build();
                if (buildError == null || buildError.IndexOf(fileName) < 0)
                {
                    try
                    {
                        tmpModel = contentManager.Load<Model>(fileName);
                    }
                    catch
                    {
                        tmpModel = null;
                    }
                }
            }
            return tmpModel;
        }

        public Model GetModel(string filename)
        {
            if (filename == null)
            {
                return null;
            }
            if (cachedModel.ContainsKey(filename))
            {
                return cachedModel[filename];
            }
            Model tmpModel = TryLoadModelFromContent(filename);
            if (tmpModel == null)
            {
                tmpModel = TryLoadModelFromFile(filename);
            }
            // 一度でも load を試みたらそれが失敗していても覚える
            cachedModel[filename] = tmpModel;
            return tmpModel;
        }

        Texture2D TryLoadTexture2DFromContent(string filename)
        {
            if (filename == null || Content == null)
            {
                return null;
            }
            Texture2D texture = null;
            try
            {
                texture = Content.Load<Texture2D>(filename);
            }
            catch
            {
                texture = null;
            }
            return texture;
        }

        Texture2D TryLoadTexture2DFromFile(string filename)
        {
            if (filename == null || !System.IO.File.Exists(filename))
            {
                return null;
            }
            Texture2D texture = null;
            try
            {
                System.IO.FileStream stream = new System.IO.FileStream(filename, System.IO.FileMode.Open);
                texture = Texture2D.FromStream(graphicsDevice, stream);
                stream.Close();
            }
            catch
            {
                texture = null;
            }
            return texture;
        }

        public Texture2D GetTexture2D(string filename)
        {
            if (filename == null)
            {
                return null;
            }
            if (cachedTexture2D.ContainsKey(filename))
            {
                return cachedTexture2D[filename];
            }
            Texture2D texture = TryLoadTexture2DFromContent(filename);
            if(texture == null)
            {
                texture = TryLoadTexture2DFromFile(filename);
            }
            // 一度でも load を試みたらそれが失敗していても覚える
            cachedTexture2D[filename] = texture;
            return texture;
        }

        SpriteFont TryLoadSpriteFontFromContent(string filename)
        {
            if (filename == null || Content == null)
            {
                return null;
            }
            SpriteFont font = null;
            try
            {
                font = Content.Load<SpriteFont>(filename);
            }
            catch
            {
                font = null;
            }
            return font;
        }

        public SpriteFont GetSpriteFont(string filename)
        {
            SpriteFont font = TryLoadSpriteFontFromContent(filename);
            // とりあえず file からの load がないので cache はいらない。
            return font;
        }

        Video TryLoadVideoFromContent(string filename)
        {
            if (filename == null || Content == null)
            {
                return null;
            }
            Video video = null;
            try
            {
                video = Content.Load<Video>(filename);
            }
            catch
            {
                video = null;
            }
            return video;
        }
        
        public Video GetVideo(string filename)
        {
            Video video = TryLoadVideoFromContent(filename);
            return video;
        }

        SoundEffect TryLoadSoundEffectFromContent(string filename)
        {
            if (filename == null || Content == null)
            {
                return null;
            }
            SoundEffect se = null;
            try
            {
                se = Content.Load<SoundEffect>(filename);
            }
            catch
            {
                se = null;
            }
            return se;
        }

        SoundEffect TryLoadSoundEffectFromFile(string filename)
        {
            if (filename == null || !System.IO.File.Exists(filename))
            {
                return null;
            }
            SoundEffect se = null;

            if (!System.IO.File.Exists(filename))
            {
                // ファイルがなかったら一応 .wav や .mp3 をつけてみる
                string[] targets = new string[] { ".mp3", ".wma", ".wav" };
                foreach (string target in targets)
                {
                    if (System.IO.File.Exists(filename + target))
                    {
                        filename += target;
                        break;
                    }
                }
            }
            if (System.IO.File.Exists(filename))
            {
                string targetFileName = System.IO.Directory.GetCurrentDirectory() + "\\" + filename;
                contentBuilder.Add(targetFileName, filename, null, "SoundEffectProcessor"); // XXX SoundEffectProcessor なんてものはないっぽい。やっぱりなー。
                string buildError = contentBuilder.Build();
                if (buildError == null || buildError.IndexOf(filename) < 0)
                {
                    try
                    {
                        se = contentManager.Load<SoundEffect>(filename);
                    }
                    catch
                    {
                        se = null;
                    }
                }
            }
            return se;
        }
        
        public SoundEffect GetSoundEffect(string filename)
        {
            if (filename == null)
            {
                return null;
            }
            if (cachedSoundEffect.ContainsKey(filename))
            {
                return cachedSoundEffect[filename];
            }
            SoundEffect se = TryLoadSoundEffectFromContent(filename);
            if(se == null)
            {
                se = TryLoadSoundEffectFromFile(filename);
            }
            // 一度でも load を試みたらそれが失敗していても覚える
            cachedSoundEffect[filename] = se;
            return se;
        }
    }
}
