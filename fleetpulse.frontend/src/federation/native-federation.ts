import {
  initFederation,
  type LoadRemoteModule,
} from '@softarc/native-federation-orchestrator';

import { config } from '@/utils/appConfig';

let loadRemoteModule: LoadRemoteModule | undefined;

export async function initializeNativeFederation(): Promise<void> {

  const remotes = config.angular_remotes;

  const federation = await initFederation(remotes);
  loadRemoteModule = federation.loadRemoteModule;
}

export function loadRemote<TModule>(
  remoteName: string,
  exposedModule: string,
): Promise<TModule> {
  if (!loadRemoteModule) {
    throw new Error('Native Federation has not been initialized.');
  }

  return loadRemoteModule<TModule>(remoteName, exposedModule);
}