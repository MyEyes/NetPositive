using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace NetPositive
{
    public static class NativeMethods
    {
        [DllImport("Kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern uint GetFinalPathNameByHandle(IntPtr hFile, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder lpszFilePath, uint cchFilePath, uint dwFlags);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr CreateFile([MarshalAs(UnmanagedType.LPTStr)] string filename, [MarshalAs(UnmanagedType.U4)] uint access, [MarshalAs(UnmanagedType.U4)] FileShare share, IntPtr securityAttributes, [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition, [MarshalAs(UnmanagedType.U4)] uint flagsAndAttributes, IntPtr templateFile);

        public static string GetFinalPathName(string path)
        {
            IntPtr h = NativeMethods.CreateFile(path, 8U, FileShare.Read | FileShare.Write | FileShare.Delete, IntPtr.Zero, FileMode.Open, 33554432U, IntPtr.Zero);
            bool flag = h == NativeMethods.INVALID_HANDLE_VALUE;
            if (flag)
            {
                throw new Win32Exception();
            }
            string text;
            try
            {
                StringBuilder sb = new StringBuilder(1024);
                uint res = NativeMethods.GetFinalPathNameByHandle(h, sb, 1024U, 0U);
                bool flag2 = res == 0U;
                if (flag2)
                {
                    throw new Win32Exception();
                }
                text = sb.ToString();
            }
            finally
            {
                NativeMethods.CloseHandle(h);
            }
            return text;
        }

        static NativeMethods()
        {
        }

        private static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        private const uint FILE_READ_EA = 8U;

        private const uint FILE_FLAG_BACKUP_SEMANTICS = 33554432U;
    }
}
