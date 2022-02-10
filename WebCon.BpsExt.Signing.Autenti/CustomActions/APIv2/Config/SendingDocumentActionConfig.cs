using WebCon.WorkFlow.SDK.Common;
using WebCon.WorkFlow.SDK.ConfigAttributes;

namespace WebCon.BpsExt.Signing.Autenti.CustomActions.APIv2.Config
{
    public class SendingDocumentActionConfig : PluginConfiguration
    {
        private const string XAssertion = @"{
  ""classifiers"" : [ ""CHALLENGE_CLASSIFIER-UNIQUE_TYPE:ACTION_SELECTION"" ],
  ""attributes"" : {
    ""selectedIds"" : [""EVENT_CLASSIFIER-UNIQUE_TYPE:DOCUMENT_SENT""]
    }
}";
        [ConfigGroupBox(DisplayName = "Authenticate")]
        public Authenticate Auth { get; set; }

        [ConfigEditableText(DisplayName = "Header X-ASSERTION", TagEvaluationMode = EvaluationMode.None, Description = XAssertion, DefaultText = XAssertion, Multiline = true)]
        public string ASSERTION { get; set; }

        [ConfigGroupBox(DisplayName = "Request body")]
        public Body RequestBody { get; set; }

        [ConfigGroupBox(DisplayName = "Attachment section")]
        public AttConfig AttConfig { get; set; }

        [ConfigEditableFormFieldID(DisplayName = "Returned id of document", Description = "Specify a text field on the form where external Documents ID will be saved", IsRequired = true)]
        public int DokumentIdFild { get; set; }
    }

    public class Body
    {
        [ConfigEditableText(DisplayName = "Title")]
        public string Title { get; set; }

        [ConfigEditableText(DisplayName = "Description")]
        public string Description { get; set; }

        [ConfigEditableItemList(DisplayName = "Users")]
        public UserColumns Users { get; set; }
    }

    public class AttConfig
    {
        [ConfigEditableText(DisplayName = "SQL query", Description = @"Query should return a list of attachments' IDs from WFDataAttachmets table.
        Example: Select [ATT_ID] from [WFDataAttachmets] Where [ATT_Name] = 'agreement.pdf'", Multiline = true, TagEvaluationMode = EvaluationMode.SQL)]
        public string AttQuery { get; set; }
    }

    public class UserColumns : IConfigEditableItemList
    {
        [ConfigEditableItemListColumnID(DisplayName = "Name")]
        public int Name { get; set; }

        [ConfigEditableItemListColumnID(DisplayName = "Email")]
        public int Email { get; set; }

        [ConfigEditableItemListColumnID(DisplayName = "PhoneNumber")]
        public int PhoneNumber { get; set; }

        [ConfigEditableItemListColumnID(DisplayName = "Role")]
        public int Role { get; set; }

        public int ItemListId { get; set; }
    }
}