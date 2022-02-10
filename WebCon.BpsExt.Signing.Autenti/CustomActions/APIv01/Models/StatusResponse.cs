using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCon.BpsExt.Signing.Autenti.CustomActions.APIv1.Models
{

    public class StatusResponse
    {
        public string status { get; set; }
        public Decisionprocess[] decisionProcesses { get; set; }
    }

    public class Decisionprocess
    {
        public string id { get; set; }
        public Signers signer { get; set; }
        public string decision { get; set; }
        public string handedOverToDecisionProcess { get; set; }
    }

    public class Signers
    {
        public string email { get; set; }
    }

}
