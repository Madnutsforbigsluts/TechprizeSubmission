using Microsoft.Xna.Framework;

namespace Glyph.Utilities
{
    public static class Collision
    {
        public static bool IsColliding(Rectangle entity1Bounds, Rectangle entity2Bounds)
        {
            if (entity1Bounds.Intersects(entity2Bounds)
                || entity2Bounds.Intersects(entity1Bounds))
            {
                return true;
            }
            return false;
        }
    }
}
