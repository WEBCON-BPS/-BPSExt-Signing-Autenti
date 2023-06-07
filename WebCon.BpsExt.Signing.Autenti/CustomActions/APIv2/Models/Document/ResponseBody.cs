using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebCon.BpsExt.Signing.Autenti.CustomActions.APIv2.Models.Document
{
    public class ResponseBody
    {
        public string id { get; set; }
        public string title { get; set; }
        public string description { get; set; }
        public object processLanguage { get; set; }
        public string status { get; set; }
        public Party[] parties { get; set; }
        public File[] files { get; set; }
        public Tag[] tags { get; set; }
        public object[] constraints { get; set; }
        public object[] flags { get; set; }
        public DateTime createdAt { get; set; }
        public DateTime modifiedAt { get; set; }
    }

    public class Party
    {
        public Party1 party { get; set; }
        public string role { get; set; }
        public string participationStatus { get; set; }
        public Constraint[] constraints { get; set; }
        public bool currentUser { get; set; }
        public Event[] events { get; set; }
    }

    public class Party1
    {
        public string id { get; set; }
        public string firstName { get; set; }
        public string lastName { get; set; }
        public string name { get; set; }
        public Contact[] contacts { get; set; }
        public object[] extIds { get; set; }
        public Relationship[] relationships { get; set; }
    }

    public class Contact
    {
        public string type { get; set; }
        public Attributes attributes { get; set; }
    }

    public class Attributes
    {
        public string email { get; set; }
    }

    public class Relationship
    {
        public string type { get; set; }
        public Party2 party { get; set; }
        public Attributes1 attributes { get; set; }
    }

    public class Party2
    {
        public object id { get; set; }
        public object firstName { get; set; }
        public object lastName { get; set; }
        public string name { get; set; }
        public object contacts { get; set; }
        public Extid[] extIds { get; set; }
        public object relationships { get; set; }
    }

    public class Extid
    {
        public string identificationSpace { get; set; }
        public string identifier { get; set; }
    }

    public class Attributes1
    {
        public string relationshipDescription { get; set; }
    }

    public class Constraint
    {
        public string[] constrainedActions { get; set; }
        public string[] classifiers { get; set; }
        public Attributes2 attributes { get; set; }
    }

    public class Attributes2
    {
        public int priority { get; set; }
        public string[] requiredClassifiers { get; set; }
        public string phoneNumber { get; set; }

    }

    public class Event
    {
        public DateTime timestamp { get; set; }
        public string id { get; set; }
        public string eventType { get; set; }
        public string[] classifiers { get; set; }
        public Actor actor { get; set; }
        public Object _object { get; set; }
        public Attributes3 attributes { get; set; }
    }

    public class Actor
    {
        public string id { get; set; }
    }

    public class Object
    {
        public string id { get; set; }
        public string type { get; set; }
    }

    public class Attributes3
    {
    }

    public class File
    {
        public string id { get; set; }
        public string filename { get; set; }
        public object description { get; set; }
        public string version { get; set; }
        public object size { get; set; }
        public object modificationTime { get; set; }
        public string filePurpose { get; set; }
        public string mimeType { get; set; }
        public object mimeTypeBeforeConversion { get; set; }
        public object conversionStatus { get; set; }
    }

    public class Tag
    {
        public string id { get; set; }
        public string description { get; set; }
        public string type { get; set; }
    }
}
