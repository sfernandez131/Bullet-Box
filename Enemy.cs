using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System;

namespace BoxMan
{
    public class Enemy
    {
        private Random random = new Random();
        public Vector2 Position { get; set; }
        public Texture2D Texture { get; private set; }
        public float ShootInterval { get; private set; }
        public float TimeSinceLastShot { get; set; }
        public float MovementSpeed { get; private set; } = 2f; // Adjust speed as needed
        private Vector2 velocity;
        private float avoidDistance = 100f; // Distance to start avoiding bullets

        // Property to get the bounding rectangle of the enemy
        public Rectangle Bounds
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
            }
        }

        public Enemy(Texture2D texture, Vector2 position, float shootInterval)
        {
            Texture = texture;
            Position = position;
            ShootInterval = shootInterval;
            TimeSinceLastShot = 0;

            // Initialize random movement direction
            ChangeDirection();
        }

        public void Update(GameTime gameTime, Vector2 playerPosition, Texture2D projectileTexture, List<Projectile> playerProjectiles, 
            List<Projectile> enemyProjectiles, GraphicsDeviceManager graphics)
        {
            TimeSinceLastShot += (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Check for nearby player projectiles and avoid them
            foreach (var projectile in playerProjectiles)
            {
                if (Vector2.Distance(Position, projectile.Position) < avoidDistance)
                {
                    // Change direction to avoid the bullet
                    ChangeDirection();
                    break; // Avoid multiple direction changes in one update
                }
            }

            // Move the enemy
            Position += velocity * MovementSpeed;

            // Move the enemy
            Vector2 newPosition = Position + velocity * MovementSpeed;

            // Keep enemy within screen boundaries
            int screenWidth = graphics.PreferredBackBufferWidth;
            int screenHeight = graphics.PreferredBackBufferHeight;

            if (newPosition.X < 0)
            {
                newPosition.X = 0;
                ChangeDirection();
            }
            else if (newPosition.X + Texture.Width > screenWidth)
            {
                newPosition.X = screenWidth - Texture.Width;
                ChangeDirection();
            }

            if (newPosition.Y < 0)
            {
                newPosition.Y = 0;
                ChangeDirection();
            }
            else if (newPosition.Y + Texture.Height > screenHeight)
            {
                newPosition.Y = screenHeight - Texture.Height;
                ChangeDirection();
            }

            // Set the new position
            Position = newPosition;

            // Shooting logic
            if (TimeSinceLastShot >= ShootInterval)
            {
                Shoot(playerPosition, projectileTexture, enemyProjectiles);
                TimeSinceLastShot = 0;
            }
        }

        private void ChangeDirection()
        {
            // Change movement direction randomly
            float angle = (float)(random.NextDouble() * Math.PI * 2);
            velocity = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        private void Shoot(Vector2 playerPosition, Texture2D projectileTexture, List<Projectile> enemyProjectiles)
        {
            // Calculate the direction from enemy to player
            Vector2 direction = playerPosition - Position;
            direction.Normalize(); // Normalize the direction vector

            // Calculate projectile velocity
            float projectileSpeed = 20f; // Adjust the speed as needed
            Vector2 projectileVelocity = direction * projectileSpeed;

            // Create and add the projectile
            Projectile enemyProjectile = new Projectile(Position, projectileVelocity, projectileTexture); // Use an appropriate texture for the enemy projectile
            enemyProjectiles.Add(enemyProjectile); // Assuming mainProjectiles is a list that handles all projectiles in the game
        }


        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, Color.Red); // Draw the enemy as a red square
        }
    }

}
