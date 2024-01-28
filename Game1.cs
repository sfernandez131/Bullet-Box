using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BoxMan
{
    public enum GameState
    {
        Playing,
        GameOver
    }
    public class Game1 : Game
    {
        // Game classes
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Random random = new Random();
        private GameState gameState = GameState.Playing;

        // Fonts
        SpriteFont font;

        // Audio
        private Song backgroundMusic;
        private SoundEffect BulletFired;

        // Projectile data
        private List<Projectile> mainProjectiles = new List<Projectile>();
        private List<Projectile> enemyProjectiles = new List<Projectile>();
        private List<Projectile> explosionDots = new List<Projectile>();
        private Texture2D projectileTexture;
        private Texture2D smallDotTexture;

        // Player data
        private Texture2D PlayerSprite;
        private Vector2 PlayerPosition;
        private float rotationAngle = 0f;
        private Vector2 spriteOrigin;
        private const float playerSpeed = 5;

        // Input data
        private KeyboardState currentKeyboardState;
        private KeyboardState previousKeyboardState;
        private MouseState previousMouseState;
        private GamePadState previousGamepadState;
        private GamePadState testState;

        // Enemys
        private List<Enemy> enemies = new List<Enemy>();
        private Texture2D enemyTexture;
        private int EnemyCount = 1;
        private int KillCount = 0;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.IsFullScreen = true;
            _graphics.PreferredBackBufferWidth = 1920;
            _graphics.PreferredBackBufferHeight = 1080;
            _graphics.ApplyChanges();

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here
            // Set frame rate to 60 FPS
            TargetElapsedTime = TimeSpan.FromSeconds(1d / 60d);
            // Disable fixed time step
            IsFixedTimeStep = false;

            // Load the audio
            backgroundMusic = Content.Load<Song>("Audio/backGround");
            BulletFired = Content.Load<SoundEffect>("Audio/bullet");

            // Play the background music on loop
            MediaPlayer.IsRepeating = true;
            //MediaPlayer.Play(backgroundMusic);

            PlayerPosition = new(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2);

            base.Initialize();
        }

        protected override async void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("Fonts/pixelText"); // Use the name of your spritefont file without the extension

            // TODO: use this.Content to load your game content here
            // Create player
            await GeneratePlayer();
            await LoadProjectiles();

            // Load enemy texture and create enemies
            enemyTexture = Content.Load<Texture2D>("Sprites/Enemy");
            CreateEnemies();
        }

        private void CreateEnemies()
        {
            for (int i = 0; i < EnemyCount; i++)
            {
                // Create a few enemies at different positions
                enemies.Add(new Enemy(enemyTexture, new Vector2(100, 100), 2f)); // Shoot every 2 seconds
            }
        }

        private async Task LoadProjectiles()
        {
            // Load projectile texture (half the size of the player sprite)
            int diameter = Math.Min(PlayerSprite.Width, PlayerSprite.Height) / 2;
            projectileTexture = new Texture2D(GraphicsDevice, diameter, diameter);
            Color[] colorData = new Color[diameter * diameter];

            // Calculate the radius and center of the circle
            int radius = diameter / 2;
            Vector2 center = new Vector2(radius, radius);

            for (int y = 0; y < diameter; y++)
            {
                for (int x = 0; x < diameter; x++)
                {
                    // Calculate the distance of the pixel from the center
                    Vector2 position = new Vector2(x, y);
                    if (Vector2.Distance(position, center) <= radius)
                    {
                        // Pixel is inside the circle
                        colorData[x + y * diameter] = Color.White;
                    }
                    else
                    {
                        // Pixel is outside the circle, make it transparent
                        colorData[x + y * diameter] = Color.Transparent;
                    }
                }
            }

            // Load texture for smaller dots (quarter size of the original projectile)
            int smallDotSize = projectileTexture.Width / 2;
            smallDotTexture = new Texture2D(GraphicsDevice, smallDotSize, smallDotSize);
            Color[] smallDotColorData = new Color[smallDotSize * smallDotSize];
            for (int i = 0; i < smallDotColorData.Length; ++i)
                smallDotColorData[i] = Color.White;
            smallDotTexture.SetData(smallDotColorData);

            projectileTexture.SetData(colorData);
        }

        private async Task GeneratePlayer()
        {
            PlayerSprite = Content.Load<Texture2D>("Sprites/Player");
            // Set the origin to the center of the sprite for rotation
            spriteOrigin = new Vector2(PlayerSprite.Width / 2f, PlayerSprite.Height / 2f);
        }

        private void RestartGame()
        {
            gameState = GameState.Playing;
            //PlayerSprite = Content.Load<Texture2D>("Sprites/Player");
            PlayerPosition = new Vector2(_graphics.PreferredBackBufferWidth / 2, _graphics.PreferredBackBufferHeight / 2);
            mainProjectiles.Clear();
            enemyProjectiles.Clear();
            explosionDots.Clear();
            enemies.Clear();
            EnemyCount = 1;
            KillCount = 0;
            CreateEnemies();
            // Reset other necessary game components
        }

        protected override async void Update(GameTime gameTime)
        {
            if (gameState == GameState.GameOver)
            {
                GamePadState gamePadState = GamePad.GetState(PlayerIndex.One);
                KeyboardState keyboardState = Keyboard.GetState();

                if (gamePadState.Buttons.Start == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Enter))
                {
                    RestartGame();
                }
                else if (gamePadState.Buttons.Back == ButtonState.Pressed || keyboardState.IsKeyDown(Keys.Escape))
                {
                    Exit(); // Close the game
                }
                return; // Skip the rest of the update logic
            }

            // TODO: Add your update logic here
            await PlayerMovementV2();

            // Update enemies
            foreach (var enemy in enemies)
            {
                enemy.Update(gameTime, PlayerPosition, projectileTexture, mainProjectiles, enemyProjectiles, _graphics);
            }

            // Update main projectiles
            for (int i = mainProjectiles.Count - 1; i >= 0; i--)
            {
                var projectile = mainProjectiles[i];
                projectile.Update(gameTime);

                // Check for collision with screen boundaries
                if (projectile.Position.X < 0 || projectile.Position.X > _graphics.PreferredBackBufferWidth - projectile.Texture.Width ||
                    projectile.Position.Y < 0 || projectile.Position.Y > _graphics.PreferredBackBufferHeight - projectile.Texture.Height)
                {
                    // Inside the Update method, where you create the smaller dots
                    for (int j = 0; j < 5; j++)
                    {
                        // Generate a random angle between 0 and 2 * PI
                        float angle = (float)(random.NextDouble() * Math.PI * 2);

                        // Create a velocity vector based on the random angle
                        Vector2 smallDotVelocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 2f; // Adjust speed as needed

                        // Create and add the smaller dot
                        Projectile smallDot = new Projectile(projectile.Position, smallDotVelocity, smallDotTexture, 1f); // Last for 1 second
                        explosionDots.Add(smallDot);
                    }

                    // Remove original projectile
                    mainProjectiles.RemoveAt(i);
                }
            }

            // Update enemy projectiles
            for (int i = enemyProjectiles.Count - 1; i >= 0; i--)
            {
                var projectile = enemyProjectiles[i];
                projectile.Update(gameTime);

                // Check for collision with screen boundaries
                if (projectile.Position.X < 0 || projectile.Position.X > _graphics.PreferredBackBufferWidth - projectile.Texture.Width ||
                    projectile.Position.Y < 0 || projectile.Position.Y > _graphics.PreferredBackBufferHeight - projectile.Texture.Height)
                {
                    enemyProjectiles.RemoveAt(i);
                }
            }

            // Update explosion dots
            for (int i = explosionDots.Count - 1; i >= 0; i--)
            {
                var dot = explosionDots[i];
                dot.Update(gameTime);

                if (dot.IsExpired)
                {
                    explosionDots.RemoveAt(i);
                }
            }

            // Create a list to store enemies that need to be removed
            List<Enemy> enemiesToRemove = new List<Enemy>();

            // Check for collisions between player bullets and enemies
            for (int i = mainProjectiles.Count - 1; i >= 0; i--)
            {
                var projectile = mainProjectiles[i];
                foreach (var enemy in enemies)
                {
                    if (projectile.Bounds.Intersects(enemy.Bounds))
                    {
                        TriggerExplosion(projectile.Position);
                        mainProjectiles.RemoveAt(i);

                        enemiesToRemove.Add(enemy);
                        KillCount++;

                        break; // Exit the inner loop since the projectile is already removed
                    }
                }
            }

            // Remove enemies that were hit
            foreach (var enemy in enemiesToRemove)
            {
                enemies.Remove(enemy);
            }

            if (enemies.Count == 0)
            {
                EnemyCount++;
                CreateEnemies();
            }
            

            // Check for collisions between enemy bullets and the player
            foreach (var projectile in enemyProjectiles)
            {
                Rectangle playerBounds = new Rectangle((int)PlayerPosition.X, (int)PlayerPosition.Y, PlayerSprite.Width, PlayerSprite.Height);
                if (projectile.Bounds.Intersects(playerBounds)) // Assuming PlayerBounds is a property for player collision detection
                {
                    //PlayerSprite.Dispose();

                    TriggerExplosion(PlayerPosition); // Explosion effect at player's position

                    gameState = GameState.GameOver;

                    //Exit(); // End the game
                    //return; // Exit the method
                }
            }

            base.Update(gameTime);
        }

        private void TriggerExplosion(Vector2 position)
        {
            // Create explosion effect using small dots (similar to bullet boundary logic)
            for (int i = 0; i < 10; i++) // Adjust the number of dots as needed
            {
                float angle = (float)(random.NextDouble() * Math.PI * 2);
                Vector2 smallDotVelocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * 2f;
                Projectile smallDot = new Projectile(position, smallDotVelocity, smallDotTexture, 1f);
                explosionDots.Add(smallDot);
            }
        }

        private async Task PlayerMovementV2()
        {
            // Get the current state of the gamepad, keyboard, and mouse
            GamePadState gamePadState = GamePad.GetState(PlayerIndex.One, GamePadDeadZone.Circular);
            if (gamePadState.IsConnected)
            {
                if (gamePadState.Buttons.Back == ButtonState.Pressed)
                    Exit();

                HandleGamepadInput(gamePadState);

                previousGamepadState = gamePadState;
            }
            else
            {
                MouseState mouseState = Mouse.GetState();
                KeyboardState keyboardState = Keyboard.GetState();

                if (keyboardState.IsKeyDown(Keys.Escape))
                    Exit();

                await HandleKeyboardAndMouseInput(keyboardState, mouseState);

                previousKeyboardState = keyboardState;
            }
        }

        private void FireProjectile(Vector2 direction)
        {
            // Calculate projectile velocity
            Vector2 projectileVelocity = Vector2.Normalize(direction) * 15f; // Adjust speed as needed

            // Create and add the projectile
            Projectile newProjectile = new Projectile(new(PlayerPosition.X, PlayerPosition.Y), projectileVelocity, projectileTexture);
            mainProjectiles.Add(newProjectile);

            BulletFired.Play();
        }

        private void HandleGamepadInput(GamePadState gamePadState)
        {
            testState = gamePadState;
            // Use left thumbstick for movement
            Vector2 leftThumbstick = gamePadState.ThumbSticks.Left;
            leftThumbstick.Y *= -1; // Invert Y-axis

            // Update player position based on thumbstick input
            if (leftThumbstick.LengthSquared() > 0.5f) // Add a dead zone
            {
                PlayerPosition += leftThumbstick * playerSpeed;
            }

            // Keep player within screen boundaries
            PlayerPosition.X = MathHelper.Clamp(PlayerPosition.X, 0, _graphics.PreferredBackBufferWidth - PlayerSprite.Width);
            PlayerPosition.Y = MathHelper.Clamp(PlayerPosition.Y, 0, _graphics.PreferredBackBufferHeight - PlayerSprite.Height);

            // Use right thumbstick for rotation
            Vector2 rightThumbstick = gamePadState.ThumbSticks.Right;
            rightThumbstick.Y *= -1; // Correctly inverting Y-axis

            if (rightThumbstick.LengthSquared() > 0.5f) // Add a dead zone
            {
                // Use right thumbstick for rotation
                //rightThumbstick.Y *= -1; // Correctly inverting Y-axis

                if (rightThumbstick.LengthSquared() > 0.1f) // Add a dead zone
                {
                    // Calculate the rotation angle to align with the thumbstick direction
                    rotationAngle = (float)Math.Atan2(rightThumbstick.Y, rightThumbstick.X) + MathHelper.PiOver2;

                    // Ensure the angle is normalized between 0 and 2 * Math.PI
                    if (rotationAngle < 0)
                    {
                        rotationAngle += MathHelper.TwoPi;
                    }
                }
            }

            // Fire projectile with gamepad
            if (gamePadState.Buttons.RightShoulder == ButtonState.Pressed && previousGamepadState.Buttons.RightShoulder == ButtonState.Released)
            {
                Vector2 projectileDirection = new Vector2((float)Math.Cos(rotationAngle - MathHelper.PiOver2),
                                                          (float)Math.Sin(rotationAngle - MathHelper.PiOver2));
                FireProjectile(projectileDirection);
            }
        }

        private async Task HandleKeyboardAndMouseInput(KeyboardState keyboardState, MouseState mouseState)
        {
            // Get input for movement
            Vector2 inputDirection = Vector2.Zero;
            if (keyboardState.IsKeyDown(Keys.W))
                inputDirection.Y -= 1;
            if (keyboardState.IsKeyDown(Keys.A))
                inputDirection.X -= 1;
            if (keyboardState.IsKeyDown(Keys.S))
                inputDirection.Y += 1;
            if (keyboardState.IsKeyDown(Keys.D))
                inputDirection.X += 1;

            // Normalize input direction
            if (inputDirection != Vector2.Zero)
                inputDirection.Normalize();

            // Update player position
            PlayerPosition += inputDirection * playerSpeed;

            // Keep player within screen boundaries
            PlayerPosition.X = MathHelper.Clamp(PlayerPosition.X, 0, _graphics.PreferredBackBufferWidth - PlayerSprite.Width);
            PlayerPosition.Y = MathHelper.Clamp(PlayerPosition.Y, 0, _graphics.PreferredBackBufferHeight - PlayerSprite.Height);

            // Calculate the rotation angle to face the cursor
            Vector2 mousePosition = new Vector2(mouseState.X, mouseState.Y);
            Vector2 direction = mousePosition - PlayerPosition;
            rotationAngle = (float)Math.Atan2(direction.Y, direction.X) + MathHelper.PiOver2;

            // Fire projectile with keyboard
            if (keyboardState.IsKeyDown(Keys.Space) && previousKeyboardState.IsKeyUp(Keys.Space))
            {
                Vector2 projectileDirection = mousePosition - PlayerPosition;
                FireProjectile(projectileDirection);
            }
            previousMouseState = mouseState;
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            // TODO: Add your drawing code here
            _spriteBatch.Begin();

            if (gameState == GameState.Playing)
            {

                // Draw main projectiles
                foreach (var projectile in mainProjectiles)
                {
                    projectile.Draw(_spriteBatch);
                }

                // Draw enemy projectiles
                foreach (var projectile in enemyProjectiles)
                {
                    projectile.Draw(_spriteBatch);
                }

                // Draw explosion dots
                foreach (var dot in explosionDots)
                {
                    dot.Draw(_spriteBatch);
                }

                // Draw enemies
                foreach (var enemy in enemies)
                {
                    enemy.Draw(_spriteBatch);
                }

                // Draw the sprite with rotation
                _spriteBatch.Draw(PlayerSprite, PlayerPosition, null, Color.White, rotationAngle, spriteOrigin, 1f, SpriteEffects.None, 0f);

                string textToDisplay = "Enemies Defeated: " + KillCount; // Replace with the actual value or variable you want to display
                Vector2 textSize = font.MeasureString(textToDisplay);
                Vector2 textPosition = new Vector2(GraphicsDevice.Viewport.Width - textSize.X - 50, 10); // 10-pixel padding from right and top edges
                _spriteBatch.DrawString(font, textToDisplay, textPosition, Color.White);

                //// Calculate position for the text in the upper right corner
                //string textToDisplay = "Right Stick POS: " + rotationAngle; // Replace with the actual value or variable you want to display
                //Vector2 textSize = font.MeasureString(textToDisplay);
                //Vector2 textPosition = new Vector2(GraphicsDevice.Viewport.Width - textSize.X - 50, 10); // 10-pixel padding from right and top edges
                //_spriteBatch.DrawString(font, textToDisplay, textPosition, Color.White);

                //string textToDisplay2 = $"Right Stick Y: {testState.ThumbSticks.Right.Y} : X: {testState.ThumbSticks.Right.X}"; // Replace with the actual value or variable you want to display
                //Vector2 textPosition2 = new Vector2(GraphicsDevice.Viewport.Width - textSize.X - 50, 40); // 10-pixel padding from right and top edges
                //_spriteBatch.DrawString(font, textToDisplay2, textPosition2, Color.White);

                //string textToDisplay3 = $"Enemy proj: {enemyProjectiles.Count}"; // Replace with the actual value or variable you want to display
                //Vector2 textPosition3 = new Vector2(GraphicsDevice.Viewport.Width - textSize.X - 50, 80); // 10-pixel padding from right and top edges
                //_spriteBatch.DrawString(font, textToDisplay3, textPosition3, Color.White);

                string textToDisplay2 = $"Enemy Count: {enemies.Count}"; // Replace with the actual value or variable you want to display
                Vector2 textPosition2 = new Vector2(GraphicsDevice.Viewport.Width - textSize.X - 50, 40); // 10-pixel padding from right and top edges
                _spriteBatch.DrawString(font, textToDisplay2, textPosition2, Color.White);
            }
            else if (gameState == GameState.GameOver)
            {
                string gameOverText = "Game Over\nPress Enter or Start to Restart\nPress Select or Esc to End";
                Vector2 textSize = font.MeasureString(gameOverText);
                Vector2 textPosition = new Vector2(
                    (_graphics.PreferredBackBufferWidth - textSize.X) / 2, // Center horizontally
                    (_graphics.PreferredBackBufferHeight - textSize.Y) / 2 // Center vertically
                );
                _spriteBatch.DrawString(font, gameOverText, textPosition, Color.White);
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
