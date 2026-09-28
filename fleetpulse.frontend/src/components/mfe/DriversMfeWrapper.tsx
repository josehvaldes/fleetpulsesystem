import { useEffect, useRef } from 'react';
import { loadRemote } from '@/federation/native-federation';
import { config} from "@/utils/appConfig";
import { getAuthToken } from "@/services/authTokenProvider";

interface DriversMfeModule {
  mountDriversDashboard(
    hostElement: Element,
    props: {
      apiBaseUrl: string;
      getAuthToken: () => Promise<string|null>;
    },
  ): Promise<() => void>;
}

const apiBaseUrl = config.api.baseUrl || "https://localhost:7234/api";

const DriversMfeWrapper = () => {
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
          getAuthToken: getAuthToken,
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