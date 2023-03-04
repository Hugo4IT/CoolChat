export interface GetChannelsResponse {
    items:  {
        id: number;
        chatId: number;
        name: string;
        icon: number;
    }[];
}