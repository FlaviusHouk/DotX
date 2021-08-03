using System;
using System.Runtime.InteropServices;
using DotX.Extensions;

namespace DotX.Platform.Linux.X
{
    internal static class XlibWrapper
    {
        [DllImport("libc.so.6", CharSet = CharSet.Ansi)]
        public static extern IntPtr setlocale(int localeType, string locale);

        [DllImport("libX11.so.6")]
        public static extern X11.Atom XInternAtom(IntPtr display, string name, bool only_if_exists);


        [DllImport("libX11.so.6")]
        public static extern string XGetAtomName(IntPtr display, X11.Atom atom);

        [DllImport("libX11.so.6", CharSet = CharSet.Ansi)]
        public static extern string XSetLocaleModifiers(string modifierList); 

        [DllImport("libX11.so.6")]
        public static extern IntPtr XOpenIM(IntPtr display,
                                            IntPtr resourceDatabase,
                                            string res_name,
                                            string res_class);

        [DllImport("libX11.so.6")]
        public static extern IntPtr XCreateIC(IntPtr inputMethod,
                                              string paramName1,
                                              int value1,
                                              string paramName2,
                                              X11.Window value2,
                                              IntPtr terminator);

        [DllImport("libX11.so.6")]
        public static extern int XmbLookupString(IntPtr inputContext,
                                                 IntPtr keyPressedEvent,
                                                 IntPtr stringBuffer,
                                                 out int bufferLen,
                                                 IntPtr keySyms,
                                                 out X11.Status status);

        [DllImport("libX11.so.6")]
        public static extern int Xutf8LookupString(IntPtr inputContext,
                                                   ref X11.XKeyEvent keyPressedEvent,
                                                   IntPtr stringBuffer,
                                                   int bufferLen,
                                                   out X11.KeySym keySym,
                                                   out X11.Status status);

        
        [DllImport("libX11.so.6")]
        public static extern int XwcLookupString(IntPtr inputContext,
                                                 IntPtr keyPressedEvent,
                                                 IntPtr stringBuffer,
                                                 int bufferLen,
                                                 out X11.KeySym keySyms,
                                                 out X11.Status status);

        [DllImport("libX11.so.6")]
        public static extern void XSetICFocus(IntPtr inputContext);

        [DllImport("libX11.so.6")]
        public static extern bool XSupportsLocale();

        public static string XLookupStringWrapper(IntPtr inputContext,
                                                  X11.XKeyEvent keyPressedEvent)
        {
            string value;
            const int BufferSize = sizeof(char) * 16;
            IntPtr stringMem = Marshal.AllocHGlobal(BufferSize);
            for(int i = 0; i<BufferSize; i++)
            {
                Marshal.WriteByte(stringMem, i, 0);
            }

            try
            {
                X11.KeySym keySym;
                X11.Status status;

                int len = Xutf8LookupString(inputContext,
                                            ref keyPressedEvent,
                                            stringMem,
                                            BufferSize,
                                            out keySym,
                                            out status);

                if(status == X11.Status.Failure)
                    throw new Exception();

                value = Marshal.PtrToStringUTF8(stringMem);
            }
            finally
            {
                Marshal.FreeHGlobal(stringMem);
            }

            return value;
        }

        [DllImport("libX11.so.6")]
        public static extern X11.Status XCloseIM(IntPtr inputMethod);

        [DllImport("libX11.so.6")]
        public static extern void XDestroyIC(IntPtr inputContext);
    }
}