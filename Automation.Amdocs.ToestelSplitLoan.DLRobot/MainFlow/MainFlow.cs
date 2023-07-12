using System;
using AutomationFramework;
using AutomationFramework.DAO;
using AutomationLoader.Classes;
using Shared.Utilities;

namespace Automation.Amdocs.ToestelSplitLoan.DLRobot
{
    public class MainFlow : AutomationBaseScript
    {
        public override void ExecuteScript()
        {
            Logger.AddProcessLog($"Started MainFlow with Key: {CurrentScriptRun.Input.Key}. Timestamp: {DateTime.Now.TimeOfDay}");
            InputDAO.UpdateRemarkById(CurrentScriptRun.Input.Key, "");

            ProcessAssistant.KillProcesses("jp2Launcher");
            CoreWrappers.IScreenActions.Wait();

            if (new StartUfix().Execute() == false)
            {
                ProcessAssistant.KillProcesses("jp2Launcher");
                CoreWrappers.IScreenActions.Wait();

                if (new StartUfix().Execute() == false)
                    Logger.FinishFlowAsError("StartUfix Failed.", "UfixStartupError");
            }

            new CollectCases().Execute();

            Logger.AddProcessLog($"Finished MainFlow with Key: {CurrentScriptRun.Input.Key}");
        }
    }
}