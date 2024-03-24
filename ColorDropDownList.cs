using System;
using System.Drawing;
using System.Windows.Forms;

class ColorDropDownList : ComboBox
{
    public ColorDropDownList()
        : base()
    {
        this.DropDownStyle = ComboBoxStyle.DropDownList;
        this.DrawMode = DrawMode.OwnerDrawFixed;
        fillList();
        this.SelectedIndex = 0;
        this.DrawItem += new DrawItemEventHandler(ColorDropDownList_DrawItem);
    }

    private void fillList()
    {
        string[] names = Enum.GetNames(typeof(KnownColor));
        this.Items.AddRange(names);
    }

    private void ColorDropDownList_DrawItem(object sender, DrawItemEventArgs e)
    {
        if (e.Index < 0)
            return;

        string colorName = (string)this.Items[e.Index];
        Color color = Color.FromName(colorName);
        Rectangle rect = new Rectangle(e.Bounds.X, e.Bounds.Y, e.Bounds.Width / 4, e.Bounds.Height - 2);
        Brush brush = new SolidBrush(color);
        e.Graphics.DrawRectangle(new Pen(Color.Black), rect);
        rect.X += 1;
        rect.Y += 1;
        rect.Width -= 1;
        rect.Height -= 1;
        e.Graphics.FillRectangle(brush, rect);
        rect.Offset(rect.Width + 4, 0);
        e.Graphics.DrawString(colorName, e.Font, Brushes.Black, rect.Location);
    }
}