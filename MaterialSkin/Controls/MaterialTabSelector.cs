using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
#if ANIMATE
using MaterialSkin.Animations;
#endif

namespace MaterialSkin.Controls
{
    public class MaterialTabSelector : Control, IMaterialControl
    {
        [Browsable(false)]
        public int Depth { get; set; }
        [Browsable(false)]
        public MaterialSkinManager SkinManager { get { return MaterialSkinManager.Instance; } }
        [Browsable(false)]
        public MouseState MouseState { get; set; }

        public IForm ParentForm { get { return base.Parent as IForm; } }

        private MaterialTabControl baseTabControl;
        public MaterialTabControl BaseTabControl
        {
            get { return baseTabControl; }
            set
            {
                baseTabControl = value;
                if (baseTabControl == null) return;
                previousSelectedTabIndex = baseTabControl.SelectedIndex;
                baseTabControl.Deselected += (sender, args) =>
                {
                    previousSelectedTabIndex = baseTabControl.SelectedIndex;
                };
                baseTabControl.SelectedIndexChanged += (sender, args) =>
                {
#if ANIMATE
                    animationManager.SetProgress(0);
                    animationManager.StartNewAnimation(AnimationDirection.In);
#else
                    Invalidate();
#endif
                };
                    baseTabControl.ControlAdded += delegate
                {
                    Invalidate();
                };
                baseTabControl.ControlRemoved += delegate
                {
                    Invalidate();
                };
            }
        }

        private int previousSelectedTabIndex;
        private Point animationSource;
#if ANIMATE
        private readonly AnimationManager animationManager;
#endif

        private List<Rectangle> tabRects;
        private const int TAB_HEADER_PADDING = 24;
        private const int TAB_INDICATOR_HEIGHT = 2;

        public MaterialTabSelector()
        {
            SetStyle(ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);
            Height = 48;
            tabRects = new List<Rectangle>();

#if ANIMATE
            animationManager = new AnimationManager
            {
                AnimationType = AnimationType.EaseOut,
                Increment = 0.04
            };
            animationManager.OnAnimationProgress += sender => Invalidate();
#endif
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.TextRenderingHint = TextRenderingHint.AntiAlias;

            g.Clear(SkinManager.ColorScheme.PrimaryColor);

            if (baseTabControl == null) return;

            double animationProgress = 1.0;
#if !ANIMATE
            if (tabRects == null || tabRects.Count != baseTabControl.TabCount)
                UpdateTabRects();
#else
            if (!animationManager.IsAnimating() || tabRects == null ||  tabRects.Count != baseTabControl.TabCount)
                UpdateTabRects();
            animationProgress = animationManager.GetProgress();

            //Click feedback
            if (animationManager.IsAnimating())
            {
                var rippleBrush = new SolidBrush(Color.FromArgb((int)(51 - (animationProgress * 50)), Color.White));
                var rippleSize = (int)(animationProgress * tabRects[baseTabControl.SelectedIndex].Width * 1.75);

                g.SetClip(tabRects[baseTabControl.SelectedIndex]);
                g.FillEllipse(rippleBrush, new Rectangle(animationSource.X - rippleSize / 2, animationSource.Y - rippleSize / 2, rippleSize, rippleSize));
                g.ResetClip();
                rippleBrush.Dispose();
            }
#endif

            if (tabRects.Count == 0)
                return;

            //Draw tab headers
            foreach (TabPage tabPage in baseTabControl.TabPages)
            {
                int currentTabIndex = baseTabControl.TabPages.IndexOf(tabPage);
                Brush textBrush = new SolidBrush(Color.FromArgb(CalculateTextAlpha(currentTabIndex, animationProgress), SkinManager.ColorScheme.TextColor));

                if (tabRects.Count >= currentTabIndex + 1)
                    g.DrawString(
                        tabPage.Text.ToUpper(),
                        SkinManager.ROBOTO_MEDIUM_10,
                        textBrush,
                        tabRects[currentTabIndex],
                        new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center });
                textBrush.Dispose();
            }

            //Animate tab indicator
            int previousSelectedTabIndexIfHasOne = previousSelectedTabIndex == -1 ? baseTabControl.SelectedIndex : previousSelectedTabIndex;
            Rectangle previousActiveTabRect = tabRects[previousSelectedTabIndexIfHasOne];
            Rectangle activeTabPageRect = tabRects[baseTabControl.SelectedIndex];

            int y = activeTabPageRect.Bottom - 2;
            int x = previousActiveTabRect.X + (int)((activeTabPageRect.X - previousActiveTabRect.X) * animationProgress);
            int width = previousActiveTabRect.Width + (int)((activeTabPageRect.Width - previousActiveTabRect.Width) * animationProgress);

            g.FillRectangle(SkinManager.ColorScheme.AccentBrush, x, y, width, TAB_INDICATOR_HEIGHT);

        }

        private int CalculateTextAlpha(int tabIndex, double animationProgress)
        {
            int primaryA = SkinManager.ACTION_BAR_TEXT.A;
            int secondaryA = SkinManager.ACTION_BAR_TEXT_SECONDARY.A;
            bool isAnimate = false;
#if ANIMATE
            isAnimate  = animationManager.IsAnimating();
#endif

            if (tabIndex == baseTabControl.SelectedIndex && !isAnimate)
            {
                return primaryA;
            }
            if (tabIndex != previousSelectedTabIndex && tabIndex != baseTabControl.SelectedIndex)
            {
                return secondaryA;
            }
            if (tabIndex == previousSelectedTabIndex)
            {
                return primaryA - (int)((primaryA - secondaryA) * animationProgress);
            }
            return secondaryA + (int)((primaryA - secondaryA) * animationProgress);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (tabRects == null) UpdateTabRects();
            for (int i = 0; i < tabRects.Count; i++)
            {
                if (tabRects[i].Contains(e.Location))
                {
                    baseTabControl.SelectedIndex = i;
                }
            }

            animationSource = e.Location;
//#if !ANIMATE
//            this.Update();  // draw
//#endif
        }

        private void UpdateTabRects()
        {
            //If there isn't a base tab control, the rects shouldn't be calculated
            //If there aren't tab pages in the base tab control, the list should just be empty which has been set already; exit the void
            if (baseTabControl == null || baseTabControl.TabCount == 0) return;

            //Calculate the bounds of each tab header specified in the base tab control
            using (var b = new Bitmap(1, 1))
            {
                using (var g = Graphics.FromImage(b))
                {
                    tabRects.Add(new Rectangle(SkinManager.FORM_PADDING, 0, TAB_HEADER_PADDING * 2
                        + (int)g.MeasureString(baseTabControl.TabPages[0].Text, SkinManager.ROBOTO_MEDIUM_10).Width, Height));
                    for (int i = 1; i < baseTabControl.TabPages.Count; i++)
                    {
                        tabRects.Add(new Rectangle(tabRects[i - 1].Right, 0, TAB_HEADER_PADDING * 2
                            + (int)g.MeasureString(baseTabControl.TabPages[i].Text, SkinManager.ROBOTO_MEDIUM_10).Width, Height));
                    }
                }
            }
        }
    }
}
