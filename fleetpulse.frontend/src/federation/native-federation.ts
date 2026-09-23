import {
  initFederation,
  type LoadRemoteModule,
} from '@softarc/native-federation-orchestrator';

const remotes = {
  drivers_mfe: 'http://localhost:4200/remoteEntry.json',
};

let loadRemoteModule: LoadRemoteModule | undefined;

export async function initializeNativeFederation(): Promise<void> {
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