using JAB;
using Shared.AutoIt;

namespace Automation.Amdocs.ToestelSplitLoan.DLRobot
{
    public class CoreWrappers
    {
        public static IScreenActions IScreenActions
        {
            get
            {
                IScreenActions screen = new ScreenActions();
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