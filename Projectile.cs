using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BoxMan
{
    public class Projectile
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public Texture2D Texture;
        public float Lifespan; // Lifespan in seconds
        public bool IsExpired => Lifespan <= 0;

        // Property to get the bounding rectangle of the projectile
        public Rectangle Bounds
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
            }
        }

        public Projectile(Vector2 position, Vector2 velocity, Texture2D texture, float lifespan = float.MaxValue)
        {
            Position = position;
            Velocity = velocity;
            Texture = texture;
            Lifespan = lifespan;
        }

        public void Update(GameTime gameTime)
        {
            Position += Velocity;
            Lifespan -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, Color.White);
        }
    }

}
