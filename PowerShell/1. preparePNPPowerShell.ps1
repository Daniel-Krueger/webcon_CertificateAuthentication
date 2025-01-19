#region execute once
Install-Module PnP.PowerShell -Scope CurrentUser
import-module PNP.Powershell     
# WARNING: 
#  No permissions specified, using default permissions                                                                                                                                                                 


# Checking if application 'PnP PowerShell' does not exist yet...Success. Application 'PnP PowerShell' can be registered.
# App PnP PowerShell with id PNP .APP.ID created.
# App created. You can now connect to your tenant using Connect-PnPOnline -Url <yourtenanturl> -Interactive -ClientId PNP .APP.ID

# AzureAppId/ClientId
# -------------------
# PNP .APP.ID
Register-PnPEntraIDAppForInteractiveLogin -ApplicationName "PnP PowerShell2" -Tenant YOURDOMAIN.onmicrosoft.com -Interactive

#endregion
#Test access
Connect-PnPOnline -Url "https://YOURDOMAIN.sharepoint.com/sites/YOURSITE/de-DE/sales" -Interactive -ClientId "PNP .APP.ID"


