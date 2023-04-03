import { View } from "../ViewStateManager";

export class EmptyView extends View {
    id = "EmptyView";

    view = () => <></>;
}