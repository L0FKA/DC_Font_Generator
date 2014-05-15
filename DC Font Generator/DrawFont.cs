using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DC_Font_Generator
{
    class DrawFont
    {
        public Font _Font; //目前字型
        private FontFamily fontFamily;
        public float ascentPixel = 0; //目前字型上升值
        public float descentPixel = 0; //目前字型下降值
        public float lineSpacingPixel = 0;//目前字型行距
        

        public Color BackColor = Color.FromArgb(0, Color.Black);
        private Color fontColor = Color.FromArgb(0xFF, Color.White);
        public Color OutlineColor = Color.FromArgb(0xFF, Color.FromArgb(80, 80, 80));
        public int OutlineWidth = 0;
        private Brush brush = new Pen(Color.FromArgb(200, Color.FromArgb(80, 80, 80)), 2f).Brush;
        private Brush brush2;
        private Bitmap image;
        private Graphics g;
        private int BackGround ;
        private SolidBrush sfbrush = new SolidBrush(Color.FromArgb(255, 255, 255));
        private GraphicsPath path = new GraphicsPath();
        private StringFormat strformat = new StringFormat();
        private Bitmap CDZ_image;
        private Graphics CDZ_g;
        public float CDZ_BottomAlign = 0; //CDZ的底部對齊位置
        private Pen[] GlowPen;
        private int glow = 4;
        private Color glowcolor = Color.FromArgb(0x80, 0x80, 0x80, 0x80);
        public float SpaceWidth = 0; //空白字型的寬度

        
        public int DrawMode = 1; //0=無特效 1=反鋸齒
        public DrawFont()
        {
            sfbrush = new SolidBrush(fontColor);
            brush2 = new Pen(fontColor).Brush;
            BackGround = BackColor.ToArgb();
            CreateGlow();
        }
        /// <summary>
        /// 製作glow用筆刷
        /// </summary>
        private void CreateGlow()
        {
            int size = OutlineWidth + glow;
            int glow_step = 0x80 / (glow + 1);
            int gs = glow_step;
            GlowPen = new Pen[glow + OutlineWidth];
            for (int i = 0; i < glow + OutlineWidth; i++)
            {
                GlowPen[i] = new Pen(Color.FromArgb(gs, glowcolor.R, glowcolor.G, glowcolor.B), size - i);
                GlowPen[i].LineJoin = LineJoin.Round;
                if (i >= OutlineWidth)
                    gs += glow_step;

            }
        }
        private void CreateOutline()
        {

        }
        public int Glow
        {
            set
            {
                if (glow != value)
                {
                    glow = value;
                    CreateGlow();
                    CreateDrawingZone();
                }
            }
            get { return glow; }
        }
        public Color GlowColor
        {
            set
            {
                if (glowcolor != value)
                {
                    glowcolor=value;
                    CreateGlow();
                    
                }
            }
            get { return glowcolor; }
        }
        public int Outline
        {
            set
            {
                if (OutlineWidth != value)
                {
                    OutlineWidth = value;
                    CreateDrawingZone();
                }
            }
        }
        public Color FontColor
        {
            set
            {
                if (fontColor != value)
                {
                    fontColor = value;
                    sfbrush = new SolidBrush(fontColor);
                    brush2 = new Pen(fontColor).Brush;
                }
            }
            get { return fontColor; }
        }
        /// <summary>
        /// 設定現在使用的字型
        /// </summary>
        public Font FontData
        {
            set
            {
                if (_Font != value)
                {
                    _Font = value;
                    fontFamily = _Font.FontFamily;

                    int ascent;             // font family ascent in design units
                    int descent;            // font family descent in design units
                    int lineSpacing;        // font family line spacing in design units

                    int em = fontFamily.GetEmHeight(_Font.Style);

                    ascent = fontFamily.GetCellAscent(_Font.Style); //上升
                    // 14.484375 = 16.0 * 1854 / 2048
                    ascentPixel = _Font.Size * ascent / em; //實際上升值

                    // Display the descent in design units and pixels.
                    descent = fontFamily.GetCellDescent(_Font.Style);
                    // 3.390625 = 16.0 * 434 / 2048
                    descentPixel = _Font.Size * descent / em;


                    // Display the line spacing in design units and pixels.
                    lineSpacing = fontFamily.GetLineSpacing(_Font.Style); //行距
                    // 18.398438 = 16.0 * 2355 / 2048
                    lineSpacingPixel = _Font.Size * lineSpacing / em;

                    CreateDrawingZone();//建立繪字空間
                    CreateSpaceWidth();//建立Space的寬度
                }
            }
            get { return _Font; }
        }
        private void CreateDrawingZone()
        {
            int shift = (OutlineWidth * 2) + (glow * 2);
            CDZ_image = new Bitmap((int)(lineSpacingPixel + (shift*2)), (int)(lineSpacingPixel + (shift*2)));

            CDZ_g = Graphics.FromImage(CDZ_image);

            if (DrawMode == 1)
            {
                CDZ_g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                CDZ_g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                CDZ_g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
                CDZ_g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                CDZ_g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            }
            else
            {
                CDZ_g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                CDZ_g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
                CDZ_g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
                CDZ_g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                CDZ_g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            }
            CDZ_g.Clear(BackColor);

            //建立底部對齊點
            CDZ_BottomAlign = (shift/2) + ascentPixel;
        }

        /// <summary>
        /// 建立Space的寬度
        /// </summary>
        private void CreateSpaceWidth()
        {
            SizeF ms = CDZ_g.MeasureString(" ", _Font); //用系統繪字測量
            SpaceWidth = ms.Width;

        }
        /// <summary>
        /// 繪製文字
        /// </summary>
        /// <param name="c">字元</param>
        /// <param name="BottomAlign">底部對齊傳出值</param>
        /// <returns></returns>
        public Bitmap DrawingFont(char c,out float BottomAlign)
        {

            Bitmap Image = CDZ_image;
            CDZ_g.Clear(BackColor);
            int shift = (glow + OutlineWidth) ;
            path = new GraphicsPath();
            Point pos = new Point(shift, shift);

            path.AddString(c.ToString(), fontFamily, (int)_Font.Style, _Font.Size, pos, strformat); //繪製原字型
            if (glow > 0)
            {
                for (int i = 1; i <= glow; ++i) { CDZ_g.DrawPath(GlowPen[i - 1], path); }
            }
            if (OutlineWidth > 0)
            {
                Pen pen2 = new Pen(OutlineColor, OutlineWidth);
                pen2.LineJoin = LineJoin.Round;
                CDZ_g.DrawPath(pen2, path);

            }
            if (DrawMode == 1)
            {
                CDZ_g.FillPath(sfbrush, path);
            }
            else
            {
                CDZ_g.DrawString(c.ToString(), _Font, sfbrush, pos);
            }

            Rectangle ef= GetFontGSize(Image); //取得字型真實大小
            Bitmap crop = cropImage(Image, ef); //裁切後回傳
            BottomAlign = CDZ_BottomAlign - (float)ef.Y; //計算底部對齊
            return crop;

        }

        /// <summary>
        /// 取得原字型真實高度
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public Size GetOriginFontHeight(char c, out SizeF DisplaySize, out float RealSpace)
        {
            //製作標準字型
            image = new Bitmap((int)lineSpacingPixel*2, (int)lineSpacingPixel);
            g = Graphics.FromImage(image);
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.Default;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Default;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
            g.Clear(BackColor);
            Rectangle p2 = new Rectangle(0, 0, image.Width, image.Height);
            lock (g)// !!
            {
                g.DrawString(c.ToString(), _Font, brush2, p2.Location);
            }
            Rectangle ef1 = GetFontGSize(image); //取得原始字型真實大小
            DisplaySize = g.MeasureString(c.ToString(), _Font); //用系統繪字測量


            if (ef1.Height == 0 && ef1.Width == 0)
            {
                //是空白
                ef1.Width = (int)SpaceWidth;
                RealSpace = (int)SpaceWidth;
            }
            else
            {
                //測量出間距(用兩個同樣的字去算出距離)
                g.Clear(BackColor);
                g.DrawString(c.ToString() + c.ToString(), _Font, brush2, p2.Location);
                Rectangle realdoublespace = GetFontGSize(image); //取得原始字型真實大小
                RealSpace = (realdoublespace.Width - (ef1.Width * 2)) / 4;
            }
            image.Dispose();
            g.Dispose();
            Size sf = new Size(ef1.Width, ef1.Height);
            return sf;
        }
        public Bitmap GetOriginFont(char c,out bool IsEmpty)
        {
            IsEmpty = true;
            //製作標準字型
            image = new Bitmap((int)lineSpacingPixel, (int)lineSpacingPixel);
            g = Graphics.FromImage(image);
            g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.Default;
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Default;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Default;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixel;
            g.Clear(BackColor);
            Rectangle p2 = new Rectangle(0, 0, image.Width, image.Height);
            g.DrawString(c.ToString(), _Font, brush2, p2.Location);
            g.Dispose();
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    if (!image.GetPixel(x, y).ToArgb().Equals(BackGround))
                    {
                        IsEmpty = false;
                        break;
                    }
                }
                if (!IsEmpty) break;
            }
            return image;
        }

        /// <summary>
        /// 裁切bitmap
        /// </summary>
        /// <param name="img">原始bitmap</param>
        /// <param name="cropArea">正方形</param>
        /// <returns>裁好的bitmap</returns>
        public Bitmap cropImage(Bitmap img, Rectangle cropArea)
        {
            Bitmap bmpImage = new Bitmap(img);
            if (cropArea.Height == 0 || cropArea.Width == 0) return bmpImage;
            Bitmap bmpCrop = bmpImage.Clone(cropArea, bmpImage.PixelFormat);
            return bmpCrop;
        }
        /// <summary>
        /// 取得字型真實大小
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        public Rectangle GetFontGSize(Bitmap image)
        {
            Rectangle ef = new Rectangle();
            //取得左上Y
            bool flag = false;
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    //Color c = image.GetPixel(x, y);
                    if (!image.GetPixel(x, y).ToArgb().Equals(BackGround))
                    {
                        ef.Y = y;
                        flag = true;
                        break;
                    }
                }
                if (flag) break;
            }
            if (!flag) return ef;
            //取得左上X
            flag = false;
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    if (!image.GetPixel(x, y).ToArgb().Equals(BackGround))
                    {
                        ef.X = x;
                        flag = true;
                        break;
                    }
                }
                if (flag) break;
            }
            if (!flag) return ef;

            flag = false;
            //取得右下Y
            for (int y = image.Height - 1; y >= 0; y--)
            {
                for (int x = image.Width - 1; x >= 0; x--)
                {
                    if (!image.GetPixel(x, y).ToArgb().Equals(BackGround))
                    {
                        ef.Height = y - ef.Y + 1;
                        flag = true;
                        break;
                    }
                }
                if (flag) break;
            }
            if (!flag) return ef;
            flag = false;
            //取得右下X
            for (int x = image.Width - 1; x >= 0; x--)
            {
                for (int y = image.Height - 1; y >= 0; y--)
                {
                    if (!image.GetPixel(x, y).ToArgb().Equals(BackGround))
                    {
                        ef.Width = x - ef.X + 1;
                        flag = true;
                        break;
                    }
                }
                if (flag) break;
            }
            return ef;
        }
        /// <summary>
        /// 複製圖
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="Target"></param>
        /// <param name="point"></param>
        public void CopyImage(Bitmap Source, ref Bitmap Target, Point point)
        {
            int sx = point.X;
            int sy = point.Y;
            int black = BackColor.ToArgb();

            for (int y = 0; y < Source.Height; y++)
            {
                
                sx = point.X;
                for (int x = 0; x < Source.Width; x++)
                {
                    Color pixel = Source.GetPixel(x, y);
                    if (pixel.ToArgb() != black)
                        Target.SetPixel(sx, sy, pixel);
                    sx++;
                    if (sx >= Target.Width) break;
                }
                sy++;
                if (sy >= Target.Height) break;
            }

        }
        public Point SearchSpace(Array2D.Char2D CharIndex, Rectangle ef)
        {
            Point fp = new Point(-1, -1);
            int black = BackColor.ToArgb();
            int LimitHeight = image.Height - ef.Height; //極限高度
            int LimitWidth = image.Width - ef.Width;//極限寬度
            bool Overflow = false;
            while (!Overflow)
            {
                //找到左上圖形

                for (int y = LimitHeight - 1; y <= 0; y--)
                {
                    for (int x = image.Width - 1; x <= 0; x--)
                    {
                        if (CharIndex==null)
                        {
                            fp.X = x + 1; fp.Y = y;
                            if (fp.X + ef.Width >= image.Width) { fp.X = -1; } //剩下寬度不足
                            

                            break;
                        }
                    }
                    if (fp.X != -1) break;
                }
                if (fp.X == -1 || fp.X >= image.Width) { Overflow = true; break; } //已經沒有空間
                //檢查高度
                for (int x2 = fp.X; x2 < LimitWidth; x2++)
                {
                    for (int y2 = fp.Y; y2 < ef.Height; y2++)
                    {
                        for (int x3 = fp.X; x3 < ef.Width; x3++)
                        {
                            Color pixel = image.GetPixel(x3, y2);
                            if (pixel.ToArgb() != black)
                            {

                            }
                        }
                    }
                }
            }

            return fp;
        }
        private bool SearchSpace_check(Bitmap image,Point p,Rectangle ef)
        {
            int black = BackColor.ToArgb();
            for (int x = p.X; x < ef.Width; x++)
            {
                for (int y = p.Y; y < ef.Height; y++)
                {
                    Color pixel = image.GetPixel(x, y);
                    if (pixel.ToArgb() != black)
                    {
                        return false;
                    }

                }
            }
            return true;
        }
    }
}