export interface GetMessagesResponse {
    items: {
        author: string;
        content: string;
        date: Date;
    }[];
}