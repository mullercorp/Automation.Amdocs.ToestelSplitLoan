using Amdocs.Shared.Ufix;
using Amdocs.Shared.Ufix.Pages;
using Amdocs.Shared.Ufix.PagesJab;
using AutomationFramework;
using AutomationFramework.DAO;
using Shared.Utilities;
using WindowsAccessBridgeInterop;

namespace Automation.Amdocs.ToestelSplitLoan.Robot
{
    public class HandleSplitLoan : InteractionHomePage
    {
        private AccessibleWindow _ufixWindow;
        private int _vmId;
        private AccessBridge _accessBridge;
        private SharedSubProcesses _sharedSubProcesses = new SharedSubProcesses();
        
        public HandleSplitLoan(AccessibleWindow ufixWindowUfix, int vmIdUfix, AccessBridge accessBridge)
        {
            _ufixWindow = ufixWindowUfix;
            _vmId = vmIdUfix;
            _accessBridge = accessBridge;
        }
        
        public bool Execute()
        {
            Logger.MethodEntry("HandleSplitLoan");
            var result = DoWork();
            Logger.MethodExit("HandleSplitLoan");
            return result;
        }
        
        private bool DoWork()
        {
            /*GetTab(_ufixWindow, _vmId, "Interaction Home", 10, out var ihTab);

            new SharedSubProcesses().OpenFinancialAccount(_ufixWindow, _vmId, "", ihTab, true);
            var financialAccountPageFrame = new ViewFinancialAccountPage().GetViewFinancialAccountFrame(_ufixWindow);
            
            var debtReasonLbl = JabBaseActions.GetAccessibleNodeWithParameters(_ufixWindow, "Debt Reason:", "label", 5,"showing", false, financialAccountPageFrame);
            if (debtReasonLbl == null)
                Logger.FinishFlowAsError("Debt Reason labelnot Found", "DebtReasonLbl");

            //var labelReasonAci = JabBaseActions.GetAccessibleContextInfo(vmIdUfix, labelReason);
            var parent = debtReasonLbl.GetParent();
            var comboBoxDebtReason = JabBaseActions.GetChildFromParentNode(parent, debtReasonLbl.GetIndexInParent() - 1);

            if (!ClickButton(_vmId, comboBoxDebtReason))
                Logger.FinishFlowAsError("Debt Reason lbl could not be Clicked", "ProblemDebtReasonClick");
            
            IScreenActions.Wait(2);
            IScreenActions.Send("split");
            IScreenActions.Wait(2);
            IScreenActions.EnterWait();
            IScreenActions.Wait();
            
            if (!ClickButton(_ufixWindow, _vmId, "Save", "push button", 3, "enabled", child: financialAccountPageFrame, waitBeforeClicking: 2))
                Logger.FinishFlowAsError("Save Button Could Not Be Clicked", "SaveButtonCouldNotBeClicked");
            
            new MainPage().ClickCloseSubScreenViaX(1, accessBridge:_accessBridge);
            
            return true;*/

            return _sharedSubProcesses.HandleSplitLoan(_ufixWindow, _vmId, _accessBridge);
        }
    }
}