using System;
using System.Reflection;
using System.Windows;

namespace NorthHorizon.Samples.SystemThemeChange
{
    public static class ThemeHelper
    {
        // Had some help from http://learnwpf.com/Posts/Post.aspx?postId=3f1f4b8b-b91a-442d-a531-919de70ac225 (dead link now),
        // Ms.Win32.UxThemeWrapper, PresentationFramework via Reflector,
        // and System.Windows.SystemResources via Reflector

        // Why they don't enable this kind of functionality without reflection is beyond me. Really.

        private const BindingFlags InstanceNonPublic = BindingFlags.Instance | BindingFlags.NonPublic;
        private const BindingFlags StaticNonPublic = BindingFlags.Static | BindingFlags.NonPublic;

        private const int ThemeChangedMessage = 0x31a;

        private static readonly MethodInfo FilteredSystemThemeFilterMessageMethod = typeof(ThemeHelper).GetMethod("FilteredSystemThemeFilterMessage", StaticNonPublic);
        private static readonly MethodInfo SystemThemeFilterMessageMethod = typeof(ThemeHelper).GetMethod("EmptySystemThemeFilterMessage", StaticNonPublic);

        private static readonly Assembly PresentationFramework = Assembly.GetAssembly(typeof(Window));

        private static readonly Type ThemeWrapper = PresentationFramework.GetType("MS.Win32.UxThemeWrapper");
        private static readonly FieldInfo ThemeWrapper_isActiveField = ThemeWrapper.GetField("_isActive", StaticNonPublic);
        private static readonly FieldInfo ThemeWrapper_themeColorField = ThemeWrapper.GetField("_themeColor", StaticNonPublic);
        private static readonly FieldInfo ThemeWrapper_themeNameField = ThemeWrapper.GetField("_themeName", StaticNonPublic);

        private static readonly Type SystemResources = PresentationFramework.GetType("System.Windows.SystemResources");
        private static readonly FieldInfo SystemResources_hwndNotifyField = SystemResources.GetField("_hwndNotify", StaticNonPublic);
        private static readonly FieldInfo SystemResources_hwndNotifyHookField = SystemResources.GetField("_hwndNotifyHook", StaticNonPublic);
        private static readonly MethodInfo SystemResources_EnsureResourceChangeListener = SystemResources.GetMethod("EnsureResourceChangeListener", StaticNonPublic);
        private static readonly MethodInfo SystemResources_SystemThemeFilterMessageMethod = SystemResources.GetMethod("SystemThemeFilterMessage", StaticNonPublic);

        private static readonly Assembly WindowsBase = Assembly.GetAssembly(typeof(DependencyObject));

        private static readonly Type HwndWrapperHook = WindowsBase.GetType("MS.Win32.HwndWrapperHook");

        private static readonly Type HwndWrapper = WindowsBase.GetType("MS.Win32.HwndWrapper");
        private static readonly MethodInfo HwndWrapper_AddHookMethod = HwndWrapper.GetMethod("AddHook");

        private static readonly Type SecurityCriticalDataClass = WindowsBase.GetType("MS.Internal.SecurityCriticalDataClass`1").MakeGenericType(HwndWrapper);
        private static readonly PropertyInfo SecurityCriticalDataClass_ValueProperty = SecurityCriticalDataClass.GetProperty("Value", InstanceNonPublic);

        /// <summary>
        /// Sets the WPF system theme.
        /// </summary>
        /// <param name="themeName">The name of the theme. (ie "aero")</param>
        /// <param name="themeColor">The name of the color. (ie "normalcolor")</param>
        public static void SetTheme(string themeName, string themeColor)
        {
            SetHwndNotifyHook(FilteredSystemThemeFilterMessageMethod);

            // Call the system message handler with ThemeChanged so it will clear the theme dictionary caches.
            bool handled = false;
            InvokeSystemThemeFilterMessage(IntPtr.Zero, ThemeChangedMessage, IntPtr.Zero, IntPtr.Zero, ref handled);

            // Need this to make sure WPF doesn't default to classic. 
            ThemeWrapper_isActiveField.SetValue(null, true);

            ThemeWrapper_themeColorField.SetValue(null, themeColor);
            ThemeWrapper_themeNameField.SetValue(null, themeName);
        }

        public static void Reset()
        {
            SetHwndNotifyHook(SystemResources_SystemThemeFilterMessageMethod);
            bool handled = false;
            InvokeSystemThemeFilterMessage(IntPtr.Zero, ThemeChangedMessage, IntPtr.Zero, IntPtr.Zero, ref handled);
        }

        private static void SetHwndNotifyHook(MethodInfo method)
        {
            var hookDelegate = Delegate.CreateDelegate(HwndWrapperHook, FilteredSystemThemeFilterMessageMethod);

            // Make sure _hwndNotify and _hwndNotifyHook are set!
            SystemResources_EnsureResourceChangeListener.Invoke(null, null);

            // Note that because the HwndwWrapper uses a WeakReference list, we don't need
            // to remove the old value. Simply killing the reference is good enough.
            SystemResources_hwndNotifyHookField.SetValue(null, hookDelegate);

            // this does SystemResources._hwndNotify.Value.AddHook(hookDelegate)
            var hwndNotify = SystemResources_hwndNotifyField.GetValue(null);
            var hwndNotifyValue = SecurityCriticalDataClass_ValueProperty.GetValue(hwndNotify, null);
            HwndWrapper_AddHookMethod.Invoke(hwndNotifyValue, new object[] { hookDelegate });
        }

        private static IntPtr EmptySystemThemeFilterMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) { return IntPtr.Zero; }

        private static IntPtr InvokeSystemThemeFilterMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            var args = new object[] { hwnd, msg, wParam, lParam, handled };
            var result = (IntPtr)SystemResources_SystemThemeFilterMessageMethod.Invoke(null, args);

            handled = (bool)args[4];

            return result;
        }

        private static IntPtr FilteredSystemThemeFilterMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == ThemeChangedMessage)
            {
                handled = true;
                return IntPtr.Zero;
            }

            return InvokeSystemThemeFilterMessage(hwnd, msg, wParam, lParam, ref handled);
        }
    }
}
