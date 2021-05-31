using System;

using static Glyph.Utilities.CheckBetween;

using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Glyph.Utilities
{
    public class Sprite
    {
        Texture2D SpriteSheet;

        public Vector2 Position { get; set; }

        public Animation animSprite;
        Animation currentAnimation;

        public int Width = (int)(21 * GlobalConstants.SpriteScaleOutside);
        public int Height = (int)(28 * GlobalConstants.SpriteScaleOutside);

        private bool stopped;

        public Rectangle BoundingBox
        {
            get { return new Rectangle((int)Position.X, (int)Position.Y, Width, Height); }
        }

        public Rectangle Reach
        {
            get { return new Rectangle((int) (Game1.player.NextPosition.X),
                (int) (Game1.player.NextPosition.Y), Width, Height);  }
        }

        private int CellHeight;
        private int CellWidth;
        private string FileName;
        private int SheetWidth;
        private int SheetHeight;
        private int FrameCount;
        private int FrameMin;
        private double FrameDuration;
        private int DisplayWidth;
        private int DisplayHeight;
        private Color color;

        public Sprite() { }

        public Sprite(GraphicsDevice graphicsDevice, string FileName,
            int CellHeight, int CellWidth, int SheetWidth, int SheetHeight,
            int FrameCount, double FrameDuration, int DisplayWidth, int DisplayHeight
            , Color color, int FrameMin = 0)
        {
            this.FileName = FileName;
            this.CellHeight = CellHeight;
            this.CellWidth = CellWidth;
            this.SheetWidth = SheetWidth;
            this.SheetHeight = SheetHeight;
            this.FrameCount = FrameCount;
            this.FrameDuration = FrameDuration;
            this.DisplayWidth = DisplayWidth;
            this.DisplayHeight = DisplayHeight;
            this.color = color;
            animSprite = new Animation();

            if (SpriteSheet == null)
            {
                using (var stream = TitleContainer.OpenStream(FileName))
                {
                    SpriteSheet = Texture2D.FromStream(graphicsDevice, stream);
                }
            }

            if (CellHeight == 0 || CellWidth == 0)
            {
                throw new System.DivideByZeroException();
            }
            else
            {
                int loopCount = 0;

                for (int i = 0; i < SheetHeight / CellHeight; i++)
                {
                    for (int j = 0; j < SheetWidth / CellWidth; j++)
                    {
                        loopCount++; 
                        if (IsBetween(FrameMin, FrameCount, loopCount)
                            || IsBetween(FrameCount, FrameMin, loopCount))
                        {
                            animSprite.AddFrame(new Rectangle(j * CellWidth,
                                i * CellHeight, DisplayWidth, DisplayHeight),
                                TimeSpan.FromSeconds(FrameDuration));
                        }
                    }
                }
            }
        }

        public void Update(GameTime gameTime, bool stopped = false)
        {
            this.stopped = stopped;
            currentAnimation = animSprite;
            currentAnimation.Update(gameTime);
        }

        public void Draw(SpriteBatch spriteBatch, float scale = GlobalConstants.SpriteScaleOutside) 
        {
            try
            {
                var sourceRectangle = currentAnimation.DefaultRectangle;
                if (!stopped)
                {
                    sourceRectangle = currentAnimation.CurrentRectangle;
                }
                spriteBatch.Draw(SpriteSheet, Position, sourceRectangle, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
            }
            // The animation should just display the last frame
            catch (System.NullReferenceException)
            {
            }
        }
    }
}
