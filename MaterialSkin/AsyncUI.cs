using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace System
{
    internal static class AsyncUI
    {
        static AsyncUI()
        {
            UISynchronizationContext = SynchronizationContext.Current;
        }

        public static SynchronizationContext UISynchronizationContext;

        public static void InvokeAct(this Control control, Action<object> action, object state = null)
        {
            SendOrPostCallback checkDisposedAndInvoke = (s) =>
            {
                if (!control.IsDisposed && !control.InvokeRequired)
                    action(s);
            };

            if (!control.IsDisposed)
            {
                if (UISynchronizationContext != null)
                    UISynchronizationContext.Post(checkDisposedAndInvoke, state);
                else 
                    checkDisposedAndInvoke(state);
            }
        }
    }
}
