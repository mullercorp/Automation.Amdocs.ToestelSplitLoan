using System;
using System.Collections.Generic;
using WindowsAccessBridgeInterop;
using Amdocs.Shared.Ufix;
using Amdocs.Shared.Ufix.Pages;
using AutomationFramework;
using AutomationFramework.DAO;
using AutomationLoader.Classes;
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

            if (!new UfixBaseJab().AccessUfix(out accessBridge, out ufixMainWindow, out vmIdUfix, out var acUfix, 3))
                Logger.FinishFlowAsError("AccessUfix Error");

            Logger.AddProcessLog($"*************Checkpoint***************** --> {CurrentScriptRun.Input.Checkpoint}");

            //TODO: GetCollectionState hopefully via HUUG.
            
            new OpenViewAssignedProductsViaCtn(ufixMainWindow, vmIdUfix, accessBridge).Execute();
            
            _sharedSubProcesses.HandleChangeOrderPartOne(ufixMainWindow, vmIdUfix, accessBridge, "Device Loan", skipNextInRelateProductScreen:true);
            _sharedSubProcesses.NextUntilOrderSummaryPageAppears(ufixMainWindow, vmIdUfix, accessBridge, true);
            new MainPage().ClickCloseSubScreenViaX(1, accessBridge: accessBridge);
            
            
            

            
            
            

            Logger.AddProcessLog($"Finished MainFlow with Key: {CurrentScriptRun.Input.Key}");
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