import { useEffect, useRef } from 'react';
import { loadRemote } from '@/federation/native-federation';

interface DriversMfeProps {
  apiBaseUrl: string;
  getAuthToken: () => string | null;
}

interface DriversMfeModule {
  mountDriversDashboard(
    hostElement: Element,
    props: {
      apiBaseUrl: string;
      getAuthToken: () => Promise<string>;
    },
  ): Promise<() => void>;
}

const DriversMfeWrapper = ({ apiBaseUrl, getAuthToken }: DriversMfeProps) => {
  const containerRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    let disposed = false;
    let unmount: (() => void) | undefined;

    void loadRemote<DriversMfeModule>('drivers_mfe', './mount')
      .then(async ({ mountDriversDashboard }) => {
        if (!containerRef.current || disposed) {
          return;
        }

        unmount = await mountDriversDashboard(containerRef.current, {
          apiBaseUrl,
          getAuthToken: async () => getAuthToken() ?? '',
        });

        if (disposed) {
          unmount();
        }
      })
      .catch((error: unknown) => {
        if (!disposed) {
          console.error('Error loading Angular MFE', error);
        }
      });

    return () => {
      disposed = true;
      unmount?.();
    };
  }, [apiBaseUrl, getAuthToken]);

  return <div ref={containerRef} />;
};

export default DriversMfeWrapper;