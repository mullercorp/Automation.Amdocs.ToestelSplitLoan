using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Automation.Amdocs.ToestelSplitLoan.Data;
using AutomationFramework.Classes;
using AutomationFramework.DAO;
using AutomationFramework.Model;
using AutomationLoader.Classes;
using Newtonsoft.Json;

namespace Automation.Amdocs.ToestelSplitLoan.DLTrigger
{
    public partial class PluginForm : LoaderConsolePlugInBaseForm
    {
        private PlugInData _plugInData = new PlugInData();

        public PluginForm()
        {
            InitializeComponent();
        }

        public override void InitializePlugIn(Loader loaderEntry)
        {
            base.InitializePlugIn(loaderEntry);
        }

        public override void ProcessInputLine(IProgress<PlugInData> progress)
        {
            AddLineToConsole(ref progress, "Start Loading ToestelSplitLoanTrigger");
            AddLineToConsole(ref progress, $"HandleToestelSplitLoanTriggerOrders(): {HandleToestelSplitLoanTriggerOrders(ref progress)}");
            AddLineToConsole(ref progress, "Finished ToestelSplitLoan Trigger");
        }

        private void AddLineToConsole(ref IProgress<PlugInData> progress, string line)
        {
            _plugInData.ConsoleText = line;
            progress?.Report(_plugInData);
            Thread.Sleep(50);
        }
        
        public bool HandleToestelSplitLoanTriggerOrders(ref IProgress<PlugInData> progress)
        {
            var finalSourceData = new JsonSourceData {CaseId = "TriggerTask"};
            var json = JsonConvert.SerializeObject(finalSourceData);

            var savedSearchesList = new List<string>();
            if (LoaderEntry.Par4.Contains(";"))
            {
                savedSearchesList = LoaderEntry.Par4.Split(';').ToList();
                AddLineToConsole(ref progress, $"savedSearchesList: {savedSearchesList.Count}");
                foreach (var savedSearch in savedSearchesList)
                    AddLineToConsole(ref progress, $"savedSearch: {savedSearch}");
            }
            else
                savedSearchesList.Add(LoaderEntry.Par4);

            foreach (var savedSearch in savedSearchesList)
            {
                var inputLine = new Input {Action = "ToestelSplitLoanDLRobot", Json = json, Identifier = $"TriggerTask:{savedSearch}", Field1 = LoaderEntry.Par1, Field2 = LoaderEntry.Par2, Field3 = LoaderEntry.Par3, Field4 = LoaderEntry.Par4, Field8 = savedSearch, Field9 = LoaderEntry.Par9, Field10 = LoaderEntry.Par10};
                InputDAO.InsertInputLine(LoaderEntry.Priority, Convert.ToInt32(LoaderEntry.Routing), LoaderEntry.StartRunWindow, LoaderEntry.EndRunWindow, DateTime.Now, DateTime.Now.AddDays(360), inputLine);
            }
            
            return true;
        }
    }
}