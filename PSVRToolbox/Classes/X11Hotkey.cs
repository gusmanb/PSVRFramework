//using System;
//using System.Runtime.InteropServices;

//public class X11Hotkey
//{
//    private const int KeyPress = 2;
//    private const int GrabModeAsync = 1;
//    private Gdk.Key key;
//    private Gdk.ModifierType modifiers;
//    private int keycode;

//    public X11Hotkey(Gdk.Key key, Gdk.ModifierType modifiers)
//    {
//        this.key = key;
//        this.modifiers = modifiers;

//        Gdk.Window rootWin = Gdk.Global.DefaultRootWindow;
//        IntPtr xDisplay = GetXDisplay(rootWin);
//        this.keycode = XKeysymToKeycode(xDisplay, (int)this.key);
//        rootWin.AddFilter(new Gdk.FilterFunc(FilterFunction));
//    }

//    public event EventHandler Pressed;

//    public void Register()
//    {
//        Gdk.Window rootWin = Gdk.Global.DefaultRootWindow;
//        IntPtr xDisplay = GetXDisplay(rootWin);

//        XGrabKey(
//                 xDisplay,
//                 this.keycode,
//                 (uint)this.modifiers,
//                 GetXWindow(rootWin),
//                 false,
//                 GrabModeAsync,
//                 GrabModeAsync);
//    }

//    public void Unregister()
//    {
//        Gdk.Window rootWin = Gdk.Global.DefaultRootWindow;
//        IntPtr xDisplay = GetXDisplay(rootWin);

//        XUngrabKey(
//                 xDisplay,
//                 this.keycode,
//                 (uint)this.modifiers,
//                 GetXWindow(rootWin));
//    }

//    private Gdk.FilterReturn FilterFunction(IntPtr xEvent, Gdk.Event evnt)
//    {
//        XKeyEvent xKeyEvent = (XKeyEvent)Marshal.PtrToStructure(
//            xEvent,
//            typeof(XKeyEvent));

//        if (xKeyEvent.type == KeyPress)
//        {
//            if (xKeyEvent.keycode == this.keycode
//                && xKeyEvent.state == (uint)this.modifiers)
//            {
//                this.OnPressed(EventArgs.Empty);
//            }
//        }

//        return Gdk.FilterReturn.Continue;
//    }

//    protected virtual void OnPressed(EventArgs e)
//    {
//        EventHandler handler = this.Pressed;
//        if (handler != null)
//        {
//            handler(this, e);
//        }
//    }

//    private static IntPtr GetXWindow(Gdk.Window window)
//    {
//        return gdk_x11_drawable_get_xid(window.Handle);
//    }

//    private static IntPtr GetXDisplay(Gdk.Window window)
//    {
//        return gdk_x11_drawable_get_xdisplay(
//            gdk_x11_window_get_drawable_impl(window.Handle));
//    }

//    [DllImport("libgtk-x11-2.0")]
//    private static extern IntPtr gdk_x11_drawable_get_xid(IntPtr gdkWindow);

//    [DllImport("libgtk-x11-2.0")]
//    private static extern IntPtr gdk_x11_drawable_get_xdisplay(IntPtr gdkDrawable);

//    [DllImport("libgtk-x11-2.0")]
//    private static extern IntPtr gdk_x11_window_get_drawable_impl(IntPtr gdkWindow);

//    [DllImport("libX11")]
//    private static extern int XKeysymToKeycode(IntPtr display, int key);

//    [DllImport("libX11")]
//    private static extern int XGrabKey(
//        IntPtr display,
//        int keycode,
//        uint modifiers,
//        IntPtr grab_window,
//        bool owner_events,
//        int pointer_mode,
//        int keyboard_mode);

//    [DllImport("libX11")]
//    private static extern int XUngrabKey(
//        IntPtr display,
//        int keycode,
//        uint modifiers,
//        IntPtr grab_window);

//#if BUILD_FOR_32_BIT_X11        

//    [StructLayout(LayoutKind.Sequential)]
//    internal struct XKeyEvent
//    {
//        public short type;
//        public uint serial;
//        public short send_event;
//        public IntPtr display;
//        public uint window;
//        public uint root;
//        public uint subwindow;
//        public uint time;
//        public int x, y;
//        public int x_root, y_root;
//        public uint state;
//        public uint keycode;
//        public short same_screen;
//    }       
//#elif BUILD_FOR_64_BIT_X11

//    [StructLayout(LayoutKind.Sequential)]
//    internal struct XKeyEvent
//    {
//        public int type;
//        public ulong serial;
//        public int send_event;
//        public IntPtr display;
//        public ulong window;
//        public ulong root;
//        public ulong subwindow;
//        public ulong time;
//        public int x, y;
//        public int x_root, y_root;
//        public uint state;
//        public uint keycode;
//        public int same_screen;
//    }
//#endif      

//}