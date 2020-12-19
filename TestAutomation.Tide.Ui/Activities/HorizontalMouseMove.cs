using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using ICSharpCode.AvalonEdit;

namespace TAF.AutomationTool.Ui.Activities
{
    public class HorizontalMouseMove
    {
        private readonly object ctrl;

        public HorizontalMouseMove(object ctrl, Window win)
        {
            this.ctrl = ctrl;
            var source = PresentationSource.FromVisual(win);
            ((HwndSource) source)?.AddHook(this.Hook);
        }

        private const int WmMousehwheel = 0x020E;

        private IntPtr Hook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg != WmMousehwheel) return IntPtr.Zero;

            var tilt = Hiword(wParam);
            this.OnMouseTilt(tilt);
            return (IntPtr) 1;
        }

        /// <summary>
        /// Gets high bits values of the pointer.
        /// </summary>
        private static short Hiword(IntPtr ptr)
        {
            var scrollValue = ptr.ToInt64();
            return (short) ((scrollValue >> 16) & 0xFFFF);
        }

        /// <summary>
        /// Gets low bits values of the pointer.
        /// </summary>
        private static int LOWORD(IntPtr ptr)
        {
            var val32 = ptr.ToInt32();
            return (val32 & 0xFFFF);
        }

        private void OnMouseTilt(Int64 tilt)
        {
            switch (this.ctrl)
            {
                case ScrollViewer scrl:
                    if (scrl.IsMouseOver)
                        scrl.ScrollToHorizontalOffset(scrl.HorizontalOffset + tilt);
                    break;
                case TextEditor editor:
                    if (editor.IsMouseOver)
                        editor.ScrollToHorizontalOffset(editor.HorizontalOffset + tilt);
                    break;
            }
        }
    }
}