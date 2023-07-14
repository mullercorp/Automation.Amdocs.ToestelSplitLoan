using System;
using System.Collections.Generic;
using WindowsAccessBridgeInterop;
using Amdocs.Shared.Ufix;
using Amdocs.Shared.Ufix.Pages;
using Amdocs.Shared.Ufix.PagesJab;
using Automation.Amdocs.Shared.Uhelp;
using AutomationFramework;
using AutomationFramework.DAO;
using AutomationFramework.Model;
using AutomationLoader.Classes;
using JAB;
using Newtonsoft.Json;
using Shared.AutoIt;
using Shared.Utilities;

namespace Automation.Amdocs.ToestelSplitLoan.Robot
{
    public class MainFlow : AutomationBaseScript
    {
        private SharedSubProcesses _sharedSubProcesses = new SharedSubProcesses();

        public override void ExecuteScript()
        {
            Logger.AddProcessLog($"Started MainFlow with Key: {CurrentScriptRun.Input.Key} on Robot: {CurrentScriptRun.Monitor.Robot}. Timestamp: {DateTime.Now.TimeOfDay}");

            Initialize();

            //Test();
            //Logger.HandleBusinessRule("Test run","TEST");

            StartAndInitialiseUfix(out var ufixMainWindow, out var vmIdUfix, out var accessBridge, out var password, true);
            Wrappers.IScreenActions.Wait(5);
            Logger.heartbeat();

            Logger.AddProcessLog($"*************Checkpoint***************** --> {CurrentScriptRun.Input.Checkpoint}");
            bool skipInitializeJabAfterNumerousAttemps = false;
            var credentials = CredentialsDAO.GetAppCredentials(0, "Ufix");
            if (CurrentScriptRun.Input.Checkpoint < 2)
            {
                accessBridge = GoToInteractionHome(accessBridge, credentials, ref ufixMainWindow, ref vmIdUfix, ref skipInitializeJabAfterNumerousAttemps);

                if (!new CollectionStateCheck(ufixMainWindow, vmIdUfix, accessBridge).Execute())
                    Logger.HandleBusinessRule("In Collection", "InCollection");
                
                Logger.CheckpointSet(2);
            }
            
            if (!skipInitializeJabAfterNumerousAttemps)
            {
                if (!new UfixBaseJab().AccessUfix(out accessBridge, out ufixMainWindow, out vmIdUfix, out JavaObjectHandle acUfix, 20))
                    Logger.FinishFlowAsError("AccessUfix Error", "AccessUfix Error");
            }

            new OpenViewAssignedProductsViaCtn(ufixMainWindow, vmIdUfix, accessBridge).Execute();
            new ChangeOrderDeviceLoanSplit(ufixMainWindow, vmIdUfix, accessBridge).Execute(out var orderNr);

            var activeCtns = HandleActiveCtns();

            accessBridge = GoToInteractionHomeJabFirst(accessBridge, credentials, ref ufixMainWindow, ref vmIdUfix);
                
            if (activeCtns > 1)
                new HandleSplitLoan(ufixMainWindow, vmIdUfix, accessBridge).Execute();
            

            //TODO: Interaction
            //TODO: CloseCase

            

            Logger.AddProcessLog($"Finished MainFlow with Key: {CurrentScriptRun.Input.Key}");
        }

        private static AccessBridge GoToInteractionHomeJabFirst(AccessBridge accessBridge, Credentials credentials, ref AccessibleWindow ufixMainWindow, ref int vmIdUfix)
        {
            string password;
            if (!new FindCallerCimSearchPage().SearchOnGenericIdViaSearchIcon(accessBridge, ufixMainWindow, vmIdUfix, Wrappers.Source.Data.Klantnummer, SearchOnGenericRadioBtn.FinancialAccount, password: credentials.Password, skipJab: false, tabToCustomerIdField: true))
            {
                Logger.AddProcessLog($"BLEHHHHH, strike 2......");
                ProcessAssistant.KillProcesses("jp2Launcher");
                Wrappers.IScreenActions.Wait();
                MainFlow.StartAndInitialiseUfix(out ufixMainWindow, out vmIdUfix, out accessBridge, out password, true);
                Wrappers.IScreenActions.Wait(40);
                Logger.heartbeat();

                if (!new FindCallerCimSearchPage().SearchOnGenericIdViaSearchIcon(accessBridge, ufixMainWindow, vmIdUfix, Wrappers.Source.Data.Klantnummer, SearchOnGenericRadioBtn.FinancialAccount, password: credentials.Password, skipJab: true, tabToCustomerIdField: true))
                    Logger.FinishFlowAsError("BLEHHHHH, strike 3......", ":-( Ufix :-(");

                if (!new UfixBaseJab().AccessUfix(out accessBridge, out ufixMainWindow, out vmIdUfix, out JavaObjectHandle acUfix, 20))
                    Logger.FinishFlowAsError("AccessUfix Error", "AccessUfix Error");
            }

            return accessBridge;
        }

        private static int HandleActiveCtns()
        {
            var baseUrl = AssetsDAO.GetAssetByName("Uhelp_RestApit_GetActiveCtns");
            Logger.AddProcessLog($"baseUrl: {baseUrl}");
            var apiKey = CredentialsDAO.GetAppCredentials(0, "Uhelp_RestApi");
            Logger.AddProcessLog($"apiKey: {apiKey.Password.Substring(0, 5)}*************\r\nEnvironment: {apiKey.Username}");
            var result = new UhelpBase().GetActiveCtnsViaApi(baseUrl, apiKey.Password, apiKey.Username, Wrappers.Source.Data.Klantnummer, out var activeCtns);
            if (!result)
                Logger.FinishFlowAsError("API GetActiveCtns error...", "ApiErrorGetActiveCtns");
            return activeCtns;
        }

        private static AccessBridge GoToInteractionHome(AccessBridge accessBridge, Credentials credentials, ref AccessibleWindow ufixMainWindow, ref int vmIdUfix, ref bool skipInitializeJabAfterNumerousAttemps)
        {
            string password;
            if (!new FindCallerCimSearchPage().SearchOnGenericIdViaSearchIcon(accessBridge, ufixMainWindow, vmIdUfix, Wrappers.Source.Data.Klantnummer, SearchOnGenericRadioBtn.FinancialAccount, password: credentials.Password, skipJab: true, tabToCustomerIdField: true))
            {
                Logger.AddProcessLog($"BLEHHHHH, strike 2......");
                ProcessAssistant.KillProcesses("jp2Launcher");
                Wrappers.IScreenActions.Wait();
                MainFlow.StartAndInitialiseUfix(out ufixMainWindow, out vmIdUfix, out accessBridge, out password, true);
                Wrappers.IScreenActions.Wait(40);
                Logger.heartbeat();

                if (!new FindCallerCimSearchPage().SearchOnGenericIdViaSearchIcon(accessBridge, ufixMainWindow, vmIdUfix, Wrappers.Source.Data.Klantnummer, SearchOnGenericRadioBtn.FinancialAccount, password: credentials.Password, skipJab: true, tabToCustomerIdField: true))
                {
                    Logger.AddProcessLog($"BLEHHHHH, strike 3......");
                    ProcessAssistant.KillProcesses("jp2Launcher");
                    Wrappers.IScreenActions.Wait();
                    MainFlow.StartAndInitialiseUfix(out ufixMainWindow, out vmIdUfix, out accessBridge, out password, false);
                    Wrappers.IScreenActions.Wait(50);
                    Logger.heartbeat();
                    if (!new FindCallerCimSearchPage().SearchOnGenericIdViaSearchIcon(accessBridge, ufixMainWindow, vmIdUfix, Wrappers.Source.Data.Klantnummer, SearchOnGenericRadioBtn.FinancialAccount, password: credentials.Password, tabToCustomerIdField: true))
                        Logger.DelayInputLine(DateTime.Now.AddSeconds(10), true, maxDelays: 3, errorMessage: "Okay, UFIX is beyond rescue....", remark: "UfixBeyondRescue(3Attemps)");

                    skipInitializeJabAfterNumerousAttemps = true;
                }
            }

            HandlePopupsAndFlash(accessBridge);
            return accessBridge;
        }

        private static void HandlePopupsAndFlash(AccessBridge accessBridge)
        {
            if (new UfixBaseJab().PopupChecker(accessBridge, "Internal Error", 3, out var ufixErrorWindow, out var vmIdUfixError))
            {
                if (!new UfixBaseJab().ClickButton(ufixErrorWindow, vmIdUfixError, "Ok", "push button", 5))
                    Logger.FinishFlowAsError("Popup could not be closed", "PopupCouldNotBeClosed");
            }

            new InteractionHomePage().CheckAndCloseFlashMessages(accessBridge);

            if (new UfixBaseJab().PopupChecker(accessBridge, "Internal Error", 3, out ufixErrorWindow, out vmIdUfixError))
            {
                if (!new UfixBaseJab().ClickButton(ufixErrorWindow, vmIdUfixError, "Ok", "push button", 5))
                    Logger.FinishFlowAsError("Popup could not be closed", "PopupCouldNotBeClosed");
            }
        }

        public static void StartAndInitialiseUfix(out AccessibleWindow ufixMainWindow, out int vmIdUfix, out AccessBridge accessBridge, out string password, bool skipJab)
        {
            password = "";
            ufixMainWindow = null;
            vmIdUfix = 0;
            accessBridge = null;

            if (new StartUfix().Execute(skipJab) == false)
            {
                ProcessAssistant.KillProcesses("jp2Launcher");
                Wrappers.IScreenActions.Wait();

                if (new StartUfix().Execute(skipJab) == false)
                    Logger.FinishFlowAsError("StartUfix Failed.", "UfixStartupError");
            }

            Wrappers.IScreenActions.Wait(2);

            if (!skipJab)
            {
                if (!new UfixBaseJab().AccessUfix(out accessBridge, out ufixMainWindow, out vmIdUfix, out JavaObjectHandle acUfix, 20))
                    Logger.FinishFlowAsError("AccessUfix Error", "AccessUfix Error");
            }

            Wrappers.IScreenActions.Wait();
            Wrappers.IScreenActions.ClickAtCenterOfImage("SearchIconMainPage", "Region_SearchIconMainPage");
            Wrappers.IScreenActions.Wait(3);

            if (Wrappers.IScreenActions.WinActivate("Login", "", 3) != 0)
            {
                Logger.AddProcessLog("Login screen popped up....");
                var credentials = CredentialsDAO.GetAppCredentials(0, "Ufix");
                password = credentials.Password;
                new UfixBase().LoginAfterTimout(credentials.Password);
                Wrappers.IScreenActions.Wait(2);

                new MainPage().ClickCloseSubScreenViaX(3);
            }
        }

        private void Initialize()
        {
            ProcessAssistant.KillProcesses("chrome");
            ProcessAssistant.KillProcesses("chromedriver");
            ProcessAssistant.KillProcesses("rdpclip");
            InputDAO.UpdateRemarkById(CurrentScriptRun.Input.Key, "");

            if (Wrappers.IScreenActions.WinActivate("Login", "", 3) != 0)
                ProcessAssistant.KillProcesses("jp2Launcher");
        }

        private void Test()
        {
            if (!new UfixBaseJab().AccessUfix(out var accessBridge, out var ufixMainWindow, out var vmIdUfix, out JavaObjectHandle acUfix, 20))
                Logger.FinishFlowAsError("AccessUfix Error", "AccessUfix Error");
        }
    }

    public static class GlobalValues
    {
        public static string FinancialAccountId { get; set; }

        static GlobalValues()
        {
            FinancialAccountId = string.Empty;
        }
    }
}