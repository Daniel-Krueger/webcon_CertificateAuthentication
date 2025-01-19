$cert = New-SelfSignedCertificate -DnsName 'Updated WEBCON to SharePoint certificate' -CertStoreLocation cert:\CurrentUser\My
$pwd = ConvertTo-SecureString -String "P@ss0word!" -Force -AsPlainText
Export-PfxCertificate -Cert $cert -FilePath .\UpdatedCert.pfx -Password $pwd
Export-Certificate -Cert $cert -FilePath .\UpdatedCert.cer