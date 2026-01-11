@description('The name of the API Management service')
param apimName string

@description('The email address of the owner of the service')
param publisherEmail string = 'dev5k@gmail.com'

@description('The name of the owner of the service')
param publisherName string = 'HiveSpace'

@description('The pricing tier of this API Management service')
param sku string = 'Consumption'

@description('The instance size of this API Management service')
param skuCount int = 0

@description('Location for all resources')
param location string = resourceGroup().location

@description('Tags for the resources')
param tags object = {}

@description('URL of the User Service Backend')
param userServiceUrl string

@description('URL of the Catalog Service Backend')
param catalogServiceUrl string

@description('URL of the Media Service Backend')
param mediaServiceUrl string

@description('Application Insights Name')
param appInsightsName string

@description('Application Insights Resource ID')
param appInsightsId string

@secure()
@description('Application Insights Instrumentation Key')
param appInsightsInstrumentationKey string

@description('Key Vault Secret URI for Custom Domain Certificate')
param customDomainCertificateUrl string

@description('Custom Domain Hostname')
param customDomainHostName string = 'dev.api.hivespace.site'


resource apimService 'Microsoft.ApiManagement/service@2023-05-01-preview' = {
  name: apimName
  location: location
  tags: tags
  sku: {
    name: sku
    capacity: skuCount
  }
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    publisherEmail: publisherEmail
    publisherName: publisherName
    hostnameConfigurations: [
      {
        type: 'Proxy'
        hostName: '${apimName}.azure-api.net'
        negotiateClientCertificate: false
        defaultSslBinding: false
        certificateSource: 'BuiltIn'
      }
      {
        type: 'Proxy'
        hostName: customDomainHostName
        keyVaultId: customDomainCertificateUrl
        negotiateClientCertificate: false
        defaultSslBinding: true
        certificateSource: 'KeyVault'
      }
    ]
  }
}

// Named Values
resource appInsightsKeyNamedValue 'Microsoft.ApiManagement/service/namedValues@2023-05-01-preview' = {
  parent: apimService
  name: 'appinsights-key'
  properties: {
    displayName: 'AppInsightsKey'
    value: appInsightsInstrumentationKey
    secret: true
  }
}

// Loggers
resource appInsightsLogger 'Microsoft.ApiManagement/service/loggers@2023-05-01-preview' = {
  parent: apimService
  name: appInsightsName
  properties: {
    loggerType: 'applicationInsights'
    credentials: {
      instrumentationKey: '{{AppInsightsKey}}'
    }
    isBuffered: true
    resourceId: appInsightsId
  }
  dependsOn: [
    appInsightsKeyNamedValue
  ]
}

// Diagnostics (Global)
resource globalDiagnostics 'Microsoft.ApiManagement/service/diagnostics@2023-05-01-preview' = {
  parent: apimService
  name: 'applicationinsights'
  properties: {
    alwaysLog: 'allErrors'
    httpCorrelationProtocol: 'Legacy'
    logClientIp: true
    loggerId: appInsightsLogger.id
    sampling: {
      samplingType: 'fixed'
      percentage: 100
    }
  }
}

// Backends
resource userServiceBackend 'Microsoft.ApiManagement/service/backends@2023-05-01-preview' = {
  parent: apimService
  name: 'user-service-backend'
  properties: {
    url: userServiceUrl
    protocol: 'https'
    description: 'Backend for User Service'
  }
}

resource catalogServiceBackend 'Microsoft.ApiManagement/service/backends@2023-05-01-preview' = {
  parent: apimService
  name: 'catalog-service-backend'
  properties: {
    url: catalogServiceUrl
    protocol: 'https'
    description: 'Backend for Catalog Service'
  }
}

resource mediaServiceBackend 'Microsoft.ApiManagement/service/backends@2023-05-01-preview' = {
  parent: apimService
  name: 'media-service-backend'
  properties: {
    url: mediaServiceUrl
    protocol: 'https'
    description: 'Backend for Media Service'
  }
}

// Global API
resource globalApi 'Microsoft.ApiManagement/service/apis@2023-05-01-preview' = {
  parent: apimService
  name: 'hivespace-global-api'
  properties: {
    displayName: 'HiveSpace Global API'
    path: '/'
    protocols: ['https']
  }
}

// Global Policy (CORS + Routing)
resource globalApiPolicy 'Microsoft.ApiManagement/service/apis/policies@2023-05-01-preview' = {
  parent: globalApi
  name: 'policy'
  properties: {
    value: '''
    <policies>
        <inbound>
            <base />
            <!-- CORS Policy -->
            <cors allow-credentials="true">
                <allowed-origins>
                    <origin>https://dev.admin.hivespace.site</origin>
                    <origin>https://dev.seller.hivespace.site</origin>
                </allowed-origins>
                <allowed-methods>
                    <method>GET</method>
                    <method>POST</method>
                    <method>PUT</method>
                    <method>DELETE</method>
                    <method>PATCH</method>
                    <method>OPTIONS</method>
                </allowed-methods>
                <allowed-headers>
                    <header>*</header>
                </allowed-headers>
            </cors>
            
            <!-- Routing Logic -->
            <choose>
                <!-- User Service Routes -->
                <when condition="@(context.Request.Url.Path.StartsWith("/identity") || context.Request.Url.Path.Contains("/users/") || context.Request.Url.Path.Contains("/accounts/") || context.Request.Url.Path.Contains("/admins/") || context.Request.Url.Path.Contains("/stores/"))">
                    <set-backend-service backend-id="user-service-backend" />
                    <!-- Rewrite /identity prefix if needed. YARP config removed it. -->
                    <choose>
                        <when condition="@(context.Request.Url.Path.StartsWith("/identity"))">
                            <rewrite-uri template="@(context.Request.Url.Path.Replace("/identity", ""))" />
                        </when>
                    </choose>
                </when>
                
                <!-- Catalog Service Routes -->
                <when condition="@(context.Request.Url.Path.Contains("/categories/") || context.Request.Url.Path.Contains("/products/"))">
                    <set-backend-service backend-id="catalog-service-backend" />
                </when>
                
                <!-- Media Service Routes -->
                <when condition="@(context.Request.Url.Path.Contains("/media/"))">
                    <set-backend-service backend-id="media-service-backend" />
                </when>
            </choose>
        </inbound>
        <backend>
            <!-- Forward request to the selected backend -->
            <base />
        </backend>
        <outbound>
            <base />
        </outbound>
        <on-error>
            <base />
        </on-error>
    </policies>
    '''
    // Dynamic string interpolation for CORS origins is tricky in Bicep multiline strings. 
    // We will construct the CORS XML manually or hardcode the params for now as the user snippet had fixed origins.
    // To make it dynamic, we'd need to loop over the array. 
    // For simplicity and robustness relative to the user snippet, I'll inject the known origins matching the param default.
    // If strict dynamic CORS is needed, we can use a logical loop or specific origin entries.
    // I will replace {0} with the hardcoded origins from the param default for now to ensure it compiles correctly without complex string manipulation.
    // Actually, I'll just write the origins directly.
  }
}

// Operations (Catch-All)
resource catchAllGet 'Microsoft.ApiManagement/service/apis/operations@2023-05-01-preview' = {
  parent: globalApi
  name: 'catch-all-get'
  properties: {
    displayName: 'Catch All GET'
    method: 'GET'
    urlTemplate: '/*'
  }
}

resource catchAllPost 'Microsoft.ApiManagement/service/apis/operations@2023-05-01-preview' = {
  parent: globalApi
  name: 'catch-all-post'
  properties: {
    displayName: 'Catch All POST'
    method: 'POST'
    urlTemplate: '/*'
  }
}

resource catchAllPut 'Microsoft.ApiManagement/service/apis/operations@2023-05-01-preview' = {
  parent: globalApi
  name: 'catch-all-put'
  properties: {
    displayName: 'Catch All PUT'
    method: 'PUT'
    urlTemplate: '/*'
  }
}

resource catchAllDelete 'Microsoft.ApiManagement/service/apis/operations@2023-05-01-preview' = {
  parent: globalApi
  name: 'catch-all-delete'
  properties: {
    displayName: 'Catch All DELETE'
    method: 'DELETE'
    urlTemplate: '/*'
  }
}

resource catchAllPatch 'Microsoft.ApiManagement/service/apis/operations@2023-05-01-preview' = {
  parent: globalApi
  name: 'catch-all-patch'
  properties: {
    displayName: 'Catch All PATCH'
    method: 'PATCH'
    urlTemplate: '/*'
  }
}
