using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

// http://hkpr.info/dotnet/RoundCornerS/ より
namespace ScreenSaverPhoto
{
    /// <summary>
    /// 角を丸くするコントロール (影付き)
    /// <para>
    /// 他のコントロールを内側に貼り付けて、
    /// Dock プロパティを Fill にして使用します。
    /// </para>
    /// </summary>
    [Description("角を丸くするコントロール")]
    [Designer("System.Windows.Forms.Design.ParentControlDesigner, System.Design", typeof(IDesigner))]
    public partial class RoundCornerS : UserControl
    {
        /// <summary>
        /// 影の大きさ
        /// </summary>
        private int shadowSize = 0;

        /// <summary>
        /// コーナーの角丸のサイズ（直径）
        /// </summary>
        private int cornerR = 1;

        /// <summary>
        /// コーナーの線の太さ
        /// </summary>
        private int penWidth = 3;

        /// <summary>
        /// コーナーの線の色
        /// </summary>
        private Color cornerColor = Color.Black;

        /// <summary>
        /// 内側の領域の色
        /// </summary>
        private Color innerColor = Color.Black;


        //---------------------------------------------------------------------
        #region "デザイン時外部公開プロパティ"
        [Category("Appearance")]
        [Browsable(true)]
        [Description("角の丸さを指定します。（半径）")]
        public int CornerR
        {
            get
            {
                return (int)(cornerR / 2);
            }
            set
            {
                if (value > 0)
                    cornerR = value * 2;
                else
                    throw new ArgumentException("Corner R", "1 以上の値を入れてください。");

                RenewPadding();
                Refresh();
            }
        }

        [Category("Appearance")]
        [Browsable(true)]
        [Description("内側に塗る色を指定します。")]
        public Color InnerColor
        {
            get
            {
                return innerColor;
            }
            set
            {
                innerColor = value;
                Refresh();
            }
        }

        [Category("Appearance")]
        [Browsable(true)]
        [Description("外枠の色を指定します。")]
        public Color ConerColor
        {
            get
            {
                return cornerColor;
            }
            set
            {
                cornerColor = value;
                Refresh();
            }
        }

        [Category("Appearance")]
        [Browsable(true)]
        [Description("外枠の線の太さを指定します。")]
        public int PenWidth
        {
            get
            {
                return penWidth;
            }
            set
            {
                if (value >= 0)
                    penWidth = value;
                else
                    throw new ArgumentException("PenWidth", "0 以上の値を入れてください。");

                RenewPadding();
                Refresh();
            }
        }

        [Category("Appearance")]
        [Browsable(true)]
        [Description("影の大きさを指定します。")]
        public int ShadowSize
        {
            get
            {
                return shadowSize;
            }
            set
            {
                if (value >= 0)
                    shadowSize = value;
                else
                    throw new ArgumentException("ShadowSize", "0 以上の値を入れてください。");

                RenewPadding();
                Refresh();
            }
        }
        #endregion
        //---------------------------------------------------------------------


        /// <summary>
        /// コンストラクタ
        /// </summary>
        public RoundCornerS()
        {
            InitializeComponent();

            // コントロールのサイズが変更された時に Paint イベントを発生させる
            SetStyle(ControlStyles.ResizeRedraw, true);
            SetStyle(ControlStyles.SupportsTransparentBackColor, true);

            this.BackColor = Color.Transparent;
            RenewPadding();
        }

        /// <summary>
        /// Paint イベントのオーバーロード
        /// </summary>
        /// <param name="e"></param>
        protected override void OnPaint(PaintEventArgs e)
        {
            //base.OnPaint(e);
            PaintCorner(e.Graphics);
        }

        //---------------------------------------------------------------------
        #region "プライベートメソッド"

        /// <summary>
        /// Padding サイズ更新
        /// </summary>
        private void RenewPadding()
        {
            int harfCornerR = (int)(cornerR / 2);
            int adjust = (int)(Math.Cos(45 * Math.PI / 180) * harfCornerR);
            this.Padding = new Padding(harfCornerR + shadowSize + penWidth + 2 - adjust);
        }

        /// <summary>
        /// コーナーの線の太さの半分の太さを取得
        /// </summary>
        /// <returns></returns>
        private int GetHarfPenWidth()
        {
            return (int)Math.Floor((double)(penWidth) / 2 + 0.5);
        }

        /// <summary>
        /// 描画品質設定
        /// </summary>
        private void SetSmoothMode(Graphics g)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            //g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighQuality;
        }

        /// <summary>
        /// 影用のパスを取得
        /// </summary>
        /// <returns></returns>
        private GraphicsPath GetShadowPath()
        {
            GraphicsPath gp = new GraphicsPath();

            int w = this.Width - cornerR;
            int h = this.Height - cornerR;

            gp.AddArc(0, 0, cornerR, cornerR, 180, 90);
            gp.AddArc(w, 0, cornerR, cornerR, 270, 90);
            gp.AddArc(w, h, cornerR, cornerR, 0, 90);
            gp.AddArc(0, h, cornerR, cornerR, 90, 90);
            gp.CloseFigure();

            return gp;
        }


        /// <summary>
        /// 影用のブラシ取得
        /// </summary>
        /// <param name="graphicsPath"></param>
        /// <returns></returns>
        private PathGradientBrush GetShadowBrush(GraphicsPath graphicsPath)
        {
            PathGradientBrush brush = new PathGradientBrush(graphicsPath);
            ColorBlend colorBlend = new ColorBlend();
            float pos = 0;

            if (this.Width < this.Height)
                pos = ((float)shadowSize * 2 / this.Height);
            else
                pos = ((float)shadowSize * 2 / this.Width);

            colorBlend.Positions = new float[3] { 0.0f, pos, 1.0f };

            colorBlend.Colors = new Color[3] { 
					Color.FromArgb(0, Color.White), 
					Color.FromArgb(20, 0, 0, 0),
					Color.FromArgb(20, 0, 0, 0)
			};

            brush.CenterColor = Color.Black;
            brush.CenterPoint = new PointF(this.Width / 2, this.Height / 2);
            brush.InterpolationColors = colorBlend;

            return brush;
        }

        /// <summary>
        /// 角丸を描く
        /// </summary>
        /// <param name="g"></param>
        private void PaintCorner(Graphics g)
        {
            // 描画品質設定
            SetSmoothMode(g);

            // 変数初期化
            int offset = shadowSize + GetHarfPenWidth();
            int w = this.Width - cornerR;
            int h = this.Height - cornerR;

            // 影用のパス初期化
            GraphicsPath shadowPath = null;
            if (shadowSize > 0)
                shadowPath = GetShadowPath();

            // 影用のブラシ初期化
            PathGradientBrush shadowBrush = null;
            if (shadowSize > 0)
                shadowBrush = GetShadowBrush(shadowPath);

            // 角丸用のペンとブラシ初期化
            Pen cornerPen = new Pen(cornerColor, penWidth);
            Brush fillBrush = new SolidBrush(innerColor);

            // 角丸用のパスの初期化
            GraphicsPath graphPath = new GraphicsPath();
            graphPath.AddArc(offset, offset, cornerR, cornerR, 180, 90);
            graphPath.AddArc(w - offset, offset, cornerR, cornerR, 270, 90);
            graphPath.AddArc(w - offset, h - offset, cornerR, cornerR, 0, 90);
            graphPath.AddArc(offset, h - offset, cornerR, cornerR, 90, 90);
            graphPath.CloseFigure();

            // 影塗り
            if (shadowSize > 0)
                g.FillPath(shadowBrush, shadowPath);

            // 角丸用のパス塗り
            g.FillPath(fillBrush, graphPath);

            // ペンの太さが 1 以上なら角丸描画
            if (penWidth > 0)
                g.DrawPath(cornerPen, graphPath);

            // 後処理
            if (shadowSize > 0)
            {
                shadowPath.Dispose();
                shadowBrush.Dispose();
            }
            fillBrush.Dispose();
            cornerPen.Dispose();
        }

        #endregion
        //---------------------------------------------------------------------


    }
}
