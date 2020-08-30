using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ScChrom.Tools {
    /// <summary>
    /// Provides functions to capture the inside of a window even if it is not on top.
    /// The logic comes from:
    /// https://ourcodeworld.com/articles/read/195/capturing-screenshots-of-different-ways-with-c-and-winforms
    /// </summary>
    public static class WindowCapture {

        private static class GDI32Api {

            public const int SRCCOPY = 0x00CC0020; // BitBlt dwRop parameter
            [DllImport("gdi32.dll")]
            public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest,
                int nWidth, int nHeight, IntPtr hObjectSource,
                int nXSrc, int nYSrc, int dwRop);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth,
                int nHeight);
            [DllImport("gdi32.dll")]
            public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteDC(IntPtr hDC);
            [DllImport("gdi32.dll")]
            public static extern bool DeleteObject(IntPtr hObject);
            [DllImport("gdi32.dll")]
            public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);
        }

        private static class User32Api {
            [StructLayout(LayoutKind.Sequential)]
            public struct RECT {
                public int left;
                public int top;
                public int right;
                public int bottom;
            }
            [DllImport("user32.dll")]
            public static extern IntPtr GetDesktopWindow();
            [DllImport("user32.dll")]
            public static extern IntPtr GetWindowDC(IntPtr hWnd);
            [DllImport("user32.dll")]
            public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);
            [DllImport("user32.dll")]
            public static extern IntPtr GetClientRect(IntPtr hWnd, ref RECT rect);            
        }

        /// <summary>
        /// Creates an Image object containing a screen shot of a specific window
        /// </summary>
        /// <param name="handle">The handle to the window. (In windows forms, this is obtained by the Handle property)</param>
        /// <returns></returns>
        public static Image CaptureWindow(IntPtr handle) {
            // get te hDC of the target window
            IntPtr hdcSrc = User32Api.GetWindowDC(handle);
            // get the size
            User32Api.RECT windowRect = new User32Api.RECT();
            User32Api.GetClientRect(handle, ref windowRect);
            int width = windowRect.right - windowRect.left;
            int height = windowRect.bottom - windowRect.top;
            // create a device context we can copy to
            IntPtr hdcDest = GDI32Api.CreateCompatibleDC(hdcSrc);
            // create a bitmap we can copy it to,
            // using GetDeviceCaps to get the width/height
            IntPtr hBitmap = GDI32Api.CreateCompatibleBitmap(hdcSrc, width, height);
            // select the bitmap object
            IntPtr hOld = GDI32Api.SelectObject(hdcDest, hBitmap);
            // bitblt over
            GDI32Api.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, GDI32Api.SRCCOPY);
            // restore selection
            GDI32Api.SelectObject(hdcDest, hOld);
            // clean up 
            GDI32Api.DeleteDC(hdcDest);
            User32Api.ReleaseDC(handle, hdcSrc);
            // get a .NET image object for it
            Image img = Image.FromHbitmap(hBitmap);
            // free up the Bitmap object
            GDI32Api.DeleteObject(hBitmap);
            return img;
        }
        

                
    }
}
