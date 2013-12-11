Fix for Cannot import the following key file ACS.ServiceManagementWrapper.pfx:

Error	1	Cannot import the following key file: ACS.ServiceManagementWrapper.pfx. The key file may be password protected. To correct this, try to import the certificate again or manually install the certificate to the Strong Name CSP with the following key container name: VS_KEY_3B54AE95CF25E3AE	ACS.ServiceManagementWrapper


Solved by executing in VS Command Prompt:
sn -i ACS.ServiceManagementWrapper.pfx VS_KEY_3B54AE95CF25E3AE

Solution Found In: http://connect.microsoft.com/VisualStudio/feedback/details/524792/importing-key-file-was-canceled
