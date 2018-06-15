using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;

namespace LabObjects.DotNetGlue.Diagnostics
{
    public interface IProcessorBase
    {
        string Arguments { get; set; }
        int BasePriority { get; }
        string Comments { get; }
        string CompanyName { get; }
        bool CreateNoWindow { get; set; }
        string Domain { get; set; }
        StringDictionary EnvironmentVariables { get; }
        bool ErrorDialog { get; set; }
        int ExitCode { get; }
        DateTime ExitTime { get; }
        int FileBuildPart { get; }
        string FileDescription { get; }
        int FileMajorPart { get; }
        int FileMinorPart { get; }
        string FileName { get; set; }
        int FilePrivatePart { get; }
        string FileVersion { get; }
        int Handle { get; }
        int HandleCount { get; }
        bool HasExited { get; }
        string InternalName { get; }
        bool IsRunning { get; }
        string Language { get; }
        string LegalCopyright { get; }
        string LegaTrademarks { get; }
        bool LoadUserProfile { get; set; }
        int NonpagedSystemMemorySize { get; }
        long NonpagedSystemMemorySize64 { get; }
        string OriginalFileName { get; }
        int PagedMemorySize { get; }
        long PagedMemorySize64 { get; }
        int PagedSystemMemorySize { get; }
        long PagedSystemMemorySize64 { get; }
        int PeakPagedMemorySize { get; }
        long PeakPagedMemorySize64 { get; }
        int PeakVirtualMemorySize { get; }
        long PeakVirtualMemorySize64 { get; }
        int PeakWorkingSet { get; }
        long PeakWorkingSet64 { get; }
        bool PriorityBoostEnabled { get; }
        ProcessPriorityClass PriorityClass { get; }
        int PrivateMemorySize { get; }
        long PrivateMemorySize64 { get; }
        TimeSpan PrivilegedProcessorTime { get; }
        int ProcessId { get; }
        string ProcessName { get; }
        IntPtr ProcessorAffinity { get; }
        string ProductName { get; }
        string ProductVersion { get; }
        bool RedirectStandardError { get; set; }
        bool RedirectStandardInput { get; set; }
        bool RedirectStandardOutput { get; set; }
        int SessionId { get; }
        Encoding StandardErrorEncoding { get; }
        Encoding StandardOutputEncoding { get; }
        DateTime StartTime { get; }
        int ThreadCount { get; }
        TimeSpan TotalProcessorTime { get; }
        string UserName { get; set; }
        TimeSpan UserProcessorTime { get; }
        bool UseShellExecute { get; set; }
        string Verb { get; set; }
        string[] Verbs { get; }
        int VirtualMemorySize { get; }
        long VirtualMemorySize64 { get; }
        ProcessWindowStyle WindowStyle { get; set; }
        string WorkingDirectory { get; set; }
        int WorkingSet { get; }
        long WorkingSet64 { get; }

        void Dispose();
        string[,] GetEnvironmentVariables();
        bool Refresh();
        bool ResetPassword();
        bool SetErrorOutputEncoding(string encodingName);
        bool SetOutputEncoding(string encodingName);
        bool SetPassword(string pwd);
        bool SetPriorityClass(string priorityClassName);
        bool SetProcessorAffinity(int processorAffinity);
        bool SetWindowStyle(string windowStyleString);
    }
}