using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using LabObjects.DotNetGlue.SharedKernel;

namespace LabObjects.DotNetGlue.Diagnostics
{
    /// <summary>
    /// HostProcessor Class - a class that wraps the Processor Host Process 
    /// and serves as a Processor Factory and Dictionary.
    /// 
    /// <para>As a class and dictionary, hosts
    /// can create Processors with an assigned key. The dictionary may be queried for a Processor
    /// using the method: GetProcessor(key). </para>
    /// </summary>
    /// <remarks>
    /// <para>Inherits from DotNetGlueBase.</para>
    /// <para>Many HostProcess Process properties are exposed as 'getters' to avoid destabalizing hosts</para>
    /// <para>This class can also be used on "Exit" of a Host application to check for running processes and 
    /// termiate them if needed.</para>
    /// <para>Note: use of Exited event for the host is not applicable - the host termination also ends the lifetime 
    /// of this class.</para>
    /// <para>Host Processes that have not been started with the Component Model may not have all STartInfo information available.</para>
    /// </remarks>
    /// <seealso cref="System.Diagnostics.Process"/>
    /// <seealso cref="LabObjects.DotNetGlue.Diagnostics.Processor"/> 
    /// <seealso cref="LabObjects.DotNetGlue.Diagnostics.ProcessorBase"/>
    /// <seealso cref="LabObjects.DotNetGlue.SharedKernel.DotNetGlueBase"/>
    /// <namespace>LabObjects.DotNetGlue.Diagnostics</namespace>
    public  class HostProcessor:ProcessorBase
    {
        //private readonly Process _hostProcess;
        private readonly Dictionary<String, Processor> _processDictionary = new Dictionary<String, Processor>();

        #region Constructors
        /// <summary>
        /// HostProcessor Constructor
        /// <para>Retreieves the host process using <see cref="Process.GetCurrentProcess()"/></para>
        /// </summary>
        /// <seealso cref="System.Diagnostics.Process"/>
        /// <seealso cref="System.Diagnostics.Process.GetCurrentProcess"/>
        /// <seealso cref="LabObjects.DotNetGlue.Diagnostics.ProcessorBase"/>
        /// <seealso cref="LabObjects.DotNetGlue.Diagnostics.Processor"/>
        public HostProcessor():base(Process.GetCurrentProcess())
        {
            GetFileVersionInfo();
            GetProcessStartInfo();
            GetStartupProperties();
            RefreshRealtimeProperties();
        }
        #endregion

        #region Public Properties

 
        public override String Arguments {  get { return base.Arguments; } }

        public override Boolean CreateNoWindow { get => base.CreateNoWindow;  }

        public override String Domain { get => base.Domain; }

        public override Boolean ErrorDialog { get => base.ErrorDialog;  }

        public override String FileName { get => base.FileName;  }

        /// <summary>
        /// HasActiveProcessors
        /// </summary>
        public Boolean HasActiveProcessors
        {
            get
            {
                ProcessorScavenger();
                return (_processDictionary.Count > 0 ? true : false);
            }
        }

        public String[] Keys
        {
            get
            {
                return _processDictionary.Select(k => k.Key.ToString()).ToArray();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override Boolean LoadUserProfile { get => base.LoadUserProfile; }

        public override Boolean RedirectStandardError { get => base.RedirectStandardError; }

        public override Boolean RedirectStandardInput { get => base.RedirectStandardInput; }

        public override Boolean RedirectStandardOutput { get => base.RedirectStandardOutput; }
        /// <summary>
        /// Gets the Wisdows user account to use to run ethe process. 
        /// </summary>
        /// <seealso cref="System.Diagnostics.ProcessStartInfo.UserName"/>
        /// <seealso cref="ProcessorBase.SetPassword(string)"/>
        /// <seealso cref="ProcessorBase.Domain"/>
        public override String UserName { get { return base.UserName; } }

        /// <summary>
        /// Gets  whether the process should be run using the operating system shell.
        /// </summary>
        /// <remarks>if the UserName property is null or an empty string, UseShellExecute must be false.</remarks>
        /// <seealso cref="System.Diagnostics.ProcessStartInfo.UseShellExecute"/>
        public override Boolean UseShellExecute { get { return base.UseShellExecute; } }

        public override ProcessWindowStyle WindowStyle { get => base.WindowStyle; }
        /// <summary>
        /// 
        /// </summary>
        public override String WorkingDirectory {  get { return base.WorkingDirectory; } }
  
        #endregion


        #region Public Methods
        /// <summary>
        /// CreateProcessor
        /// </summary>
        /// <returns></returns>
        public Processor CreateProcessor()
        {
 
            // factories can also provide overloads allowing for fileName, etc.          
            Process process = new Process();
            Processor processor = new Processor(process);
            _processDictionary.Add(processor.Key, processor);
            return processor;
        }

        /// <summary>
        /// GetProcessor
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public Processor GetProcessor(String key)
        {
            if (_processDictionary.ContainsKey(key))
            {
                var p = _processDictionary[key];
                if (p == null)
                {
                    _processDictionary.Remove(key);
                    SetLastError($"Processor for key: {key} is null");
                    return null;
                }
                else
                    return p;
            }
            else
            {
                SetLastError($"Processor not found for key: {key}");
                return null;
            }
        }

        /// <summary>
        /// GetProcessor
        /// </summary>
        /// <param name="key">GUID string that is a key to Processor Dictrionary</param>
        /// <returns></returns>
        public Processor GetProcessor(Guid key)
        {
            if (key != Guid.Empty)
            {
                return GetProcessor(key.ToString());
            }
            else
            {
                SetLastError($"key is not a valid GUID: {key.ToString()}");
                return null;
            }
        }
        #endregion


        #region private methods
        private void ProcessorScavenger()
        {
            foreach (var item in _processDictionary)
            {
                if (item.Value == null)
                    _processDictionary.Remove(item.Key);                   
            }
        }
        #endregion

    }
}
