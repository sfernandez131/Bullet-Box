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

        // Add an Owner property
        public object Owner { get; private set; }

        // Property to get the bounding rectangle of the projectile
        public Rectangle Bounds
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, Texture.Width, Texture.Height);
            }
        }

        // Modify the constructor to accept the owner of the projectile
        public Projectile(Vector2 position, Vector2 velocity, Texture2D texture, object owner)
        {
            Position = position;
            Velocity = velocity;
            Texture = texture;
            Owner = owner;
            Lifespan = 1f;
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
