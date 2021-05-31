using Microsoft.Xna.Framework; 
using Microsoft.Xna.Framework.Input;
using System; 

namespace Glyph.Utilities
{
    public class CheckDoubleClick
    {
        public bool DoubleClicked; 
        const float TimerDelay = 0.01f;
        float lastTimeOfClick; 
        MouseState current;
        MouseState last; 

        public bool LeftClick()
        {
            return last.LeftButton == ButtonState.Pressed && current.LeftButton == ButtonState.Released;
        }

        public void Update(GameTime gameTime, MouseState current, MouseState last)
        {
            this.current = current;
            this.last = last;
            DoubleClicked = false;
            if (LeftClick() && gameTime.TotalGameTime.Seconds - lastTimeOfClick < TimerDelay)
            {
                DoubleClicked = true; 
            }
            if (LeftClick())
            {
                lastTimeOfClick = gameTime.TotalGameTime.Milliseconds;
            }
        }
    }
}
