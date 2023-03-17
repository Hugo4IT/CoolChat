const version = 1;

export class IDBManager { 
    private static instance: IDBManager;

    private db?: IDBDatabase;
    private errored: boolean = false;

    public constructor() {
        IDBManager.instance = this;
    }

    private connectToDb = async (): Promise<IDBDatabase|undefined> => {
        return new Promise(resolve => {
            const request = indexedDB.open("db", version);
    
            request.onsuccess = event => {
                this.db = request.result;

                resolve(this.db);
            };
    
            request.onerror = event => {
                this.errored = true;

                console.error(`Database error: ${event}`);

                resolve(undefined);
            };

            request.onblocked = event => {
                this.errored = true;

                resolve(undefined);
            };

            request.onupgradeneeded = event => {
                this.db?.createObjectStore("login");
            };
        });
    };

    private getDb = async () => {
        if (this.db != undefined)
            return this.db;
        
        return await this.connectToDb();
    };

    public static get = async () => {
        return await IDBManager.instance.getDb();
    }
}