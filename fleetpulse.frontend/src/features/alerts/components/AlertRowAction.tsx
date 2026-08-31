import { useState } from "react"
import {
  Combobox,
  ComboboxContent,
  ComboboxEmpty,
  ComboboxInput,
  ComboboxItem,
  ComboboxList,
} from "@/components/ui/combobox"

const actions = ["Acknowledge", "Resolve", "Dismiss", "Escalate" ];

interface AlertActionComboboxProps {
        onSelectAction: (action: string, alertId: string) => void;
        alertId: string; 
}

export function AlertActionCombobox({ onSelectAction, alertId }: AlertActionComboboxProps) { 
    const [selectedAction, setSelectedAction] = useState<string | null>(null);

    return (
        <>
        <Combobox
            items={actions}
            value={selectedAction}
            onValueChange={(value) => {
                if (value) {
                    onSelectAction(value, alertId);
                    setSelectedAction(value);
                } ;
            }}
        >
        <ComboboxInput placeholder="Select an action" />
        <ComboboxContent>
            <ComboboxEmpty>No items found.</ComboboxEmpty>
            <ComboboxList  >
            {(item) => (
                <ComboboxItem key={item} value={item}>
                {item}
                </ComboboxItem>
            )}
            </ComboboxList>
        </ComboboxContent>
        </Combobox>
        
        </>
    )
}