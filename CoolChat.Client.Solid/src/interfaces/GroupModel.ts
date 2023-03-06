import { ChannelModel } from "./ChannelModel";
import { Resource } from "./Resource";

export interface GroupModel {
    id: number;
    title: string;
    icon: Resource;
    channels: ChannelModel[];
}