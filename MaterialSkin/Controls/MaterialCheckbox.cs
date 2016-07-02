using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
#if ANIMATE
using MaterialSkin.Animations;
#endif

namespace MaterialSkin.Controls
{
    public class MaterialCheckBox : CheckBox, IMaterialControl
    {
        [Browsable(false)]
        public int Depth { get; set; }
        [Browsable(false)]
        public MaterialSkinManager SkinManager { get { return MaterialSkinManager.Instance; } }
        [Browsable(false)]
        public MouseState MouseState { get; set; }
        [Browsable(false)]
        public Point MouseLocation { get; set; }

        public IForm ParentForm { get { return this.ParentForm as IForm; } }

        private bool ripple;
        [Category("Behavior")]
        public bool Ripple
        {
            get { return ripple; }
            set
            {
                ripple = value;
                AutoSize = AutoSize; //Make AutoSize directly set the bounds.

                if (value)
                {
                    Margin = new Padding(0);
                }

                Invalidate();
            }
        }

#if ANIMATE
        private readonly AnimationManager animationManager;
        private readonly AnimationManager rippleAnimationManager;
#endif

        private const int CHECKBOX_SIZE = 18;
        private const int CHECKBOX_SIZE_HALF = CHECKBOX_SIZE / 2;
        private const int CHECKBOX_INNER_BOX_SIZE = CHECKBOX_SIZE - 4;

        private int boxOffset;
        private Rectangle boxRectangle;

        public MaterialCheckBox()
        {
#if ANIMATE
            animationManager = new AnimationManager
            {
                AnimationType = AnimationType.EaseInOut,
                Increment = 0.05
            };
            rippleAnimationManager = new AnimationManager(false)
            {
                AnimationType = AnimationType.Linear,
                Increment = 0.10,
                SecondaryIncrement = 0.08
            };
            animationManager.OnAnimationProgress += sender => Invalidate();
            rippleAnimationManager.OnAnimationProgress += sender => Invalidate();

            CheckedChanged += (sender, args) =>
            {
                animationManager.StartNewAnimation(Checked ? AnimationDirection.In : AnimationDirection.Out);
            };
#else
            CheckedChanged += (sender, args) =>
            {
                Invalidate();
            };
#endif
            Ripple = true;
            MouseLocation = new Point(-1, -1);
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);

            boxOffset = Height / 2 - 9;
            boxRectangle = new Rectangle(boxOffset, boxOffset, CHECKBOX_SIZE - 1, CHECKBOX_SIZE - 1);
        }

        public override Size GetPreferredSize(Size proposedSize)
        {
            int w = boxOffset + CHECKBOX_SIZE + 2 + (int)CreateGraphics().MeasureString(Text, SkinManager.ROBOTO_MEDIUM_10).Width;
            return Ripple ? new Size(w, 30) : new Size(w, 20);
        }

        private static readonly Point[] CHECKMARK_LINE = { new Point(3, 8), new Point(7, 12), new Point(14, 5) };
        public const int TEXT_OFFSET = 22;

        protected override void OnPaint(PaintEventArgs pevent)
        {
            var g = pevent.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            // clear the control
            g.Clear(Parent.BackColor);

            var CHECKBOX_CENTER = boxOffset + CHECKBOX_SIZE_HALF - 1;

            int colorAlpha = SkinManager.GetCheckBoxOffDisabledColor().A;
            var brush = new SolidBrush(Color.FromArgb(colorAlpha, Enabled ? SkinManager.ColorScheme.AccentColor : SkinManager.GetCheckBoxOffDisabledColor()));
            Rectangle checkMarkLineFill = new Rectangle(boxOffset, boxOffset, (int)(17.0), 17);

            double animationProgress = 1.0;
            var pen = new Pen(brush.Color);
            int backgroundAlpha = Enabled ? (int)(SkinManager.GetCheckboxOffColor().A * (1.0 - animationProgress)) : SkinManager.GetCheckBoxOffDisabledColor().A;
#if ANIMATE
			var brush3 = new SolidBrush(Enabled ? SkinManager.ColorScheme.AccentColor : SkinManager.GetCheckBoxOffDisabledColor());

            animationProgress = animationManager.GetProgress();
            int colorAlpha = Enabled ? (int)(animationProgress * 255.0) : SkinManager.GetCheckBoxOffDisabledColor().A;
            // draw ripple animation
            if (Ripple && rippleAnimationManager.IsAnimating())
            {
                for (int i = 0; i < rippleAnimationManager.GetAnimationCount(); i++)
                {
                    var animationValue = rippleAnimationManager.GetProgress(i);
                    var animationSource = new Point(CHECKBOX_CENTER, CHECKBOX_CENTER);
                    var rippleBrush = new SolidBrush(Color.FromArgb((int)((animationValue * 40)), ((bool)rippleAnimationManager.GetData(i)[0]) ? Color.Black : brush.Color));
                    var rippleHeight = (Height % 2 == 0) ? Height - 3 : Height - 2;
                    var rippleSize = (rippleAnimationManager.GetDirection(i) == AnimationDirection.InOutIn) ? (int)(rippleHeight * (0.8d + (0.2d * animationValue))) : rippleHeight;
                    using (var path = DrawHelper.CreateRoundRect(animationSource.X - rippleSize / 2, animationSource.Y - rippleSize / 2, rippleSize, rippleSize, rippleSize / 2))
                    {
                        g.FillPath(rippleBrush, path);
                    }

                    rippleBrush.Dispose();
                }
            }

            brush3.Dispose();
            checkMarkLineFill = new Rectangle(boxOffset, boxOffset, (int)(17.0 * animationProgress), 17);
#endif

            using (var checkmarkPath = DrawHelper.CreateRoundRect(boxOffset, boxOffset, 17, 17, 1f))
            {
                SolidBrush brush2 = new SolidBrush(DrawHelper.BlendColor(Parent.BackColor, Enabled ? SkinManager.GetCheckboxOffColor()
                        : SkinManager.GetCheckBoxOffDisabledColor(), backgroundAlpha));
                Pen pen2 = new Pen(brush2.Color);
                g.FillPath(brush2, checkmarkPath);
                g.DrawPath(pen, checkmarkPath);

                if (Enabled && !Checked)
                {
                    g.FillRectangle(new SolidBrush(Parent.BackColor), boxOffset + 2, boxOffset + 2, CHECKBOX_INNER_BOX_SIZE - 1, CHECKBOX_INNER_BOX_SIZE - 1);
                    g.FillPath(brush2, checkmarkPath);
                    // double frame
                    g.DrawRectangle(pen, boxOffset + 1, boxOffset + 1, CHECKBOX_INNER_BOX_SIZE + 1, CHECKBOX_INNER_BOX_SIZE + 1);
                }
                else if (Checked)
                {
                    // g.DrawPath(pen2, checkmarkPath);
                    // g.DrawRectangle(new Pen(Parent.BackColor), boxOffset + 2, boxOffset + 2, CHECKBOX_INNER_BOX_SIZE - 1, CHECKBOX_INNER_BOX_SIZE - 1);

                    g.SmoothingMode = SmoothingMode.None;
                    g.FillRectangle(brush, boxOffset, boxOffset, CHECKBOX_INNER_BOX_SIZE + 3, CHECKBOX_INNER_BOX_SIZE + 3);
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.DrawImageUnscaledAndClipped(DrawCheckMarkBitmap(), checkMarkLineFill);

                    if (MouseState != MouseState.OUT)
                        g.DrawRectangle(pen2, boxOffset + 1, boxOffset + 1, CHECKBOX_INNER_BOX_SIZE + 1, CHECKBOX_INNER_BOX_SIZE + 1);
                }
                // else if (!Checked)

                brush2.Dispose();
                pen2.Dispose();

            }

            // draw checkbox text
            DrawText(g);

            // dispose used paint objects
            pen.Dispose();
            brush.Dispose();
        }

        public delegate void PaintEventHandler(object sender, PaintEventArgs e);
        public event PaintEventHandler OnDrawText;

        public virtual void DrawText(Graphics g)
        {
            if (OnDrawText != null)
            {
                OnDrawText(this, new PaintEventArgs(g, ClientRectangle));
                return;
            }

            SizeF stringSize = g.MeasureString(Text, SkinManager.ROBOTO_MEDIUM_10);
            g.DrawString(
                    Text,
                    SkinManager.ROBOTO_MEDIUM_10,
                    Enabled ? SkinManager.GetPrimaryTextBrush() : SkinManager.GetDisabledOrHintBrush(),
                    boxOffset + TEXT_OFFSET, Height / 2 - stringSize.Height / 2);
        }

        private Bitmap DrawCheckMarkBitmap()
        {
            var checkMark = new Bitmap(CHECKBOX_SIZE, CHECKBOX_SIZE);
            var g = Graphics.FromImage(checkMark);

            // clear everything, transparent
            g.Clear(Color.Transparent);

            // draw the checkmark lines
            using (var pen = new Pen(Parent.BackColor, 2))
            {
                g.DrawLines(pen, CHECKMARK_LINE);
            }

            return checkMark;
        }

        public override bool AutoSize
        {
            get { return base.AutoSize; }
            set
            {
                base.AutoSize = value;
                if (value)
                {
                    Size = new Size(10, 10);
                }
            }
        }

        private bool IsMouseInCheckArea()
        {
            return boxRectangle.Contains(MouseLocation);
        }

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            Font = SkinManager.ROBOTO_MEDIUM_10;

            if (DesignMode) return;

            MouseState = MouseState.OUT;
            MouseEnter += (sender, args) =>
            {
                MouseState = MouseState.HOVER;
            };
            MouseLeave += (sender, args) =>
            {
                MouseLocation = new Point(-1, -1);
                MouseState = MouseState.OUT;
            };

            MouseDown += (sender, args) =>
            {
                MouseState = MouseState.DOWN;
#if ANIMATE

                if (Ripple && args.Button == MouseButtons.Left && IsMouseInCheckArea())
                {
                    rippleAnimationManager.SecondaryIncrement = 0;
                    rippleAnimationManager.StartNewAnimation(AnimationDirection.InOutIn, new object[] { Checked });
                }
#else
                if (Ripple && args.Button == MouseButtons.Left && IsMouseInCheckArea())
                {
                    Checked = Checked;
                    Invalidate();
                }
#endif
            };
            MouseUp += (sender, args) =>
            {
                MouseState = MouseState.HOVER;
#if ANIMATE
                rippleAnimationManager.SecondaryIncrement = 0.08;
#endif
            };

            MouseMove += (sender, args) =>
            {
                MouseLocation = args.Location;
                Cursor = IsMouseInCheckArea() ? Cursors.Hand : Cursors.Default;
            };
        }

    }
}
