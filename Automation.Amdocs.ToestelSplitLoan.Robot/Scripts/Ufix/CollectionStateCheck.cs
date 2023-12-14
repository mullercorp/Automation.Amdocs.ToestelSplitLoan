using System;
using System.Globalization;
using Amdocs.Shared.Ufix;
using Amdocs.Shared.Ufix.Pages;
using Amdocs.Shared.Ufix.PagesJab;
using AutomationFramework;
using WindowsAccessBridgeInterop;

namespace Automation.Amdocs.ToestelSplitLoan.Robot
{
    public class CollectionStateCheck : InteractionHomePage
    {
        private AccessibleWindow _ufixWindow;
        private int _vmId;
        private AccessBridge _accessBridge;
        private SharedSubProcesses _sharedSubProcesses = new SharedSubProcesses();
        
        public CollectionStateCheck(AccessibleWindow ufixWindowUfix, int vmIdUfix, AccessBridge accessBridge)
        {
            _ufixWindow = ufixWindowUfix;
            _vmId = vmIdUfix;
            _accessBridge = accessBridge;
        }
        
        public bool Execute()
        {
            Logger.MethodEntry("CollectionStateCheck");
            var result = DoWork();
            Logger.MethodExit("CollectionStateCheck");
            return result;
        }
        
        private bool DoWork()
        {
            /*GetTab(_ufixWindow, _vmId, "Interaction Home", 10, out var ihTab);
            new SharedSubProcesses().OpenFinancialAccount(_ufixWindow, _vmId, "", ihTab, true);
            var financialAccountPageFrame = new ViewFinancialAccountPage().GetViewFinancialAccountFrame(_ufixWindow);
            var result = new ViewFinancialAccountPage().GetCollectionState(_ufixWindow, _vmId, out var collectionState, financialAccountPageFrame);
            if(!result)
                Logger.FinishFlowAsError("Unable to read out Collection State, please analyse.","ErrorWhileReadingCollectionState");

            if (collectionState.ToUpper().Contains("In Collection Since".ToUpper()))
            {
                Logger.AddProcessLog("Klant is in collection...");
                return false;
            }
            
            new MainPage().ClickCloseSubScreenViaX(2, true, _accessBridge);*/
            
            return _sharedSubProcesses.CollectionStateCheckToestelSplit(_ufixWindow, _vmId, _accessBridge);
        }
    }
}