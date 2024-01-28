using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace BoxMan
{
    public class Bomb
    {
        public Vector2 Position;
        public Texture2D Texture;
        public float Timer; // Duration of the explosion in seconds

        public Bomb(Vector2 position, Texture2D texture, float timer)
        {
            Position = position;
            Texture = texture;
            Timer = timer;
        }

        public void Update(GameTime gameTime)
        {
            // Decrease the timer by the elapsed game time
            Timer -= (float)gameTime.ElapsedGameTime.TotalSeconds;
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, Position, Color.White);
        }
    }

}
