using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace MaterialSkin
{
    public interface IMaterialControl : IWin32Window
    {
        int Depth { get; set; }
        MaterialSkinManager SkinManager { get; }
        MouseState MouseState { get; set; }

        IForm ParentForm { get; }
    }

    public enum MouseState
    {
        HOVER,
        DOWN,
        OUT
    }

    public interface IForm : IMaterialControl
    {
        string Text { get; set; }

        IEnumerable<IMaterialControl> SkinControls { get; }
    }

    public interface IButton : IMaterialControl
    {
        string Text { get; set; }
    }
}
