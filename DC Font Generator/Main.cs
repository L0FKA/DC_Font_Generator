using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace DC_Font_Generator
{
    public class Main : System.IDisposable
    {
        #region Members

        public int ID = 0;
        public string name = ""; //fnt NAME 名稱
        private FL_FONT iFntFile;
        private Font ifont1;
        private Font ifont2;
        private bool iisTextOverFlow;

        public string ImportFont1name = "";
        public string ImportFont2name = "";

        public string PictureFileName = "";
        public event EventHandler TextOverFlow; //圖片空間不足事件

        public bool SkipASCII = false; //忽略ASCII的輸出
        public bool fixedFont = false; //等寬字旗標

        private Font _Font;
        private MainForm mainform;

        private DrawFont SysDraw = new DrawFont();
        public float FontMaxWidth = 17;
        public float FontMaxHeight = 0;

        public int DCfontLink = -1; //如果大於-1，代表fnt要使用別的區段
        public List<Main> parent;
        public List<bool> Fallout3INI = new List<bool>(8);//所屬ini的編號(由1開始)
        #endregion
        
        #region Constructors

        public Main(MainForm Mainform,List<Main> P,int id)
        {
            this.mainform = Mainform;
            this.parent = P;
            
            this.ifont1 = Control.DefaultFont;
            this.ifont2 = this.ifont1;
            this.NowFont = this.ifont1;
            this.iFntFile = new FL_FONT();
            this.iisTextOverFlow = false;

            this.ID = id;
            for (int i = 0; i < 8; i++)
                Fallout3INI.Add(false);
        }

        #endregion
        /// <summary>
        /// Пришивает к основному щрифту DC символы.
        /// </summary>
        public void LinkClone()
        {
            if (DCfontLink < 0) return;
            Main Source = parent[DCfontLink];
            int max = Source.FntFile.CharList.Count;
            if (Source.FntFile.CharList.Count < 256) return;
            
            for (int i = 256; i < max; i++)
            {
                Fnt_char sf = Source.FntFile.CharList[i];
                Fnt_char tf = new Fnt_char();

                tf.ID = ID;
                tf.IsDC = true;
                tf.c = sf.c;
                tf.HEX = sf.HEX;
                tf.Enable = sf.Enable;
                if (sf.Enable)
                {
                    tf.BottomAlign = sf.BottomAlign;
                    tf.BottomAlignFixed = sf.BottomAlignFixed;
                    
                    tf.charViewHeight = sf.charViewHeight;
                    tf.charViewHeightFixed = sf.charViewHeightFixed;
                    tf.charViewWidth = sf.charViewWidth;
                    tf.charViewWidthFixed = sf.charViewWidthFixed;
                    tf.Empty = sf.Empty;
                    tf.FixedWidth = sf.FixedWidth;
                    tf.IsSpace = sf.IsSpace;
                    tf.LeftSpace = sf.LeftSpace;
                    tf.LeftSpaceFixed = sf.LeftSpaceFixed;
                    tf.RightSpace = sf.RightSpace;
                    tf.RightSpaceFixed = sf.RightSpaceFixed;
                    tf.x1 = sf.x1;
                    tf.x2 = sf.x2;
                    tf.x3 = sf.x3;
                    tf.x4 = sf.x4;
                    tf.y1 = sf.y1;
                    tf.y2 = sf.y2;
                    tf.y3 = sf.y3;
                    tf.y4 = sf.y4;
                    
                }
                FntFile.Add(tf, tf.HEX, ID); 
            }
            //FntFile.EmptyDC
        }

        public void Clear()
        {
            if (this.ImportFont2name != "") return;
            if (this.ImportFont1name != "") 
                this.iFntFile.reset(true); //保留ASCII的部分
            else
                this.iFntFile.reset(false);
        }

        /// <summary>
        /// 重繪製
        /// </summary>
        public bool NewDrawing(FontEncoding enc)
        {
            this.iisTextOverFlow = false;
            this.TextOverFlow(this, new EventArgs());

            mainform.ProgressBar = 0;

            mainform.ProgressBarMax = enc.Temp.Count;

            NowFont = this.font1;
            

            int loop_count = -1;
            //製作全文字
            foreach (string str in enc.Temp)
            {
                mainform.ProgressBarAdd();
                loop_count++;
       
                if (this.iFntFile.CharList.Count >= 24322) continue; //已經都跑過了
                if (this.iFntFile.HasCode(str.Substring(2, 4)))
                {
                    continue; //已經有的字就跳過
                }
                if (DCfontLink > -1 && loop_count>255) continue; //是連結字

                if (str.Length == 6)
                {
                    this.iFntFile.AddEmpty(str.Substring(2, 4), ID); //無效的字碼
                    continue;
                }

                bool dc;
                bool IsError;
                char c = enc.CheckFontCode(str,out dc,out IsError);
                
                if (IsError)
                {
                    this.iFntFile.AddEmpty(str.Substring(2, 4),ID);
                    continue;
                }

                if (dc)
                {
                    NowFont = this.font2;
                }
                else
                {
                    NowFont = this.font1;

                }

                CreateFont(c, dc, str.Substring(2, 4));
            }

            //修正同寬字
            if (fixedFont)
            {
                FixedFont(fixedFont, this.FontMaxWidth);
            }
            else if (ImportFont1name == "" && ImportFont2name == "")
            {

                //this.iFntFile.Header.LineHeight = SysDraw.lineSpacingPixel;

                this.iFntFile.Header.LineHeight = (float)FontMaxHeight * 1.3f;//登記行高
            }
            return true;
        }
        /// <summary>
        /// 固定寬度修正
        /// </summary>
        /// <param name="IsFixed"></param>
        /// <param name="fixedwidth"></param>
        public void FixedFont(bool IsFixed,float fixedwidth)
        {
            this.fixedFont = IsFixed;
            if (IsFixed)
            {
                this.FontMaxWidth = fixedwidth;
                if (ImportFont1name == "") this.iFntFile.FixedWidth = fixedwidth;

                foreach (Fnt_char fnt in this.iFntFile.CharList)
                {
                    if (!fnt.Enable) continue;
                    //if (SkipASCII && !fnt.IsDC) continue;
                    //if (fnt.FixedWidth == FontMaxWidth) continue; //已經處理過

                    if (FontMaxWidth > fnt.charViewWidth)
                    {
                        float shift = ((float)FontMaxWidth - fnt.charViewWidth) / 2f;

                        fnt.LeftSpace = shift; fnt.LeftSpaceFixed = 0;
                        fnt.RightSpace = shift; fnt.RightSpaceFixed = 0;
                    }
                    else if (fnt.charViewWidth > FontMaxWidth)
                    {
                        float shift = (fnt.charViewWidth - (float)FontMaxWidth) / 2f;

                        fnt.LeftSpace = -shift; fnt.LeftSpaceFixed = 0;
                        fnt.RightSpace = -shift; fnt.RightSpaceFixed = 0;
                    }
                    else
                    {
                        fnt.LeftSpace = 0; fnt.LeftSpaceFixed = 0;
                        fnt.RightSpace = 0; fnt.RightSpaceFixed = 0;
                    }
                    fnt.FixedWidth = FontMaxWidth;
                }

            }
            
            
        }
        /// <summary>
        /// 製造文字圖像
        /// </summary>
        /// <param name="c"></param>
        /// <param name="font"></param>
        /// <param name="OriginalRectangleF"></param>
        /// <returns></returns>
        public bool DrawToScreen(Bitmap TextImage, ref Rectangle p, ref int LineShift, Fnt_char fnt, Array2D.List2D<Fnt_char> CharIndex, int gap, bool Vertical, Graphics graphics)
        {
            bool Overflow = false;
            if (!fnt.Enable) return Overflow;

            //尋找繪圖空間
            bool AddSpace = false;
            if (gap == 0 && fnt.IsSpace) //空白字要保留些周邊空間
            {
                gap = 1;

                if (Vertical)
                    p.X += 1;
                else
                    p.Y += 1;
                
                AddSpace = true;
            }

            if (Vertical)
            {
                //檢查Y邊界
                if ((p.Y + fnt.FontImage.Height + gap) >= TextImage.Height)
                {
                    p.Y = gap;
                    p.X += (int)LineShift + gap;
                    LineShift = fnt.FontImage.Width;
                }
                //檢查X邊界
                if (p.X + fnt.FontImage.Width >= TextImage.Width)
                {
                    this.iisTextOverFlow = true;
                    this.TextOverFlow(this, new EventArgs());
                    Overflow = true;
                    return Overflow;
                }
            }
            else
            {
                //檢查X邊界
                if ((p.X + fnt.FontImage.Width + gap) >= TextImage.Width)
                {
                    p.X = gap;
                    p.Y += (int)LineShift + gap;
                    LineShift = fnt.FontImage.Height;
                }
                //檢查Y邊界
                if (p.Y + fnt.FontImage.Height >= TextImage.Height)
                {
                    this.iisTextOverFlow = true;
                    this.TextOverFlow(this, new EventArgs());
                    Overflow = true;
                    return Overflow;
                }
            }

            //搬移image到this.iTextImage
            graphics.DrawImageUnscaledAndClipped(fnt.FontImage, new Rectangle(p.X, p.Y, fnt.FontImage.Width, fnt.FontImage.Height));
            int sx = p.X;
            int sy = p.Y;
            //int black = SysDraw.BackColor.ToArgb();
            for (int y = 0; y < fnt.FontImage.Height; y++)
            {
                sx = p.X;
                for (int x = 0; x < fnt.FontImage.Width; x++)
                {
                    //Color pixel = fnt.FontImage.GetPixel(x, y);
                    
                        //TextImage.SetPixel(sx, sy, pixel);
                    CharIndex[sx, sy] = fnt;
                    sx++;
                }
                sy++;
            }

            fnt.x1 = (float)p.X / (float)TextImage.Width;                 //圖片左上X
            fnt.y1 = (float)p.Y / (float)TextImage.Height;                //圖片左上Y

            fnt.x2 = (float)(p.X + fnt.FontImage.Width) / (float)TextImage.Width;    //圖片右上X
            fnt.y2 = (float)p.Y / (float)TextImage.Height;                //圖片右上Y

            fnt.x3 = (float)p.X / (float)TextImage.Width;                 //圖片左下X
            fnt.y3 = (float)(p.Y + fnt.FontImage.Height) / (float)TextImage.Height;  //圖片左下Y

            fnt.x4 = (float)(p.X + fnt.FontImage.Width) / (float)TextImage.Width;    //圖片右下X
            fnt.y4 = (float)(p.Y + fnt.FontImage.Height) / (float)TextImage.Height;  //圖片右下Y

            if (Vertical)
            {   
                if (LineShift < fnt.FontImage.Width + gap)
                {
                    LineShift = fnt.FontImage.Width + gap; //補足寬度
                }
                int LineShiftY = fnt.FontImage.Height + gap; //下個偏移座標X
                this.PointFOffset(ref p, 0, LineShiftY);
            }
            else
            {
                if (LineShift < fnt.FontImage.Height + gap)
                {
                    LineShift = fnt.FontImage.Height + gap;//補足高度
                }
                int LineShiftX = fnt.FontImage.Width + gap; //下個偏移座標X
                this.PointFOffset(ref p, LineShiftX, 0);
            }
            if (AddSpace)
            {
                if (Vertical)
                    p.Y += 1;
                else
                    p.X += 1;
            }
            return Overflow;

            //Rectangle ef3 = new Rectangle(p.X, p.Y, image.Width, image.Height);
            //return ef3;
        }

        private void CreateFont(char c, bool dc, string hex)
        {
            SizeF DisplaySize; float RealSpace;
            Rectangle p = new Rectangle();
            p.Size = SysDraw.GetOriginFontHeight(c, out DisplaySize,out RealSpace); //取得原文字圖形大小

            if (hex == "A3AC")
            {
                DisplaySize.Height = DisplaySize.Height;
            }

            bool IsSpace = false;
            if (p.Size.Height == 0)
                IsSpace = true;
            else
                IsSpace = false;


            Fnt_char fnt = new Fnt_char();
            fnt.c = c;
            fnt.IsDC = dc;

            float Height = 0;

            if (p.Width > 0)
            {
                SizeF ViewSize;
                float BottomAlign = 0;
                if (!IsSpace)
                {
                    //繪製文字

                    fnt.FontImage = SysDraw.DrawingFont(c,out BottomAlign);

                    ViewSize = new SizeF(fnt.FontImage.Width, fnt.FontImage.Height);

                }
                else //製造空白
                {
                    //fnt.FontImage = new Bitmap(1, 1);
                    ViewSize = new SizeF(SysDraw.SpaceWidth, 0);
                    

                }

                //ef.X += this.sc_i左上角.X;
                //ef.Y += this.sc_i左上角.Y;
                //ef.Width += this.sc_i右下角.X;
                //ef.Height += this.sc_i右下角.Y;

                fnt.BottomAlign = BottomAlign;
                fnt.charViewHeight = (float)ViewSize.Height;  //顯示高度
                fnt.charViewWidth = (float)ViewSize.Width;      //顯示寬度

                fnt.LeftSpace = 0;
                fnt.RightSpace = 0;
                if (RealSpace > 0)
                {
                    fnt.LeftSpace = RealSpace;
                    fnt.RightSpace = RealSpace;
                }
                /*
                if (SysDraw.Glow > 0)
                {
                    float shift = ((float)ef.Width - DisplaySize.Width) / 4;
                    fnt.LeftSpace = shift;
                    fnt.RightSpace = shift;
                }
                */

                if (IsSpace)
                {
                    fnt.LeftSpace = 0;
                    fnt.RightSpace = fnt.charViewWidth;
                    fnt.charViewHeight = 1f;
                    fnt.charViewWidth = 1f;
                    fnt.Empty = true;
                    fnt.IsSpace = true;
                }
                Height = fnt.charViewHeight;
            }

            this.iFntFile.Add(fnt, hex, ID);

            
            //this.iFntFile.Header.LineHeight = (this.iFntFile.Header.LineHeight < Height) ? Height : this.iFntFile.Header.LineHeight;

            
            //if (ef.Width > FontMaxWidth) FontMaxWidth = ef.Width; //登記最大寬度
            if (Height > FontMaxHeight) FontMaxHeight = (int)Height; //登記最大高度
            //if (!this.onSave)
            //{
                //graphics.DrawRectangle(Pens.Red, p.X, p.Y, ef.Width, ef.Height);
            //}
            //this.PointFOffset(ref p, p.Width + this.sc_Interval_X, 0f);
            
            //return true;

        }

        /// <summary>
        /// 底部對齊偏移
        /// </summary>
        /// <param name="fnt"></param>
        /// <param name="shift"></param>
        public void BottomAlignShift(float shift,bool sc_only)
        {
            foreach (Fnt_char fnt in this.iFntFile.CharList)
            {
                if (!fnt.Enable) continue;
                if (SkipASCII && !fnt.IsDC) continue;
                if (sc_only && fnt.IsDC) continue;
                fnt.BottomAlign += shift;
                fnt.BottomAlignFixed += shift;
            }

        }

        private static void PointFOffset(ref Point p, int x, int y)
        {
            p.X = p.X + x;
            p.Y = p.Y + y;
        }

        private void PointFOffset(ref Rectangle p, int x, int y)
        {
            p.X += x;
            p.Y += y;
        }

        #region Properties
        /// <summary>
        /// 設定字型
        /// </summary>
        private Font NowFont
        {
            set
            {
                if (_Font != value)
                {
                    _Font = value;
                    SysDraw.FontData = value;
                }
            }
            get
            {
                return _Font;
            }
        }

        /// <summary>
        /// 設定Glow
        /// </summary>
        /// <param name="glow"></param>
        public int Glow
        {
            set { this.SysDraw.Glow = value; }
            get { return this.SysDraw.Glow; }

        }
        public Color GlowColor
        {
            set { this.SysDraw.GlowColor = value; }
            get { return this.SysDraw.GlowColor; }
        }
        public Color OutlineColor
        {
            set { this.SysDraw.OutlineColor = value; }
            get { return this.SysDraw.OutlineColor; }
        }
        public Color FontColor
        {
            set { this.SysDraw.FontColor = value; }
            get { return this.SysDraw.FontColor; }
        }
        /// <summary>
        /// 設定Outline
        /// </summary>
        /// <param name="outline"></param>
        public int Outline
        {
            set { this.SysDraw.OutlineWidth = value; }
            get { return this.SysDraw.OutlineWidth; }
        }

        public int DrawMode
        {
            set { SysDraw.DrawMode = value; SysDraw.DrawMode = 1; }
            get { return SysDraw.DrawMode; }
        }
        public string TexName
        {
            set { iFntFile.Header.TexFileName = value; }
            get { return iFntFile.Header.TexFileName; }
        }

        public FL_FONT FntFile
        {
            get { return this.iFntFile; }
        }

        public Font font1
        {
            get { return this.ifont1; }
            set { this.ifont1 = value; }
        }

        public Font font2
        {
            get { return this.ifont2; }
            set { this.ifont2 = value; }
        }

        public bool isTextOverFlow
        {
            get { return this.iisTextOverFlow; }
        }

        #endregion

        #region Save

        public void SaveFnt(string path,Encoding enc)
        {
            bool ASCII_Only = false;
            if (DCfontLink > -1) ASCII_Only = true;
            this.FntFile.Header.TexFileName = PictureFileName;
            this.FntFile.save(path, enc, ASCII_Only);
            if (DCfontLink > -1)
            {
                FL_FONT ff = parent[DCfontLink].FntFile;
                ff.save_append(path); //存另一個dc
            }

        }
        /// <summary>
        /// Сохранчетвекор в растр
        /// </summary>
        /// <param name="path">путь к файлу</param>
        /// <param name="b">изображение для сохранения</param>
        public void SaveTex(string path,Bitmap b)
        {
            FileStream output = new FileStream(path, FileMode.Create);
            BinaryWriter writer = new BinaryWriter(output);
            writer.Write(b.Width);
            writer.Write(b.Height);
            this.mainform.ProgressBarMax = b.Height;
            this.mainform.ProgressBar = 0;
            //
            for (int y = 0; y < b.Height; y++)
            {
                for (int x = 0; x < b.Width; x++)
                {
                    Color pixel = b.GetPixel(x, y);
                    writer.Write(pixel.R);
                    writer.Write(pixel.G);
                    writer.Write(pixel.B);
                    writer.Write(pixel.A);

                }
                this.mainform.ProgressBarAdd();
            }
            writer.Flush();
            writer.Close();
            output.Close();

        }

        public void SaveBmp(string path,Bitmap b)
        {
            b.Save(path, ImageFormat.Png); //save bmp
        }

        #endregion

        #region Load

        /// <summary>
        /// 讀取fnt+tex並建立圖庫
        /// </summary>
        /// <param name="path"></param>
        /// <param name="Tex"></param>
        /// <param name="CharIndex"></param>
        /// <param name="b_tex"></param>
        /// <param name="fenc"></param>
        /// <returns></returns>
        public bool LoadFnt(string path, bool Tex, Array2D.List2D<Fnt_char> CharIndex, out Bitmap b_tex, FontEncoding fenc)
        {
            b_tex = new Bitmap(1, 1);
            bool ok = false;
            if (fenc.Temp.Count <256) return ok;


            this.FntFile.load(path, fenc.enc, fenc.Temp, ID);
            if (Tex)
            {
                string Dir = Path.GetDirectoryName(path);
                string tex_path = Path.Combine(Dir, this.FntFile.Header.TexFileName + ".Tex");
                if (File.Exists(tex_path))
                {
                    b_tex = LoadTex(tex_path);

                }
                else
                {
                    return false;
                }
                //建立字元索引
                //CharIndex = new char[this.iTextImageSize.Width, this.iTextImageSize.Height];
                CharIndex.Clear();
                int black = SysDraw.BackColor.ToArgb();
                int index = 0;

                this.mainform.ProgressBarMax = this.iFntFile.CharList.Count;
                this.mainform.ProgressBar = 0;

                foreach (Fnt_char fnt in this.iFntFile.CharList)
                {
                    if (!fnt.Enable) continue;
                    bool notBlack = false;
                    if (fnt.c==' ')
                        this.mainform.ProgressBar = 0;
                    Bitmap b;
                    int px = (int)(fnt.x1 * (float)b_tex.Width);
                    int py = (int)(fnt.y1 * (float)b_tex.Height);
                    int rx = (int)(fnt.x4 * (float)b_tex.Width);
                    int ry = (int)(fnt.y4 * (float)b_tex.Height);
                    int pw = rx - px;
                    int ph = ry - py;
                    if (pw > 0 && ph > 0)
                    {
                        b = new Bitmap(pw, ph);
                        int bx = 0; int by = 0;
                        for (int y = py; y < ry; y++)
                        {
                            if (b_tex.Height < y) continue;
                            for (int x = px; x < rx; x++)
                            {
                                if (b_tex.Width < x) continue;

                                CharIndex[x, y] = fnt;
                                Color color = b_tex.GetPixel(x, y);
                                b.SetPixel(bx, by, color);
                                if (color.ToArgb() != 0) notBlack = true;
                                bx++;
                            }
                            by++; bx = 0;
                        }
                        fnt.FontImage = b;
                        if (notBlack)
                        {
                            fnt.Empty = true;
                            if (pw == 1 && ph == 1)
                            {
                                fnt.IsSpace = true;
                                if (fnt.IsDC)
                                {
                                    if (this.iFntFile.EmptyDC == -1) this.iFntFile.EmptyDC = index;
                                }
                                else
                                {
                                    if (this.iFntFile.EmptySC == -1) this.iFntFile.EmptySC = index;
                                }
                            }
                        }
                        /*
                        Graphics g = Graphics.FromImage(b_tex);
                        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
                        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.Bilinear;
                        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.None;
                        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.None;
                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
                        g.DrawRectangle(Pens.Red, px, py, pw, ph);
                         */
                    }
                    else
                    {
                        fnt.Empty = true; fnt.IsSpace = true;
                        if (fnt.IsDC)
                        {
                            if (this.iFntFile.EmptyDC == -1) this.iFntFile.EmptyDC = index;
                        }
                        else
                        {
                            if (this.iFntFile.EmptySC == -1) this.iFntFile.EmptySC = index;
                        }
                    }


                    index++;
                    this.mainform.ProgressBarAdd();
                }

                if (this.iFntFile.FixedWidth > 0)
                {
                    fixedFont = true;
                    this.FontMaxWidth = this.iFntFile.FixedWidth;
                }
            }
            ok = true;
            return ok;
        }
        public Bitmap LoadTex(string path)
        {
            
            FileStream input = new FileStream(path, FileMode.Open);
            BinaryReader reader = new BinaryReader(input);

            int Width = reader.ReadInt32();
            int Height = reader.ReadInt32();
            Bitmap b = new Bitmap(Width, Height);

            this.mainform.ProgressBarMax = Height;
            this.mainform.ProgressBar = 0;
            //perpixel drawing of bitmap
            for (int y = 0; y < Height; y++)
            {
                for (int x = 0; x < Width; x++)
                {
                    byte R = reader.ReadByte();
                    byte G = reader.ReadByte();
                    byte B = reader.ReadByte();
                    byte A = reader.ReadByte();
                    /*
                     0x100 = 256
                     0x10000 = 65536
                     0x1000000 = 16777216
                     */
                    int c = (int)R + ((int)G * 0x100) + ((int)B * 0x10000) + ((int)A * 0x1000000);
                    if (c != 0)
                    {
                        Color color = Color.FromArgb(c);


                        b.SetPixel(x, y, color);//here
                    }
                }
                this.mainform.ProgressBarAdd();
            }
            reader.Close();
            input.Close();
            return b;
        }
        public Bitmap LoadBmp(string path)
        {
            Bitmap b = (Bitmap)Bitmap.FromFile(path, true);
            //this.iTextImageSize = this.iTextImage.Size;
            return b;
        }

        #endregion

        #region Other

        /// <summary>
        /// 判斷是否為雙字原
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        private bool isDoubledChar(char c, Encoding enc)
        {
            if (enc.GetBytes(c.ToString()).Length == 1)
            {
                return false;
            }
            return true;
        }

        //比较器类 
        public class Fnt_char_Height : IComparer<Fnt_char>
        {
            //按照圖形高度排序
            public int Compare(Fnt_char x, Fnt_char y)
            {
                return y.FontImage.Height - x.FontImage.Height;
            }
        }
        public class Fnt_char_Width : IComparer<Fnt_char>
        {
            //按照圖形寬度排序
            public int Compare(Fnt_char x, Fnt_char y)
            {
                return y.FontImage.Width - x.FontImage.Width;
            }
        }

        #endregion

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
