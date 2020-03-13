using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace MemSearch
{
    /// <summary>
    /// This class provides the code to read memory of another process
    /// </summary>
    internal static class ProcessReader
    {
        public static IEnumerable<byte[]> ReadMemory(IntPtr processId)
        {
            // Based on https://stackoverflow.com/questions/57717504/reading-all-process-memory-to-find-address-of-a-string-variable-c-sharp
            // The way I had originally done it was just reading the modules
            // e.g. it was something like Process.Modules.ForEach(x => x.BaseAddress)
            // Which is clearly wrong

            Imports.GetSystemInfo(out Imports.SystemInfo sysInfo);

            IntPtr maxAddress = sysInfo.MaximumApplicationAddress;
            IntPtr processHandle = Imports.OpenProcess(Imports.PROCESS_QUERY_INFORMATION | Imports.PROCESS_VM_READ, IntPtr.Zero, processId);
            IntPtr current = IntPtr.Zero;
            int dwLength = Marshal.SizeOf(typeof(Imports.MEMORY_BASIC_INFORMATION));

            if (processHandle == IntPtr.Zero)
                Console.WriteLine($"Failed to open process {processId}");
            else
                Console.WriteLine($"Got process handle {processHandle} for pid {processId}");

            while (current.ToInt64() < maxAddress.ToInt64() && Imports.VirtualQueryEx(processHandle, current, out Imports.MEMORY_BASIC_INFORMATION mem_basic_info, dwLength) != 0)
            {
                // if this memory chunk is accessible
                if (mem_basic_info.Protect == Imports.PAGE_READWRITE && mem_basic_info.State == Imports.MEM_COMMIT)
                {
                    byte[] buffer = new byte[(int)mem_basic_info.RegionSize];

                    if (Imports.ReadProcessMemory(processHandle, mem_basic_info.BaseAddress, buffer, (int)mem_basic_info.RegionSize, out IntPtr bytesRead))
                    {
                        if (bytesRead.ToInt32() != buffer.Length)
                            Console.WriteLine($"Size wrong while reading chunk, reported length: {bytesRead}, actual length: {buffer.Length}");
                        yield return buffer;
                    }
                    else
                        Console.WriteLine($"Error code while reading memory: {Marshal.GetLastWin32Error()}");
                }

                // move to the next memory chunk
                current = (IntPtr)(mem_basic_info.BaseAddress.ToInt64() + mem_basic_info.RegionSize.ToInt64());
            }
        }
    }
}
