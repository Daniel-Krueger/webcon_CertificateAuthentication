using System.Collections.Generic;
using WebCon.WorkFlow.SDK.Common;
using WebCon.WorkFlow.SDK.ConfigAttributes;

namespace CertificateAuthentication
{
    public class SharePointCertificateRESTAuthenticationConfig : PluginConfiguration
    {
   

        [ConfigEditableText(DisplayName = "Tenant ID", Description = "The tenant ID for the authentication", IsRequired = true, Order = 1)]
        public string TenantId { get; set; }

        [ConfigEditableText(DisplayName = "Client ID", Description = "The client ID for the authentication", IsRequired = true, Order = 2)]
        public string ClientId { get; set; }

        [ConfigEditableText(DisplayName = "Certificate (Base64) (pfx)", Description = "The base64 encoded certificate string", IsPasswordField = true, Order = 3)]
        public string Base64Certificate { get; set; }

        [ConfigEditableText(DisplayName = "Certificate Password", Description = "The password for the certificate, if any", IsPasswordField = true, Order = 4)]
        public string CertificatePassword { get; set; }

        [ConfigEditableGrid(DisplayName = "Headers", Description = "Additional headers for the request", Order = 6)]
        public List<HeaderElement> Headers { get; set; }

        [ConfigEditableGrid(DisplayName = "Body", Description = "Additional body elements of the request to get the element You need to add at least a scope element. The following values are added by default client_id, client_assertion(type), grant_type", Order = 5)]
        public List<BodyElement> Body { get; set; }

        [ConfigEditableText(DisplayName = "Access Token Property Name", Description = "The name of the property in the response that contains the access token", DefaultText = "access_token", IsRequired = true, Order = 6)]
        public string AccessTokenPropertyName { get; set; }
    }

    public class BodyElement
    {
        [ConfigEditableGridColumn(DisplayName = "Key", Description = "The key value which should be used for the value.", IsRequired = true)]
        public string Key { get; set; }

        [ConfigEditableGridColumn(DisplayName = "Value", Description = "The value of the key. This will be encoded by the plugin.")]
        public string Value { get; set; }
    }

    public class HeaderElement
    {
        [ConfigEditableGridColumn(DisplayName = "Key", Description = "The key value which should be used for the header.", IsRequired = true)]
        public string Key { get; set; }

        [ConfigEditableGridColumn(DisplayName = "Value", Description = "The value of the key. This will be encoded by the plugin.")]
        public string Value { get; set; }
    }
}