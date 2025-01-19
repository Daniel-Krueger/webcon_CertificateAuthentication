using System;
using System.Net.Http;
using System.Threading.Tasks;
using WebCon.WorkFlow.SDK.AutenticationPlugins;
using WebCon.WorkFlow.SDK.AutenticationPlugins.Model;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Security.Cryptography.X509Certificates;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using WebCon.WorkFlow.SDK.Common;
using System.Linq;

namespace CertificateAuthentication
{
    public class SharePointCertificateRESTAuthentication : CustomRESTAuthentication<SharePointCertificateRESTAuthenticationConfig>
    {
        private readonly IndentTextLogger _logger = new IndentTextLogger();
        private string TokenEndpoint => $"https://login.microsoftonline.com/{Configuration.TenantId}/oauth2/v2.0/token";

        private static string accessToken;
        private static DateTime? expirationTime;

        // Executing the "Test" in the REST Web Service connection will reset the acccess token.
        public override Task<TestCustomRESTAuthenticationResult> TestClientCreationAsync(CustomRestAuthenticationParams customAuthenticationParams)
        {
            _logger.Log($"Resetting the access token because Test was executed.");
            accessToken = "";
            return base.TestClientCreationAsync(customAuthenticationParams);
        }

        public override async Task<HttpClient> CreateAuthenticatedClientAsync(CustomRestAuthenticationParams customAuthenticationParams)
        {

            try
            {
                if (IsTokenValid())
                {
                    _logger.Log($"Reusing existing access token. It is valid for at least one minute");
                    HttpClient client = GetClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    return client;
                }

                _logger.Log($"Getting new access token");

                try
                {
                    X509Certificate2 certificate = GetCertificate();
                    HttpClient client = GetClient();
                    SetHeaders(client);


                    HttpResponseMessage response = null;
                    try
                    {
                        List<KeyValuePair<string, string>> requestBody = GetBody(certificate, Configuration.Body);
                        _logger.Log("Starting token request process.");
                        var requestContent = new FormUrlEncodedContent(requestBody);
                        response = await client.PostAsync(TokenEndpoint, requestContent);
                        response.EnsureSuccessStatusCode();
                        _logger.Log("Token request successful.");
                    }
                    catch (Exception ex)
                    {
                        _logger.Log($"Error occurred while requesting token: {ex.Message}", ex);
                        throw new Exception($"Error occurred while requesting token from {TokenEndpoint}.", ex);
                    }

                    var responseContent = await response.Content.ReadAsStringAsync();
                    accessToken = ExtractTokenFromResponse(responseContent, Configuration.AccessTokenPropertyName);
                    expirationTime = ExtractExpirationTimeFromToken(accessToken);
                    _logger.Log($"Access token is valid until '{(expirationTime.HasValue ? expirationTime.Value.ToString("g"):"")}' token: '{accessToken}' ");
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    _logger.Log("Authenticated client created successfully.");
                    return client;
                }
                catch (Exception ex)
                {
                    _logger.Log($"Error occurred while creating authenticated client: {ex.Message}", ex);
                    throw new Exception("Error occurred while creating authenticated client.", ex);
                }
            }
            finally
            {
                this.Context.PluginLogger.AppendDebug(_logger.ToString());
            }
        }

        private void SetHeaders(HttpClient client)
        {
            _logger.Log("Setting configured headers.");
            _logger.Indent();
            var headerElements = Configuration.Headers;
            foreach (var header in headerElements)
            {
                if (!client.DefaultRequestHeaders.Contains(header.Key))
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                    _logger.Log($"Added header: {header.Key} = {header.Value}");
                }
                else
                {
                    _logger.Log($"Header {header.Key} already exists. Overriding the value.");
                    client.DefaultRequestHeaders.Remove(header.Key);
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
            _logger.Outdent();
        }

        private List<KeyValuePair<string, string>> GetBody(X509Certificate2 certificate, List<BodyElement> bodyElements)
        {
            _logger.Log("Generating body.");
            _logger.Indent();

            var requestBody = new List<KeyValuePair<string, string>>();
            _logger.Log("Adding default keys: grant_type,client_id,client_assertion,client_assertion_type");
            requestBody.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
            requestBody.Add(new KeyValuePair<string, string>("client_id", Configuration.ClientId));
            var clientAssertion = CreateClientAssertion(certificate, Configuration.ClientId, Configuration.TenantId);
            requestBody.Add(new KeyValuePair<string, string>("client_assertion", clientAssertion));
            requestBody.Add(new KeyValuePair<string, string>("client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer"));


            _logger.Log("Adding additional body elements");
            _logger.Indent();
            foreach (var element in bodyElements)
            {
                if (!requestBody.Exists(e => e.Key == element.Key))
                {
                    requestBody.Add(new KeyValuePair<string, string>(element.Key, element.Value));
                    _logger.Log($"Added body element: {element.Key} = {element.Value}");
                }
                else
                {
                    _logger.Log($"Body element {element.Key} already exists. Skipping.");
                }
            }
            _logger.Outdent(); ;
            _logger.Outdent(); ;

            return requestBody;
        }

        private HttpClient GetClient()
        {
            _logger.Log("Preparing HttpClient");
            _logger.Indent();
            HttpClient client = null;
            var proxy = new WebCon.WorkFlow.SDK.Tools.Data.ConnectionsHelper(Context).GetProxy(TokenEndpoint);
            if (proxy != null)
            {
                _logger.Log("Adding proxy.");
                var clientHandler = new HttpClientHandler
                {
                    Proxy = proxy,
                    UseProxy = proxy != null
                };
                client = new HttpClient(clientHandler);
            }
            else
            {
                client = new HttpClient();
            }
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _logger.Outdent();


            return client;
        }

        private X509Certificate2 GetCertificate()
        {
            X509Certificate2 certificate = null;
            _logger.Log("Generating the certificate");
            _logger.Indent();
            if (!string.IsNullOrEmpty(Configuration.Base64Certificate))
            {
                _logger.Log("Adding certificate to the client.");
                try
                {
                    var certificateBytes = System.Convert.FromBase64String(Configuration.Base64Certificate);
                    certificate = string.IsNullOrEmpty(Configuration.CertificatePassword)
                        ? new X509Certificate2(certificateBytes)
                        : new X509Certificate2(certificateBytes, Configuration.CertificatePassword);

                }
                catch (FormatException ex)
                {
                    _logger.Log($"Invalid certificate format: {ex.Message}", ex);
                    throw new Exception("The provided certificate is not in a valid Base64 format.", ex);
                }
                catch (System.Security.Cryptography.CryptographicException ex)
                {
                    _logger.Log($"Error occurred while creating certificate: {ex.Message}", ex);
                    throw new Exception("An error occurred while creating the certificate from the provided Base64 string.", ex);
                }
            }
            else
            {
                _logger.Log("No certificate provided.");
            }
            _logger.Outdent();
            return certificate;
        }

        private string CreateClientAssertion(X509Certificate2 certificate, string clientId, string tenantId)
        {
            _logger.Log("Building client assertion from certificate.");
            var privateKey = certificate.GetRSAPrivateKey();
            var securityKey = new RsaSecurityKey(privateKey);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.RsaSha256);

            var now = DateTime.UtcNow;
            var jwtHeader = new JwtHeader(credentials)
            {
                { "x5t", Convert.ToBase64String(certificate.GetCertHash()) } // SHA-1 thumbprint
            };
            var jwtPayload = new JwtPayload
            {
                 { "aud", $"https://login.microsoftonline.com/{tenantId}/oauth2/token" },
                { "exp", new DateTimeOffset(now.AddMinutes(15)).ToUnixTimeSeconds() },
                { "iss", clientId },
                { "jti", Guid.NewGuid().ToString() },
                { "nbf", new DateTimeOffset(now).ToUnixTimeSeconds() },
                { "sub", clientId }
            };

            var token = new JwtSecurityToken(jwtHeader, jwtPayload);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string ExtractTokenFromResponse(string responseContent, string accessTokenPropertyName)
        {
            _logger.Log("Extract access token from response.");
            try
            {
                var json = System.Text.Json.JsonDocument.Parse(responseContent);
                if (json.RootElement.TryGetProperty(accessTokenPropertyName, out var accessToken))
                {
                    _logger.Log("Access token extracted successfully.");
                    return accessToken.GetString();
                }
                throw new Exception($"The {accessTokenPropertyName} property was not found in the response.");
            }
            catch (Exception ex)
            {
                _logger.Log($"Error occurred while extracting token: {ex.Message}", ex);
                throw new Exception($"Error occurred while extracting token from response. Response content: {responseContent}", ex);
            }
        }

        private DateTime? ExtractExpirationTimeFromToken(string token)
        {
            _logger.Log("Extracting expiration time from token");
            try
            {
                var handler = new JwtSecurityTokenHandler();

                if (handler.CanReadToken(token))
                {
                    var jwtToken = handler.ReadJwtToken(token);
                    var expClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "exp");

                    if (expClaim != null && long.TryParse(expClaim.Value, out long expSeconds))
                    {
                        // Convert Unix epoch to DateTime
                        return DateTimeOffset.FromUnixTimeSeconds(expSeconds).UtcDateTime;
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                _logger.Log($"Could not extract expiration time from token :'{token}'", ex);
                return null;
            }
        }

        private static bool IsTokenValid()
        {
            if (string.IsNullOrEmpty(accessToken) || !expirationTime.HasValue)
            {
                return false;
            }

            // Check if the token is valid for at least 1 more minute
            return expirationTime.Value > DateTime.UtcNow.AddMinutes(1);
        }
    }
}
