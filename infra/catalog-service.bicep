
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

@description('Connection String for Catalog Db')
@secure()
param connectionStringCatalogDb string

// @description('RabbitMQ Host')
// param rabbitMqHost string

// @description('RabbitMQ Username')
// @secure()
// param rabbitMqUsername string

// @description('RabbitMQ Password')
// @secure()
// param rabbitMqPassword string

resource webApp 'Microsoft.Web/sites@2023-12-01' = {
  name: webAppName
  location: location
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlanId
    reserved: true
    isXenon: false
    hyperV: false
    httpsOnly: true
    clientCertMode: 'Optional'
    siteConfig: {
      linuxFxVersion: 'sitecontainers' 
      http20Enabled: false
      minTlsVersion: '1.2'
      scmMinTlsVersion: '1.2'
      ftpsState: 'FtpsOnly'
      acrUseManagedIdentityCreds: true
      appSettings: [
        {
          name: 'DOCKER_REGISTRY_SERVER_URL'
          value: dockerRegistryUrl
        }
        {
          name: 'WEBSITES_ENABLE_APP_SERVICE_STORAGE'
          value: 'false'
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: 'Development'
        }
        {
          name: 'ConnectionStrings__CatalogDb'
          value: connectionStringCatalogDb
        }
        // {
        //   name: 'Messaging__RabbitMq__Host'
        //   value: rabbitMqHost
        // }
        // {
        //   name: 'Messaging__RabbitMq__Port'
        //   value: '5672'
        // }
        // {
        //   name: 'Messaging__RabbitMq__Username'
        //   value: rabbitMqUsername
        // }
        // {
        //   name: 'Messaging__RabbitMq__Password'
        //   value: rabbitMqPassword
        // }
        // {
        //   name: 'Messaging__Kafka__BootstrapServers'
        //   value: 'broker:29092'
        // }
        // {
        //   name: 'Messaging__Kafka__ClientId'
        //   value: 'catalog-service'
        // }
        // {
        //   name: 'Messaging__Kafka__ConsumerGroup'
        //   value: 'catalog-service'
        // }
        // {
        //   name: 'Messaging__Kafka__SecurityProtocol'
        //   value: 'PLAINTEXT'
        // }
      ]
    }
  }
}

resource webAppContainer 'Microsoft.Web/sites/sitecontainers@2023-12-01' = {
  parent: webApp
  name: 'main'
  properties: {
    image: dockerImage
    targetPort: '8080'
    isMain: true
    authType: 'SystemIdentity'
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
