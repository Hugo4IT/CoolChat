Date.prototype.isSameDay = function (other: Date) {
    return this.getUTCDay()      == other.getUTCDay()
        && this.getUTCMonth()    == other.getUTCMonth()
        && this.getUTCFullYear() == other.getUTCFullYear();
}

Date.prototype.addDays = function (days: number) {
    const date = new Date(this.valueOf());
    date.setDate(date.getDate() + days);
    return date;
}

export class DateFormatter {
    public static Format = (date: Date) => {
        const timeString = [
            date.getHours().toString().padStart(2, '0'),
            date.getMinutes().toString().padStart(2, '0'),
        ].join(':');
        
        const now = new Date(Date.now());

        if (date.isSameDay(now))
            return `Today at ${timeString}`;
        
        if (date.isSameDay(now.addDays(-1)))
            return `Yesterday at ${timeString}`;

        return [date.toLocaleDateString(), timeString].join(' ');
    };
}