using System.Linq;
using WindowsAccessBridgeInterop;
using Amdocs.Shared.Ufix.Pages;
using Amdocs.Shared.Ufix.PagesJab;
using AutomationFramework;

namespace Automation.Amdocs.ToestelSplitLoan.Robot
{
    public class OpenViewAssignedProductsViaCtn : InteractionHomePage
    {
        private AccessibleWindow _ufixWindow;
        private int _vmId;
        private AccessBridge _accessBridge;
        private ViewBillingArrangementPage _viewBillingArrangementPage = new ViewBillingArrangementPage();

        public OpenViewAssignedProductsViaCtn(AccessibleWindow ufixWindowUfix, int vmIdUfix, AccessBridge accessBridge)
        {
            _ufixWindow = ufixWindowUfix;
            _vmId = vmIdUfix;
            _accessBridge = accessBridge;
        }

        public bool Execute()
        {
            Logger.MethodEntry("OpenViewAssignedProductsViaCtn");

            var result = DoWork();

            Logger.MethodExit("OpenViewAssignedProductsViaCtn");

            return result;
        }

        private bool DoWork()
        {
            new MainPage().ClickCloseSubScreenViaX(2, accessBridge: _accessBridge, withDiscardSaveFormCheck:true);
            
            var mobilePhoneSearch = SearchWithPhoneNumberAndSelect(_accessBridge, _ufixWindow, _vmId, Wrappers.Source.Data.Ctn, out var noEntry, 5);

            if (!mobilePhoneSearch && noEntry)
                Logger.HandleBusinessRule($"No results on {Wrappers.Source.Data.Ctn}. Please analyse.", $"NoResults:{Wrappers.Source.Data.Ctn}");
            else if (!mobilePhoneSearch)
                Logger.FinishFlowAsError("Search with Mobile Phone Error", "SearchWithMobilePhoneError");

            if (!ClickViewAssignedProductsBtnGeneric(_ufixWindow, _vmId, "Search: Contact and Subscription"))
                Logger.FinishFlowAsError("ClickButton Select Error", "ClickButtonSelectError");
            
            return true;
        }
    }
}