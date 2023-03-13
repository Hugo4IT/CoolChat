import { ChannelDto } from "./ChannelDto";
import { Resource } from "./Resource";

export interface GroupDto {
    id: number;
    title: string;
    icon: Resource;
    channels: ChannelDto[];
}