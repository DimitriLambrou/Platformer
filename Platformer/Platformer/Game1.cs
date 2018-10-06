﻿#define DEBUG

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Audio;
using MonoGame.Extended;
using MonoGame.Extended.Tiled;
using MonoGame.Extended.Tiled.Graphics;
using MonoGame.Extended.ViewportAdapters;
using System.Collections;
using System.Collections.Generic;
using ParticleEffects;

namespace Platformer
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        Player player = new Player(); // Creates an instance of our player class

        public List<Enemy> enemies = new List<Enemy>();
        public Chest goal = null;

        Camera2D camera = null; // Creates an instance of a 2D camera
        TiledMap map = null; // Creates an instance of a Tiled map
        TiledMapRenderer mapRenderer = null; // Creates an instance of what makes a Tiled map

        TiledMapTileLayer collisionLayer;
        public ArrayList allCollisionTiles = new ArrayList();
        public Sprite[,] levelGrid;

        public int tileHeight = 0;
        public int levelTileWidth = 0;
        public int levelTileHeight = 0;

        public Vector2 gravity = new Vector2(0, 1500);

        Song gameMusic;

        SpriteFont arialFont;
        int score = 0;
        int lives = 3;
        Texture2D heart = null;

        Rectangle myMap;
public Texture2D fireTexture = null;
        public Emitter fireEmitter = null;
        

#if (DEBUG)
        static public Texture2D whiteRectangle;
#endif

        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = 1600;
            graphics.PreferredBackBufferHeight = 900;

            Content.RootDirectory = "Content";
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
            myMap.X = 0;
            myMap.Y = 0;
            myMap.Width = 1600;
            myMap.Height = 900;

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);

            player.Load(Content, this); // Call the 'Load' function in the Player Class
            // 'this' basically means "pass all information in our class through as a variable"

            arialFont = Content.Load<SpriteFont>("Arial");
            heart = Content.Load<Texture2D>("heart");

            BoxingViewportAdapter viewportAdapter = new BoxingViewportAdapter(Window, GraphicsDevice, graphics.GraphicsDevice.Viewport.Width, graphics.GraphicsDevice.Viewport.Height);

            camera = new Camera2D(viewportAdapter);
            camera.Position = new Vector2(0, graphics.GraphicsDevice.Viewport.Height);

            map = Content.Load<TiledMap>("Level1");
            mapRenderer = new TiledMapRenderer(GraphicsDevice);

            gameMusic = Content.Load<Song>("SuperHero_original");
            MediaPlayer.Play(gameMusic);

#if (DEBUG)
            whiteRectangle = new Texture2D(GraphicsDevice, 1, 1);
            whiteRectangle.SetData(new[] { Color.White });
#endif

            fireTexture = Content.Load<Texture2D>("fire");
            fireEmitter = new Emitter(fireTexture, player.playerSprite.position);

            // TODO: use this.Content to load your game content here

            SetUpTiles();
            LoadObjects();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
#if (DEBUG)
            whiteRectangle.Dispose();
#endif
        }

        public void SetUpTiles()
        {
            tileHeight = map.TileHeight;
            levelTileHeight = map.Height;
            levelTileWidth = map.Width;
            levelGrid = new Sprite[levelTileWidth, levelTileHeight];
            foreach(TiledMapTileLayer layer in map.TileLayers)
            {
                if (layer.Name == "Collision")
                {
                    collisionLayer = layer;
                }
            }

            int columns = 0;
            int rows = 0;
            int loopCount = 0;
            while (loopCount < collisionLayer.Tiles.Count)
            {
                if (collisionLayer.Tiles[loopCount].GlobalIdentifier != 0)
                {
                    Sprite tileSprite = new Sprite();
                    tileSprite.position.X = columns * tileHeight;
                    tileSprite.position.Y = rows * tileHeight;
                    tileSprite.width = tileHeight;
                    tileSprite.height = tileHeight;
                    tileSprite.UpdateHitBox();
                    allCollisionTiles.Add(tileSprite);
                    levelGrid[columns, rows] = tileSprite;
                }

                columns++;

                if (columns == levelTileWidth)
                {
                    columns = 0;
                    rows++;
                }

                loopCount++;
            }
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            player.Update(deltaTime); // Call the 'Update' from our Player Class

            foreach (Enemy enemy in enemies)
            {
                enemy.Update(deltaTime);
            }

            camera.Position = player.playerSprite.position - new Vector2(graphics.GraphicsDevice.Viewport.Width / 2, graphics.GraphicsDevice.Viewport.Height / 2);

            fireEmitter.position = player.playerSprite.position;
            fireEmitter.minVelocity = new Vector2(player.playerSprite.velocity.X, player.playerSprite.velocity.Y);
            fireEmitter.maxVelocity = new Vector2(player.playerSprite.velocity.X * 2, player.playerSprite.velocity.Y * 2);
            fireEmitter.Update(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            // Clears anything previously drawn to the screen
            GraphicsDevice.Clear(Color.Gray);

            var viewMatrix = camera.GetViewMatrix();
            var projectionMatrix = Matrix.CreateOrthographicOffCenter(0, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0.0f, -1.0f);

            //Begin drawing
            spriteBatch.Begin(transformMatrix: viewMatrix);

            mapRenderer.Draw(map, ref viewMatrix, ref projectionMatrix);
            // Call the 'draw' function from our Player class
            player.Draw(spriteBatch);
            goal.Draw(spriteBatch);
            fireEmitter.Draw(spriteBatch);

            foreach (Enemy enemy in enemies)
            {
                enemy.Draw(spriteBatch);
            }     

            // Finish drawing
            spriteBatch.End();

            // Draw all the GUI components in a SpriteBatch section
            spriteBatch.Begin();
            spriteBatch.DrawString(arialFont, "Score : " + score.ToString(), new Vector2(20, 20), Color.Orange);
            int loopCount = 0;
            while(loopCount < lives)
            {
                spriteBatch.Draw(heart, new Vector2(GraphicsDevice.Viewport.Width - 80 - loopCount * 20, 20), Color.White);
                loopCount++;
            }
            spriteBatch.End();


            // TODO: Add your drawing code here

            base.Draw(gameTime);
        }

        void LoadObjects()
        {
            foreach (TiledMapObjectLayer layer in map.ObjectLayers)
            {
                if (layer.Name == "Enemies")
                {
                    foreach (TiledMapObject thing in layer.Objects)
                    {
                        Enemy enemy = new Enemy();
                        Vector2 tiles = new Vector2((int)(thing.Position.X / tileHeight), (int)(thing.Position.Y / tileHeight));
                        enemy.enemySprite.position = tiles * tileHeight;
                        enemy.Load(Content, this);
                        enemies.Add(enemy);
                    }
                }

                if (layer.Name == "Goal")
                {
                    TiledMapObject thing = layer.Objects[0];
                    if (thing != null)
                    {
                        Chest chest = new Chest();
                        chest.chestSprite.position = new Vector2(thing.Position.X, thing.Position.Y);
                        chest.Load(Content, this);
                        goal = chest;
                    }
                }
            }
        }
    }
}
