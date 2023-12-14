using System.Linq;
using Amdocs.Shared.Ufix;
using WindowsAccessBridgeInterop;
using Amdocs.Shared.Ufix.Pages;
using Amdocs.Shared.Ufix.PagesJab;
using AutomationFramework;

namespace Automation.Amdocs.ToestelSplitLoan.Robot
{
    public class PendingOrderCheck : ProductDetailsPage
    {
        private AccessibleWindow _ufixWindow;
        private int _vmId;
        private AccessBridge _accessBridge;
        private ViewBillingArrangementPage _viewBillingArrangementPage = new ViewBillingArrangementPage();

        public PendingOrderCheck(AccessibleWindow ufixWindowUfix, int vmIdUfix, AccessBridge accessBridge)
        {
            _ufixWindow = ufixWindowUfix;
            _vmId = vmIdUfix;
            _accessBridge = accessBridge;
        }

        public bool Execute()
        {
            Logger.MethodEntry("PendingOrderCheck");

            var result = DoWork();

            Logger.MethodExit("PendingOrderCheck");

            return result;
        }

        private bool DoWork()
        {
            /*new MainPage().ClickCloseSubScreenViaX(2, accessBridge: _accessBridge, withDiscardSaveFormCheck:true);
            
            var mobilePhoneSearch = SearchWithPhoneNumberAndSelect(_accessBridge, _ufixWindow, _vmId, Wrappers.Source.Data.Ctn, out var noEntry, 5);

            if (!mobilePhoneSearch && noEntry)
                Logger.HandleBusinessRule($"No results on {Wrappers.Source.Data.Ctn}. Please analyse.", $"NoResults:{Wrappers.Source.Data.Ctn}");
            else if (!mobilePhoneSearch)
                Logger.FinishFlowAsError("Search with Mobile Phone Error", "SearchWithMobilePhoneError");

            if (!ClickViewAssignedProductsBtnGeneric(_ufixWindow, _vmId, "Search: Contact and Subscription"))
                Logger.FinishFlowAsError("ClickButton Select Error", "ClickButtonSelectError");
            
            var assignedProductsDetailsFrame = GetAssignedProductsDetailsFrame(_ufixWindow);

            if(PopupChecker(_accessBridge, "Internal Error", 2, out var ufixErrorWindow1, out var vmIdUfixError1) 
               || PopupChecker(_accessBridge, "Problem", 1, out ufixErrorWindow1, out vmIdUfixError1))
            {
                if (!ClickButton(ufixErrorWindow1, vmIdUfixError1, "Ok", "push button", 5))
                    Logger.FinishFlowAsError("Popup could not be closed", "PopupCouldNotBeClosed");
            }

            var result = CheckIfNoPendingOrders(_ufixWindow, _vmId, 2, assignedProductsDetailsFrame);
            
            new MainPage().ClickCloseSubScreenViaX(2, accessBridge: _accessBridge, withDiscardSaveFormCheck:true);*/
            
            return new SharedSubProcesses().PendingOrderCheckToesteSplit(_ufixWindow, _vmId, _accessBridge, Wrappers.Source.Data.Ctn);
        }
    }
}