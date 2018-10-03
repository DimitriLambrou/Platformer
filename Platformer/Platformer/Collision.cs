using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platformer
{
    class Collision
    {
        public Game1 game;

        public bool IsColliding(Sprite hero, Sprite otherSprite)
        {
            // Compares the position of each rectangles edges against the other.
            // it compares opposite edges of each rectangle, ie, the left edge of one with the right of the other
            if (hero.rightEdge < otherSprite.leftEdge || hero.leftEdge > otherSprite.rightEdge || hero.bottomEdge < otherSprite.topEdge || hero.topEdge > otherSprite.bottomEdge)
            {
                // These two rectangles are not colliding
                return false;
            }
            //Otherwise, the two AABB rectangles 
            return true;
        }
    }
}
