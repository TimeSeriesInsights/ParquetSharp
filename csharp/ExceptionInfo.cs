
using System;
using System.Runtime.InteropServices;

namespace ParquetSharp
{
    /// <summary>
    /// Marshaling logic for exceptions from the native C/C++ code to the managed layer.
    /// </summary>
    internal sealed class ExceptionInfo
    {
        public delegate IntPtr GetAction<TValue>(out TValue value);
        public delegate IntPtr GetAction<in TArg0, TValue>(TArg0 arg0, out TValue value);
        public delegate IntPtr GetAction<in TArg0, in TArg1, TValue>(TArg0 arg0, TArg1 arg1, out TValue value);
        public delegate IntPtr GetFunction<TValue>(IntPtr handle, out TValue value);
        public delegate IntPtr GetFunction<in TArg0, TValue>(IntPtr handle, TArg0 arg0, out TValue value);

        public delegate IntPtr GetStringFunction(IntPtr handle, out IntPtr stringPtr, out int stringLength);
        public delegate IntPtr GetStringAction(IntPtr handle, IntPtr stringPtr, int length, out int index);

        public static void Check(IntPtr exceptionInfo)
        {
            if (exceptionInfo == IntPtr.Zero)
            {
                return;
            }

            var type = Marshal.PtrToStringAnsi(ExceptionInfo_Type(exceptionInfo));
            var message = Marshal.PtrToStringAnsi(ExceptionInfo_Message(exceptionInfo));

            ExceptionInfo_Free(exceptionInfo);

            throw new ParquetException(type, message);
        }

        public static TValue Return<TValue>(GetAction<TValue> getter)
        {
            Check(getter(out var value));
            return value;
        }

        public static TValue Return<TArg0, TValue>(TArg0 arg0, GetAction<TArg0, TValue> getter)
        {
            Check(getter(arg0, out var value));
            return value;
        }

        public static int Return(ParquetHandle handle, IntPtr stringPtr, int stringLength, GetStringAction getter)
        {
            Check(getter(handle.IntPtr, stringPtr, stringLength, out var index));
            return index;
        }

        public static TValue Return<TArg0, TArg1, TValue>(TArg0 arg0, TArg1 arg1, GetAction<TArg0, TArg1, TValue> getter)
        {
            Check(getter(arg0, arg1, out var value));
            return value;
        }

        public static TValue Return<TValue>(ParquetHandle handle, GetFunction<TValue> getter)
        {
            var value = Return(handle.IntPtr, getter);
            GC.KeepAlive(handle);
            return value;
        }

        public static TValue Return<TValue>(IntPtr handle, GetFunction<TValue> getter)
        {
            Check(getter(handle, out var value));
            return value;
        }

        public static TValue Return<TValue>(ParquetHandle handle, ParquetHandle arg0, GetFunction<IntPtr, TValue> getter)
        {
            var value = Return(handle.IntPtr, arg0.IntPtr, getter);
            GC.KeepAlive(handle);
            return value;
        }

        public static TValue Return<TArg0, TValue>(ParquetHandle handle, TArg0 arg0, GetFunction<TArg0, TValue> getter)
        {
            var value = Return(handle.IntPtr, arg0, getter);
            GC.KeepAlive(handle);
            return value;
        }

        public static TValue Return<TArg0, TValue>(IntPtr handle, TArg0 arg0, GetFunction<TArg0, TValue> getter)
        {
            Check(getter(handle, arg0, out var value));
            return value;
        }

        public static string ReturnString(ParquetHandle handle, GetFunction<IntPtr> getter, Action<IntPtr> deleter = null)
        {
            Check(getter(handle.IntPtr, out var value));
            var str = Marshal.PtrToStringAnsi(value);
            deleter?.Invoke(value);
            GC.KeepAlive(handle);
            return str;
        }
        public static string ReturnString(ParquetHandle handle, GetStringFunction getter, Action<IntPtr> deleter = null)
        {
            Check(getter(handle.IntPtr, out var stringValue, out int stringLength));
            var str = StringFromNativeUtf8(stringValue, stringLength);
            deleter?.Invoke(stringValue);
            GC.KeepAlive(handle);
            return str;
        }

        private static string StringFromNativeUtf8(IntPtr utf8StringValue, int stringLength)
        {
            byte[] buffer = new byte[stringLength];
            Marshal.Copy(utf8StringValue, buffer, 0, buffer.Length);
            return System.Text.Encoding.UTF8.GetString(buffer);
        }

        [DllImport(ParquetDll.Name)]
        private static extern void ExceptionInfo_Free(IntPtr exceptionInfo);

        [DllImport(ParquetDll.Name)]
        private static extern IntPtr ExceptionInfo_Type(IntPtr exceptionInfo);

        [DllImport(ParquetDll.Name)]
        private static extern IntPtr ExceptionInfo_Message(IntPtr exceptionInfo);
    }
}