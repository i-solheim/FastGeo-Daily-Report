import { CalendarIcon } from "lucide-react";
import { format, parseISO } from "date-fns";

import { Button } from "@/components/ui/button";
import { Calendar } from "@/components/ui/calendar";
import {
    Popover,
    PopoverContent,
    PopoverTrigger,
} from "@/components/ui/popover";

export default function DatePicker({
    value,
    onChange,
}) {
    const selectedDate = parseISO(value);

    return (
        <Popover>
            <PopoverTrigger
                className="inline-flex h-10 items-center justify-start rounded-md border bg-background px-3 text-sm font-normal shadow-xs hover:bg-accent hover:text-accent-foreground"
            >
                <CalendarIcon className="mr-2 h-4 w-4" />
                {format(selectedDate, "MMM d, yyyy")}
            </PopoverTrigger>

            <PopoverContent
                className="w-auto p-0"
                align="start"
            >
                <Calendar
                    mode="single"
                    selected={selectedDate}
                    disabled={(date) => date > new Date()}
                    onSelect={(date) => {
                        if (!date) return;

                        onChange(format(date, "yyyy-MM-dd"));
                    }}
                />
            </PopoverContent>
        </Popover>
    );
}