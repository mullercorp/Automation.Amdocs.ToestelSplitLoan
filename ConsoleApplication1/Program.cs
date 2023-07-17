using System;
using Automation.Amdocs.ToestelSplitLoan.Data;
using Newtonsoft.Json;

namespace ConsoleApplication1
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Start");
            
            var finalSourceData = new JsonSourceData {CaseId = "TriggerTask"};
            var json = JsonConvert.SerializeObject(finalSourceData);
            
            
            
            Console.WriteLine(json);
            
            Console.WriteLine("Finished");
        }
    }
}