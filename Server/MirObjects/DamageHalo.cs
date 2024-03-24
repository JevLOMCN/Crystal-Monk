using Server.MirEnvir;
using System.Drawing;

namespace Server.MirObjects
{
    class DamageHalo : BuffV2
    {
        public override void Tick()
        {
            int value = (int)Values[2];
            Point location = (Point)Values[3];
            Map map = Owner.CurrentMap;
            PlayerObject player = (PlayerObject)Owner;
            for (int y = location.Y - 3; y <= location.Y + 3; y++)
            {
                if (y < 0) continue;
                if (y >= map.Height) break;

                for (int x = location.X - 3; x <= location.X + 3; x++)
                {
                    if (x < 0) continue;
                    if (x >= map.Width) break;

                    var cellObjects = map.GetCellObjects(x, y);

                    if ( cellObjects == null) continue;

                    for (int i = 0; i < cellObjects.Count; i++)
                    {
                        MapObject target = cellObjects[i];
                        switch (target.Race)
                        {
                            case ObjectType.Monster:
                            case ObjectType.Player:
                                //Only targets
                                if (!target.IsAttackTarget(player)) break;

                                target.Attacked(player, value, DefenceType.MAC, false);
                                break;
                        }
                    }
                }
            }
        }
    }
}
