
[SecureString]$CertificatePassword = Read-Host -Prompt "Enter certificate password" -AsSecureString
Connect-PnPOnline -Url "https://YOURDOMAIN.sharepoint.com/sitse/YOURSITE" -Interactive -ClientId "PNP .APP.ID"

# https://pnp.github.io/powershell/cmdlets/Register-PnPAzureADApp.html
$app = Register-PnPAzureADApp -ApplicationName "WEBCON to SharePoint v7" -Tenant "TENANT.ID" -CertificatePassword $CertificatePassword -SharePointApplicationPermissions "Sites.Selected" -GraphApplicationPermissions "Sites.Selected" -Interactive
$app.'AzureAppId/ClientId'

# https://pnp.github.io/powershell/cmdlets/Grant-PnPAzureADAppSitePermission.html
Grant-PnPAzureADAppSitePermission -AppId "SharePoint.APP.Registration.ID" -DisplayName "WEBCON to SharePoint v7" -Site "https://YOURDOMAIN.sharepoint.com/sites/YOURSITE" -Permissions Write
Disconnect-PnPOnline
