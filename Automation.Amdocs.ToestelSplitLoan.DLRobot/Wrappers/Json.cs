using Automation.Amdocs.ToestelSplitLoan.Data;
using AutomationFramework;
using Newtonsoft.Json;

namespace Automation.Amdocs.ToestelSplitLoan.DLRobot
{
    public class Json
    {
        public static JsonSourceData Data
        {
            get { return JsonConvert.DeserializeObject<JsonSourceData>(CurrentScriptRun.Input.Json); }
        }
    }
}