const version = "v1";

const addResourcesToCache = async (resources) => {
    const cache = await caches.open(version);
    await cache.addAll(resources);
};

const deleteCache = async (key) => {
    await caches.delete(key);
};

const deleteOldCaches = async () => {
    await Promise.all(new Array(caches.keys()).filter(c => c != version).map(deleteCache));
};

self.addEventListener("install", (event) => {
    event.skipWaiting();
    // event.waitUntil(addResourcesToCache([

    // ]));
});

self.addEventListener("activate", (event) => {
    event.waitUntil(deleteOldCaches());
});

self.addEventListener("push", (event) => {
    const { type, data } = JSON.parse(event.data.text());

    if (type == "message") {
        const { author, content, date } = data;

        event.waitUntil(self.registration.showNotification(author, {
            body: content,
            vibrate: [100, 200, 100, 200, 100],
        }));
    }
});