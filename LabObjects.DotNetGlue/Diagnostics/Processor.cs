using System;
using System.Text;
using System.ComponentModel;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Timers;
//using LabObjects.DotNetGlue.SharedKernel;

namespace LabObjects.DotNetGlue.Diagnostics
{  
    /// <summary>
    /// Processor Class
    /// </summary>
    public class Processor : ProcessorBase
    {
        #region Constructors
        /// <summary>
        /// Processor Constructor
        /// </summary>
        /// <param name="process">System.Disagnostics.Process object to wrap as a Processor. Use the HostProcessor.GetProcessor factory to create Proessors.</param>
        public Processor(Process process) : base(process)
        {
            _timeoutTimer.Enabled = false;
            _timeoutTimer.Elapsed += new ElapsedEventHandler(TimeoutTimerElapsed);

            Process.EnableRaisingEvents = true;
            Process.Exited += new EventHandler(ProcessExited);
        }
        #endregion

        #region events

        /// <summary>
        /// Process Timeout Timer Elapsed Event Handler. Used Terminate Processes that exceed the allowed timeout value.
        /// This routine will set the DidTimeout property to true and call Process.Kill() to terminate the process if it has not Exited (Process.Exited)
        /// </summary>
        /// <param name="source">Internal Process Clock (Timer)</param>
        /// <param name="e">ElapsedEventArgs</param>
        private void TimeoutTimerElapsed(object source, ElapsedEventArgs e)
        {
            if (!Process.HasExited)
            {
                try
                {
                    DidTimeout = true;      
                    SetLastError($"Process timed-out: {Process.ProcessName}");
                    Process.Kill();
                    Process.WaitForExit();
                }
                catch (Win32Exception ex)
                {
                    SetLastError($"Timeout Timer Elapsed: Unable to Kill Process: {ex.Message}");
                }
                catch (NotSupportedException ex)
                {
                    SetLastError($"Timeout Timer Elapsed: Unable to Kill Process: {ex.Message}");
                }
                catch (InvalidOperationException ex)
                {
                    SetLastError($"Timeout Timer Elapsed: Unable to Kill Process: {ex.Message}");
                }
                catch (Exception ex)
                {
                    SetLastError($"Timeout Timer Elapsed: Unable to Kill Process: {ex.Message}");
                }
            }
        }

        private void ProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!String.IsNullOrEmpty(e.Data))
            {
                _bufferOutput.AppendLine(e.Data);
                if (_isDynamicTimeout)
                    ProcessClockRestart();
            }
        }
        private void ProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            _bufferError.AppendLine(e.Data);
            if (_isDynamicTimeout)
                ProcessClockRestart();
        }

        private void ProcessExited(object sender, System.EventArgs e)
        {
            try
            {
                System.Diagnostics.Process process = (System.Diagnostics.Process)sender;
                HasExited = process.HasExited;
                ExitCode = process.ExitCode;
                ExitTime = process.ExitTime;
                IsRunning = !HasExited;
                ProcessClockStop();

                _runtime_ms = CalcRunTime((DateTime)StartTime, (DateTime)ExitTime);
                _totalProcessorTime_ms = Process.TotalProcessorTime.Milliseconds;
                _userProcessorTime_ms = Process.UserProcessorTime.Milliseconds;
            }
            catch (InvalidOperationException ex)
            {
                SetLastError($"ProcessExited: {ex.Message}");
            }
            catch (Exception ex)
            {
                SetLastError($"ProcessExited: {ex.Message}");
            }
        }
        private async void ProcessExitedReadErrorOutput(Process process)
        {
            //-------------------------------------------------------
            // read standard error
            //-------------------------------------------------------
            try
            {
                if (process.StartInfo.RedirectStandardError)
                {
                    String errText = String.Empty;
                    if (_isErrorStreamAsync)
                    {
                        errText = await process.StandardError.ReadToEndAsync();
                        if (!String.IsNullOrEmpty(errText))
                            _bufferError.Append(errText);
                    }
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                SetLastError($"ProcessExited Standard Error Exception: {ex.Message}");
            }
            catch (ObjectDisposedException ex)
            {
                SetLastError($"ProcessExited Standard Error Exception: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                SetLastError($"ProcessExited Standard Error Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                SetLastError($"ProcessExited Standard Error Exception: {ex.Message}");
            }
        }
        private async void ProcessExitedReadOutput(Process process)
        {
            //-------------------------------------------------------
            // standard output 
            //-------------------------------------------------------
            try
            {
                if (process.StartInfo.RedirectStandardOutput)
                {
                    String outText = String.Empty;
                    if (_isOutputStreamAsync)
                    {
                        outText = await process.StandardOutput.ReadToEndAsync();
                        if (!String.IsNullOrEmpty(outText))
                            _bufferOutput.Append(outText);
                    }
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                SetLastError($"ProcessExited Standard Output Exception: {ex.Message}");
            }
            catch (ObjectDisposedException ex)
            {
                SetLastError($"ProcessExited Standard Output Exception: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                SetLastError($"ProcessExited Standard Output Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                SetLastError($"ProcessExited Standard Output Exception: {ex.Message}");
            }
        }
        private void ProcessExitedFlushInput(Process process)
        {
            //-------------------------------------------------------
            // standard Input
            //-------------------------------------------------------
            try
            {
                if (process.StartInfo.RedirectStandardInput)
                {
                    process.StandardInput.Flush();
                }
            }
            catch (EncoderFallbackException ex)
            {
                SetLastError($"ProcessExited Standard Input Exception: {ex.Message}");
            }
            catch (IOException ex)
            {
                SetLastError($"ProcessExited Standard Input Exception: {ex.Message}");
            }
            catch (ObjectDisposedException ex)
            {
                SetLastError($"ProcessExited Standard Input Exception: {ex.Message}");
            }
            catch (Exception ex)
            {
                SetLastError($"ProcessExited Standard Input Exception: {ex.Message}");
            }
        }
        #endregion

        #region fields

        private const int _defaultTimeout_ms = 10 * 60 * 1000;     // 30 secs
        private System.Timers.Timer _timeoutTimer = new System.Timers.Timer(_defaultTimeout_ms);
        private bool _isDynamicTimeout = true;

        private bool _isOutputStreamAsync = false;            
        private bool _isErrorStreamAsync = false;
        private bool _hasBeenStarted = false;       
        
        private StreamWriter _processInputWriter;

        ProcessBuffer _bufferOutput = new ProcessBuffer();
        ProcessBuffer _bufferError = new ProcessBuffer();

        private int _runtime_ms;
        private int _totalProcessorTime_ms;
        private int _userProcessorTime_ms;
        #endregion

        #region Properties

        /// <summary>
        /// Arguments
        /// </summary>
        public override String Arguments { get => base.Arguments; set { if (!IsRunning) base.Arguments = value; } }

        public override Boolean CreateNoWindow { get => base.CreateNoWindow; set { if (!IsRunning) base.CreateNoWindow = value; } }

        /// <summary>
        /// Domain
        /// </summary>
        public override String Domain { get => base.Domain; set  { if (!IsRunning) base.Domain = value; } }

        public override Boolean ErrorDialog { get => base.ErrorDialog; set { if (!IsRunning) base.ErrorDialog = value; } }

        /// <summary>
        /// FileName
        /// </summary>
        public override String FileName { get => base.FileName; set { if (!IsRunning) base.FileName = value; } }

        /// <summary>
        /// Key value used in HostProcess Process Dictionary
        /// </summary>
        public String Key { get; } = Guid.NewGuid().ToString();
 
        /// <summary>
        /// Load User Profile when running the Process. 
        /// Note: This property is referenced if the process is being started by using the user name, password, and domain.
        /// If the value is true, the user's profile in the HKEY_USERS registry key is loaded. Loading the profile can be time-consuming. Therefore, it is best to use this value only if you must access the information in the HKEY_CURRENT_USER registry key.
        /// </summary>
        public override Boolean LoadUserProfile { get => base.LoadUserProfile; set { if (!IsRunning) base.LoadUserProfile = value; } }

        public override Boolean RedirectStandardError { get => base.RedirectStandardError; set { if (!IsRunning) base.RedirectStandardError = value; } }

        public override Boolean RedirectStandardInput { get => base.RedirectStandardInput; set { if (!IsRunning) base.RedirectStandardInput = value; } }

        public override Boolean RedirectStandardOutput { get => base.RedirectStandardOutput; set { if (!IsRunning) base.RedirectStandardOutput = value; } }

        /// <summary>
        /// Wrapper property to Process.StartInfo.UseShellExecute.
        /// Set to false when streaming I/O, Error
        /// </summary>
        public override Boolean UseShellExecute { get => base.UseShellExecute; set { if (!IsRunning) base.UseShellExecute = value; } }

        /// <summary>
        /// The User Name to use for the process.
        /// </summary>
        public override String UserName { get => base.UserName; set { if (!IsRunning) base.UserName = value; } }

        #region timeout properties
        /// <summary>
        /// DidTimeout Property. Boolean Flag indicating wheteh rlast run process timedout.
        /// </summary>
        public bool DidTimeout { get; private set; }
        /// <summary>
        /// DynamicTimeout - when TRUE allows Process timeout determinations to be made on based on last Output or Input Activity
        /// otherwise process timeout is based on when the process is started. Must be set before the process starts.
        /// </summary>
        public bool DynamicTimeout
        {
            get { return _isDynamicTimeout; }
            set
            {
                if (IsRunning)
                {
                    throw new Exception("Process.DynamicTimeout: Can't set property when Process is running");
                }
                else
                    _isDynamicTimeout = value;
            }
        }
        /// <summary>
        /// Timeout Property in Milliseconds.
        /// </summary>
        /// <remarks>A value &lt;= 1 will default to five (5) minutes</remarks>
        public int TimeoutMilliSeconds { get; set; }

        #endregion

        #region I/O Properties 
        /// <summary>
        /// Indicates whether data is available from the process standard output stream. 
        /// The StandardOutput must be redirected and the process running for the IsOutputAvailable to have meaning.
        /// <para>If output is available it can be read using OutputRead(). IsOutputAvailable and OutputRead() operate for synchrnous and asynchrnous StandardOutput redirection modes.</para>
        /// </summary>
        /// <see cref="Processor.OutputRead"/>
        /// <value>True if data can be read from the StandardOutputelse false.</value>
        public bool IsOutputAvailable
        {
            get
            {
                bool status = false;
                if (this.RedirectStandardOutput && this.IsRunning)
                {
                    if (_isOutputStreamAsync)
                        status = _bufferOutput.HasUnreadData;
                    else
                    {
                        try
                        {
                            if (this.Process != null)
                                status = !this.Process.StandardOutput.EndOfStream;
                        }
                        catch
                        { // do nothing 
                        }

                    }
                }
                return status;
            }
        }
        /// <summary>
        /// Is (new) Error Stream Data Available?
        /// </summary>
        public bool IsErrorAvailable
        {
            get { return _bufferError.HasUnreadData; }
        }

        ///// <summary>
        ///// The combined progam output from the Standard Out and Standard Error.  
        ///// </summary>
        //public string ProgramOutput
        //{
        //    get; private set;
        //}
        public string Output
        {
            get { return _bufferOutput.Data; }
        }
        public string ErrorOutput
        {
            get { return _bufferError.Data; }
        }

        #endregion


        #region process runtime metrics
        /// <summary>
        /// The process execution time in milliseconds.
        /// </summary>
        public int RunTimeMilliseconds
        {
            get { return _runtime_ms; }
        }
        /// <summary>
        /// The process CPU usage time in milliseconds.
        /// </summary>
        public int CPUTimeMilliseconds
        {
            get { return _totalProcessorTime_ms; }
        }
        #endregion

        public override String Verb { get => base.Verb; set { if (!IsRunning) base.Verb = value; } }

        /// <summary>
        /// Defines what type of window the process should run in. 
        /// Normal=0, Hidden=1, Minimized=2, Maximized=3
        /// </summary>
        public override ProcessWindowStyle WindowStyle { get => base.WindowStyle; set { if (!IsRunning) base.WindowStyle = value; } }

        /// <summary>
        /// Working Directory for the process.
        /// </summary>
        public override String WorkingDirectory { get => base.WorkingDirectory; set { if (!IsRunning) base.WorkingDirectory = value; } }

        #endregion

        #region Public Methods
        /// <summary>
        /// Close a Process and release resources allocated.
        /// </summary>
        /// <returns></returns>
        public void Close()
        {
            Process.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Boolean CloseMainWindow()
        {
            try
            {
                return Process.CloseMainWindow();
            }
            catch (Win32Exception ex)
            {
                SetLastError($"CloseMainWindow failed: {ex.Message}");
            }
            catch (NotSupportedException ex)
            {
                SetLastError($"CloseMainWindow failed: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                SetLastError($"CloseMainWindow failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                SetLastError($"CloseMainWindow failed: {ex.Message}");
            }
            return false; 
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public String ErrorOutputRead()
        {
            return _bufferError.Read();
        }

        /// <summary>
        /// Write to the process Input Stream
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        public Boolean InputWrite(string inputData)
        {
            bool status = false;
            try
            {
                if (this.IsRunning && this.RedirectStandardInput)
                {
#if DEBUG
                    Debug.Write(String.Format("Writing to Process Input  Stream: {0}....", inputData));
#endif
                    _processInputWriter.Write(inputData);
                    if (_isDynamicTimeout)
                        ProcessClockRestart();
                    status = true;
#if DEBUG
                    Debug.WriteLine(String.Format("...Complete"));
#endif
                }
                else if (!this.RedirectStandardInput)
                {
                    throw new InvalidOperationException("Process Input is not redirected!");
                }
                else if (!this.IsRunning)
                {
                    throw new InvalidOperationException("Process is not running!");
                }
            }
            catch (InvalidOperationException ex)
            {
                SetLastError(string.Format("Unable to Write Process Input: {0}", ex.Message));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                SetLastError(string.Format("Unable to Write Process Input: {0}", ex.Message));
            }
            catch (Exception ex)
            {
                SetLastError(string.Format("Unable to Write Process Input: {0}", ex.Message), ex.InnerException);
            }
            return status;
        }
        /// <summary>
        /// InputWriteLine
        /// </summary>
        /// <param name="inputData"></param>
        /// <returns></returns>
        public Boolean InputWriteLine(string inputData)
        {
            bool status = false;
            try
            {
                if (this.IsRunning && this.RedirectStandardInput)
                {
                    //_process.StandardInput.Write(inputData);
#if DEBUG
                    Debug.Write(String.Format("Writing to Process Input  Stream: {0}....", inputData));
#endif
                    _processInputWriter.WriteLine(inputData);
                    if (_isDynamicTimeout)
                        ProcessClockRestart();
                    status = true;
#if DEBUG
                    Debug.WriteLine(String.Format("...Complete"));
#endif
                }
                else if (!this.RedirectStandardInput)
                {
                    throw new InvalidOperationException("Process Input is not redirected!");
                }
                else if (!this.IsRunning)
                {
                    throw new InvalidOperationException("Process is not running!");
                }
            }
            catch (InvalidOperationException ex)
            {
                SetLastError(string.Format("Unable to Write Process Input: {0}", ex.Message));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                SetLastError(string.Format("Unable to Write Process Input: {0}", ex.Message));
            }
            catch (Exception ex)
            {
                SetLastError(string.Format("Unable to Write Process Input: {0}", ex.Message), ex.InnerException);
            }
            return status;
        }

        /// <summary>
        /// Forces a Process to Terminate
        /// </summary>
        /// <returns type="System.Boolean">True if successful</returns>
        /// <remarks>
        /// Data edited by the process or resources allocated to the process can be lost if you call Kill. 
        /// Kill causes an abnormal process termination and should be used only when necessary. 
        /// IF RedirectStardardOutput or RedirectStandardError are true this class uses asynchronous stream reads. 
        /// Check for available output after this method closes.
        /// </remarks>
        /// <seealso cref="System.Diagnostics.Process.Kill"/>
        public Boolean Kill()
        {

            try
            {
                Process.Kill();
                Process.WaitForExit();
                return true;
            }
            catch (Win32Exception ex)
            {
                SetLastError($"Kill failed: {ex.Message}");
            }
            catch (NotSupportedException ex)
            {
                SetLastError($"Kill failed: {ex.Message}");
            }
            catch (InvalidOperationException ex)
            {
                SetLastError($"Kill failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                SetLastError($"Kill failed: {ex.Message}");
            }               
            return false;

        }

        /// <summary>
        /// Reads from the asynchronous output buffer any data that has not been Read when IsOutputAvailable is true. This is used in conjunction with the Start Method
        /// and allows for retrieving process output while the process is executing. 
        /// </summary>
        /// <returns>String - all the data in the output buffer that has not been read</returns>
        public string OutputRead()
        {
            string tmp = "";
            string pout = string.Empty;
            if (this.RedirectStandardOutput && this.IsRunning)
            {
                if (_isOutputStreamAsync)
                    pout = _bufferOutput.Read();
                else
                {
                    try
                    {
                        if (this.Process != null)
                        {
                            if (!this.Process.StandardOutput.EndOfStream)
                            {
                                //while (!_process.StandardOutput.EndOfStream)
                                while (this.Process.StandardOutput.Peek() > -1)
                                {
#if DEBUG
                                    Debug.WriteLine("Reading Output...");
#endif
                                    //bufferOutput.Append(string.Format("{0}", (char)_process.StandardOutput.Read()));
                                    tmp = this.Process.StandardOutput.ReadLine();
#if DEBUG
                                    Debug.WriteLine(tmp);
#endif
                                    _bufferOutput.AppendLine(tmp);
                                }
                                pout = _bufferOutput.Read();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
#if DEBUG
                        Debug.WriteLine(String.Format("Output Read Exception: {0} - {1}", ex.Message, ex.InnerException.ToString()));
#endif
                    }
                }
            }
            return pout;
        }
  
        public Boolean RunHidden()
        {
            return RunHidden(null);
        }

        /// <summary>
        /// Runs to completion a process using a hidden woindow with redirected standard output and standard error output.
        /// </summary>
        /// <returns type="System.Boolean">
        /// True if the process was run successfully, otherwise it returns false. 
        /// If not run successfully the LastError property will contain the error message.
        /// </returns>
        /// <remarks>
        /// The process runs synchronously until completed. Callers must minimally specifiy a valid FileName to execute as a hidden process.
        /// If Arguments are needed, they must also be sepecific before calling RunHidden. If the process requires user credentials set 
        /// the UserName and, optionally, the Domain properties and use the SetPassword(pwd) method to set the password.
        /// </remarks>
        public Boolean RunHidden(String processInput)
        {         
            ResetLastError();
            if (CanProcessStart())
            {
                try
                {
                    // override current setting on the following properties
                    UseShellExecute = false;
                    CreateNoWindow = true;
                    WindowStyle = ProcessWindowStyle.Hidden;
                    ErrorDialog = false;
                    DidTimeout = false;
                    RedirectStandardError = true;
                    RedirectStandardOutput = true;
                    if (processInput != null)
                        RedirectStandardInput = true;

                    SetProcessStartInfo();

                    if (!_isErrorStreamAsync)
                        Process.ErrorDataReceived += new DataReceivedEventHandler(ProcessErrorDataReceived);
                    _isErrorStreamAsync = true;

                    if (!_isOutputStreamAsync)
                        Process.OutputDataReceived += new DataReceivedEventHandler(ProcessOutputDataReceived);
                    _isOutputStreamAsync = true;
                    _hasBeenStarted = true;     // can't change the stream mode

                    if (Process.Start())
                    {
                        IsRunning = true;
                        ProcessClockStart();
                        Process.BeginErrorReadLine();
                        Process.BeginOutputReadLine();
                        GetStartupProperties();

                        if (RedirectStandardInput)
                        {
                            StreamWriter inputWriter = Process.StandardInput;
                            inputWriter.Write(processInput);
                            inputWriter.Flush();
                        }

                        while (!Process.HasExited)
                        {
                            Thread.Sleep(100);
                            Refresh();
                        }
                        Process.WaitForExit();
                        return true;
                    }
                }
                catch (Win32Exception ex)
                {
                    SetLastError($"RunHidden: {ex.Message}", ex.InnerException);
                }
                catch (ObjectDisposedException ex)
                {
                    SetLastError($"RunHidden: {ex.Message}", ex.InnerException);
                }
                catch (InvalidOperationException ex)
                {
                    SetLastError($"RunHidden: {ex.Message}", ex.InnerException);
                }
                catch (Exception ex)
                {
                    SetLastError($"RunHidden: {ex.Message}", ex.InnerException);
                }
            }
            return false;
        }

        /// <summary>
        /// SetEnvironmentVariable
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public Boolean SetEnvironmentVariable(string name, string value)
        {
            SetLastError("Not Implemented");
            return false;
        }

        /// <summary>
        /// Start Process
        /// </summary>
        /// <returns></returns>
        public Boolean Start()
        {
            ResetLastError();
            if (CanProcessStart())
            {
                try
                {
                    SetProcessStartInfo();
                    if (RedirectStandardError)
                    {
                        Process.ErrorDataReceived += new DataReceivedEventHandler(ProcessErrorDataReceived);
                        _isErrorStreamAsync = true;
                    }
                    if (RedirectStandardOutput)
                    {
                        Process.OutputDataReceived += new DataReceivedEventHandler(ProcessOutputDataReceived);
                        _isOutputStreamAsync = true;
                    }

                    _hasBeenStarted = true;     // can't change the stream mode, even if start faileds

                    if (Process.Start())
                    {
                        IsRunning = true;
                        ProcessClockStart();
                        if (RedirectStandardError)
                            Process.BeginErrorReadLine();
                        if (RedirectStandardOutput)
                            Process.BeginOutputReadLine();
                        GetStartupProperties();

                        if (RedirectStandardInput)
                            _processInputWriter = Process.StandardInput;
                        return true;
                    }
                }
                catch (Win32Exception ex)
                {
                    SetLastError($"Start: {ex.Message}", ex.InnerException);
                }
                catch (ObjectDisposedException ex)
                {
                    SetLastError($"Start: {ex.Message}", ex.InnerException);
                }
                catch (InvalidOperationException ex)
                {
                    SetLastError($"Start: {ex.Message}", ex.InnerException);
                }
                catch (Exception ex)
                {
                    SetLastError($"Start: {ex.Message}", ex.InnerException);
                }
            }
            return false;
        }

        public Boolean WaitForExit()
        {
            ResetLastError();
            try
            {
                Process.WaitForExit();
                return true;
            }
            catch (Win32Exception ex)
            {
                SetLastError($"WaitForExit failed: {ex.Message}");
            }
            catch (SystemException ex)
            {
                SetLastError($"WaitForExit failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                SetLastError($"WaitForExit failed: {ex.Message}");
            }
            return false;
        }
        public Boolean WaitForExit(Int32 millseconds)
        {
            ResetLastError();
            try
            {
                return Process.WaitForExit(millseconds);
            }
            catch (Win32Exception ex)
            {
                SetLastError($"WaitForExit failed: {ex.Message}");
            }
            catch (SystemException ex)
            {
                SetLastError($"WaitForExit failed: {ex.Message}");
            }
            catch (Exception ex)
            {
                SetLastError($"WaitForExit failed: {ex.Message}");
            }
            return false;
        }
        #endregion

        #region private methods

        private bool CanProcessStart()
        {
            bool status;
            if (IsRunning)
            {
                SetLastError($"CanProcessStart: Process is already running: {ProcessName}");
                status = false;
            }
            else if (FileName == null)
            {
                SetLastError("CanProcessStart: FileName not defined.");
                status = false;
            }
            else if (_hasBeenStarted && !_isErrorStreamAsync)
            {
                SetLastError("CanProcessStart: ErrorOutput is Not Asynchronous - create a new Processor.");
                status = false;
            }
            else if (_hasBeenStarted && !_isOutputStreamAsync)
            {
                SetLastError("CanProcessStart: Output is Not Asynchronous - create a new Processor.");
                status = false;
            }
            else
            {
                status = true;
            }
            return status;
        }

        #region Clock Methods
        private void ProcessClockRestart()
        {
            ProcessClockStop();
            ProcessClockStart();
        }
        private void ProcessClockStart()
        {
            this.DidTimeout = false;
            _timeoutTimer.Enabled = false;
            // set interval to time out value
            _timeoutTimer.Interval = TimeoutIntervalGet();
            _timeoutTimer.Enabled = true;
            _timeoutTimer.Start();
        }
        private void ProcessClockStop()
        {
            _timeoutTimer.Stop();
            _timeoutTimer.Enabled = false;
        }

        /// <summary>
        /// Returns the Timeout Interval in milliseconds. 
        /// If the TimeoutMilliseconds property is less than or equal to zero the function returns the default timeout interval (e.g., 300000 ms or 5 minutes)
        /// otherwise the function returns the TimeoutMilliseconds property value
        /// </summary>
        /// <returns></returns>
        private int TimeoutIntervalGet()
        {
            int timeout = 0;
            if (this.TimeoutMilliSeconds <= 0)
            {
                timeout = _defaultTimeout_ms;
            }
            else
            {
                timeout = this.TimeoutMilliSeconds;
            }
            return timeout;
        }
        #endregion

        private bool ProcessCloseMainWindow()
        {
            bool status = false;
            const string errMsg = "Unable to CloseMainWindow";
            if (IsRunning && (Process != null) && !Process.HasExited)
            {
                try
                {
                    status = Process.CloseMainWindow();
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("{0}: {1}", errMsg, ex.Message), ex);
                }
            }
            else
            {
                throw new Exception(string.Format("{0}: {1}", errMsg, "Process Not Running"));
            }
            return status;
        }
  
        /// <summary>
        /// Private helper function to reset the metric counters for a process. 
        /// </summary>
        private void ResetProcessMetrics()
        {
            _runtime_ms = 0;
            _userProcessorTime_ms = 0;
            _totalProcessorTime_ms = 0;
        }

        /// <summary>
        /// Private helper method to calculate the process run time.
        /// </summary>
        /// <param name="startedOn"></param>
        /// <param name="endedOn"></param>
        /// <returns></returns>
        private int CalcRunTime(DateTime startedOn, DateTime endedOn)
        {
            int ms = 0;

            try
            {
                TimeSpan ts = endedOn.Subtract(startedOn);
                ms = ts.Milliseconds;
            }
            catch
            {
                // do nothing - no exceptions on calculating the run time.
            }

            return ms;
        }
                   
        /// <summary>
        /// Private helper method to validate a process path.
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        private bool ValidateProgramPath(string filePath)
        {
            bool status = false;
            try
            {
                FileInfo fi = new FileInfo(filePath);
                status = true;
            }
            catch (FileNotFoundException fnfEx)
            {
                SetLastError(fnfEx.Message);
            }
            catch (Exception ex)
            {
                SetLastError(ex.Message);
            }

            return status;
        }
        #endregion

        #region Disposal
        /// <summary>
        /// IDisposable.Dispose method.
        /// </summary>
   
        ~Processor()
        {
            this.Dispose(false);
        }
        #endregion
    }
}
