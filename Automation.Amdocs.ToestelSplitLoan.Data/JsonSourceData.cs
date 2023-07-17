using System;

namespace Automation.Amdocs.ToestelSplitLoan.Data
{
    [Serializable]
    public class JsonSourceData
    {
        public string Ctn { get; set; }
        public string Klantnummer { get; set; }
        public string GewensteEinddatum { get; set; }
        public string Telefoonnummer { get; set; }
        public string EinddatumContract { get; set; }
        public string CaseId { get; set; }
        public string AccountId { get; set; }
        public bool GatherCaseAttributesComplete { get; set; }
        
        public JsonSourceData()
        {
            Ctn = string.Empty;
            Klantnummer = string.Empty;
            GewensteEinddatum = string.Empty;
            Telefoonnummer = string.Empty;
            EinddatumContract = string.Empty;
            CaseId = string.Empty;
            AccountId = string.Empty;
            GatherCaseAttributesComplete = false;
        }
    }
}