using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCon.BpsExt.Signing.Autenti.CustomActions.Models
{
    public class EnvelopeRequest
    {
        public string title { get; set; }
        public string message { get; set; }
        public Sender sender { get; set; }
        public Viewer[] viewers { get; set; }
        public Signer[] signers { get; set; }
    }

    public class Sender
    {
        public string type { get; set; }
    }

    public class Viewer
    {
        public string email { get; set; }
        public Personaldata personalData { get; set; }
        public Organizationinfo organizationInfo { get; set; }
    }

    public class Personaldata
    {
        public string givenName { get; set; }
        public string surname { get; set; }
    }

    public class Organizationinfo
    {
        public string name { get; set; }
        public string taxNumber { get; set; }
        public string jobTitle { get; set; }
    }

    public class Signer
    {
        public string email { get; set; }
        public string signatureType { get; set; }
        public Personaldata1 personalData { get; set; }
        public Organizationinfo1 organizationInfo { get; set; }
        public Smsauthorization smsAuthorization { get; set; }
    }

    public class Personaldata1
    {
        public string givenName { get; set; }
        public string surname { get; set; }
    }

    public class Organizationinfo1
    {
        public string name { get; set; }
        public string taxNumber { get; set; }
        public string jobTitle { get; set; }
    }

    public class Smsauthorization
    {
        public string phoneNumber { get; set; }
    }


    public class EnvelopeResponse
    {
        public string documentId { get; set; }
    }

}
