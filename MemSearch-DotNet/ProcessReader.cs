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

            // While 1) inside process memory bounds and 2) Getting memory information didn't fail
            while (current.ToInt64() < maxAddress.ToInt64() 
                && Imports.VirtualQueryEx(processHandle, current, out Imports.MEMORY_BASIC_INFORMATION memBasicInfo, dwLength) != 0)
            {
                // if this memory chunk is accessible
                if (memBasicInfo.Protect == Imports.PAGE_READWRITE && memBasicInfo.State == Imports.MEM_COMMIT)
                {
                    // Read the whole region at once
                    byte[] buffer = new byte[(int)memBasicInfo.RegionSize];

                    // Attempt to read process memory
                    if (Imports.ReadProcessMemory(processHandle, memBasicInfo.BaseAddress, buffer, (int)memBasicInfo.RegionSize, out IntPtr bytesRead))
                    {
                        // If the sizes don't match for whatever reason, notify someone
                        if (bytesRead.ToInt32() != buffer.Length)
                            Console.WriteLine($"Size wrong while reading chunk, reported length: {bytesRead}, actual length: {buffer.Length}");
                        yield return buffer;
                    }
                    else
                        Console.WriteLine($"Error code while reading memory: {Marshal.GetLastWin32Error()}");
                }

                // move to the next memory chunk
                current = (IntPtr)(memBasicInfo.BaseAddress.ToInt64() + memBasicInfo.RegionSize.ToInt64());
            }
        }
    }
}
