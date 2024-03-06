using System;
using System.Collections.Generic;
using System.Linq;
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
                if (!new UfixBaseJab().AccessUfix(out accessBridge, out ufixMainWindow, out vmIdUfix, out JavaObjectHandle acUfix, 20))
                    Logger.FinishFlowAsError("AccessUfix Error", "AccessUfix Error");
                
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

            if(new PendingOrderCheck(ufixMainWindow, vmIdUfix, accessBridge).Execute())
                Logger.HandleBusinessRule("Pending order.", "PendingOrder");
            
            new ChangeOrderDeviceLoanSplit(ufixMainWindow, vmIdUfix, accessBridge).Execute(out var orderNr);
            InputDAO.UpdateRemarkById(CurrentScriptRun.Input.Key, $"OrderNr:{orderNr}");

            //RFC 9-1-2024: Zie mail Berna 28-12-2023 onderstaande uit -->
            //var activeCtns = HandleActiveCtns();

            accessBridge = GoToInteractionHomeJabFirst(accessBridge, credentials, ref ufixMainWindow, ref vmIdUfix);
            HandlePopupsAndFlash(accessBridge);

            //RFC 9-1-2024: Zie mail Berna 28-12-2023 onderstaande uit -->
            //if (activeCtns == 1)
                //new HandleSplitLoan(ufixMainWindow, vmIdUfix, accessBridge).Execute();

            var caseCreationDate = GetCreationDateCase();

            var txt = $"Split Loan case {Wrappers.Source.Data.CaseId} verwerkt met einddatum {Wrappers.Source.Data.EinddatumContract} voor nummer {Wrappers.Source.Data.Ctn} nav port out vanaf {caseCreationDate}";
            new SharedSubProcesses().CreateInteraction(ufixMainWindow, vmIdUfix, accessBridge, "Split Loan","Rekening en betalen", "Lening"/*"Toestel lening"*/, "Lening"/*"Toestel lening uitgelegd"*/, txt);

            new SharedSubProcesses().CloseCase(ufixMainWindow, vmIdUfix, Wrappers.Source.Data.CaseId, txt, false, retrieveIfCloseCaseBtnError:true);

            Logger.AddProcessLog($"Finished MainFlow with Key: {CurrentScriptRun.Input.Key}");
        }

        private static string GetCreationDateCase()
        {
            var caseCreationDate = "";
            Credentials credentials;
            var baseUrl = AssetsDAO.GetAssetByName("Uhelp_RestApi_FetchCasesDataUrl");
            credentials = CredentialsDAO.GetAppCredentials(0, "Uhelp_RestApi");
            var result = new UhelpBase().FetchCasesViaRestApi(baseUrl, Wrappers.Source.Data.AccountId, credentials.Password, credentials.Username, out var cases);
            
            if (result)
            {
                foreach (var @case in cases.Where(@case => string.Equals(@case.CaseId, Wrappers.Source.Data.CaseId, StringComparison.CurrentCultureIgnoreCase)))
                {
                    caseCreationDate = @case.CreationDateTime;
                    break;
                }
            }

            if (caseCreationDate.Contains("T"))
            {
                var temp = caseCreationDate.Split(new string[] { "T" }, StringSplitOptions.None)[0].Trim();
                var caseDateTime = DateTime.ParseExact(temp, "yyyy-MM-dd", null);
                var plusThreeWorkdays = FunctionsAssistant.AddWorkDays(caseDateTime, 3);
                caseCreationDate = plusThreeWorkdays.ToString("dd-MM-yyyy");
            }
            else
            {
                var plusThreeWorkdays = FunctionsAssistant.AddWorkDays(DateTime.Today, 3);
                caseCreationDate = plusThreeWorkdays.ToString("dd-MM-yyyy");
            }

            return caseCreationDate;
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
            var baseUrl = AssetsDAO.GetAssetByName("Uhelp_RestApi_GetActiveCtns");
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
            if (!new FindCallerCimSearchPage().SearchOnGenericIdViaSearchIcon(accessBridge, ufixMainWindow, vmIdUfix, Wrappers.Source.Data.Klantnummer, SearchOnGenericRadioBtn.FinancialAccount, password: credentials.Password, skipJab: false, tabToCustomerIdField: true))
            {
                Logger.AddProcessLog($"BLEHHHHH, strike 2......");
                ProcessAssistant.KillProcesses("jp2Launcher");
                Wrappers.IScreenActions.Wait();
                MainFlow.StartAndInitialiseUfix(out ufixMainWindow, out vmIdUfix, out accessBridge, out password, true);
                Wrappers.IScreenActions.Wait(40);
                Logger.heartbeat();

                if (!new FindCallerCimSearchPage().SearchOnGenericIdViaSearchIcon(accessBridge, ufixMainWindow, vmIdUfix, Wrappers.Source.Data.Klantnummer, SearchOnGenericRadioBtn.FinancialAccount, password: credentials.Password, skipJab: false, tabToCustomerIdField: true))
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
            Wrappers.IScreenActions.MoveTo("CloseSubPageHelper");

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