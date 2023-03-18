declare var ccManager: any;
declare var getManager: () => any;

interface Date {
    addDays(days: number): Date;
    isSameDay(other: Date): boolean;
}