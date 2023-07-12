using Automation.Amdocs.ToestelSplitLoan.Data;
using AutomationFramework;
using Newtonsoft.Json;
using JAB;
using Shared.AutoIt;

namespace Automation.Amdocs.ToestelSplitLoan.Robot
{
    public class Wrappers
    {
        public class Source
        {
            public static JsonSourceData Data
            {
                get
                {
                    return JsonConvert.DeserializeObject<JsonSourceData>(CurrentScriptRun.Input.Json);
                }
            }
        }
        
        public static IScreenActions IScreenActions
        {
            get
            {
                IScreenActions screen = new ScreenActions();
                screen.Options.OCRMode = true;
                return screen;
            }
        }
        
        public static JabBase JabBaseActions
        {
            get
            {
                JabBase jabBase = new JabBase();
                return jabBase;
            }
        }
    }
}