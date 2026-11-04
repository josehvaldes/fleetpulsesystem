import { config } from './appConfig';

export const msalConfig = {
  auth: {
    clientId: config.azure.clientId,
    authority: `https://login.microsoftonline.com/${config.azure.tenantId}`,
    // Must point to the MSAL redirect bridge page (redirect.html), not the app itself, so popup/silent flows can complete
    redirectUri: "http://localhost:5173/redirect.html",
    postLogoutRedirectUri: "http://localhost:5173/redirect.html",
    navigateToLoginRequestUrl: false,
  },
  cache: {
        cacheLocation: 'localStorage',
        storeAuthStateInCookie: false,
    },
};

