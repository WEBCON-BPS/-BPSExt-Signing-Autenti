using WebCon.WorkFlow.SDK.Common;
using WebCon.WorkFlow.SDK.ConfigAttributes;

namespace WebCon.BpsExt.Signing.Autenti.CustomActions.SendEnvelope
{
    public class SendEnvelopeActionConfig : PluginConfiguration
    {
        [ConfigGroupBox(DisplayName = "Autenti API Settings")]
        public ApiConfiguration ApiConfig { get; set; }   

        [ConfigGroupBox(DisplayName = "Attachment selection")]
        public InputAttConfig AttConfig { get; set; }

        [ConfigGroupBox(DisplayName = "Recipients selection")]
        public ItemListConfig Users { get; set; }

        [ConfigGroupBox(DisplayName = "Message content")]
        public MessageContent MessageContent { get; set; }      
    }

    public class ApiConfiguration
    {
        [ConfigEditableText(DisplayName = "Integration Key", IsRequired = true)]
        public string TokenValue { get; set; }
    }
    public class MessageContent
    {
        [ConfigEditableText(DisplayName = "Subject", IsRequired = true)]
        public string Title { get; set; }

        [ConfigEditableText(DisplayName = "Content", Multiline = true)]
        public string Msg { get; set; }

        [ConfigEditableEnum(DisplayName = "Sender details", Description = "Role of the sender in the signing process.", DefaultValue = 0)]
        public SenderType Type { get; set; }
    }

        public class InputAttConfig
        {

        [ConfigEditableEnum(DisplayName = "Selection mode", Description = "The attachments to sign can be selected by category ID and regex (optional) or by SQL query", DefaultValue = 0)]
        public InputType InputAttType { get; set; }

        [ConfigEditableEnum(DisplayName = "Category mode", Description = "Select ‘None’ for files not associated with any category or ‘All’ for attachment from the element.", DefaultValue = 0)]
        public CategoryType CatType { get; set; }

        [ConfigEditableText(DisplayName = "Category", Description = "Select the attachment category to be signed.")]
        public int InputCategoryId { get; set; }

        [ConfigEditableText(DisplayName = "Regex expression", Description = "Regular expression can be used as an additional filter for attachments from the selected category.", DefaultText = ".*[.]pdf")]
        public string AttRegularExpression { get; set; }

        [ConfigEditableText(DisplayName = "SQL query", Description = @"Query should return a list of attachments' IDs from WFDataAttachmets table.
Example: Select[ATT_ID] from[WFDataAttachmets] Where[ATT_Name] = 'agreement.pdf'", Multiline = true, TagEvaluationMode = EvaluationMode.SQL)]
        public string AttQuery { get; set; }

        [ConfigEditableFormFieldID(DisplayName = "Copy source attachment ID", Description = "Specify a text field on the form where source attachment ID will be saved")]
        public int AttTechnicalFieldID { get; set; }

        [ConfigEditableFormFieldID(DisplayName = "Copy sent Document ID to field: ", Description = "Specify a text field on the form where external Documents ID will be saved", IsRequired = true)]
        public int DokumentIdFild { get; set; }
    }

    public class ItemListConfig
    {
        [ConfigEditableItemList(DisplayName = "Signers Item List")]
        public SignersList SignersList { get; set; }

        [ConfigEditableBool(DisplayName = "Additional SMS verification", Description = "Mark this field if additional verification should be required.)")]
        public bool PhoneAutorization { get; set; }

        [ConfigEditableEnum(DisplayName = "Signature Type", Description = "The type of electronic signature.", DefaultValue = 0)]
        public SignType Type { get; set; }
    }

    public class SignersList : IConfigEditableItemList
    {
        public int ItemListId { get; set; }

        [ConfigEditableItemListColumnID(DisplayName = "Name", IsRequired = true)]
        public int SignerNameColumnID { get; set; }

        [ConfigEditableItemListColumnID(DisplayName = "E-mail", IsRequired = true)]
        public int SignerMailColumnID { get; set; }

        [ConfigEditableItemListColumnID(DisplayName = "Phone Number", IsRequired = true)]
        public int SignerPhoneNumberColumnID { get; set; }

    }

    public enum InputType
    {
        Category,
        SQL
    }

    public enum CategoryType
    {
        ID,
        All,
        None
    }

    public enum SenderType
    {
        VIEWER,
        SIGNER
    }

    public enum SignType
    {
        ESIGNATURE,
        QUALIFIED_ESIGNATURE
    }
}