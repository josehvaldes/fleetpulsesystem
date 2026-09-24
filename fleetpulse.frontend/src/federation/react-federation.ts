import { loadAppConfig } from '@/utils/appConfig';
import { init } from '@module-federation/enhanced/runtime';

export async function InitializeReactFederation(): Promise<void> {
  const config = await loadAppConfig();

  init({
    name: 'fleet_host',
    remotes: [
      {
        name: 'alerts_mfe',
        entry: config.react_remotes.alerts,
        type: 'module',
      },
    ],
  });
}