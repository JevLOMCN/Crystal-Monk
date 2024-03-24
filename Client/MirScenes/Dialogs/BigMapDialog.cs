using Client.MirControls;
using Client.MirGraphics;
using Client.MirObjects;
using Client.MirSounds;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client.MirScenes.Dialogs
{
    public sealed class BigMapDialog : MirControl
    {
        private MirLabel CoordLabel;

        private int StartPointX;
        private int StartPointY;
        private float ScaleX;
        private float ScaleY;

        public BigMapDialog()
        {
       //   NotControl = true;
            Location = new Point(130, 100);
            BeforeDraw += (o, e) => OnBeforeDraw();
            Sort = true;
            MouseDown += OnMouseClick;
            MouseMove += OnMouseMove;

            BorderColour = Color.Lime;
            Border = true;

            CoordLabel = new MirLabel
            {
                AutoSize = true,
                BackColour = Color.Transparent,
                ForeColour = Color.White,
                Parent = this,
            };
        }

        private void OnBeforeDraw()
        {
            MapControl map = GameScene.Scene.MapControl;
            if (map == null || !Visible) return;

            //int index = map.BigMap <= 0 ? map.MiniMap : map.BigMap;
            int index = map.BigMap;

            if (index <= 0)
            {
                if (Visible)
                {
                    Visible = false;
                }
                return;
            }

            //TrySort();

            Rectangle viewRect = new Rectangle(0, 0, 600, 400);

            Size = Libraries.MiniMap.GetSize(index);

            if (Size.Width < 600)
                viewRect.Width = Size.Width;

            if (Size.Height < 400)
                viewRect.Height = Size.Height;

            viewRect.X = (Settings.ScreenWidth - viewRect.Width) / 2;
            viewRect.Y = (Settings.ScreenHeight - 120 - viewRect.Height) / 2;

            Location = viewRect.Location;
            Size = viewRect.Size;

            ScaleX = Size.Width / (float)map.Width;
            ScaleY = Size.Height / (float)map.Height;

            viewRect.Location = new Point(
                (int)(ScaleX * MapObject.User.CurrentLocation.X) - viewRect.Width / 2,
                (int)(ScaleY * MapObject.User.CurrentLocation.Y) - viewRect.Height / 2);

            if (viewRect.Right >= Size.Width)
                viewRect.X = Size.Width - viewRect.Width;
            if (viewRect.Bottom >= Size.Height)
                viewRect.Y = Size.Height - viewRect.Height;

            if (viewRect.X < 0) viewRect.X = 0;
            if (viewRect.Y < 0) viewRect.Y = 0;

            Libraries.MiniMap.Draw(index, Location, Size, Color.FromArgb(255, 255, 255));

            StartPointX = (int)(viewRect.X / ScaleX);
            StartPointY = (int)(viewRect.Y / ScaleY);

            for (int i = MapControl.Objects.Count - 1; i >= 0; i--)
            {
                MapObject ob = MapControl.Objects[i];


                if (ob.Race == ObjectType.Item || ob.Dead || ob.Race == ObjectType.Spell) continue; // || (ob.ObjectID != MapObject.User.ObjectID)
                float x = ((ob.CurrentLocation.X - StartPointX) * ScaleX) + Location.X;
                float y = ((ob.CurrentLocation.Y - StartPointY) * ScaleY) + Location.Y;

                Color colour;

                if ((GroupDialog.GroupList.Contains(ob.Name) && MapObject.User != ob) || ob.Name.EndsWith(string.Format("({0})", MapObject.User.Name)))
                    colour = Color.FromArgb(0, 0, 255);
                else
                    if (ob is PlayerObject)
                    colour = Color.FromArgb(255, 255, 255);
                else if (ob is NPCObject || ob.AI == 6)
                    colour = Color.FromArgb(0, 255, 50);
                else
                    colour = Color.FromArgb(255, 0, 0);

                DXManager.Sprite.Draw2D(DXManager.RadarTexture, Point.Empty, 0, new PointF((int)(x - 0.5F), (int)(y - 0.5F)), colour);
            }


            if (GameScene.Scene.MapControl.AutoPath)
            {
                foreach (var node in GameScene.Scene.MapControl.CurrentPath)
                {
                    Color colour = Color.White;

                    float x = ((node.Location.X - StartPointX) * ScaleX) + Location.X;
                    float y = ((node.Location.Y - StartPointY) * ScaleY) + Location.Y;

                    DXManager.Sprite.Draw2D(DXManager.RadarTexture, Point.Empty, 0, new PointF((int)(x - 0.5F), (int)(y - 0.5F)), colour);
                }
            }
        }


        public void Toggle()
        {
            Visible = !Visible;

            Redraw();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            MapControl map = GameScene.Scene.MapControl;
            if (map == null || !Visible) return;

            int index = map.BigMap;

            if (index <= 0)
            {
                if (Visible)
                    Visible = false;
                return;
            }

            int X = (int)Math.Floor((CMain.MPoint.X - Location.X) / ScaleX) + StartPointX;
            int Y = (int)Math.Floor((CMain.MPoint.Y - Location.Y) / ScaleY) + StartPointY;

            CoordLabel.Text = String.Format("{0}:{1}", X, Y);
        }

        private void OnMouseClick(object sender, MouseEventArgs e)
        {
            MapControl map = GameScene.Scene.MapControl;
            if (map == null || !Visible) return;

            int index = map.BigMap;

            if (index <= 0)
            {
                if (Visible)
                    Visible = false;
                return;
            }

            int X = (int)Math.Floor((CMain.MPoint.X - Location.X) / ScaleX) + StartPointX;
            int Y = (int)Math.Floor((CMain.MPoint.Y - Location.Y) / ScaleY) + StartPointY;

            var path = GameScene.Scene.MapControl.PathFinder.FindPath(MapObject.User.CurrentLocation, new Point(X, Y));

            if (path == null || path.Count == 0)
            {
                GameScene.Scene.ChatDialog.ReceiveChatTr(string.Format("Could not find suitable path. {0} {1}", X, Y ), ChatType.System);
            }
            else
            {
                GameScene.Scene.MapControl.CurrentPath = path;
                GameScene.Scene.MapControl.AutoPath = true;
            }
        }
    }


}
