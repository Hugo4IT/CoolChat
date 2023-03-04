import { Resource } from "./Resource";

export interface MyGroupsResponse {
    items: {
        id: number;
        title: string;
        icon: Resource;
    }[];
}