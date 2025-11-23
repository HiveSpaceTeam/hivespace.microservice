// Custom types for secure parameters
@secure()
type GoogleAuthConfig = {
  clientId: string
  clientSecret: string
}

@secure()
type LicenseKeysConfig = {
  duende: string
  mediatr: string
}

@secure()
type SmtpCredentialsConfig = {
  user: string
  password: string
}

@description('Container App name for the UserService')
param containerAppName string

@description('Container App Environment ID')
param containerAppEnvironmentId string

@description('Container image to deploy')
param containerImage string

@description('Environment name (dev, staging, prod)')
@allowed(['dev', 'staging', 'prod'])
param environment string = 'dev'

@description('Application configuration')
param appConfig object = {
  defaultRedirectUrl: ''
  issuerUrl: ''
}

@description('Client configurations')
param clientConfigs object = {
  adminPortal: {
    clientUri: ''
    redirectUri: ''
    logoutRedirectUri: ''
    corsOrigin: ''
  }
  sellerCenter: {
    clientUri: ''
    redirectUri: ''
    logoutRedirectUri: ''
    corsOrigin: ''
  }
}

@description('SMTP configuration')
param smtpConfig object = {
  server: ''
  port: '587'
  fromEmail: ''
  fromName: ''
}

@secure()
@description('Database connection string')
param connectionString string

@description('Google OAuth client secrets')
param googleAuth GoogleAuthConfig

@description('License keys')
param licenseKeys LicenseKeysConfig

@description('SMTP credentials')
param smtpCredentials SmtpCredentialsConfig

@description('Custom domain configuration')
param customDomainConfig object = {
  name: 'dev.auth.hivespace.site'
  certificateId: ''
}

@description('ACR server configuration')
param acrConfig object = {
  server: 'acrhivespace.azurecr.io'
  identity: 'system-environment'
}

@description('Deployment timestamp')
param deploymentTimestamp string = utcNow()

var containerAppSecrets = [
  {
    name: 'connection-string-user-service-db'
    value: connectionString
  }
  {
    name: 'google-client-id'
    value: googleAuth.clientId
  }
  {
    name: 'google-client-secret'
    value: googleAuth.clientSecret
  }
  {
    name: 'duende-license-key'
    value: licenseKeys.duende
  }
  {
    name: 'mediatr-license-key'
    value: licenseKeys.mediatr
  }
  {
    name: 'smtp-user'
    value: smtpCredentials.user
  }
  {
    name: 'smtp-password'
    value: smtpCredentials.password
  }
]

var environmentVariables = [
  // ASP.NET Core Configuration
  {
    name: 'ASPNETCORE_ENVIRONMENT'
    value: 'Production'
  }
  {
    name: 'ASPNETCORE_URLS'
    value: 'http://+:8080'
  }
  
  // Logging Configuration
  {
    name: 'Serilog__MinimumLevel__Default'
    value: 'Information'
  }
  {
    name: 'Serilog__MinimumLevel__Override__Microsoft'
    value: 'Warning'
  }
  {
    name: 'Serilog__MinimumLevel__Override__Microsoft.Hosting.Lifetime'
    value: 'Information'
  }
  {
    name: 'Serilog__MinimumLevel__Override__Microsoft.AspNetCore.Authentication'
    value: 'Warning'
  }
  {
    name: 'Serilog__MinimumLevel__Override__System'
    value: 'Warning'
  }
  
  // Application Configuration
  {
    name: 'DefaultRedirectUrl'
    value: appConfig.defaultRedirectUrl
  }
  {
    name: 'Issuer'
    value: appConfig.issuerUrl
  }
  
  // Database Configuration
  {
    name: 'ConnectionStrings__UserServiceDb'
    secretRef: 'connection-string-user-service-db'
  }
  
  // Admin Portal Client Configuration
  {
    name: 'Clients__adminportal__ClientId'
    value: 'adminportal'
  }
  {
    name: 'Clients__adminportal__ClientName'
    value: 'Admin Portal'
  }
  {
    name: 'Clients__adminportal__ClientUri'
    value: clientConfigs.adminPortal.clientUri
  }
  {
    name: 'Clients__adminportal__RequireClientSecret'
    value: 'false'
  }
  {
    name: 'Clients__adminportal__AllowedGrantTypes__0'
    value: 'authorization_code'
  }
  {
    name: 'Clients__adminportal__AllowAccessTokensViaBrowser'
    value: 'false'
  }
  {
    name: 'Clients__adminportal__RequireConsent'
    value: 'false'
  }
  {
    name: 'Clients__adminportal__AllowOfflineAccess'
    value: 'false'
  }
  {
    name: 'Clients__adminportal__AlwaysIncludeUserClaimsInIdToken'
    value: 'true'
  }
  {
    name: 'Clients__adminportal__RequirePkce'
    value: 'true'
  }
  {
    name: 'Clients__adminportal__RedirectUris__0'
    value: clientConfigs.adminPortal.redirectUri
  }
  {
    name: 'Clients__adminportal__PostLogoutRedirectUris__0'
    value: clientConfigs.adminPortal.logoutRedirectUri
  }
  {
    name: 'Clients__adminportal__AllowedCorsOrigins__0'
    value: clientConfigs.adminPortal.corsOrigin
  }
  {
    name: 'Clients__adminportal__AllowedScopes__0'
    value: 'openid'
  }
  {
    name: 'Clients__adminportal__AllowedScopes__1'
    value: 'profile'
  }
  {
    name: 'Clients__adminportal__AllowedScopes__2'
    value: 'user.fullaccess'
  }
  {
    name: 'Clients__adminportal__AccessTokenLifetime'
    value: '7200'
  }
  {
    name: 'Clients__adminportal__IdentityTokenLifetime'
    value: '7200'
  }
  
  // Seller Center Client Configuration
  {
    name: 'Clients__sellercenter__ClientId'
    value: 'sellercenter'
  }
  {
    name: 'Clients__sellercenter__ClientName'
    value: 'Seller Center'
  }
  {
    name: 'Clients__sellercenter__ClientUri'
    value: clientConfigs.sellerCenter.clientUri
  }
  {
    name: 'Clients__sellercenter__RequireClientSecret'
    value: 'false'
  }
  {
    name: 'Clients__sellercenter__AllowedGrantTypes__0'
    value: 'authorization_code'
  }
  {
    name: 'Clients__sellercenter__AllowAccessTokensViaBrowser'
    value: 'false'
  }
  {
    name: 'Clients__sellercenter__RequireConsent'
    value: 'false'
  }
  {
    name: 'Clients__sellercenter__AllowOfflineAccess'
    value: 'true'
  }
  {
    name: 'Clients__sellercenter__AbsoluteRefreshTokenLifetime'
    value: '2592000'
  }
  {
    name: 'Clients__sellercenter__SlidingRefreshTokenLifetime'
    value: '1296000'
  }
  {
    name: 'Clients__sellercenter__RefreshTokenExpiration'
    value: 'Sliding'
  }
  {
    name: 'Clients__sellercenter__RefreshTokenUsage'
    value: 'OneTimeOnly'
  }
  {
    name: 'Clients__sellercenter__AlwaysIncludeUserClaimsInIdToken'
    value: 'true'
  }
  {
    name: 'Clients__sellercenter__RequirePkce'
    value: 'true'
  }
  {
    name: 'Clients__sellercenter__RedirectUris__0'
    value: clientConfigs.sellerCenter.redirectUri
  }
  {
    name: 'Clients__sellercenter__PostLogoutRedirectUris__0'
    value: clientConfigs.sellerCenter.logoutRedirectUri
  }
  {
    name: 'Clients__sellercenter__AllowedCorsOrigins__0'
    value: clientConfigs.sellerCenter.corsOrigin
  }
  {
    name: 'Clients__sellercenter__AllowedScopes__0'
    value: 'openid'
  }
  {
    name: 'Clients__sellercenter__AllowedScopes__1'
    value: 'profile'
  }
  {
    name: 'Clients__sellercenter__AllowedScopes__2'
    value: 'user.fullaccess'
  }
  {
    name: 'Clients__sellercenter__AllowedScopes__3'
    value: 'offline_access'
  }
  {
    name: 'Clients__sellercenter__AccessTokenLifetime'
    value: '7200'
  }
  {
    name: 'Clients__sellercenter__IdentityTokenLifetime'
    value: '7200'
  }
  
  // Authentication Configuration
  {
    name: 'Authentication__Google__ClientId'
    secretRef: 'google-client-id'
  }
  {
    name: 'Authentication__Google__ClientSecret'
    secretRef: 'google-client-secret'
  }
  
  // License Configuration
  {
    name: 'Duende__LicenseKey'
    secretRef: 'duende-license-key'
  }
  {
    name: 'MediatR__LicenseKey'
    secretRef: 'mediatr-license-key'
  }
  
  // Email Configuration
  {
    name: 'EmailSettings__SmtpServer'
    value: smtpConfig.server
  }
  {
    name: 'EmailSettings__SmtpPort'
    value: smtpConfig.port
  }
  {
    name: 'EmailSettings__SmtpUser'
    secretRef: 'smtp-user'
  }
  {
    name: 'EmailSettings__SmtpPassword'
    secretRef: 'smtp-password'
  }
  {
    name: 'EmailSettings__FromEmail'
    value: smtpConfig.fromEmail
  }
  {
    name: 'EmailSettings__FromName'
    value: smtpConfig.fromName
  }
]

resource containerApp 'Microsoft.App/containerApps@2025-02-02-preview' = {
  name: containerAppName
  location: resourceGroup().location
  kind: 'containerapps'
  identity: {
    type: 'SystemAssigned'
  }
  tags: {
    Environment: environment
    Service: 'UserService'
    ManagedBy: 'Bicep'
    LastDeployedAt: deploymentTimestamp
  }
  properties: {
    managedEnvironmentId: containerAppEnvironmentId
    environmentId: containerAppEnvironmentId
    workloadProfileName: 'Consumption'
    configuration: {
      secrets: containerAppSecrets
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
        exposedPort: 0
        transport: 'Auto'
        allowInsecure: false
        clientCertificateMode: 'Ignore'
        stickySessions: {
          affinity: 'none'
        }
        traffic: [
          {
            weight: 100
            latestRevision: true
          }
        ]
        customDomains: [
          {
            name: customDomainConfig.name
            certificateId: customDomainConfig.certificateId
            bindingType: 'SniEnabled'
          }
        ]
      }
      registries: [
        {
          server: acrConfig.server
          identity: acrConfig.identity
        }
      ]
      identitySettings: []
      runtime: {
        dotnet: {
          autoConfigureDataProtection: false
        }
      }
      maxInactiveRevisions: 100
    }
    template: {
      containers: [
        {
          name: 'userservice'
          image: containerImage
          imageType: 'ContainerImage'
          env: environmentVariables
          resources: {
            cpu: json('0.5')
            memory: '1Gi'
          }
          probes: []
        }
      ]
      scale: {
        minReplicas: 0
        maxReplicas: 1
        cooldownPeriod: 300
        pollingInterval: 30
      }
      volumes: []
    }
  }
}

@description('The FQDN of the deployed Container App')
output containerAppFQDN string = containerApp.properties.configuration.ingress.fqdn

@description('Container App resource ID')
output containerAppId string = containerApp.id

@description('Latest revision name')
output latestRevisionName string = containerApp.properties.latestRevisionName
