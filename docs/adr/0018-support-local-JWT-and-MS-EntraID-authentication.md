# Architecture Decision Record: 0018

## Title

Support Local and Microsoft Entra ID Authentication

## Status

Accepted

## Date
27/09/2026

## Context

The application currently uses username/password authentication with JWT bearer tokens.

JWTs are generated using System.IdentityModel.Tokens.Jwt and validated by the API using Microsoft.AspNetCore.Authentication.JwtBearer.

For the local authentication flow, the username and password are currently stored in application configuration. *The authentication data source is intentionally kept outside the scope of this application and may be replaced by another data source in the future.*

The application will also support authentication through Microsoft Entra ID using MSAL on the frontend and Microsoft Entra ID bearer tokens on the SignalRHub API.

The application is intended to be easily runnable by developers who clone the repository. Requiring every developer to configure an Azure Entra ID tenant, App Registration, users, and roles would make the local development experience unnecessarily complex.

Therefore, the application will support both a local authentication mode for development/demo purposes and an Entra ID authentication mode for demonstrating an enterprise identity-provider integration.

## Decision

The application will support two authentication mechanisms:

** Local authentication
Username/password authentication.
The existing JWT-based authentication mechanism will be retained.
Intended primarily for local development, demonstrations, and environments where Entra ID infrastructure is not available.

** Microsoft Entra ID authentication
The frontend will authenticate users through Microsoft Entra ID using MSAL.
The SignalRHub API will validate Entra ID access tokens.
An Azure Entra ID App Registration will be required.
Users and application roles will be configured in the Entra ID tenant.

The authentication mechanism will be selected through the AUTH_MODE configuration setting.

** Frontend

The React frontend will use AUTH_MODE during the build/configuration process to determine which authentication experience is exposed:

local — username/password authentication.
msal — Microsoft Entra ID/MSAL authentication.

The frontend authentication implementation will provide an access token to the API and Micro Frontends without requiring the API consumers to know how the token was obtained.

** SignalRHub API

The SignalRHub API will support three authentication modes:

local — only local JWT authentication is accepted.
msal — only Microsoft Entra ID bearer tokens are accepted.
both — both authentication mechanisms are accepted.

both is intended primarily for development and integration testing, where different clients may need to authenticate using different mechanisms.

The authentication mechanism will be selected when the API starts. Changing AUTH_MODE will therefore normally require restarting the API.

Regardless of the authentication mechanism, authenticated requests will be represented by the standard ASP.NET Core ClaimsPrincipal. Authorization logic should remain independent of the authentication mechanism whenever possible.

** Rationale

Supporting local authentication preserves the project's zero-friction development experience. A developer can clone the repository and run the application without creating an Azure Entra ID tenant or configuring an App Registration.

Supporting Microsoft Entra ID provides an enterprise-oriented authentication path and allows the project to demonstrate integration with a managed identity provider.

Separating authentication configuration from authorization also allows the application to evolve from the local authentication mechanism toward Entra ID without requiring application endpoints to be rewritten.

** Security Considerations

The local username/password authentication mechanism is intended primarily for development and demonstration purposes.

In a production environment, a managed identity provider such as Microsoft Entra ID, Amazon Cognito, Okta, or another appropriate identity provider should generally be preferred unless there is a specific business requirement to maintain application-managed credentials.

The local and both authentication modes should therefore not be enabled in production unless there is an explicit business or operational requirement and the corresponding authentication mechanism has been appropriately secured.

Local/demo credentials must not be treated as production credentials or committed as sensitive secrets to source control.

## Alternatives
** OKta. 
Okta is another cloud-based Identity and Access Management platform. Previous experience with the tool make is a good option. It's a strong alternative when being cloud-agnostic is required, and there are external clients that required authentication outside of enterprise infrastructure. Since the apps and users will be handled inside Azure. MSAL EntraID was the natural option instead of OKta

** Cognito
Cognito is a fully managed identity platform from Amazon Web Services (AWS) that provides the same functionality as EntraID. Previous POC's with Cognito worked well, and it is a strong tools for apps deployed in AWS. Since the main target in this project is Azure and Microsoft infrastructure, Cognito was discarded by now.


## Consequences

** Positive
Developers can run FleetPulse locally without requiring Azure infrastructure.
The application demonstrates a real enterprise identity-provider integration.
Existing local JWT authentication remains available for development.
The API can support clients using different authentication mechanisms during migration and testing.
Authorization logic can remain independent from the authentication provider.

** Negative
The application must maintain two authentication mechanisms.
Authentication configuration and testing become more complex.
The frontend must maintain separate local and MSAL authentication flows.
The API must configure and test multiple authentication schemes.
Additional care is required to ensure the local authentication mechanism is not unintentionally enabled in production.

## Migration Direction

The local authentication mechanism is retained as a development and migration path.

The intended long-term direction for production deployments is to use Microsoft Entra ID or another managed identity provider rather than application-managed username/password credentials.

## Related ADR's