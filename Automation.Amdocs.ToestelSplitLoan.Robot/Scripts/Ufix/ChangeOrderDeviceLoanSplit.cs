using System;
using Amdocs.Shared.Ufix;
using Amdocs.Shared.Ufix.Pages;
using Amdocs.Shared.Ufix.PagesJab;
using AutomationFramework;
using Shared.AutoIt;
using WindowsAccessBridgeInterop;

namespace Automation.Amdocs.ToestelSplitLoan.Robot
{
    public class ChangeOrderDeviceLoanSplit : SharedSubProcesses
    {
        private AccessibleWindow _ufixWindow;
        private int _vmId;
        private AccessBridge _accessBridge;

        public ChangeOrderDeviceLoanSplit(AccessibleWindow ufixWindowUfix, int vmIdUfix, AccessBridge accessBridge)
        {
            _ufixWindow = ufixWindowUfix;
            _vmId = vmIdUfix;
            _accessBridge = accessBridge;
        }

        public bool Execute(out string orderNr)
        {
            Logger.MethodEntry("ChangeOrderDeviceLoanSplit");
            var result = DoWork(out orderNr);
            Logger.MethodExit("ChangeOrderDeviceLoanSplit");
            return result;
        }

        private bool DoWork(out string orderNr)
        {
            orderNr = "";
            
            GoToSearchOrderingAssignedProducts(_ufixWindow, _vmId);

            var internalFrame = new SearchAssignedProductsPage().GetSearchAssignedProductsInternalFrame(_ufixWindow);

            if (!FillIntoTabField(_ufixWindow, _vmId, "Service ID", "push button", 10, Wrappers.Source.Data.Ctn, extraJabClick: true, parent: internalFrame, enterAfter: true))
                Logger.FinishFlowAsError("TypeIntoServiceID Error", "TypeIntoServiceIDError");

            IScreenActions.Wait(5);

            internalFrame = new SearchAssignedProductsPage().GetSearchAssignedProductsInternalFrame(_ufixWindow);
            //var table = JabBaseActions.GetAccessibleNodeWithParameters(_ufixWindow, "", "table", 5, child:internalFrame);
            //if (table == null)
            //Logger.FinishFlowAsError("Can't claim table", "ResultTableError");

            var resultBtn = JabBaseActions.GetAccessibleNodeWithParameters(_ufixWindow, "1 Record(s)", "push button", 2, child:internalFrame);
            //if (!CheckResultsAfterSearch(table, out var rowCount, out var columnCount))
            if (resultBtn == null)
                Logger.HandleBusinessRule($"No or Mulitple results on {Wrappers.Source.Data.Ctn}", $"NoOrMultipleResults:{Wrappers.Source.Data.Ctn}");

            if (!new SearchAssignedProductsPage().ClickSelectAllProductsCheckbox(_ufixWindow, _vmId, internalFrame))
                Logger.FinishFlowAsError("Unable to select all assigned prodcuts", "UnableToCheckAllAssignedProducts");

            IScreenActions.Wait(3);

            if (!GoToActionsOrderingChange(_ufixWindow, _vmId))
                Logger.FinishFlowAsError("Actions-Ordering-Change Tab not Open", "ActionsOrderingChangeError");

            IScreenActions.Wait(3);

            Logger.AddProcessLog("Double check if Handle Pending OrderActions Dialog occurs....");
            if(IScreenActions.WinWaitActiveImage("HandlePendingOrderActionsDialog","Region_HandlePendingOrderActionsDialog",false, 2))
            {
                IScreenActions.ClickAtCenterOfImage("xCloseBtnInDialog", "Region_xCloseBtnInDialog");
                new MainPage().ClickCloseSubScreenViaX();
                Logger.HandleBusinessRule("Handle Pending Order Action Dialog popups...", "HandlePendingOrderActionDialog");
            }

            if (PopupChecker(_accessBridge, "Internal Error", 5, out var ufixErrorWindow2, out var vmIdUfixError2) || PopupChecker(_accessBridge, "Problem", 5, out ufixErrorWindow2, out vmIdUfixError2))
            {
                if (!ClickButton(ufixErrorWindow2, vmIdUfixError2, "Ok", "push button", 5))
                    Logger.FinishFlowAsError("Popup could not be closed", "PopupCouldNotBeClosed");
            }
            Logger.heartbeat();

            new SharedSubProcesses().HandleSelectFrameworkAgreementScreen(_ufixWindow, _vmId, 10);
            
            if (!new UpdateOrderActionAttributesPage().SelectReasonOfChangeOrder(_ufixWindow, _vmId, 3, "Device Loan"))
                Logger.FinishFlowAsError("SelectReasonOfChangeOrder error", "SelectReasonOfChangeOrderError");

            //if (!new CeasePage().SetTerminationDate(_ufixWindow, _vmId, DateTime.Today.AddDays(15), 3))
                //Logger.FinishFlowAsError("Set termination day error", "SetTerminationDayError");
            
            IScreenActions.Tab(2);

            bool inOrderSummaryTab = GetTab(_ufixWindow, _vmId, "Order Summary", 4, out var orderSummaryTab);
            int count = 0;
            while (!inOrderSummaryTab)
            {
                ClickNextBtnGeneric(_ufixWindow, _vmId, 3);
                IScreenActions.Wait(7);
                if (PopupChecker(_accessBridge, "Message", 4, out var message, out var messageId))
                {
                    if (!ClickButton(message, messageId, "Ok", "push button", 5))
                        Logger.FinishFlowAsError("Popup could not be closed", "PopupCouldNotBeClosed");
                }
                if (PopupChecker(_accessBridge, "Validation Messages", 2, out var validationMessage, out var validationMessageId))
                {
                    if (!ClickButton(validationMessage, validationMessageId, "Close", "push button", 2))
                        Logger.FinishFlowAsError("Popup could not be closed", "PopupCouldNotBeClosed");
                }

                if (GetTab(_ufixWindow, _vmId, "Order Summary", 4, out orderSummaryTab))
                    inOrderSummaryTab = true;

                count++;
                if(count > 9)
                    Logger.FinishFlowAsError("Unable to get to submit btn.....","UnableToGetToSubmit");
            }

            if (!ClickSubmitOrderBtnGeneric(_ufixWindow, _vmId))
                Logger.FinishFlowAsError("SubmitOrder error", "SubmitOrderError");

            IScreenActions.Wait(5);

            orderNr = GetOrderNumberAfterSubmit(_ufixWindow, _vmId);

            CloseOrderAndCustomer(_ufixWindow, _vmId, _accessBridge, 2);

            return true;
        }
    }
}