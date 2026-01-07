
@description('Name of the Web App')
param webAppName string

@description('Location for all resources.')
param location string = 'East Asia'

@description('App Service Plan ID')
param appServicePlanId string

@description('Docker Registry Server URL')
param dockerRegistryUrl string = 'https://acrhivespace.azurecr.io'

@description('Docker Registry Image and Tag')
param dockerImage string

@description('Docker Registry Username')
@secure()
param dockerRegistryUsername string

@description('Docker Registry Password')
@secure()
param dockerRegistryPassword string

@description('Connection String for Catalog Db')
@secure()
param connectionStringCatalogDb string

@description('RabbitMQ Host')
param rabbitMqHost string

@description('RabbitMQ Username')
@secure()
param rabbitMqUsername string

@description('RabbitMQ Password')
@secure()
param rabbitMqPassword string

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: webAppName
  location: location
  kind: 'app,linux'
  properties: {
    serverFarmId: appServicePlanId
    reserved: true
    isXenon: false
    hyperV: false
    httpsOnly: true
    clientCertMode: 'Required'
    siteConfig: {
      linuxFxVersion: 'DOCKER|${dockerImage}'
      http20Enabled: false
      minTlsVersion: '1.2'
      scmMinTlsVersion: '1.2'
      ftpsState: 'FtpsOnly'
      appSettings: [
        {
          name: 'DOCKER_REGISTRY_SERVER_URL'
          value: dockerRegistryUrl
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_USERNAME'
          value: dockerRegistryUsername
        }
        {
          name: 'DOCKER_REGISTRY_SERVER_PASSWORD'
          value: dockerRegistryPassword
        }
        {
          name: 'WEBSITES_ENABLE_APP_SERVICE_STORAGE'
          value: 'false'
        }
        {
          name: 'ConnectionStrings__CatalogDb'
          value: connectionStringCatalogDb
        }
        {
          name: 'Messaging__RabbitMq__Host'
          value: rabbitMqHost
        }
        {
          name: 'Messaging__RabbitMq__Port'
          value: '5672'
        }
        {
          name: 'Messaging__RabbitMq__Username'
          value: rabbitMqUsername
        }
        {
          name: 'Messaging__RabbitMq__Password'
          value: rabbitMqPassword
        }
      ]
    }
  }
}

resource webAppFtp 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2023-12-01' = {
  parent: webApp
  name: 'ftp'
  properties: {
    allow: false
  }
}

resource webAppScm 'Microsoft.Web/sites/basicPublishingCredentialsPolicies@2023-12-01' = {
  parent: webApp
  name: 'scm'
  properties: {
    allow: false
  }
}
