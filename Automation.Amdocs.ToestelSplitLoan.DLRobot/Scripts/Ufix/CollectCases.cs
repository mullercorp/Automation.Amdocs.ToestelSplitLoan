using System;
using System.Collections.Generic;
using System.Linq;
using Amdocs.Shared.Ufix;
using Amdocs.Shared.Ufix.PagesJab;
using Automation.Amdocs.Shared.Uhelp;
using Automation.Amdocs.ToestelSplitLoan.Data;
using AutomationFramework;
using AutomationFramework.DAO;
using AutomationFramework.Model;
using Newtonsoft.Json;
using MainPage = Amdocs.Shared.Ufix.Pages.MainPage;

namespace Automation.Amdocs.ToestelSplitLoan.DLRobot
{
    public class CollectCases : MyQueuesPage
    {
        private MainPage _mainPage = new MainPage();
        private UfixBaseJab _ufixBaseJab = new UfixBaseJab();

        public bool Execute()
        {
            Logger.MethodEntry("CollectCases.Execute");

            if (!_mainPage.OpenQueueBoDoxGeneric("M"))
                Logger.FinishFlowAsError("Unable to detect BO_DOX_M", "BO_DOX_M_ERROR");

            var casesToUploadIntoDb = CasesToUploadIntoDb("M", CurrentScriptRun.Input.Field8, "My Queues: BO_DOX_M");

            var baseUrl = AssetsDAO.GetAssetByName("Uhelp_TouAttributesApi");
            var apiKey = CredentialsDAO.GetAppCredentials(0, "Uhelp_RestApi");
            foreach (var caseToUpload in casesToUploadIntoDb)
            {
                var jsonSourceData = new JsonSourceData();
                try
                {
                    jsonSourceData.CaseId = caseToUpload.CaseId;
                    var attributes = new UhelpBase().GetCaseAttributesViaApi(baseUrl, caseToUpload.CaseId, apiKey.Password, apiKey.Username);
                    jsonSourceData.AccountId = attributes.account_id.ToString();
                    foreach (var flexibleAttribute in attributes.flexible_attributes)
                    {
                        if (string.Equals(flexibleAttribute.name, "Mobiele nummer", StringComparison.CurrentCultureIgnoreCase))
                            jsonSourceData.Ctn = flexibleAttribute.value.Trim();
                        else if (string.Equals(flexibleAttribute.name, "Klantnummer", StringComparison.CurrentCultureIgnoreCase))
                            jsonSourceData.Klantnummer = flexibleAttribute.value.Trim();
                        else if (string.Equals(flexibleAttribute.name, "Gewenste einddatum", StringComparison.CurrentCultureIgnoreCase))
                            jsonSourceData.GewensteEinddatum = flexibleAttribute.value.Trim();
                        else if (string.Equals(flexibleAttribute.name, "Telefoonnummer", StringComparison.CurrentCultureIgnoreCase))
                            jsonSourceData.Telefoonnummer = flexibleAttribute.value.Trim();
                        else if (string.Equals(flexibleAttribute.name, "Einddatum contract", StringComparison.CurrentCultureIgnoreCase))
                            jsonSourceData.EinddatumContract = flexibleAttribute.value.Trim();
                    }
                    
                    var mobNrEdited = jsonSourceData.Ctn.Replace("-", "").Replace(" ", "").Trim();
                    if (mobNrEdited.StartsWith("06"))
                        jsonSourceData.Ctn = $"316{mobNrEdited.Remove(0, 2).Trim()}";
                    else if (mobNrEdited.StartsWith("+31"))
                        jsonSourceData.Ctn = $"{mobNrEdited.Remove(0, 1).Trim()}";

                    jsonSourceData.GatherCaseAttributesComplete = true;
                    UploadingCasesIntoDb(jsonSourceData);
                }
                catch (Exception e)
                {
                    Logger.AddProcessLog(e.ToString());
                    UploadingCasesIntoDb(jsonSourceData);
                }
            }
            

            InputDAO.UpdateRemarkById(CurrentScriptRun.Input.Key, $"Collected:{casesToUploadIntoDb.Count}");

            new MainPage().ClickCloseSubScreenViaX();

            Logger.MethodExit("CollectCases.Execute");
            return true;
        }

        private static void UploadingCasesIntoDb(JsonSourceData jsonSourceData)
        {
            Logger.AddProcessLog($"Trying to create a Automation inputLine for caseId: {jsonSourceData.CaseId}");

            var json = JsonConvert.SerializeObject(jsonSourceData);
            var inputLine = new Input
            {
                Action = "ToestelSplitLoan",
                Json = json,
                Identifier = $"{jsonSourceData.CaseId}",
                Field2 = jsonSourceData.CaseId,
                ImportUser = "ToestelSplitLoanDLRobot"
            };

            InputDAO.InsertInputLine(int.Parse(CurrentScriptRun.Input.Field9.ToLower().Replace("priority", "")), int.Parse(CurrentScriptRun.Input.Field10.ToLower().Replace("routing", "")), new TimeSpan(0, 0, 0), new TimeSpan(23, 59, 59), DateTime.Now, DateTime.Today.AddMonths(3), inputLine);
            Logger.AddProcessLog($"InputLine for {jsonSourceData.CaseId} created");
        }
    }
}