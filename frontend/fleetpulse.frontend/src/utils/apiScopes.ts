import { config } from './appConfig';

export const apiScopes = [
  `api://${config.azure.apiclientid}/access_as_user`
];