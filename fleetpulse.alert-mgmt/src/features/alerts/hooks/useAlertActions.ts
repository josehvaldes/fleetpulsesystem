import { useMutation, useQueryClient } from '@tanstack/react-query';


interface AlertAction {
    alertId: string;
    action: string;
}

export function useAlertActions() {
    const queryClient = useQueryClient();

    const mutation = useMutation({
        mutationFn: async ({ alertId, action }: AlertAction) => {
            // Simulate an API call to perform the alert action
            console.log(`Performing action: '${action}' on alert: '${alertId}'`);
            return await new Promise((resolve) => setTimeout(resolve, 1000));
        },
        onSuccess: (_, variables: AlertAction) => {
            queryClient.invalidateQueries({
                queryKey: ['alerts', variables.alertId]
            });
        },
        retryDelay: (attemptIndex: number) => Math.min(1000 * 2 ** attemptIndex, 30000),
    });
    return mutation;
}

