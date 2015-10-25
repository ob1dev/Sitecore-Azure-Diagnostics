Modify both the Web.Debug.config and Web.Release.config files under the \configuration\appSettings element. 
Replace the {account-name} with the name of your storage account, and the {account-key} with your account access key.
 
<configuration>
...
  <appSettings>
  ...
    <add key="StorageConnectionString" value="DefaultEndpointsProtocol=https;AccountName={account-name};AccountKey={account-key}" />
  </appSettings>
...
</configuration>